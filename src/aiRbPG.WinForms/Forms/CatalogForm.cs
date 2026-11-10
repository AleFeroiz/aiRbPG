using aiRbPG.Domain.Catalog;
using System.Globalization;

namespace aiRbPG.WinForms.Forms;

public sealed class CatalogForm : Form
{
    private readonly ICatalogRepository _catalog;
    private readonly CatalogKind _kind;
    private readonly DataGridView _grid = new();
    private readonly Dictionary<string, TextBox> _inputs = new();
    private readonly Label _message = new();
    private int _selectedId;

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

    private string Title => _kind switch
    {
        CatalogKind.Clients => "Clientes",
        CatalogKind.Spaces => "Espaços",
        CatalogKind.RpgSystems => "Sistemas de RPG",
        CatalogKind.Accessories => "Acessórios e estoque",
        _ => "Cadastros"
    };

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
        _grid.SelectionChanged += GridSelectionChanged;

        var editor = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = FieldCount() + 3,
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
        buttons.Controls.Add(CreateButton("Salvar", SaveAsync, primary: true));
        buttons.Controls.Add(CreateButton("Novo", (_, _) => ClearInputs()));
        buttons.Controls.Add(CreateButton("Inativar", DeactivateAsync));
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
    }

    private IEnumerable<(string Key, string Label)> Fields() => _kind switch
    {
        CatalogKind.Clients => new[] { ("name", "Nome"), ("phone", "Telefone"), ("email", "E-mail") },
        CatalogKind.Spaces => new[]
        {
            ("name", "Nome"), ("description", "Descrição"), ("address", "Endereço"),
            ("capacity", "Capacidade"), ("hourlyRate", "Tarifa do espaço por hora"),
            ("personRate", "Tarifa opcional por pessoa/hora")
        },
        CatalogKind.RpgSystems => new[] { ("name", "Nome"), ("description", "Descrição") },
        CatalogKind.Accessories => new[]
        {
            ("name", "Nome"), ("description", "Descrição"), ("stock", "Quantidade em estoque"),
            ("hourlyRate", "Tarifa por unidade/hora")
        },
        _ => Array.Empty<(string, string)>()
    };

    private int FieldCount() => Fields().Count();

    private static Button CreateButton(string text, EventHandler onClick, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            Width = primary ? 94 : 88,
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

    private async void LoadRowsAsync(object? sender, EventArgs e) => await RefreshRowsAsync();

    private async Task RefreshRowsAsync()
    {
        try
        {
            _grid.DataSource = _kind switch
            {
                CatalogKind.Clients => await _catalog.GetClientsAsync(),
                CatalogKind.Spaces => await _catalog.GetSpacesAsync(),
                CatalogKind.RpgSystems => await _catalog.GetRpgSystemsAsync(),
                CatalogKind.Accessories => await _catalog.GetAccessoriesAsync(),
                _ => Array.Empty<object>()
            };
            _message.Text = string.Empty;
        }
        catch (Exception)
        {
            _message.Text = "Não foi possível carregar os dados. Confira a conexão e o script do banco.";
        }
    }

    private void GridSelectionChanged(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is not object selected)
        {
            return;
        }

        switch (selected)
        {
            case Client client:
                _selectedId = client.Id;
                Set("name", client.Name); Set("phone", client.Phone); Set("email", client.Email);
                break;
            case RpgSpace space:
                _selectedId = space.Id;
                Set("name", space.Name); Set("description", space.Description); Set("address", space.Address);
                Set("capacity", space.Capacity); Set("hourlyRate", space.HourlyRate); Set("personRate", space.PerPersonHourlyRate);
                break;
            case RpgSystem system:
                _selectedId = system.Id;
                Set("name", system.Name); Set("description", system.Description);
                break;
            case Accessory accessory:
                _selectedId = accessory.Id;
                Set("name", accessory.Name); Set("description", accessory.Description);
                Set("stock", accessory.QuantityInStock); Set("hourlyRate", accessory.HourlyRate);
                break;
        }
    }

    private void Set(string key, object? value)
    {
        if (_inputs.TryGetValue(key, out var input))
        {
            input.Text = Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
        }
    }

    private async void SaveAsync(object? sender, EventArgs e)
    {
        var name = Value("name").Trim();
        if (name.Length == 0)
        {
            _message.Text = "O nome é obrigatório.";
            return;
        }

        try
        {
            switch (_kind)
            {
                case CatalogKind.Clients:
                    await _catalog.SaveClientAsync(new Client(_selectedId, name, Optional("phone"), Optional("email")));
                    break;
                case CatalogKind.Spaces:
                    if (!int.TryParse(Value("capacity"), out var capacity) || capacity <= 0 ||
                        !TryDecimal(Value("hourlyRate"), out var spaceRate) || spaceRate < 0 ||
                        !TryDecimal(Value("personRate"), out var personRate) || personRate < 0)
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
                    if (!int.TryParse(Value("stock"), out var stock) || stock < 0 ||
                        !TryDecimal(Value("hourlyRate"), out var accessoryRate) || accessoryRate < 0)
                    {
                        _message.Text = "Informe estoque não negativo e uma tarifa válida.";
                        return;
                    }

                    await _catalog.SaveAccessoryAsync(new Accessory(_selectedId, name, Optional("description"), stock, accessoryRate));
                    break;
            }

            _selectedId = 0;
            ClearInputs();
            await RefreshRowsAsync();
            _message.Text = "Cadastro salvo.";
        }
        catch (Exception)
        {
            _message.Text = "Não foi possível salvar. Confira os dados e a conexão com o banco.";
        }
    }

    private async void DeactivateAsync(object? sender, EventArgs e)
    {
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

        try
        {
            switch (_kind)
            {
                case CatalogKind.Clients: await _catalog.DeactivateClientAsync(_selectedId); break;
                case CatalogKind.Spaces: await _catalog.DeactivateSpaceAsync(_selectedId); break;
                case CatalogKind.RpgSystems: await _catalog.DeactivateRpgSystemAsync(_selectedId); break;
                case CatalogKind.Accessories: await _catalog.DeactivateAccessoryAsync(_selectedId); break;
            }

            _selectedId = 0;
            ClearInputs();
            await RefreshRowsAsync();
            _message.Text = "Cadastro inativado.";
        }
        catch (Exception)
        {
            _message.Text = "Não foi possível inativar este registro.";
        }
    }

    private string Value(string key) => _inputs.TryGetValue(key, out var input) ? input.Text : string.Empty;

    private string? Optional(string key)
    {
        var value = Value(key).Trim();
        return value.Length == 0 ? null : value;
    }

    private static bool TryDecimal(string value, out decimal result) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    private void ClearInputs()
    {
        _selectedId = 0;
        foreach (var input in _inputs.Values)
        {
            input.Clear();
        }
        _grid.ClearSelection();
    }
}
