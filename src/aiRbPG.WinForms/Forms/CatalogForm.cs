using aiRbPG.Domain.Catalog;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aiRbPG.WinForms.Forms
{
public sealed class CatalogForm : Form
{
    private readonly ICatalogRepository _catalog;
    private readonly CatalogKind _kind;
    private readonly DataGridView _grid = new DataGridView();
    private readonly Dictionary<string, TextBox> _inputs = new Dictionary<string, TextBox>();
    private readonly Label _message = new Label();
    private readonly Label _mode = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
    private Button _saveButton;
    private Button _deactivateButton;
    private int _selectedId;
    private bool _refreshing;
    private bool _saving;

    public CatalogForm(ICatalogRepository catalog, CatalogKind kind)
    {
        _catalog = catalog;
        _kind = kind;
        Text = $"aiRbPG — {Title}";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1080, 720);
        MinimumSize = new Size(900, 600);
        BackColor = Color.FromArgb(242, 240, 235);
        Font = new Font("Segoe UI", 10F);

        BuildLayout();
        Load += LoadRowsAsync;
    }

    private string Title
    {
        get
        {
            switch (_kind)
            {
                case CatalogKind.Clients: return "Clientes";
                case CatalogKind.Spaces: return "Espaços";
                case CatalogKind.RpgSystems: return "Sistemas de RPG";
                case CatalogKind.Accessories: return "Acessórios e estoque";
                default: return "Cadastros";
            }
        }
    }

    private void BuildLayout()
    {
        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(22),
            BackColor = BackColor
        };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.RowHeadersVisible = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.CellClick += (sender, args) => EditRow(args.RowIndex);
        _grid.KeyUp += GridKeyUp;

        var editor = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = FieldCount() + 4,
            Padding = new Padding(24, 8, 8, 8)
        };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var heading = new Label
        {
            Text = Title,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(36, 41, 51),
            TextAlign = ContentAlignment.MiddleLeft
        };
        editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        editor.Controls.Add(heading, 0, 0);

        var row = 1;
        editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        editor.Controls.Add(_mode, 0, row++);
        foreach (var field in Fields())
        {
            var group = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0, 4, 0, 4) };
            group.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            group.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            group.Controls.Add(new Label { Text = field.Label, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(74, 78, 86) }, 0, 0);

            var input = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            _inputs.Add(field.Key, input);
            group.Controls.Add(input, 0, 1);
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            editor.Controls.Add(group, 0, row++);
        }

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 10, 0, 0)
        };
        _saveButton = CreateButton("Salvar novo", SaveAsync, primary: true);
        buttons.Controls.Add(_saveButton);
        buttons.Controls.Add(CreateButton("Novo", StartNew));
        _deactivateButton = CreateButton("Inativar", DeactivateAsync);
        buttons.Controls.Add(_deactivateButton);
        editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        editor.Controls.Add(buttons, 0, row++);

        _message.Dock = DockStyle.Fill;
        _message.ForeColor = Color.FromArgb(117, 74, 41);
        _message.AutoEllipsis = true;
        editor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        editor.Controls.Add(_message, 0, row);

        shell.Controls.Add(_grid, 0, 0);
        shell.Controls.Add(editor, 1, 0);
        Controls.Add(shell);
        UpdateMode();
    }

    private IEnumerable<CatalogField> Fields()
    {
        switch (_kind)
        {
            case CatalogKind.Clients:
                return new[] { new CatalogField("name", "Nome"), new CatalogField("phone", "Telefone"), new CatalogField("email", "E-mail") };
            case CatalogKind.Spaces:
                return new[] { new CatalogField("name", "Nome"), new CatalogField("description", "Descrição"), new CatalogField("address", "Endereço"), new CatalogField("capacity", "Capacidade"), new CatalogField("hourlyRate", "Tarifa do espaço por hora"), new CatalogField("personRate", "Tarifa opcional por pessoa/hora") };
            case CatalogKind.RpgSystems:
                return new[] { new CatalogField("name", "Nome"), new CatalogField("description", "Descrição") };
            case CatalogKind.Accessories:
                return new[] { new CatalogField("name", "Nome"), new CatalogField("description", "Descrição"), new CatalogField("stock", "Quantidade em estoque"), new CatalogField("hourlyRate", "Tarifa por unidade/hora") };
            default:
                return new CatalogField[0];
        }
    }

    private int FieldCount() => Fields().Count();

    private static Button CreateButton(string text, EventHandler onClick, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            Width = primary ? 112 : 88,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Color.FromArgb(183, 132, 55) : Color.FromArgb(225, 222, 215),
            ForeColor = primary ? Color.White : Color.FromArgb(36, 41, 51),
            Margin = new Padding(0, 0, 8, 0)
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += onClick;
        return button;
    }

    private async void LoadRowsAsync(object sender, EventArgs e) => await RefreshRowsAsync();

    private async Task RefreshRowsAsync(string successMessage = "")
    {
        _refreshing = true;
        try
        {
            switch (_kind)
            {
                case CatalogKind.Clients: _grid.DataSource = await _catalog.GetClientsAsync(); break;
                case CatalogKind.Spaces: _grid.DataSource = await _catalog.GetSpacesAsync(); break;
                case CatalogKind.RpgSystems: _grid.DataSource = await _catalog.GetRpgSystemsAsync(); break;
                case CatalogKind.Accessories: _grid.DataSource = await _catalog.GetAccessoriesAsync(); break;
                default: _grid.DataSource = new object[0]; break;
            }
            ClearInputs();
            _message.Text = successMessage;
        }
        catch (Exception)
        {
            _message.Text = (successMessage + " Não foi possível atualizar a lista. Confira a conexão com o banco.").TrimStart();
        }
        finally { _refreshing = false; }
    }

    private void GridKeyUp(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Up:
            case Keys.Down:
            case Keys.Home:
            case Keys.End:
            case Keys.PageUp:
            case Keys.PageDown:
            case Keys.Space:
                if (_grid.CurrentRow != null) EditRow(_grid.CurrentRow.Index);
                break;
        }
    }

    private void EditRow(int rowIndex)
    {
        if (_refreshing || _saving || rowIndex < 0 || rowIndex >= _grid.Rows.Count) return;
        var selected = _grid.Rows[rowIndex].DataBoundItem;
        _message.Text = string.Empty;

        var client = selected as Client;
        if (client != null)
        {
                _selectedId = client.Id;
                Set("name", client.Name); Set("phone", client.Phone); Set("email", client.Email);
                UpdateMode();
                return;
        }
        var space = selected as RpgSpace;
        if (space != null)
        {
                _selectedId = space.Id;
                Set("name", space.Name); Set("description", space.Description); Set("address", space.Address);
                Set("capacity", space.Capacity); Set("hourlyRate", space.HourlyRate); Set("personRate", space.PerPersonHourlyRate);
                UpdateMode();
                return;
        }
        var system = selected as RpgSystem;
        if (system != null)
        {
                _selectedId = system.Id;
                Set("name", system.Name); Set("description", system.Description);
                UpdateMode();
                return;
        }
        var accessory = selected as Accessory;
        if (accessory != null)
        {
                _selectedId = accessory.Id;
                Set("name", accessory.Name); Set("description", accessory.Description);
                Set("stock", accessory.QuantityInStock); Set("hourlyRate", accessory.HourlyRate);
                UpdateMode();
        }
    }

    private void Set(string key, object value)
    {
        TextBox input;
        if (_inputs.TryGetValue(key, out input))
        {
            input.Text = Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
        }
    }

    private async void SaveAsync(object sender, EventArgs e)
    {
        if (_saving || _refreshing) return;
        var name = Value("name").Trim();
        if (name.Length == 0)
        {
            _message.Text = "O nome é obrigatório.";
            return;
        }

        _saving = true;
        try
        {
            switch (_kind)
            {
                case CatalogKind.Clients:
                    await _catalog.SaveClientAsync(new Client(_selectedId, name, Optional("phone"), Optional("email")));
                    break;
                case CatalogKind.Spaces:
                    int capacity;
                    decimal spaceRate;
                    decimal personRate = 0;
                    var personRateInput = Value("personRate");
                    if (!int.TryParse(Value("capacity"), out capacity) || capacity <= 0 ||
                        !TryDecimal(Value("hourlyRate"), out spaceRate) || spaceRate < 0 ||
                        (!string.IsNullOrWhiteSpace(personRateInput) &&
                            (!TryDecimal(personRateInput, out personRate) || personRate < 0)))
                    {
                        _message.Text = "Informe capacidade positiva e tarifas válidas, iguais ou maiores que zero.";
                        return;
                    }

                    await _catalog.SaveSpaceAsync(new RpgSpace(_selectedId, name, Optional("description"), Optional("address"), capacity, spaceRate, personRate));
                    break;
                case CatalogKind.RpgSystems:
                    await _catalog.SaveRpgSystemAsync(new RpgSystem(_selectedId, name, Optional("description")));
                    break;
                case CatalogKind.Accessories:
                    int stock;
                    decimal accessoryRate;
                    if (!int.TryParse(Value("stock"), out stock) || stock < 0 ||
                        !TryDecimal(Value("hourlyRate"), out accessoryRate) || accessoryRate < 0)
                    {
                        _message.Text = "Informe estoque não negativo e uma tarifa válida.";
                        return;
                    }

                    await _catalog.SaveAccessoryAsync(new Accessory(_selectedId, name, Optional("description"), stock, accessoryRate));
                    break;
            }

            ClearInputs();
            await RefreshRowsAsync("Cadastro salvo.");
        }
        catch (Exception ex)
        {
            _message.Text = "Não foi possível salvar: " + ex.Message;
        }
        finally { _saving = false; }
    }

    private async void DeactivateAsync(object sender, EventArgs e)
    {
        if (_refreshing || _saving) return;
        if (_selectedId == 0)
        {
            _message.Text = "Selecione um registro para inativar.";
            return;
        }

        var answer = MessageBox.Show(
            "O registro deixará de aparecer em novos cadastros, mas seu histórico será preservado. Continuar?",
            "Inativar cadastro",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (answer != DialogResult.Yes)
        {
            return;
        }

        _saving = true;
        try
        {
            switch (_kind)
            {
                case CatalogKind.Clients: await _catalog.DeactivateClientAsync(_selectedId); break;
                case CatalogKind.Spaces: await _catalog.DeactivateSpaceAsync(_selectedId); break;
                case CatalogKind.RpgSystems: await _catalog.DeactivateRpgSystemAsync(_selectedId); break;
                case CatalogKind.Accessories: await _catalog.DeactivateAccessoryAsync(_selectedId); break;
            }

            ClearInputs();
            await RefreshRowsAsync("Cadastro inativado.");
        }
        catch (Exception)
        {
            _message.Text = "Não foi possível inativar este registro.";
        }
        finally { _saving = false; }
    }

    private string Value(string key)
    {
        TextBox input;
        return _inputs.TryGetValue(key, out input) ? input.Text : string.Empty;
    }

    private string Optional(string key)
    {
        var value = Value(key).Trim();
        return value.Length == 0 ? null : value;
    }

    private static bool TryDecimal(string value, out decimal result) =>
        decimal.TryParse(value, NumberStyles.Number & ~NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out result) ||
        decimal.TryParse(value, NumberStyles.Number & ~NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);

    private void StartNew(object sender, EventArgs e)
    {
        if (_refreshing || _saving) return;
        ClearInputs();
        _message.Text = "Preencha os campos e clique em Salvar novo.";
        _inputs["name"].Focus();
    }

    private void ClearInputs()
    {
        _selectedId = 0;
        _grid.CurrentCell = null;
        _grid.ClearSelection();
        foreach (var input in _inputs.Values)
        {
            input.Clear();
        }
        UpdateMode();
    }

    private void UpdateMode()
    {
        bool isNew = _selectedId == 0;
        _mode.Text = isNew ? "Novo cadastro" : "Editando registro #" + _selectedId;
        _saveButton.Text = isNew ? "Salvar novo" : "Salvar edição";
        _deactivateButton.Enabled = !isNew;
    }
}

internal sealed class CatalogField
{
    public CatalogField(string key, string label) { Key = key; Label = label; }
    public string Key { get; private set; }
    public string Label { get; private set; }
}
}
