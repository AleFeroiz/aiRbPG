using aiRbPG.Domain.Users;
using aiRbPG.Domain.Catalog;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace aiRbPG.WinForms.Forms
{
public sealed class DashboardForm : Form
{
    private readonly ICatalogRepository _catalog;
    private readonly aiRbPG.Domain.Reservations.IReservationRepository _reservations;

    public DashboardForm(UserAccount user, ICatalogRepository catalog, aiRbPG.Domain.Reservations.IReservationRepository reservations)
    {
        _catalog = catalog;
        _reservations = reservations;
        Text = "aiRbPG — Administração";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1024, 680);
        BackColor = Color.FromArgb(242, 240, 235);
        Font = new Font("Segoe UI", 10F);

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = BackColor
        };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var sidebar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(20),
            BackColor = Color.FromArgb(28, 33, 43)
        };

        var brand = new Label
        {
            Text = "aiRbPG",
            AutoSize = false,
            Width = 205,
            Height = 62,
            Font = new Font("Segoe UI Semibold", 23F, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 183, 92),
            TextAlign = ContentAlignment.MiddleLeft
        };
        sidebar.Controls.Add(brand);

        AddNavigationButton(sidebar, "Reservas", () => OpenReservations(user));
        AddNavigationButton(sidebar, "Clientes", () => OpenCatalog(CatalogKind.Clients));

        if (user.Role == UserRole.Administrator)
        {
            AddNavigationButton(sidebar, "Espaços", () => OpenCatalog(CatalogKind.Spaces));
            AddNavigationButton(sidebar, "Acessórios", () => OpenCatalog(CatalogKind.Accessories));
            AddNavigationButton(sidebar, "Sistemas de RPG", () => OpenCatalog(CatalogKind.RpgSystems));
            AddNavigationButton(sidebar, "Tarifas e relatórios", () => ShowPlaceholder("Relatórios e administração de usuários serão adicionados em seguida."));
        }

        var main = new Panel { Dock = DockStyle.Fill, Padding = new Padding(44, 36, 44, 36) };
        var welcome = new Label
        {
            Text = $"Bem-vindo(a), {user.FullName}",
            Dock = DockStyle.Top,
            Height = 58,
            Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold),
            ForeColor = Color.FromArgb(36, 41, 51)
        };
        var role = new Label
        {
            Text = user.Role == UserRole.Administrator ? "Administrador" : "Atendente",
            Dock = DockStyle.Top,
            Height = 32,
            ForeColor = Color.FromArgb(117, 90, 53)
        };
        var intro = new Label
        {
            Text = "Base inicial pronta. Use o menu para acessar os módulos do aiRbPG.",
            Dock = DockStyle.Top,
            Height = 50,
            ForeColor = Color.FromArgb(74, 78, 86)
        };
        main.Controls.Add(intro);
        main.Controls.Add(role);
        main.Controls.Add(welcome);

        shell.Controls.Add(sidebar, 0, 0);
        shell.Controls.Add(main, 1, 0);
        Controls.Add(shell);
    }

    private void OpenCatalog(CatalogKind kind)
    {
        using (var form = new CatalogForm(_catalog, kind))
        {
            form.ShowDialog(this);
        }
    }

    private void OpenReservations(UserAccount user)
    {
        using (var form = new ReservationForm(user, _catalog, _reservations))
        {
            form.ShowDialog(this);
        }
    }

    private void ShowPlaceholder(string message) =>
        MessageBox.Show(message, "aiRbPG", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private static void AddNavigationButton(FlowLayoutPanel sidebar, string title, Action onClick)
    {
        var button = new Button
        {
            Text = title,
            Width = 205,
            Height = 44,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(38, 43, 56),
            ForeColor = Color.WhiteSmoke,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 6, 0, 4)
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (sender, args) => onClick();
        sidebar.Controls.Add(button);
    }
}
}
