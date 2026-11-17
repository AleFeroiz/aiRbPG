using System.Threading.Tasks;
using aiRbPG.Domain.Users;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace aiRbPG.WinForms.Forms
{
public sealed class InitialAdministratorForm : Form
{
    private readonly AuthenticationService _authentication;
    private readonly TextBox _name = new TextBox();
    private readonly TextBox _login = new TextBox();
    private readonly TextBox _password = new TextBox();
    private readonly TextBox _confirmPassword = new TextBox();
    private readonly Label _message = new Label();

    public InitialAdministratorForm(AuthenticationService authentication)
    {
        _authentication = authentication;
        Text = "aiRbPG — Primeiro acesso";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(500, 520);
        MinimumSize = Size;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(24, 28, 38);
        ForeColor = Color.WhiteSmoke;
        Font = new Font("Segoe UI", 10F);

        BuildLayout();
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(32),
            BackColor = BackColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        for (var index = 0; index < 5; index++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label
        {
            Text = "Criar Administrador",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 19F, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 183, 92),
            TextAlign = ContentAlignment.MiddleLeft
        };

        ConfigureInput(_name, "Nome completo");
        ConfigureInput(_login, "Usuário de acesso");
        ConfigureInput(_password, "Senha (mínimo 10 caracteres)", password: true);
        ConfigureInput(_confirmPassword, "Confirme a senha", password: true);

        var create = new Button
        {
            Text = "Criar conta de Administrador",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(183, 132, 55),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
        };
        create.FlatAppearance.BorderSize = 0;
        create.Click += CreateAdministratorAsync;

        _message.Dock = DockStyle.Fill;
        _message.ForeColor = Color.FromArgb(239, 190, 118);

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(_name, 0, 1);
        layout.Controls.Add(_login, 0, 2);
        layout.Controls.Add(_password, 0, 3);
        layout.Controls.Add(_confirmPassword, 0, 4);
        layout.Controls.Add(create, 0, 5);
        layout.Controls.Add(_message, 0, 6);
        Controls.Add(layout);
        AcceptButton = create;
    }

    private static void ConfigureInput(TextBox input, string placeholder, bool password = false)
    {
        input.Dock = DockStyle.Fill;
        input.AccessibleDescription = placeholder;
        input.UseSystemPasswordChar = false;
        input.BackColor = Color.FromArgb(38, 43, 56);
        input.ForeColor = Color.Gray;
        input.BorderStyle = BorderStyle.FixedSingle;
        input.Font = new Font("Segoe UI", 10F);
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

    private async void CreateAdministratorAsync(object sender, EventArgs e)
    {
        if (_password.Text != _confirmPassword.Text)
        {
            _message.Text = "As senhas não coincidem.";
            return;
        }

        try
        {
            await _authentication.CreateInitialAdministratorAsync(_name.Text, _login.Text, _password.Text);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            _message.Text = exception.Message;
        }
    }
}
}
