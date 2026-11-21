namespace VT3CyberZone.Desktop.Forms;

using System.Drawing;

/// <summary>
/// Form Dịch vụ tại quầy (F&B và Hàng hóa) chuẩn phong cách Cyber Game
/// </summary>
public class FoodOrderForm : Form
{
    private readonly string _defaultMachine;
    private ComboBox cboMachine = null!;
    private ComboBox cboCategory = null!;
    private ListView lvProducts = null!;
    private DataGridView dgvCart = null!;
    private Label lblTotal = null!;
    private TextBox txtNote = null!;
    private Button btnConfirm = null!;
    private Button btnCancel = null!;

    // Danh sách món ăn mẫu (dữ liệu từ vt3_cyberzone_db.sql)
    private readonly (string Cat, string Name, decimal Price)[] _allProducts =
    [
        ("Món ăn nhanh", "Mì tôm trứng bò khô", 25000m),
        ("Món ăn nhanh", "Mì tôm xúc xích rau cải", 22000m),
        ("Món ăn nhanh", "Mì xào bò rau cải", 35000m),
        ("Món ăn nhanh", "Cơm rang dưa bò", 35000m),
        ("Món ăn nhanh", "Cơm sườn nướng", 40000m),
        ("Món ăn nhanh", "Bánh mì pate trứng", 20000m),
        ("Món ăn nhanh", "Bánh mì xúc xích", 20000m),
        ("Món ăn nhanh", "Khoai tây chiên giòn", 25000m),
        ("Nước giải khát", "Sting dâu tây (lon)", 12000m),
        ("Nước giải khát", "Bò húc Thái Lan (RedBull)", 15000m),
        ("Nước giải khát", "Trà xanh C2 / Không độ", 10000m),
        ("Nước giải khát", "Nước ngọt Coca-Cola / Pepsi", 12000m),
        ("Nước giải khát", "Nước khoáng Aquafina 500ml", 8000m),
        ("Nước giải khát", "Revive bù khoáng", 12000m),
        ("Cà phê & Trà", "Trà đào cam sả (ly lớn)", 20000m),
        ("Cà phê & Trà", "Trà vải thạch dừa", 20000m),
        ("Cà phê & Trà", "Cà phê sữa đá pha phin", 15000m),
        ("Cà phê & Trà", "Cà phê đen đá đậm vị", 14000m),
        ("Thuốc lá & Khác", "Thuốc lá Thăng Long mềm", 15000m),
        ("Thuốc lá & Khác", "Thuốc lá White Horse", 22000m),
        ("Thuốc lá & Khác", "Bật lửa / Quẹt ga", 5000m)
    ];

    public FoodOrderForm(string defaultMachine = "STADIUM-1")
    {
        _defaultMachine = defaultMachine;
        InitializeComponent();
        LoadCategories();
        FilterProducts("Tất cả");
    }

    private void InitializeComponent()
    {
        Text = "V-T3 CyberZone — Dịch vụ gọi món tại quầy (F&B)";
        Size = new Size(820, 560);
        MinimumSize = Size;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(246, 248, 252);
        Font = new Font("Segoe UI", 9f);

        // Header Panel
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52,
            BackColor = Color.FromArgb(0, 120, 215),
            Padding = new Padding(16, 12, 16, 12)
        };
        var lblTitle = new Label
        {
            Text = "🛎️  DỊCH VỤ TẠI QUẦY & ĐẶT MÓN BẾP",
            Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Left,
            AutoSize = true
        };
        pnlHeader.Controls.Add(lblTitle);

