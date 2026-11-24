using System.Threading.Tasks;
using aiRbPG.Domain.Users;
using System;
using System.Drawing;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace aiRbPG.WinForms.Forms
{
public sealed class LoginForm : Form
{
    private readonly AuthenticationService _authentication;
    private readonly TextBox _loginInput = new TextBox();
    private readonly TextBox _passwordInput = new TextBox();
    private readonly Label _message = new Label();
    private readonly Button _signInButton = new Button();

    public LoginForm(AuthenticationService authentication)
    {
        _authentication = authentication;
        Text = "aiRbPG — Acesso";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(520, 560);
        Size = new Size(520, 560);
        BackColor = Color.FromArgb(24, 28, 38);
        ForeColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        BuildLayout();
        AcceptButton = _signInButton;
        Shown += CheckInitialSetupAsync;
    }

    public UserAccount AuthenticatedUser { get; private set; }

    private void BuildLayout()
    {
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(48, 34, 48, 24),
            BackColor = BackColor
        };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label
        {
            Text = "aiRbPG",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 28F, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 183, 92),
            TextAlign = ContentAlignment.MiddleCenter
        };
        var subtitle = new Label
        {
            Text = "Um bom espaço para grandes aventuras.",
            Dock = DockStyle.Fill,
            ForeColor = Color.Gainsboro,
            TextAlign = ContentAlignment.MiddleCenter
        };

        ConfigureInput(_loginInput, "Usuário");
        ConfigureInput(_passwordInput, "Senha", password: true);

        _signInButton.Text = "Entrar";
        _signInButton.Dock = DockStyle.Fill;
        _signInButton.FlatStyle = FlatStyle.Flat;
        _signInButton.FlatAppearance.BorderSize = 0;
        _signInButton.BackColor = Color.FromArgb(183, 132, 55);
        _signInButton.ForeColor = Color.White;
        _signInButton.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _signInButton.Cursor = Cursors.Hand;
        _signInButton.Click += SignInAsync;

        _message.Dock = DockStyle.Fill;
        _message.TextAlign = ContentAlignment.MiddleCenter;
        _message.ForeColor = Color.FromArgb(239, 190, 118);
        _message.AutoEllipsis = true;

        content.Controls.Add(new Label(), 0, 0);
        content.Controls.Add(title, 0, 1);
        content.Controls.Add(subtitle, 0, 2);
        content.Controls.Add(_loginInput, 0, 3);
        content.Controls.Add(_passwordInput, 0, 4);
        content.Controls.Add(new Label(), 0, 5);
        content.Controls.Add(_signInButton, 0, 6);
        content.Controls.Add(_message, 0, 7);
        Controls.Add(content);
    }

    private static void ConfigureInput(TextBox input, string placeholder, bool password = false)
    {
        input.Dock = DockStyle.Fill;
        input.AccessibleDescription = placeholder;
        input.Margin = new Padding(0, 6, 0, 6);
        input.BackColor = Color.FromArgb(38, 43, 56);
        input.ForeColor = Color.Gray;
        input.BorderStyle = BorderStyle.FixedSingle;
        input.Font = new Font("Segoe UI", 11F);
        input.Tag = placeholder;
        input.Text = placeholder;
        input.Enter += delegate
        {
            if (input.Text == placeholder)
            {
                input.Clear();
                input.ForeColor = Color.WhiteSmoke;
                input.UseSystemPasswordChar = password;
            }
        };
        input.Leave += delegate
        {
            if (input.Text.Length == 0)
            {
                input.UseSystemPasswordChar = false;
                input.Text = placeholder;
                input.ForeColor = Color.Gray;
            }
        };
    }

    private async void CheckInitialSetupAsync(object sender, EventArgs e)
    {
        try
        {
            if (await _authentication.NeedsInitialAdministratorAsync())
            {
                using (var setup = new InitialAdministratorForm(_authentication))
                {
                    if (setup.ShowDialog(this) != DialogResult.OK)
                    {
                        Close();
                        return;
                    }
                }

                _message.Text = "Administrador criado. Entre com o usuário cadastrado.";
            }
        }
        catch (Exception error)
        {
            ShowAccessError(error);
        }
    }

    private void ShowAccessError(Exception error)
    {
        string detail;
        var sql = error as SqlException;
        if (sql != null)
        {
            switch (sql.Number)
            {
                case 18456:
                    detail = "O SQL Server recusou a autenticação. Confira o acesso do login SQL ou da conta Windows.";
                    break;
                case 4060:
                    detail = "O banco aiRbPG está indisponível para a conta configurada. Verifique o banco e as permissões.";
                    break;
                case 207:
                case 208:
                case 2812:
                    detail = "O esquema do banco está incompleto ou incompatível. Execute INICIAR.cmd para verificar as migrações.";
                    break;
                case 229:
                case 916:
                    detail = "A conta configurada precisa de permissões no banco aiRbPG.";
                    break;
                default:
                    detail = "Confira se a instância SQL está acessível e se a conta tem acesso ao banco.";
                    break;
            }

            detail += string.Format("\r\n\r\nSQL {0}, estado {1}.", sql.Number, sql.State);
        }
        else if (error is ArgumentException || error is InvalidOperationException)
        {
            detail = "Confira AIRBPG_SQL_CONNECTION ou execute INICIAR.cmd para preparar a conexão.";
        }
        else
        {
            detail = "Falha inesperada durante o acesso (" + error.GetType().Name + ").";
        }

        _message.Text = "Falha de acesso. Verifique a configuração e tente novamente.";
        MessageBox.Show(this, detail, "aiRbPG — Diagnóstico de acesso", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }


    private async void SignInAsync(object sender, EventArgs e)
    {
        _signInButton.Enabled = false;
        try
        {
            var result = await _authentication.AuthenticateAsync(_loginInput.Text, _passwordInput.Text);
            if (!result.Succeeded || result.User == null)
            {
                _message.Text = result.Message;
                _passwordInput.Clear();
                _passwordInput.Focus();
                return;
            }

            AuthenticatedUser = result.User;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception error)
        {
            ShowAccessError(error);
        }
        finally
        {
            _signInButton.Enabled = true;
        }
    }
}
}
