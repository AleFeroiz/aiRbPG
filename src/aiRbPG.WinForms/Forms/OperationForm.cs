using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using aiRbPG.Data.Reservations;
using aiRbPG.Domain.Users;
using aiRbPG.Domain;

namespace aiRbPG.WinForms.Forms
{
    internal static class Ui
    {
        internal static Button Action(string text, Func<Task> action)
        {
            var button = new Button { Text = text, AutoSize = true, Height = 38, Margin = new Padding(4), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(225,222,215) };
            button.Click += async (s,e) => { button.Enabled = false; try { await action(); } catch (Exception ex) { MessageBox.Show(ex.Message, "aiRbPG", MessageBoxButtons.OK, MessageBoxIcon.Information); } finally { button.Enabled = true; } };
            return button;
        }
        internal static bool ShowResult<T>(OperationResult<T> result)
        {
            if (result.Succeeded) return true;
            MessageBox.Show(result.Error, "aiRbPG", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        internal static DataGridView Grid()
        { return new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, RowHeadersVisible = false }; }
    }

    internal sealed class FieldsDialog : Form
    {
        private readonly TableLayoutPanel fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(16) };
        private readonly Dictionary<string,Control> inputs = new Dictionary<string,Control>();
        internal FieldsDialog(string title)
        {
            Text = title; Size = new Size(540,420); StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI",10); BackColor = Color.FromArgb(242,240,235); AutoScroll = true;
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,180)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            var ok = new Button { Text = "Confirmar", Dock = DockStyle.Bottom, Height = 42, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Voltar", Dock = DockStyle.Bottom, Height = 38, DialogResult = DialogResult.Cancel };
            Controls.Add(fields); Controls.Add(cancel); Controls.Add(ok); AcceptButton = ok; CancelButton = cancel;
        }
        private void Add(string key, string label, Control input)
        {
            int row = fields.RowCount++; fields.RowStyles.Add(new RowStyle(SizeType.Absolute,62));
            fields.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft },0,row);
            input.Dock = DockStyle.Fill; input.Margin = new Padding(4,16,4,12); fields.Controls.Add(input,1,row); inputs.Add(key,input);
        }
        internal void TextField(string key, string label, bool secret = false) { Add(key,label,new TextBox { UseSystemPasswordChar = secret }); }
        internal void Number(string key, string label) { Add(key,label,new NumericUpDown { DecimalPlaces = 2, Maximum = 99999999.99m }); }
        internal void Choice(string key, string label, string[] choices)
        { var input = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }; input.Items.AddRange(choices); input.SelectedIndex = 0; Add(key,label,input); }
        internal string Value(string key) { return inputs[key].Text; }
        internal decimal Amount(string key) { return ((NumericUpDown)inputs[key]).Value; }
    }

    public sealed class OperationForm : Form
    {
        private readonly int id;
        private readonly SqlOperationsRepository operations;
        private readonly Label summary = new Label { Dock = DockStyle.Top, Height = 62, Padding = new Padding(12), Font = new Font("Segoe UI",12,FontStyle.Bold) };
        private readonly DataGridView items = Ui.Grid(), payments = Ui.Grid(), history = Ui.Grid();
        public OperationForm(int id, SqlOperationsRepository operations, UserAccount user)
        {
            this.id = id; this.operations = operations; Text = "aiRbPG - Operar reserva #" + id;
            Size = new Size(1160,720); MinimumSize = new Size(1000,640); StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI",10); BackColor = Color.FromArgb(242,240,235);
            var tabs = new TabControl { Dock = DockStyle.Fill };
            foreach (var pair in new[] { Tuple.Create("Itens e devolução",items), Tuple.Create("Pagamentos",payments), Tuple.Create("Histórico",history) })
            { var tab = new TabPage(pair.Item1); tab.Controls.Add(pair.Item2); tabs.TabPages.Add(tab); }
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 108, AutoScroll = true };
            buttons.Controls.Add(Ui.Action("Confirmar reserva", async () => { if (Ui.ShowResult(await operations.ConfirmAsync(id))) await RefreshAsync(); }));
            buttons.Controls.Add(Ui.Action("Registrar pagamento", PayAsync));
            buttons.Controls.Add(Ui.Action("Retirada integral", async () => { if (Ui.ShowResult(await operations.PickupAsync(id))) await RefreshAsync(); }));
            buttons.Controls.Add(Ui.Action("Registrar devolução", ReturnAsync));
            buttons.Controls.Add(Ui.Action("Finalizar / quitar", async () => { if (Ui.ShowResult(await operations.FinishAsync(id))) await RefreshAsync(); }));
            buttons.Controls.Add(Ui.Action("Cancelar reserva", async () =>
            {
                using (var dialog = new FieldsDialog("Motivo do cancelamento"))
                { dialog.TextField("reason","Motivo"); if (dialog.ShowDialog(this) != DialogResult.OK) return; if (Ui.ShowResult(await operations.CancelAsync(id,dialog.Value("reason")))) await RefreshAsync(); }
            }));
            if (user.Role == UserRole.Administrator) buttons.Controls.Add(Ui.Action("Definir desconto", async () =>
            {
                using (var dialog = new FieldsDialog("Desconto total da reserva"))
                { dialog.Number("discount","Desconto em R$"); dialog.TextField("reason","Justificativa"); if (dialog.ShowDialog(this) != DialogResult.OK) return; if (Ui.ShowResult(await operations.DiscountAsync(id,dialog.Amount("discount"),dialog.Value("reason")))) await RefreshAsync(); }
            }));
            buttons.Controls.Add(Ui.Action("Atualizar",RefreshAsync));
            var hint = new Label { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(12,0,8,0), Text = "Devolução: preencha as colunas Boas, Danos e Perdas com as quantidades desta operação. Finalização exige saldo zero e todos os itens resolvidos." };
            Controls.Add(tabs); Controls.Add(hint); Controls.Add(buttons); Controls.Add(summary);
            items.DataError += (s,e) => { e.ThrowException = false; MessageBox.Show("Informe quantidades inteiras."); };
            Load += async (s,e) => { try { await RefreshAsync(); } catch (Exception ex) { summary.Text = ex.Message; } };
        }
        private async Task RefreshAsync()
        {
            var detail = await operations.DetailsAsync(id);
            if (!Ui.ShowResult(detail)) return;
            if (detail.Value.Rows.Count == 0) { summary.Text = "Reserva não encontrada."; return; }
            var row = detail.Value.Rows[0]; var total = Convert.ToDecimal(row["ValorTotal"]); var paid = Convert.ToDecimal(row["Pago"]);
            summary.Text = string.Format("Reserva #{0}   |   {1}   |   Total R$ {2:F2}   |   Pago R$ {3:F2}   |   Saldo R$ {4:F2}",id,row["Status"],total,paid,total-paid);
            var itemResult = await operations.ItemsAsync(id);
            if (!Ui.ShowResult(itemResult)) return;
            var table = itemResult.Value;
            foreach (var name in new[] { "Boas","Danos","Perdas" }) { var col = table.Columns.Add(name,typeof(int)); col.DefaultValue = 0; foreach (DataRow item in table.Rows) item[name] = 0; }
            items.DataSource = table; items.ReadOnly = false;
            foreach (DataGridViewColumn col in items.Columns) col.ReadOnly = !new[] { "Boas","Danos","Perdas" }.Contains(col.Name);
            foreach (var name in new[] { "ItemReservaId","AcessorioId","TarifaHoraAplicada" }) items.Columns[name].Visible = false;
            var paymentResult = await operations.PaymentsAsync(id);
            if (Ui.ShowResult(paymentResult)) payments.DataSource = paymentResult.Value;
            var historyResult = await operations.HistoryAsync(id);
            if (Ui.ShowResult(historyResult)) history.DataSource = historyResult.Value;
        }
        private async Task PayAsync()
        {
            using (var dialog = new FieldsDialog("Pagamento manual - sem integração bancária"))
            {
                dialog.Number("amount","Valor em R$"); dialog.Choice("method","Método",new[] { "Dinheiro","Pix","Cartão","Transferência" }); dialog.TextField("note","Observação");
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                if (Ui.ShowResult(await operations.PayAsync(id,dialog.Amount("amount"),dialog.Value("method"),dialog.Value("note")))) await RefreshAsync();
            }
        }
        private async Task ReturnAsync()
        {
            if (!items.EndEdit()) { summary.Text = "Confira as quantidades."; return; }
            var table = items.DataSource as DataTable;
            if (table == null) { summary.Text = "Atualize a reserva antes de registrar a devolução."; return; }
            var lines = new List<ReturnLine>();
            foreach (DataRow row in table.Rows)
                lines.Add(new ReturnLine { ItemId = Convert.ToInt32(row["ItemReservaId"]), Good = Convert.ToInt32(row["Boas"]), Damaged = Convert.ToInt32(row["Danos"]), Lost = Convert.ToInt32(row["Perdas"]) });
            using (var dialog = new FieldsDialog("Registrar devolução e eventual multa"))
            {
                dialog.Number("fine","Multa em R$ (opcional)"); dialog.TextField("reason","Justificativa / observação");
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                if (Ui.ShowResult(await operations.ReturnAsync(id,lines,dialog.Amount("fine"),dialog.Value("reason")))) await RefreshAsync();
            }
        }
    }
}
