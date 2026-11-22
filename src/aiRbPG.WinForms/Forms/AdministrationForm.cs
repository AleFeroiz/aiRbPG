using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using aiRbPG.Data.Reservations;

namespace aiRbPG.WinForms.Forms
{
    public sealed class AdministrationForm : Form
    {
        private readonly SqlOperationsRepository operations;
        private readonly DataGridView users = Ui.Grid(), report = Ui.Grid();
        private readonly DateTimePicker start = new DateTimePicker { Value = DateTime.Today.AddDays(-30), Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker end = new DateTimePicker { Value = DateTime.Today, Format = DateTimePickerFormat.Short };
        private readonly Label message = new Label { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(12) };
        public AdministrationForm(SqlOperationsRepository operations)
        {
            this.operations = operations; Text = "aiRbPG - Usuários e relatórios"; Size = new Size(1080,700);
            MinimumSize = new Size(920,600); StartPosition = FormStartPosition.CenterParent; Font = new Font("Segoe UI",10);
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var userTab = new TabPage("Usuários"); var reportTab = new TabPage("Relatórios");
            var userButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60 };
            userButtons.Controls.Add(Ui.Action("Cadastrar usuário",AddAsync));
            userButtons.Controls.Add(Ui.Action("Ativar / inativar selecionado",async () =>
            {
                if (users.CurrentRow == null) throw new InvalidOperationException("Selecione um usuário.");
                int id = Convert.ToInt32(users.CurrentRow.Cells["UsuarioId"].Value);
                bool active = Convert.ToBoolean(users.CurrentRow.Cells["Ativo"].Value);
                await operations.SetUserActiveAsync(id,!active); await LoadUsersAsync();
            }));
            userTab.Controls.Add(users); userTab.Controls.Add(userButtons);
            var filters = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(8), AutoScroll = true };
            filters.Controls.Add(new Label { Text = "De",AutoSize = true }); filters.Controls.Add(start);
            filters.Controls.Add(new Label { Text = "Até (inclusive)",AutoSize = true }); filters.Controls.Add(end);
            filters.Controls.Add(Ui.Action("Recebimentos",async () =>
            {
                var table = await operations.RevenueAsync(start.Value.Date,end.Value.Date.AddDays(1)); report.DataSource = table;
                decimal sum = 0; foreach (System.Data.DataRow row in table.Rows) sum += Convert.ToDecimal(row["Recebido"]);
                message.Text = "Total recebido no período: R$ " + sum.ToString("F2") + ". Pagamentos manuais; não é o valor de reservas em aberto.";
            }));
            filters.Controls.Add(Ui.Action("Acessórios mais reservados",async () => { report.DataSource = await operations.MostRentedAsync(start.Value.Date,end.Value.Date.AddDays(1)); message.Text = "Por início da reserva no período; canceladas excluídas."; }));
            reportTab.Controls.Add(report); reportTab.Controls.Add(filters); tabs.TabPages.Add(userTab); tabs.TabPages.Add(reportTab);
            Controls.Add(tabs); Controls.Add(message);
            Load += async (s,e) => { try { await LoadUsersAsync(); } catch (Exception ex) { message.Text = ex.Message; } };
        }
        private async Task LoadUsersAsync() { users.DataSource = await operations.UsersAsync(); }
        private async Task AddAsync()
        {
            using (var dialog = new FieldsDialog("Cadastrar usuário do aplicativo"))
            {
                dialog.TextField("name","Nome completo"); dialog.TextField("login","Login"); dialog.TextField("password","Senha (mín. 10)",true);
                dialog.Choice("role","Perfil",new[] { "Atendente","Administrador" });
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                await operations.AddUserAsync(dialog.Value("name"),dialog.Value("login"),dialog.Value("password"),dialog.Value("role") == "Administrador");
                await LoadUsersAsync(); message.Text = "Usuário cadastrado.";
            }
        }
    }
}
