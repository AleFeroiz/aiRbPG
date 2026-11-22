using aiRbPG.Domain.Catalog;
using aiRbPG.Domain.Pricing;
using aiRbPG.Domain.Reservations;
using aiRbPG.Domain.Users;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aiRbPG.WinForms.Forms
{
public sealed class ReservationForm : Form
{
    private readonly UserAccount _user;
    private readonly ICatalogRepository _catalog;
    private readonly IReservationRepository _reservations;
    private readonly DataGridView _list = new DataGridView();
    private readonly DataGridView _accessories = new DataGridView();
    private readonly ComboBox _client = new ComboBox();
    private readonly ComboBox _space = new ComboBox();
    private readonly DateTimePicker _start = DatePicker(DateTime.Now.AddHours(1));
    private readonly DateTimePicker _end = DatePicker(DateTime.Now.AddHours(4));
    private readonly NumericUpDown _people = new NumericUpDown() { Minimum = 1, Maximum = 100, Value = 4, Width = 90 };
    private readonly TextBox _notes = new TextBox();
    private readonly Label _status = new Label();
    private readonly Label _quote = new Label();
    private readonly TextBox _search = new TextBox { Width = 180 };
    private readonly ComboBox _filterStatus = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _editorTitle = Heading("Nova reserva");
    private int _editingId;
    private bool _loading;
    private int _availabilityVersion;
    private IReadOnlyList<Reservation> _rows = new Reservation[0];
    private readonly aiRbPG.Data.Reservations.SqlOperationsRepository _operations;

    public ReservationForm(UserAccount user, ICatalogRepository catalog, IReservationRepository reservations)
    {
        _user = user;
        _catalog = catalog;
        _reservations = reservations;
        _operations = new aiRbPG.Data.Reservations.SqlOperationsRepository(new aiRbPG.Data.SqlConnectionFactory(),user);
        Text = "aiRbPG — Reservas";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1080, 720);
        BackColor = Color.FromArgb(242, 240, 235);
        Font = new Font("Segoe UI", 10F);
        BuildLayout();
        Load += async (sender, args) => await LoadAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 2, RowCount = 1 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        _list.Dock = DockStyle.Fill;
        ConfigureGrid(_list);
        _list.ReadOnly = true;
        _accessories.Dock = DockStyle.Fill;
        ConfigureGrid(_accessories);
        _accessories.AllowUserToAddRows = false;
        _accessories.AutoGenerateColumns = false;
        _accessories.Columns.Add(new DataGridViewTextBoxColumn { Name = "AccessoryId", DataPropertyName = "AccessoryId", Visible = false });
        _accessories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Acessório", DataPropertyName = "Name", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _accessories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Disponível", DataPropertyName = "Available", ReadOnly = true, Width = 90 });
        _accessories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "R$/h", DataPropertyName = "Rate", ReadOnly = true, Width = 80 });
        _accessories.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Qtd.", DataPropertyName = "Quantity", Width = 70 });
        _accessories.CellEndEdit += (sender, args) => UpdateQuote();
        _accessories.CurrentCellDirtyStateChanged += (sender, args) => { if (_accessories.IsCurrentCellDirty) _accessories.CommitEdit(DataGridViewDataErrorContexts.Commit); };
        _accessories.DataError += (sender, args) => { args.ThrowException = false; _status.Text = "Informe uma quantidade inteira válida."; };

        var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(0, 0, 18, 0) };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.Controls.Add(Heading("Reservas cadastradas"), 0, 0);
        var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
        _filterStatus.Items.AddRange(new object[] { "Todos","Pendente","Confirmada","EmAndamento","AguardandoDevolucao","Finalizada","Cancelada" });
        _filterStatus.SelectedIndex = 0;
        tools.Controls.Add(new Label { Text = "Buscar cliente / espaço", AutoSize = true }); tools.Controls.Add(_search); tools.Controls.Add(_filterStatus);
        tools.Controls.Add(Ui.Action("Operar selecionada",async () => { int id = SelectedId(); using (var form = new OperationForm(id,_operations,_user)) form.ShowDialog(this); await RefreshReservationsAsync(); await LoadAccessoriesAsync(); }));
        tools.Controls.Add(Ui.Action("Carregar para editar",LoadSelectedAsync));
        tools.Controls.Add(Ui.Action("Atualizar lista",RefreshReservationsAsync));
        _search.TextChanged += (s,e) => ApplyFilter(); _filterStatus.SelectedIndexChanged += (s,e) => ApplyFilter();
        left.Controls.Add(tools,0,1); left.Controls.Add(_list, 0, 2);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7 };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        right.Controls.Add(_editorTitle, 0, 0);
        right.Controls.Add(BuildFields(), 0, 1);
        right.Controls.Add(new Label { Text = "Acessórios adicionais (disponibilidade no período)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        right.Controls.Add(_accessories, 0, 3);
        _quote.Dock = DockStyle.Fill;
        _quote.TextAlign = ContentAlignment.MiddleLeft;
        _quote.ForeColor = Color.FromArgb(117, 74, 41);
        right.Controls.Add(_quote, 0, 4);
        _status.Dock = DockStyle.Fill;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        _status.ForeColor = Color.FromArgb(117, 74, 41);
        right.Controls.Add(_status, 0, 5);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        buttons.Controls.Add(Ui.Action("Salvar reserva", CreateAsync));
        buttons.Controls.Add(Ui.Action("Nova / limpar",async () => { _editingId = 0; _editorTitle.Text = "Nova reserva"; _notes.Clear(); _accessories.DataSource = null; await LoadAccessoriesAsync(); }));
        buttons.Controls.Add(Button("Atualizar estoque", async (sender, args) => await LoadAccessoriesAsync()));
        buttons.Controls.Add(Ui.Action("Espaços livres",async () =>
        {
            if (_end.Value <= _start.Value) throw new ArgumentException("Período inválido.");
            var reservations = await _reservations.GetReservationsAsync();
            var spaces = await _catalog.GetSpacesAsync();
            _space.DataSource = spaces.Where(s => !reservations.Any(r => r.Id != _editingId && r.SpaceId == s.Id && r.Status != "Cancelada" && r.Status != "Finalizada" && r.Start < _end.Value && (r.End > _start.Value || r.Status == "EmAndamento" || r.Status == "AguardandoDevolucao"))).ToList();
            _status.Text = "Espaços livres para o período carregados. A agenda é validada novamente ao salvar.";
        }));
        right.Controls.Add(buttons, 0, 6);
        _start.ValueChanged += async (sender, args) => await LoadAccessoriesAsync();
        _end.ValueChanged += async (sender, args) => await LoadAccessoriesAsync();
        _space.SelectedIndexChanged += (sender, args) => UpdateQuote();
        _people.ValueChanged += (sender, args) => UpdateQuote();
        root.Controls.Add(left, 0, 0);
        root.Controls.Add(right, 1, 0);
        Controls.Add(root);
    }

    private Control BuildFields()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
        for (int i = 0; i < 3; i++) table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / 3F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        AddField(table, "Cliente", _client, 0, 0);
        AddField(table, "Espaço", _space, 1, 0);
        AddField(table, "Início", _start, 0, 1);
        AddField(table, "Término", _end, 1, 1);
        AddField(table, "Participantes", _people, 0, 2);
        AddField(table, "Observações", _notes, 1, 2);
        _client.DropDownStyle = ComboBoxStyle.DropDownList;
        _space.DropDownStyle = ComboBoxStyle.DropDownList;
        return table;
    }

    private static void AddField(TableLayoutPanel table, string label, Control input, int column, int row)
    {
        var group = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(4) };
        group.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        group.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        group.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill }, 0, 0);
        input.Dock = DockStyle.Fill;
        group.Controls.Add(input, 0, 1);
        table.Controls.Add(group, column, row);
    }

    private async Task LoadAsync()
    {
        try
        {
            _client.DataSource = (await _catalog.GetClientsAsync()).ToList();
            _client.DisplayMember = nameof(Client.Name);
            _client.ValueMember = nameof(Client.Id);
            _space.DataSource = (await _catalog.GetSpacesAsync()).ToList();
            _space.DisplayMember = nameof(RpgSpace.Name);
            _space.ValueMember = nameof(RpgSpace.Id);
            await RefreshReservationsAsync();
            await LoadAccessoriesAsync();
        }
        catch { _status.Text = "Não foi possível carregar cadastros. Verifique a conexão com o SQL Server."; }
    }

    private async Task RefreshReservationsAsync() { _rows = await _reservations.GetReservationsAsync(); ApplyFilter(); }
    private void ApplyFilter()
    {
        var query = _search.Text.Trim(); var state = Convert.ToString(_filterStatus.SelectedItem);
        _list.DataSource = _rows.Where(r => (state == "Todos" || state == r.Status) && (r.ClientName + " " + r.SpaceName).IndexOf(query,StringComparison.OrdinalIgnoreCase) >= 0)
            .Select(r => new { Codigo = r.Id, Cliente = r.ClientName, Espaco = r.SpaceName, Inicio = r.Start, Fim = r.End, Pessoas = r.People, Status = r.Status, Total = r.Total }).ToList();
    }
    private int SelectedId()
    {
        if (_list.CurrentRow == null) throw new InvalidOperationException("Selecione uma reserva na lista.");
        return Convert.ToInt32(_list.CurrentRow.Cells["Codigo"].Value);
    }
    private async Task LoadSelectedAsync()
    {
        int id = SelectedId(); var table = await _operations.DetailsAsync(id);
        if (table.Rows.Count == 0) return;
        var row = table.Rows[0]; string state = Convert.ToString(row["Status"]);
        if ((state != "Pendente" && state != "Confirmada") || Convert.ToDecimal(row["Pago"]) != 0) throw new InvalidOperationException("Só edite reservas pendentes/confirmadas sem pagamento.");
        _loading = true;
        try
        {
            _editingId = id; _client.SelectedValue = Convert.ToInt32(row["ClienteId"]); _space.SelectedValue = Convert.ToInt32(row["EspacoId"]);
            _start.Value = Convert.ToDateTime(row["Inicio"]); _end.Value = Convert.ToDateTime(row["Fim"]); _people.Value = Convert.ToDecimal(row["QuantidadePessoas"]); _notes.Text = Convert.ToString(row["Observacao"]);
            _editorTitle.Text = "Editar reserva #" + id;
        }
        finally { _loading = false; }
        await LoadAccessoriesAsync(true);
        UpdateQuote();
    }

    private async Task LoadAccessoriesAsync(bool restoreSaved = false)
    {
        if (_loading) return;
        int version = ++_availabilityVersion;
        if (_end.Value <= _start.Value) { _status.Text = "O término deve ser posterior ao início."; return; }
        try
        {
            var quantities = ReadAccessoryRows().ToDictionary(r => r.AccessoryId,r => r.Quantity);
            var availability = await _reservations.GetAvailableAccessoriesAsync(_start.Value, _end.Value);
            if (version != _availabilityVersion) return;
            _accessories.DataSource = availability.Select(row => new AccessoryRow(row.Accessory.Id, row.Accessory.Name, row.Available, row.Accessory.HourlyRate, quantities.ContainsKey(row.Accessory.Id) ? quantities[row.Accessory.Id] : 0)).ToList();
            if (_editingId != 0)
            {
                var detail = await _operations.DetailsAsync(_editingId);
                var ownItems = await _operations.ItemsAsync(_editingId);
                if (version != _availabilityVersion) return;
                bool overlaps = detail.Rows.Count > 0 && Convert.ToDateTime(detail.Rows[0]["Inicio"]) < _end.Value && Convert.ToDateTime(detail.Rows[0]["Fim"]) > _start.Value;
                foreach (DataGridViewRow gridRow in _accessories.Rows)
                    {
                        var own = ownItems.Rows.Cast<System.Data.DataRow>().FirstOrDefault(r => Convert.ToInt32(r["AcessorioId"]) == Convert.ToInt32(gridRow.Cells[0].Value));
                        var model = (AccessoryRow)gridRow.DataBoundItem;
                        if (own != null && overlaps) model.Available += Convert.ToInt32(own["Reservados"]);
                        if (restoreSaved) model.Quantity = own == null ? 0 : Convert.ToInt32(own["Reservados"]);
                    }
                _accessories.Refresh();
            }
            _status.Text = string.Empty;
            UpdateQuote();
        }
        catch { _status.Text = "Não foi possível consultar o estoque para o período."; }
    }

    private void UpdateQuote()
    {
        var space = _space.SelectedItem as RpgSpace;
        if (space == null || _end.Value <= _start.Value) return;
        try
        {
            var charges = ReadAccessoryRows().Where(row => row.Quantity > 0).Select(row => new AccessoryCharge(row.Quantity, row.Rate));
            var quote = PricingCalculator.Calculate((decimal)(_end.Value - _start.Value).TotalMinutes / 60m, (int)_people.Value, space.HourlyRate, space.PerPersonHourlyRate, charges);
            _quote.Text = $"Estimativa: {quote.Total.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}  (espaço, pessoas e acessórios)";
        }
        catch { _quote.Text = "Confira as quantidades para visualizar o valor estimado."; }
    }

    private async Task CreateAsync()
    {
        var client = _client.SelectedItem as Client;
        var space = _space.SelectedItem as RpgSpace;
        if (client == null || space == null)
        { _status.Text = "Cadastre ou selecione um cliente e um espaço ativo."; return; }
        try
        {
            var rows = ReadAccessoryRows();
            if (rows.Any(row => row.Quantity < 0 || row.Quantity > row.Available))
            { _status.Text = "As quantidades precisam estar entre zero e o estoque disponível."; return; }
            var draft = new NewReservation(client.Id, space.Id, _user.Id, _start.Value, _end.Value, (int)_people.Value, string.IsNullOrWhiteSpace(_notes.Text) ? null : _notes.Text, rows.Where(row => row.Quantity > 0).Select(row => new ReservationAccessory(row.AccessoryId, row.Name, row.Quantity, row.Rate)).ToArray());
            var id = _editingId == 0 ? await _reservations.CreateAsync(draft) : await _reservations.UpdateAsync(_editingId,draft);
            _editingId = 0; _editorTitle.Text = "Nova reserva";
            _accessories.DataSource = null;
            await RefreshReservationsAsync();
            await LoadAccessoriesAsync();
            _status.Text = $"Reserva #{id} salva.";
        }
        catch (Exception ex) { _status.Text = ex.Message; }
    }

    private List<AccessoryRow> ReadAccessoryRows()
    {
        _accessories.EndEdit();
        return _accessories.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow)
            .Select(row =>
            {
                int quantity;
                if (!int.TryParse(Convert.ToString(row.Cells[4].Value), out quantity)) throw new ArgumentException("Informe uma quantidade inteira de acessórios.");
                return new AccessoryRow(Convert.ToInt32(row.Cells[0].Value), Convert.ToString(row.Cells[1].Value) ?? "Acessório",
                    Convert.ToInt32(row.Cells[2].Value), Convert.ToDecimal(row.Cells[3].Value), quantity);
            }).ToList();
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.ReadOnly = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
    }

    private static DateTimePicker DatePicker(DateTime value) => new DateTimePicker() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", ShowUpDown = true, Value = new DateTime(value.Year,value.Month,value.Day,value.Hour,value.Minute,0) };
    private static Label Heading(string text) => new Label() { Text = text, Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(36, 41, 51), TextAlign = ContentAlignment.MiddleLeft };
    private static Button Button(string text, EventHandler onClick, bool primary = false)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 36, FlatStyle = FlatStyle.Flat, BackColor = primary ? Color.FromArgb(183, 132, 55) : Color.FromArgb(225, 222, 215), ForeColor = primary ? Color.White : Color.FromArgb(36, 41, 51) };
        button.Click += onClick;
        return button;
    }

    private sealed class AccessoryRow
    {
        public AccessoryRow(int accessoryId, string name, int available, decimal rate, int quantity)
        { AccessoryId = accessoryId; Name = name; Available = available; Rate = rate; Quantity = quantity; }
        public int AccessoryId { get; private set; }
        public string Name { get; private set; }
        public int Available { get; set; }
        public decimal Rate { get; private set; }
        public int Quantity { get; set; }
    }
}
}
