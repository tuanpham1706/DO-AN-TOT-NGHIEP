using VT3CyberZone.Desktop.Api;

namespace VT3CyberZone.Desktop.Forms;

/// <summary>Form mở máy cho hội viên hoặc khách vãng lai</summary>
public class OpenMachineForm : Form
{
    static readonly Color NavyBlue   = Color.FromArgb(10, 25, 60);
    static readonly Color AccentGreen = Color.FromArgb(0, 155, 60);

    private readonly string _machineCode;
    private readonly string _machineGroup;
    private readonly decimal _hourlyRate;
    private readonly int _machineId;

    private RadioButton rdoMember = null!;
    private RadioButton rdoGuest  = null!;
    private Panel pnlMember = null!;
    private TextBox txtUsername = null!;
    private Button btnFindMember = null!;
    private Panel pnlMemberFound = null!;
    private Label lblFoundName = null!;
    private Label lblFoundBalance = null!;
    private Label lblEstimated = null!;
    private Button btnOpen = null!;

    private int? _memberId;
    private decimal _memberBalance;

    public OpenMachineForm(string machineCode, string group, decimal hourlyRate, int machineId = 0)
    {
        _machineCode = machineCode;
        _machineGroup = group;
        _hourlyRate = hourlyRate;
        _machineId = machineId;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = $"Mở máy — {_machineCode}";
        Size = new Size(460, 540);
        MinimumSize = Size; MaximumSize = Size;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(240, 242, 248);
        Font = new Font("Segoe UI", 9.5f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        // Header
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = NavyBlue };
        pnlHeader.Controls.Add(new Label
        {
            Text = $"▶  Mở máy  {_machineCode}  —  {_machineGroup}  ({_hourlyRate:N0}đ/giờ)",
            Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0)
        });