        // Top Filter Bar
        var pnlFilter = new Panel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = Color.White,
            Padding = new Padding(12, 8, 12, 8)
        };

        var lblMachine = new Label { Text = "Máy phục vụ:", AutoSize = true, Location = new Point(14, 13), Font = new Font("Segoe UI Semibold", 9f) };
        cboMachine = new ComboBox { Location = new Point(102, 10), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
        cboMachine.Items.AddRange(["STADIUM-1", "STADIUM-4", "STADIUM-5", "STADIUM-6", "STADIUM-8", "SVIP-01", "SVIP-03", "SVIP-05", "SVIP-06", "SVIP-07", "SVIP-08", "SVIP-12", "SVIP-13", "SVIP-14", "SVIP-16", "SVIP-21", "SVIP-24", "SVIP-25", "SVIP-26", "SVIP-28", "SVIP-29", "SVIP-30", "KHÁCH TẠI QUẦY"]);
        cboMachine.SelectedItem = cboMachine.Items.Contains(_defaultMachine) ? _defaultMachine : "STADIUM-1";

        var lblCat = new Label { Text = "Danh mục món:", AutoSize = true, Location = new Point(260, 13), Font = new Font("Segoe UI Semibold", 9f) };
        cboCategory = new ComboBox { Location = new Point(360, 10), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
        cboCategory.SelectedIndexChanged += (s, e) => FilterProducts(cboCategory.SelectedItem?.ToString() ?? "Tất cả");

        pnlFilter.Controls.AddRange([lblMachine, cboMachine, lblCat, cboCategory]);

        // Main Layout (Split: Left Product List, Right Order Cart)
        var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        // Left: Products ListView
        lvProducts = new ListView
        {
            Dock = DockStyle.Left,
            Width = 430,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = new Font("Segoe UI", 9f)
        };
        lvProducts.Columns.Add("Tên món / Sản phẩm", 230);
        lvProducts.Columns.Add("Đơn giá", 90, HorizontalAlignment.Right);
        lvProducts.Columns.Add("Danh mục", 100);
        lvProducts.DoubleClick += (s, e) => AddSelectedToCart();

        // Right: Cart Panel
        var pnlCart = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        var lblCartTitle = new Label
        {
            Text = "🛒 Giỏ hàng đặt món",
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 24
        };

        dgvCart = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 8.75f)
        };
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên món", Width = 150, ReadOnly = true });
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL", Width = 45 });
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đơn giá", Width = 70, ReadOnly = true });
        dgvCart.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thành tiền", Width = 80, ReadOnly = true });
        dgvCart.CellValueChanged += (s, e) => RecalculateCart();

        // Bottom summary panel
        var pnlSummary = new Panel { Dock = DockStyle.Bottom, Height = 130, Padding = new Padding(4) };

        lblTotal = new Label
        {
            Text = "Tổng tiền: 0 đ",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 0, 0),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleRight
        };

        var lblNoteTitle = new Label { Text = "Ghi chú bếp (VD: không cay, nhiều đá):", AutoSize = true, Location = new Point(4, 34) };
        txtNote = new TextBox { Location = new Point(4, 54), Width = 340, Height = 24 };

        btnConfirm = new Button
        {
            Text = "✔  XÁC NHẬN GỌI MÓN",
            BackColor = Color.FromArgb(0, 150, 70),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            Location = new Point(4, 88),
            Width = 190,
            Height = 36,
            Cursor = Cursors.Hand
        };
        btnConfirm.FlatAppearance.BorderSize = 0;
        btnConfirm.Click += BtnConfirm_Click;

        btnCancel = new Button
        {
            Text = "Đóng",
            BackColor = Color.FromArgb(230, 235, 242),
            ForeColor = Color.FromArgb(40, 50, 60),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f),
            Location = new Point(204, 88),
            Width = 80,
            Height = 36,
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => Close();

        pnlSummary.Controls.AddRange([lblTotal, lblNoteTitle, txtNote, btnConfirm, btnCancel]);

        pnlCart.Controls.Add(dgvCart);
        pnlCart.Controls.Add(pnlSummary);
        pnlCart.Controls.Add(lblCartTitle);

        pnlBody.Controls.Add(pnlCart);
        pnlBody.Controls.Add(lvProducts);

        Controls.AddRange([pnlBody, pnlFilter, pnlHeader]);
    }

    private void LoadCategories()
    {
        cboCategory.Items.Add("Tất cả");
        var cats = _allProducts.Select(p => p.Cat).Distinct().ToArray();
        cboCategory.Items.AddRange(cats);
        cboCategory.SelectedIndex = 0;
    }

    private void FilterProducts(string cat)
    {
        lvProducts.Items.Clear();
        var prods = (cat == "Tất cả") ? _allProducts : _allProducts.Where(p => p.Cat == cat).ToArray();
        foreach (var p in prods)
        {
            var item = new ListViewItem(p.Name);
            item.SubItems.Add(p.Price.ToString("#,##0") + " đ");
            item.SubItems.Add(p.Cat);
            item.Tag = p;
            lvProducts.Items.Add(item);
        }
    }

    private void AddSelectedToCart()
    {
        if (lvProducts.SelectedItems.Count == 0) return;
        var tag = ((string Cat, string Name, decimal Price))lvProducts.SelectedItems[0].Tag!;

        // Kiểm tra xem đã có trong giỏ chưa
        foreach (DataGridViewRow row in dgvCart.Rows)
        {
            if (row.Cells[0].Value?.ToString() == tag.Name)
            {
                int qty = Convert.ToInt32(row.Cells[1].Value) + 1;
                row.Cells[1].Value = qty;
                row.Cells[3].Value = (qty * tag.Price).ToString("#,##0");
                RecalculateCart();
                return;
            }
        }

        dgvCart.Rows.Add(tag.Name, 1, tag.Price.ToString("#,##0"), tag.Price.ToString("#,##0"));
        RecalculateCart();
    }

    private void RecalculateCart()
    {
        decimal sum = 0;
        foreach (DataGridViewRow row in dgvCart.Rows)
        {
            var name = row.Cells[0].Value?.ToString();
            var p = _allProducts.FirstOrDefault(x => x.Name == name);
            if (p != default && int.TryParse(row.Cells[1].Value?.ToString(), out int qty) && qty > 0)
            {
                decimal rowTotal = qty * p.Price;
                row.Cells[3].Value = rowTotal.ToString("#,##0");
                sum += rowTotal;
            }
        }
        lblTotal.Text = $"Tổng tiền: {sum:#,##0} đ";
    }

    private void BtnConfirm_Click(object? sender, EventArgs e)
    {
        if (dgvCart.Rows.Count == 0)
        {
            MessageBox.Show("Vui lòng nhấp đúp để chọn ít nhất 1 món vào giỏ hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string machine = cboMachine.SelectedItem?.ToString() ?? "STADIUM-1";
        string note = string.IsNullOrWhiteSpace(txtNote.Text) ? "Không có" : txtNote.Text.Trim();

        MessageBox.Show(
            $"Đã tiếp nhận yêu cầu gọi món cho [{machine}]!\n\n" +
            $"- Số món: {dgvCart.Rows.Count}\n" +
            $"- {lblTotal.Text}\n" +
            $"- Ghi chú bếp: {note}\n\n" +
            "Đơn hàng đã được đẩy về màn hình Bếp & Bar để chế biến.",
            "Đặt món thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

        DialogResult = DialogResult.OK;
        Close();
    }
}
