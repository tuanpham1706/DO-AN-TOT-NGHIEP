namespace VT3CyberZone.Desktop.Forms;

using System.Drawing.Drawing2D;
using VT3CyberZone.Desktop.Api;

/// <summary>
/// Form nạp tiền vào tài khoản hội viên chuẩn phong cách CSM Billing
/// </summary>
public sealed class TopUpForm : Form
{
    // ── Controls ──────────────────────────────────────────────────────────────
    private TextBox txtUser = null!;
    private TextBox txtAmount = null!;
    private TextBox txtBalance = null!;
    private Button btnTopUp = null!;
    private CheckBox chkAllowChangeAmount = null!;

    // ── State ─────────────────────────────────────────────────────────────────
    private int? _foundMemberId;
    private decimal _currentBalance;

    public TopUpForm(string? initialUser = null, decimal? initialBalance = null)
    {
        InitializeComponent();

        if (!string.IsNullOrWhiteSpace(initialUser))
        {
            txtUser.Text = initialUser.Trim();
            if (initialBalance.HasValue)
            {
                _currentBalance = initialBalance.Value;
                txtBalance.Text = $"{_currentBalance:N0}";
            }
            _ = LookupMemberAsync(initialUser.Trim(), false);
        }
    }

    private void InitializeComponent()
    {
        Text = "【 Nạp tiền [Hội viên ONLINE] 】";
        ClientSize = new Size(528, 280);
        MinimumSize = Size;
        MaximumSize = Size;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(238, 240, 244);
        Font = new Font("Segoe UI", 9f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // ── 1. Cột trái: Thông tin nạp tiền ────────────────────────────────────
        var lblUser = new Label
        {
            Text = "Người dùng",
            Location = new Point(12, 18),
            Size = new Size(88, 24),
            TextAlign = ContentAlignment.MiddleLeft
        };
        txtUser = new TextBox
        {
            Location = new Point(102, 16),
            Size = new Size(180, 25),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9.5f)
        };
        txtUser.Leave += async (s, e) => await LookupMemberAsync(txtUser.Text.Trim(), true);
        txtUser.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LookupMemberAsync(txtUser.Text.Trim(), true);
                txtAmount.Focus();
            }
        };

        var lblAmount = new Label
        {
            Text = "Số tiền",
            Location = new Point(12, 58),
            Size = new Size(88, 24),
            TextAlign = ContentAlignment.MiddleLeft
        };
        txtAmount = new TextBox
        {
            Location = new Point(102, 56),
            Size = new Size(180, 27),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold)
        };
        txtAmount.KeyPress += (s, e) =>
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != ',')
                e.Handled = true;
        };
        txtAmount.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await ConfirmTopUpAsync();
            }
        };

        var lblBalance = new Label
        {
            Text = "Số tiền còn lại",
            Location = new Point(12, 98),
            Size = new Size(88, 24),
            TextAlign = ContentAlignment.MiddleLeft
        };
        txtBalance = new TextBox
        {
            Location = new Point(102, 96),
            Size = new Size(180, 25),
            ReadOnly = true,
            BackColor = Color.FromArgb(246, 247, 249),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9.5f),
            Text = "0"
        };

        // ── 2. Nút hành động Nạp Tiền (Góc dưới trái) ─────────────────────────
        btnTopUp = new Button
        {
            Location = new Point(12, 146),
            Size = new Size(270, 52),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Image = CsmIcons.CreateCashPaymentIcon(),
            ImageAlign = ContentAlignment.MiddleLeft,
            Text = "      Nạp Tiền",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnTopUp.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
        btnTopUp.FlatAppearance.BorderSize = 2;
        btnTopUp.MouseEnter += (s, e) => btnTopUp.BackColor = Color.FromArgb(240, 246, 255);
        btnTopUp.MouseLeave += (s, e) => btnTopUp.BackColor = Color.White;
        btnTopUp.Click += async (s, e) => await ConfirmTopUpAsync();

        // ── 3. Cột phải: Bàn phím số tiền preset (4x3 + Hàng hành động) ──────
        int[,] presetValues = {
            { 10000,  20000,  30000 },
            { 40000,  50000,  60000 },
            { 70000, 150000, 200000 },
            { 100000, 80000, 500000 }
        };

        int btnW = 68;
        int btnH = 34;
        int startX = 296;
        int startY = 16;
        int gapX = 6;
        int gapY = 5;

        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                int val = presetValues[r, c];
                var btnPreset = new Button
                {
                    Text = $"{val:N0}",
                    Location = new Point(startX + c * (btnW + gapX), startY + r * (btnH + gapY)),
                    Size = new Size(btnW, btnH),
                    BackColor = Color.FromArgb(236, 237, 241),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.25f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Tag = val
                };
                btnPreset.FlatAppearance.BorderColor = Color.FromArgb(195, 200, 210);
                btnPreset.Click += (s, e) =>
                {
                    txtAmount.Text = $"{val:N0}";
                    txtAmount.Focus();
                    txtAmount.SelectionStart = txtAmount.Text.Length;
                };
                Controls.Add(btnPreset);
            }
        }

        // Hàng 5: Nút [ － ], [ Xóa ], [ ＋ ]
        int row5Y = startY + 4 * (btnH + gapY);

        // Nút trừ: giảm 10,000đ
        var btnMinus = new Button
        {
            Text = "－",
            Location = new Point(startX, row5Y),
            Size = new Size(btnW, btnH),
            BackColor = Color.FromArgb(236, 237, 241),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnMinus.FlatAppearance.BorderColor = Color.FromArgb(195, 200, 210);
        btnMinus.Click += (s, e) =>
        {
            var clean = txtAmount.Text.Replace(",", "").Replace(".", "").Trim();
            if (decimal.TryParse(clean, out decimal cur))
            {
                cur -= 10000;
                if (cur < 0) cur = 0;
                txtAmount.Text = cur > 0 ? $"{cur:N0}" : "";
                txtAmount.Focus();
                txtAmount.SelectionStart = txtAmount.Text.Length;
            }
        };

        // Nút xóa: xóa trắng ô số tiền
        var btnClear = new Button
        {
            Text = "Xóa",
            Location = new Point(startX + btnW + gapX, row5Y),
            Size = new Size(btnW, btnH),
            BackColor = Color.FromArgb(236, 237, 241),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f),
            Cursor = Cursors.Hand
        };
        btnClear.FlatAppearance.BorderColor = Color.FromArgb(195, 200, 210);
        btnClear.Click += (s, e) =>
        {
            txtAmount.Text = "";
            txtAmount.Focus();
        };

        // Nút cộng: tăng thêm 10,000đ
        var btnPlus = new Button
        {
            Text = "＋",
            Location = new Point(startX + 2 * (btnW + gapX), row5Y),
            Size = new Size(btnW, btnH),
            BackColor = Color.FromArgb(236, 237, 241),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnPlus.FlatAppearance.BorderColor = Color.FromArgb(195, 200, 210);
        btnPlus.Click += (s, e) =>
        {
            var clean = txtAmount.Text.Replace(",", "").Replace(".", "").Trim();
            decimal cur = 0;
            if (!string.IsNullOrWhiteSpace(clean))
            {
                decimal.TryParse(clean, out cur);
            }
            cur += 10000;
            txtAmount.Text = $"{cur:N0}";
            txtAmount.Focus();
            txtAmount.SelectionStart = txtAmount.Text.Length;
        };

        Controls.AddRange([btnMinus, btnClear, btnPlus]);

        // Checkbox: Cho phép hội viên đổi số tiền nạp
        chkAllowChangeAmount = new CheckBox
        {
            Text = "Cho phép hội viên đổi số tiền nạp",
            Location = new Point(startX, row5Y + btnH + 12),
            AutoSize = true,
            Checked = true
        };

        // ── Thêm các controls bên trái vào Form ────────────────────────────────────
        Controls.AddRange([
            lblUser, txtUser,
            lblAmount, txtAmount,
            lblBalance, txtBalance,
            btnTopUp,
            chkAllowChangeAmount
        ]);

        AcceptButton = btnTopUp;
    }

    private async Task LookupMemberAsync(string keyword, bool showWarningIfNotFound)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return;

        try
        {
            var result = await ApiClient.Instance.GetMembersAsync(keyword);
            var match = result.Data.FirstOrDefault(m =>
                string.Equals(m.Username, keyword, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.Phone, keyword, StringComparison.OrdinalIgnoreCase) ||
                (m.FullName != null && m.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase)));

            if (match != null)
            {
                _foundMemberId = match.Id;
                _currentBalance = match.Balance;
                txtBalance.Text = $"{match.Balance:N0}";
                txtUser.Text = match.Username;
                Text = match.Status == "ACTIVE"
                    ? $"【 Nạp tiền [Hội viên ONLINE: {match.Username}] 】"
                    : $"【 Nạp tiền [Hội viên: {match.Username}] 】";
            }
            else if (showWarningIfNotFound)
            {
                MessageBox.Show($"Không tìm thấy hội viên nào khớp với '{keyword}'.",
                    "Tìm hội viên", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch
        {
            // Bỏ qua lỗi kết nối nền
        }
    }

    private async Task ConfirmTopUpAsync()
    {
        var rawAmount = txtAmount.Text.Replace(",", "").Replace(".", "").Trim();
        if (!decimal.TryParse(rawAmount, out decimal amount) || amount <= 0)
        {
            MessageBox.Show("Vui lòng nhập hoặc chọn số tiền nạp hợp lệ!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtAmount.Focus();
            return;
        }

        var username = txtUser.Text.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            MessageBox.Show("Vui lòng nhập tên đăng nhập hoặc số điện thoại người dùng!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtUser.Focus();
            return;
        }

        if (_foundMemberId is null)
        {
            await LookupMemberAsync(username, true);
            if (_foundMemberId is null) return;
        }

        var confirm = MessageBox.Show(
            $"Xác nhận nạp {amount:N0} đ cho hội viên [{username}]?\n\nSố dư sau khi nạp: {_currentBalance + amount:N0} đ",
            "Xác nhận nạp tiền", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        btnTopUp.Enabled = false;
        try
        {
            var res = await ApiClient.Instance.TopUpAsync(_foundMemberId.Value, amount);
            MessageBox.Show(
                $"✅ Nạp tiền thành công cho [{username}]!\n\nSố tiền: {amount:N0} đ\nSố dư mới: {res.NewBalance:N0} đ\nMã GD: {res.TransCode}",
                "Nạp tiền thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ApiException ex)
        {
            MessageBox.Show(ex.Message, "Không nạp được tiền", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (HttpRequestException)
        {
            MessageBox.Show("Không kết nối được server. Vui lòng kiểm tra lại dịch vụ máy chủ.",
                "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (!IsDisposed)
            {
                btnTopUp.Enabled = true;
            }
        }
    }
}
