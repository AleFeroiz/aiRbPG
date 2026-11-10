using aiRbPG.Domain.Catalog;
using aiRbPG.Domain.Pricing;
using aiRbPG.Domain.Reservations;
using aiRbPG.Domain.Users;
using System.Globalization;

namespace aiRbPG.WinForms.Forms;

public sealed class ReservationForm : Form
{
    private readonly UserAccount _user;
    private readonly ICatalogRepository _catalog;
    private readonly IReservationRepository _reservations;
    private readonly DataGridView _list = new();
    private readonly DataGridView _accessories = new();
    private readonly ComboBox _client = new();
    private readonly ComboBox _space = new();
    private readonly DateTimePicker _start = DatePicker(DateTime.Now.AddHours(1));
    private readonly DateTimePicker _end = DatePicker(DateTime.Now.AddHours(4));
    private readonly NumericUpDown _people = new() { Minimum = 1, Maximum = 100, Value = 4, Width = 90 };
    private readonly TextBox _notes = new();
    private readonly Label _status = new();
    private readonly Label _quote = new();

    public ReservationForm(UserAccount user, ICatalogRepository catalog, IReservationRepository reservations)
    {
        _user = user;
        _catalog = catalog;
        _reservations = reservations;
        Text = "aiRbPG — Reservas";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1080, 720);
        BackColor = Color.FromArgb(242, 240, 235);
        Font = new Font("Segoe UI", 10F);
        BuildLayout();
        Load += async (_, _) => await LoadAsync();
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
        _accessories.CellEndEdit += (_, _) => UpdateQuote();
        _accessories.CurrentCellDirtyStateChanged += (_, _) => { if (_accessories.IsCurrentCellDirty) _accessories.CommitEdit(DataGridViewDataErrorContexts.Commit); };
        _accessories.DataError += (_, e) => { e.ThrowException = false; _status.Text = "Informe uma quantidade inteira válida."; };

        var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(0, 0, 18, 0) };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.Controls.Add(Heading("Reservas cadastradas"), 0, 0);
        left.Controls.Add(_list, 0, 1);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7 };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        right.Controls.Add(Heading("Nova reserva"), 0, 0);
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
        buttons.Controls.Add(Button("Criar reserva", async (_, _) => await CreateAsync(), true));
        buttons.Controls.Add(Button("Atualizar estoque", async (_, _) => await LoadAccessoriesAsync()));
        right.Controls.Add(buttons, 0, 6);
        _start.ValueChanged += async (_, _) => await LoadAccessoriesAsync();
        _end.ValueChanged += async (_, _) => await LoadAccessoriesAsync();
        _space.SelectedIndexChanged += (_, _) => UpdateQuote();
        _people.ValueChanged += (_, _) => UpdateQuote();
        root.Controls.Add(left, 0, 0);
        root.Controls.Add(right, 1, 0);
        Controls.Add(root);
    }

    private Control BuildFields()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5 };
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

    private async Task RefreshReservationsAsync() => _list.DataSource = (await _reservations.GetReservationsAsync()).ToList();

    private async Task LoadAccessoriesAsync()
    {
        if (_end.Value <= _start.Value) { _status.Text = "O término deve ser posterior ao início."; return; }
        try
        {
            var availability = await _reservations.GetAvailableAccessoriesAsync(_start.Value, _end.Value);
            _accessories.DataSource = availability.Select(row => new AccessoryRow(row.Accessory.Id, row.Accessory.Name, row.Available, row.Accessory.HourlyRate, 0)).ToList();
            _status.Text = string.Empty;
            UpdateQuote();
        }
        catch { _status.Text = "Não foi possível consultar o estoque para o período."; }
    }

    private void UpdateQuote()
    {
        if (_space.SelectedItem is not RpgSpace space || _end.Value <= _start.Value) return;
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
        if (_client.SelectedItem is not Client client || _space.SelectedItem is not RpgSpace space)
        { _status.Text = "Cadastre ou selecione um cliente e um espaço ativo."; return; }
        try
        {
            var rows = ReadAccessoryRows();
            if (rows.Any(row => row.Quantity < 0 || row.Quantity > row.Available))
            { _status.Text = "As quantidades precisam estar entre zero e o estoque disponível."; return; }
            var id = await _reservations.CreateAsync(new NewReservation(client.Id, space.Id, _user.Id, _start.Value, _end.Value, (int)_people.Value, string.IsNullOrWhiteSpace(_notes.Text) ? null : _notes.Text, rows.Where(row => row.Quantity > 0).Select(row => new ReservationAccessory(row.Id, row.Name, row.Quantity, row.Rate)).ToArray()));
            _status.Text = $"Reserva #{id} criada como pendente.";
            await RefreshReservationsAsync();
            await LoadAccessoriesAsync();
        }
        catch (Exception ex) { _status.Text = ex.Message; }
    }

    private List<AccessoryRow> ReadAccessoryRows()
    {
        _accessories.EndEdit();
        return _accessories.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow)
            .Select(row => new AccessoryRow(Convert.ToInt32(row.Cells[0].Value), Convert.ToString(row.Cells[1].Value) ?? "Acessório",
                Convert.ToInt32(row.Cells[2].Value), Convert.ToDecimal(row.Cells[3].Value),
                int.TryParse(Convert.ToString(row.Cells[4].Value), out var quantity) ? quantity : 0)).ToList();
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.ReadOnly = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
    }

    private static DateTimePicker DatePicker(DateTime value) => new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", ShowUpDown = true, Value = value };
    private static Label Heading(string text) => new() { Text = text, Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(36, 41, 51), TextAlign = ContentAlignment.MiddleLeft };
    private static Button Button(string text, EventHandler onClick, bool primary = false)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 36, FlatStyle = FlatStyle.Flat, BackColor = primary ? Color.FromArgb(183, 132, 55) : Color.FromArgb(225, 222, 215), ForeColor = primary ? Color.White : Color.FromArgb(36, 41, 51) };
        button.Click += onClick;
        return button;
    }

    private sealed class AccessoryRow(int accessoryId, string name, int available, decimal rate, int quantity)
    {
        public int AccessoryId { get; } = accessoryId;
        public string Name { get; } = name;
        public int Available { get; } = available;
        public decimal Rate { get; } = rate;
        public int Quantity { get; set; } = quantity;
    }
}