        // Chọn loại khách
        var lblType = new Label
        {
            Text = "Loại khách:",
            Bounds = new Rectangle(24, 72, 120, 24),
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 40, 70)
        };
        rdoMember = new RadioButton
        {
            Text = "🎮  Hội viên (trừ tài khoản)",
            Bounds = new Rectangle(24, 100, 240, 28),
            Font = new Font("Segoe UI", 10f),
            Checked = true,
            Cursor = Cursors.Hand
        };
        rdoGuest = new RadioButton
        {
            Text = "👤  Khách vãng lai (trả tiền mặt)",
            Bounds = new Rectangle(24, 130, 280, 28),
            Font = new Font("Segoe UI", 10f),
            Cursor = Cursors.Hand
        };
        rdoMember.CheckedChanged += (s, e) =>
        {
            pnlMember.Visible = rdoMember.Checked;
            UpdateOpenButtonState();
        };

        // Panel hội viên
        pnlMember = new Panel { Bounds = new Rectangle(20, 170, 416, 120), BackColor = Color.White };
        pnlMember.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlMember.ClientRectangle,
            Color.FromArgb(180, 200, 230), 1, ButtonBorderStyle.Solid,
            Color.FromArgb(180, 200, 230), 1, ButtonBorderStyle.Solid,
            Color.FromArgb(180, 200, 230), 1, ButtonBorderStyle.Solid,
            Color.FromArgb(180, 200, 230), 1, ButtonBorderStyle.Solid);

        var lblUser = new Label { Text = "Tên đăng nhập hội viên:", Bounds = new Rectangle(12, 10, 220, 20), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        txtUsername = new TextBox { Bounds = new Rectangle(12, 32, 300, 30), Font = new Font("Segoe UI", 10.5f), PlaceholderText = "Nhập username...", BorderStyle = BorderStyle.FixedSingle };
        btnFindMember = new Button
        {
            Text = "Tìm",
            Bounds = new Rectangle(320, 32, 80, 30),
            BackColor = Color.FromArgb(0, 100, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9f),
            Cursor = Cursors.Hand
        };
        btnFindMember.FlatAppearance.BorderSize = 0;
        btnFindMember.Click += BtnFindMember_Click;
        txtUsername.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnFindMember_Click(s!, e); };

        pnlMemberFound = new Panel { Bounds = new Rectangle(12, 68, 390, 42), BackColor = Color.FromArgb(235, 248, 235), Visible = false };
        lblFoundName    = new Label { Bounds = new Rectangle(8, 4, 250, 18), Font = new Font("Segoe UI Semibold", 9.5f), ForeColor = Color.FromArgb(10, 25, 60) };
        lblFoundBalance = new Label { Bounds = new Rectangle(8, 22, 300, 18), Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(0, 130, 50) };
        pnlMemberFound.Controls.AddRange([lblFoundName, lblFoundBalance]);
        pnlMember.Controls.AddRange([lblUser, txtUsername, btnFindMember, pnlMemberFound]);

        // Thông tin ước tính
        lblEstimated = new Label
        {
            Text = $"⏱️  Giá: {_hourlyRate:N0}đ/giờ  ({_hourlyRate / 60:N0}đ/phút)",
            Bounds = new Rectangle(24, 305, 416, 28),
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.FromArgb(0, 100, 180)
        };

        // Nút mở máy
        btnOpen = new Button
        {
            Text = "▶  MỞ MÁY NGAY",
            Bounds = new Rectangle(24, 350, 300, 52),
            BackColor = AccentGreen,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Black", 12f, FontStyle.Bold),
            Enabled = false,
            Cursor = Cursors.Hand
        };
        btnOpen.FlatAppearance.BorderSize = 0;
        btnOpen.Click += BtnOpen_Click;
        rdoGuest.CheckedChanged += (s, e) => UpdateOpenButtonState();

        var btnCancel = new Button
        {
            Text = "Hủy",
            Bounds = new Rectangle(334, 350, 102, 52),
            BackColor = Color.FromArgb(180, 185, 195),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 10f),
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => Close();

        Controls.AddRange([pnlHeader, lblType, rdoMember, rdoGuest, pnlMember, lblEstimated, btnOpen, btnCancel]);
    }

    private async void BtnFindMember_Click(object? sender, EventArgs e)
    {
        var kw = txtUsername.Text.Trim();
        if (string.IsNullOrEmpty(kw))
        {
            MessageBox.Show("Vui lòng nhập tên đăng nhập hoặc số điện thoại hội viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var page = await ApiClient.Instance.GetMembersAsync(kw);
            var member = page.Data.FirstOrDefault(m =>
                string.Equals(m.Username, kw, StringComparison.OrdinalIgnoreCase)
                || (m.FullName is not null && m.FullName.Contains(kw, StringComparison.OrdinalIgnoreCase))
                || (m.Phone is not null && m.Phone.Contains(kw, StringComparison.OrdinalIgnoreCase)));

            if (member is null)
            {
                _memberId = null;
                _memberBalance = 0m;
                pnlMemberFound.Visible = false;
                UpdateOpenButtonState();
                MessageBox.Show("Không tìm thấy tài khoản hội viên phù hợp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _memberId = member.Id;
            _memberBalance = member.Balance;
            lblFoundName.Text = $"✅  {member.FullName ?? member.Username}  (@{member.Username})";

            // Tính đúng: giây/phút = balance / (hourlyRate / 60)
            var ratePerMinute = _hourlyRate > 0 ? _hourlyRate / 60m : 1m;
            var estimatedMinutes = (int)Math.Floor(_memberBalance / ratePerMinute);
            lblFoundBalance.Text = $"Số dư: {_memberBalance:N0}đ  —  Chơi được ~{Math.Max(0, estimatedMinutes)} phút";
            pnlMemberFound.Visible = true;

            if (_memberBalance < ratePerMinute)
            {
                MessageBox.Show("Số dư không đủ! Yêu cầu tối thiểu 1 phút chơi.", "Không đủ tiền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            UpdateOpenButtonState();
        }
        catch (Exception ex)
        {
            _memberId = null;
            _memberBalance = 0m;
            pnlMemberFound.Visible = false;
            UpdateOpenButtonState();
            MessageBox.Show("Không tìm thấy hội viên từ server:\n" + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnOpen_Click(object? sender, EventArgs e)
    {
        if (_machineId <= 0)
        {
            MessageBox.Show("Máy trạm chưa có định danh hợp lệ, không thể mở phiên.", "Lỗi máy", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            int? memberId = rdoMember.Checked ? _memberId : null;
            var result = await ApiClient.Instance.OpenSessionAsync(_machineId, memberId);

            string target = rdoMember.Checked ? (result.MemberName ?? "Hội viên") : "Khách vãng lai";
            MessageBox.Show($"Mở máy {_machineCode} cho {target} thành công.\n{result.Message}", "Mở máy thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể mở máy: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateOpenButtonState()
    {
        if (rdoGuest.Checked)
        {
            btnOpen.Enabled = true;
            return;
        }

        var ratePerMin = _hourlyRate > 0 ? _hourlyRate / 60m : 1m;
        btnOpen.Enabled = _memberId.HasValue && _memberBalance >= ratePerMin;
    }
}

