namespace VT3CyberZone.Desktop.Forms;

using System.Drawing;
using System.Drawing.Drawing2D;
using VT3CyberZone.Desktop.Api;

/// <summary>
/// Giao diện chính phần mềm quản lý phòng máy — Hoàn thiện chuẩn 85%+ phong cách CSM Billing
/// </summary>
public class MainForm : Form
{
    private readonly string _currentUser;
    private readonly string _currentRole;

    // ── Controls chính ────────────────────────────────────────────────────────
    private MenuStrip menuBar = null!;
    private Panel pnlActionBanner = null!;
    private Panel pnlContentContainer = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel lblStatusLeft = null!;
    private ToolStripStatusLabel lblStatusRight = null!;
    private ContextMenuStrip cmsMachine = null!;

    // ── Các View của từng Tab ────────────────────────────────────────────────
    private Panel pnlWorkstationsView = null!;
    private Panel pnlMembersView = null!;
    private Panel pnlSystemLogsView = null!;
    private Panel pnlTransactionsView = null!;
    private Panel pnlMachineGroupsView = null!;
    private Panel pnlUserGroupsView = null!;
    private Panel pnlPrepaidCardsView = null!;
    private Panel pnlAppsView = null!;

    // ── Bảng máy trạm ────────────────────────────────────────────────────────
    private bool _isLoadingMachines;
    private DataGridView dgvMachines = null!;
    private DataGridView dgvMembers = null!;
    private DataGridView dgvTransactions = null!;
    private TextBox txtMemberSearch = null!;
    private ComboBox cboMemberGroup = null!;
    private Label lblMembersTotal = null!;
    private Label lblMembersPage = null!;
    private Label lblTransactionsTotal = null!;
    private Label lblTransSummaryRevenue = null!;
    private DateTimePicker dtpTransactionsFrom = null!;
    private DateTimePicker dtpTransactionsTo = null!;
    private TextBox txtSearchMachine = null!;
    private Button btnFilterAll = null!;
    private Button btnFilterInUse = null!;
    private Button btnFilterOffline = null!;
    private Label lblDashboardOnline = null!;
    private Label lblDashboardInUse = null!;
    private Label lblDashboardOffline = null!;
    private Label lblDashboardTotal = null!;
    private string currentFilterStatus = "ALL";

    // ── Nút thanh điều hướng Tab ─────────────────────────────────────────────
    private readonly List<Button> tabButtons = [];
    private int selectedTab = 0;

    // ── Icons Cache ───────────────────────────────────────────────────────────
    private Bitmap bmpMonitorOnline = null!;
    private Bitmap bmpMonitorOffline = null!;
    private Bitmap bmpMonitorWarning = null!;

    // ── Timers ────────────────────────────────────────────────────────────────
    private System.Windows.Forms.Timer timerClock = null!;
    private System.Windows.Forms.Timer timerLiveCounters = null!;

    // ── Bảng màu chuẩn CSM Billing ────────────────────────────────────────────
    static readonly Color ClrBannerBg      = Color.FromArgb(236, 244, 252);
    static readonly Color ClrBannerBorder  = Color.FromArgb(195, 215, 235);
    static readonly Color ClrTabBg         = Color.FromArgb(242, 246, 251);
    static readonly Color ClrTabActiveBg   = Color.FromArgb(220, 235, 250);
    static readonly Color ClrTabActiveBdr  = Color.FromArgb(160, 195, 235);
    static readonly Color ClrTabHoverBg    = Color.FromArgb(232, 242, 252);
    static readonly Color ClrGridHeaderBg  = Color.FromArgb(238, 242, 247);
    static readonly Color ClrGridLine      = Color.FromArgb(226, 230, 236);
    // Màu hàng: Xanh dương đậm cho "Đang sử dụng"
    static readonly Color ClrRowInUseBg    = Color.FromArgb(0, 120, 215);
    static readonly Color ClrRowInUseFg    = Color.White;
    // Màu hàng: Đỏ cho "Mất kết nối" / "Cảnh báo"
    static readonly Color ClrRowOfflineBg  = Color.FromArgb(210, 30, 30);
    static readonly Color ClrRowOfflineFg  = Color.White;
    // Màu hàng: Xám cho máy trống/sẵn sàng
    static readonly Color ClrRowIdleBg     = Color.FromArgb(195, 195, 195);
    static readonly Color ClrRowIdleFg     = Color.FromArgb(60, 60, 60);
    // Legacy (kept for compat)
    static readonly Color ClrRedAlert      = Color.FromArgb(210, 30, 30);
    static readonly Color ClrUsedRedText   = Color.FromArgb(180, 0, 0);
    static readonly Color ClrRemainBlueTxt = Color.FromArgb(0, 60, 160);
    static readonly Color ClrInUseBg       = Color.FromArgb(0, 120, 215);
    static readonly Color ClrInUseText     = Color.White;
    static readonly Color ClrRowSelectBg   = Color.FromArgb(0, 90, 170);

    // ── Font cache cho Badges & DataGridView ──────────────────────────────────
    private static readonly Font FontBadge = new("Segoe UI Semibold", 8.25f, FontStyle.Bold);
    private static readonly Font FontMono = new("Consolas", 9f, FontStyle.Regular);
    private static readonly Font FontMonoBold = new("Consolas", 9f, FontStyle.Bold);
    private static readonly Font FontAmountBold = new("Segoe UI", 9f, FontStyle.Bold);

    private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }
        int d = radius * 2;
        if (d > rect.Width) d = rect.Width;
        if (d > rect.Height) d = rect.Height;

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static void DrawPillBadge(Graphics g, Rectangle cellBounds, string text, Color bg, Color border, Color textColor, Font? font = null, int minWidth = 0)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        font ??= FontBadge;

        var textSize = TextRenderer.MeasureText(text, font);
        int badgeW = Math.Max(minWidth, textSize.Width + 18);
        int badgeH = Math.Min(22, cellBounds.Height - 6);
        int badgeX = cellBounds.X + 8;
        int badgeY = cellBounds.Y + (cellBounds.Height - badgeH) / 2;

        var badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);

        using var path = GetRoundedPath(badgeRect, badgeH / 2);
        using var brushBg = new SolidBrush(bg);
        using var penBorder = new Pen(border, 1f);

        g.FillPath(brushBg, path);
        g.DrawPath(penBorder, path);

        TextRenderer.DrawText(g, text, font, badgeRect, textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
    }

    private static void DrawCellGridBorder(Graphics g, Rectangle bounds, Color gridColor)
    {
        g.SmoothingMode = SmoothingMode.None;
        using var pen = new Pen(gridColor);
        g.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
        g.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
    }

    private static void EnableDoubleBuffered(DataGridView dgv)
    {
        typeof(DataGridView)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
            .SetValue(dgv, true);
    }

    public MainForm(string username = "Admin", string role = "Quản lý")
    {
        _currentUser = username;
        _currentRole = role;

        InitIconCache();
        InitializeComponent();
        SetupTimers();
        Shown += async (_, _) => await LoadMembersAsync();
    }

    private void InitIconCache()
    {
        bmpMonitorOnline  = CsmIcons.CreateMonitorOnline(16, 16);
        bmpMonitorOffline = CsmIcons.CreateMonitorOffline(16, 16);
        bmpMonitorWarning = CsmIcons.CreateMonitorWarning(16, 16);
    }

    private void InitializeComponent()
    {
        Text = "V-T3 CyberZone — Quản lý phòng máy (CSM)";
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1280, 760);
        MinimumSize = new Size(1150, 680);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9f);
        Icon = SystemIcons.Application;

        BuildMenuBar();
        BuildTopActionBanner();
        BuildContextMenu();

        // Container chính chứa các Panel của từng Tab
        pnlContentContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        BuildWorkstationsView();
        BuildMembersView();
        BuildSystemLogsView();
        BuildTransactionsView();
        BuildMachineGroupsView();
        BuildUserGroupsView();
        BuildPrepaidCardsView();
        BuildAppsView();

        pnlContentContainer.Controls.AddRange([
            pnlAppsView,
            pnlPrepaidCardsView,
            pnlUserGroupsView,
            pnlMachineGroupsView,
            pnlTransactionsView,
            pnlSystemLogsView,
            pnlMembersView,
            pnlWorkstationsView
        ]);

        Controls.Add(pnlContentContainer);
        BuildBottomStatusBar();

        // Đảm bảo thứ tự Docking chuẩn WinForms tuyệt đối:
        // 1. menuBar ở trên cùng (Dock = Top, gửi về cuối Z-order)
        // 2. pnlActionBanner ở ngay dưới menuBar (Dock = Top, kế tiếp Z-order)
        // 3. statusStrip ở dưới cùng (Dock = Bottom)
        // 4. pnlContentContainer (Dock = Fill, đưa lên đầu Z-order) lấp đầy vùng giữa, không bao giờ bị che
        pnlActionBanner.SendToBack();
        menuBar.SendToBack();
        pnlContentContainer.BringToFront();

        // Mặc định hiển thị tab Máy trạm
        SwitchTab(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. MENU STRIP (Chức năng, Công cụ, Hỗ trợ) — Chuẩn CSM Billing
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildMenuBar()
    {
        menuBar = new MenuStrip
        {
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(246, 248, 250),
            ForeColor = Color.FromArgb(33, 37, 41),
            Font = new Font("Segoe UI", 9f),
            Padding = new Padding(6, 3, 0, 3),
            RenderMode = ToolStripRenderMode.System
        };

        // Menu "Chức năng"
        var mnuChucNang = new ToolStripMenuItem("Chức năng");
        mnuChucNang.DropDownItems.Add("▶  Mở máy / Tính giờ", null, (s, e) => OpenSelectedMachine());
        mnuChucNang.DropDownItems.Add("⏹  Đóng phiên máy", null, (s, e) => CloseSelectedSession());
        mnuChucNang.DropDownItems.Add("💰  Nạp tiền tài khoản", null, (s, e) => OpenTopUpDialog());
        mnuChucNang.DropDownItems.Add(new ToolStripSeparator());
        mnuChucNang.DropDownItems.Add("🔒  Khóa chương trình", null, (s, e) => LockProgram());
        mnuChucNang.DropDownItems.Add(new ToolStripSeparator());
        mnuChucNang.DropDownItems.Add("🚪  Thoát ứng dụng", null, (s, e) => Close());

        // Menu "Công cụ"
        var mnuCongCu = new ToolStripMenuItem("Công cụ");
        mnuCongCu.DropDownItems.Add("🔄  Làm mới danh sách (F5)", null, (s, e) => LoadCsmMachineData());
        mnuCongCu.DropDownItems.Add("📊  Báo cáo doanh thu & ca làm", null, (s, e) => OpenReportDialog());
        mnuCongCu.DropDownItems.Add("💳  Giao dịch trực tuyến VIP", null, (s, e) => OpenOnlineTransactions());
        mnuCongCu.DropDownItems.Add(new ToolStripSeparator());
        mnuCongCu.DropDownItems.Add("⚙️  Tùy chọn cấu hình hệ thống", null, (s, e) => MessageBox.Show("Cấu hình hệ thống V-T3 CyberZone", "Cài đặt", MessageBoxButtons.OK, MessageBoxIcon.Information));

        // Menu "Hỗ trợ"
        var mnuHoTro = new ToolStripMenuItem("Hỗ trợ");
        mnuHoTro.DropDownItems.Add("📖  Hướng dẫn sử dụng (F1)", null, (s, e) => ShowHelp());
        mnuHoTro.DropDownItems.Add("ℹ️  Thông tin phiên bản V-T3", null, (s, e) => MessageBox.Show("V-T3 CyberZone Management Edition v2.1.4\nPhát triển cho đồ án tốt nghiệp Quản lý phòng máy.", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information));
        mnuHoTro.DropDownItems.Add("📞  Hotline hỗ trợ kỹ thuật: 0762****240", null, null);

        menuBar.Items.AddRange([mnuChucNang, mnuCongCu, mnuHoTro]);
        MainMenuStrip = menuBar;
        Controls.Add(menuBar);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. TOP ACTION BANNER — Khung trên chuẩn 100% theo Ảnh mẫu 1
    // ─────────────────────────────────────────────────────────────────────────
    private readonly Dictionary<int, Button> bannerTabMap = [];

    private void BuildTopActionBanner()
    {
        pnlActionBanner = new Panel
        {
            Dock = DockStyle.Top,
            Height = 86,
            BackColor = Color.FromArgb(242, 244, 247),
            Padding = new Padding(0)
        };

        pnlActionBanner.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(215, 222, 230), 1f);
            e.Graphics.DrawLine(pen, 0, pnlActionBanner.Height - 1, pnlActionBanner.Width, pnlActionBanner.Height - 1);
        };

        // 1. Logo khối tối bên trái (195x86) căn chỉnh đều, to đẹp phù hợp chế độ toàn màn hình
        var logoImage = new PictureBox
        {
            Location = new Point(0, 0),
            Size = new Size(195, 86),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(20, 38, 50),
            Cursor = Cursors.Hand,
            Padding = Padding.Empty
        };
        logoImage.Click += (s, e) => SwitchTab(0);
        logoImage.Paint += (s, e) =>
        {
            if (logoImage.Image != null)
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                e.Graphics.DrawImage(logoImage.Image, logoImage.ClientRectangle);
            }
        };

        try
        {
            var brandPath = Path.Combine(AppContext.BaseDirectory, "Assets", "vt3-header-brand.png");
            if (File.Exists(brandPath))
            {
                using var stream = new MemoryStream(File.ReadAllBytes(brandPath));
                logoImage.Image = new Bitmap(stream);
            }
            else
            {
                var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "vt3-logo.jpg");
                if (File.Exists(logoPath))
                {
                    using var stream = new MemoryStream(File.ReadAllBytes(logoPath));
                    logoImage.Image = new Bitmap(stream);
                }
            }
        }
        catch { /* Fallback */ }

        // 2. Nút "Khóa chương trình" (Hàng trên, Y=8, nền xanh pastel, icon xanh lá, to rõ đẹp)
        var btnLock = new Button
        {
            Text = "  Khóa chương trình",
            Image = CsmIcons.CreateGreenLockBadge(22, 22),
            ImageAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleRight,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Location = new Point(205, 8),
            Size = new Size(180, 30),
            BackColor = Color.FromArgb(204, 242, 208),
            ForeColor = Color.FromArgb(20, 60, 30),
            Font = new Font("Segoe UI", 9.25f, FontStyle.Regular),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Padding = new Padding(8, 0, 10, 0)
        };
        btnLock.FlatAppearance.BorderColor = Color.FromArgb(144, 218, 155);
        btnLock.FlatAppearance.BorderSize = 1;
        btnLock.FlatAppearance.MouseOverBackColor = Color.FromArgb(184, 232, 190);
        btnLock.Click += (s, e) => LockProgram();

        // 3. Toolbar các nút chức năng (Hàng dưới, Y=44, Height=38, to rõ cân đối với logo)
        var toolbarItems = new (string Text, string IconKey, int TabIndex, Action? Action)[]
        {
            ("Nhóm máy",           "Nhóm máy",            0, null),
            ("Nhật ký giao dịch",  "Nhật ký giao dịch",   3, null),
            ("Nhóm người dùng",    "Nhóm người dùng",     1, null),
            ("Bán thẻ",            "Bán thẻ",             6, null),
            ("Nhật ký Website",    "Nhật ký Website",     2, null)
        };

        int curBtnX = 205;
        foreach (var (title, iconKey, tabIdx, act) in toolbarItems)
        {
            int assignedTab = tabIdx;
            var btn = new Button
            {
                Text = "  " + title,
                Image = CsmIcons.CreateToolbarIcon(iconKey, 20, 20),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleRight,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Location = new Point(curBtnX, 44),
                Height = 38,
                AutoSize = true,
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(30, 45, 65),
                Font = new Font("Segoe UI", 9.25f, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Padding = new Padding(12, 0, 14, 0),
                Margin = new Padding(0)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(215, 222, 230);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 240, 252);

            btn.Click += (_, _) =>
            {
                if (act != null) act();
                else if (assignedTab >= 0) SwitchTab(assignedTab);
            };

            if (assignedTab >= 0)
            {
                bannerTabMap[assignedTab] = btn;
            }

            pnlActionBanner.Controls.Add(btn);
            curBtnX += btn.PreferredSize.Width - 1; // Ghép liền viền
        }

        // Nút mũi tên dropdown ▾ ở cuối thanh toolbar
        var btnDropdown = new Button
        {
            Text = "▾",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(80, 95, 115),
            BackColor = Color.FromArgb(248, 250, 252),
            Location = new Point(curBtnX, 44),
            Size = new Size(26, 38),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnDropdown.FlatAppearance.BorderColor = Color.FromArgb(215, 222, 230);
        btnDropdown.FlatAppearance.BorderSize = 1;
        btnDropdown.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 240, 252);
        btnDropdown.Click += (s, e) =>
        {
            var cms = new ContextMenuStrip();
            cms.Items.Add("💳 Giao dịch trực tuyến VIP", null, (_, _) => OpenOnlineTransactions());
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add("⚙️ Cấu hình hệ thống", null, (_, _) => MessageBox.Show("Cấu hình hệ thống V-T3 CyberZone", "Cài đặt"));
            cms.Items.Add("📊 Báo cáo ca làm", null, (_, _) => OpenReportDialog());
            cms.Show(btnDropdown, 0, btnDropdown.Height);
        };
        pnlActionBanner.Controls.Add(btnDropdown);

        pnlActionBanner.Controls.Add(logoImage);
        pnlActionBanner.Controls.Add(btnLock);

        Controls.Add(pnlActionBanner);
    }

    private void SwitchTab(int index)
    {
        selectedTab = index;

        // Cập nhật trạng thái active trên các nút toolbar thanh banner
        foreach (var (tabIdx, btn) in bannerTabMap)
        {
            bool isSel = (tabIdx == index);
            btn.BackColor = isSel ? Color.FromArgb(220, 236, 252) : Color.FromArgb(248, 250, 252);
            btn.ForeColor = isSel ? Color.FromArgb(0, 65, 140) : Color.FromArgb(30, 45, 65);
            btn.Font = new Font("Segoe UI", 8.75f, isSel ? FontStyle.Bold : FontStyle.Regular);
        }

        // Chỉ khi chọn tab 0 (Nhóm máy) thì mới hiển thị danh sách các máy trạm
        pnlWorkstationsView.Visible = (index == 0);
        pnlMembersView.Visible      = (index == 1);
        pnlSystemLogsView.Visible   = (index == 2);
        pnlTransactionsView.Visible = (index == 3);
        pnlMachineGroupsView.Visible= (index == 4);
        pnlUserGroupsView.Visible   = (index == 5);
        pnlPrepaidCardsView.Visible = (index == 6);
        pnlAppsView.Visible         = (index == 7);

        Panel? target = index switch
        {
            0 => pnlWorkstationsView,
            1 => pnlMembersView,
            2 => pnlSystemLogsView,
            3 => pnlTransactionsView,
            4 => pnlMachineGroupsView,
            5 => pnlUserGroupsView,
            6 => pnlPrepaidCardsView,
            7 => pnlAppsView,
            _ => null
        };
        target?.BringToFront();

        if (index == 3)
        {
            _ = LoadTransactionsAsync();
        }
        else if (index == 1)
        {
            _ = LoadMembersAsync();
        }
        else if (index == 0)
        {
            LoadCsmMachineData();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. TAB 0: VIEW MÁY TRẠM (CSM Workstations Main Grid)
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildWorkstationsView()
    {
        pnlWorkstationsView = new Panel { Dock = DockStyle.Fill };

        var pnlDashboard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 82,
            BackColor = Color.FromArgb(245, 249, 254),
            Padding = new Padding(10, 8, 10, 8),
            Visible = false
        };

        var cards = new[]
        {
            ("ONLINE", "Online", Color.FromArgb(22, 163, 74), 0),
            ("USING", "Đang dùng", Color.FromArgb(59, 130, 246), 1),
            ("OFFLINE", "Offline", Color.FromArgb(239, 68, 68), 2),
            ("TOTAL", "Tổng máy", Color.FromArgb(124, 58, 237), 3)
        };

        var dashboardCards = new List<Panel>();
        for (int i = 0; i < cards.Length; i++)
        {
            var (key, title, color, idx) = cards[i];
            var card = new Panel
            {
                Width = 180,
                Height = 64,
                BackColor = color,
                Dock = DockStyle.Left,
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(10, 6, 10, 6)
            };
            card.Controls.Add(new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 8)
            });
            var value = new Label
            {
                Text = "0",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 26)
            };
            switch (key)
            {
                case "ONLINE": lblDashboardOnline = value; break;
                case "USING": lblDashboardInUse = value; break;
                case "OFFLINE": lblDashboardOffline = value; break;
                case "TOTAL": lblDashboardTotal = value; break;
            }
            card.Controls.Add(value);
            dashboardCards.Add(card);
        }

        foreach (var card in dashboardCards)
        {
            pnlDashboard.Controls.Add(card);
        }

        // Thanh công cụ phụ: Bộ lọc nhanh & Tìm kiếm
        var pnlFilterBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(246, 249, 253),
            Padding = new Padding(8, 6, 8, 6)
        };

        pnlFilterBar.Paint += (s, e) =>
        {
            using var pen = new Pen(ClrGridLine);
            e.Graphics.DrawLine(pen, 0, pnlFilterBar.Height - 1, pnlFilterBar.Width, pnlFilterBar.Height - 1);
        };

        btnFilterAll = CreateFilterChip("Tất cả (0)", true, (s, e) => ApplyStatusFilter("ALL"));
        btnFilterAll.Location = new Point(8, 6);
        btnFilterAll.Height = 28;

        btnFilterInUse = CreateFilterChip("Đang dùng (0)", false, (s, e) => ApplyStatusFilter("IN_USE"));
        btnFilterInUse.Location = new Point(108, 6);
        btnFilterInUse.Height = 28;

        btnFilterOffline = CreateFilterChip("Mất kết nối (0)", false, (s, e) => ApplyStatusFilter("OFFLINE"));
        btnFilterOffline.Location = new Point(226, 6);
        btnFilterOffline.Height = 28;

        // Khung tìm kiếm tích hợp (Search Box Container) đồng bộ chuẩn CSM, đẹp và dễ nhìn
        var pnlSearch = new Panel
        {
            Location = new Point(356, 6),
            Size = new Size(260, 28),
            BackColor = Color.White,
            Cursor = Cursors.IBeam
        };

        var picSearchIcon = new PictureBox
        {
            Image = CsmIcons.CreateSearchIcon(16, 16, Color.FromArgb(120, 135, 160)),
            SizeMode = PictureBoxSizeMode.CenterImage,
            Location = new Point(6, 6),
            Size = new Size(16, 16),
            BackColor = Color.White,
            Cursor = Cursors.IBeam
        };

        txtSearchMachine = new TextBox
        {
            PlaceholderText = "Tìm máy / SĐT hội viên...",
            Location = new Point(26, 5),
            Width = pnlSearch.Width - 32,
            Font = new Font("Segoe UI", 9f),
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 40, 55)
        };
        txtSearchMachine.TextChanged += (s, e) => FilterWorkstationsList();

        bool isSearchFocused = false;
        txtSearchMachine.GotFocus += (s, e) => { isSearchFocused = true; pnlSearch.Invalidate(); };
        txtSearchMachine.LostFocus += (s, e) => { isSearchFocused = false; pnlSearch.Invalidate(); };

        pnlSearch.Paint += (s, e) =>
        {
            var borderClr = isSearchFocused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(200, 215, 230);
            using var pen = new Pen(borderClr, 1f);
            e.Graphics.DrawRectangle(pen, 0, 0, pnlSearch.Width - 1, pnlSearch.Height - 1);
        };

        pnlSearch.Click += (s, e) => txtSearchMachine.Focus();
        picSearchIcon.Click += (s, e) => txtSearchMachine.Focus();

        pnlSearch.Controls.Add(picSearchIcon);
        pnlSearch.Controls.Add(txtSearchMachine);

        pnlFilterBar.Controls.AddRange([btnFilterAll, btnFilterInUse, btnFilterOffline, pnlSearch]);

        // Main Grid - Tự động giãn cột lấp đầy toàn màn hình (không chừa khoảng trống)
        dgvMachines = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.Single,
            GridColor = ClrGridLine,
            BackgroundColor = Color.White,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 9.25f),
            ColumnHeadersHeight = 32,
            RowTemplate = { Height = 28 },
            ContextMenuStrip = cmsMachine
        };

        dgvMachines.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ClrGridHeaderBg,
            ForeColor = Color.FromArgb(40, 45, 55),
            SelectionBackColor = ClrGridHeaderBg,
            SelectionForeColor = Color.FromArgb(40, 45, 55),
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(2, 0, 0, 0),
            WrapMode = DataGridViewTriState.False
        };
        dgvMachines.EnableHeadersVisualStyles = false;
        dgvMachines.AllowUserToResizeColumns = false;
        dgvMachines.AllowUserToResizeRows = false;
        dgvMachines.AllowUserToOrderColumns = false;
        dgvMachines.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvMachines.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

        dgvMachines.DefaultCellStyle.SelectionBackColor = ClrRowSelectBg;
        dgvMachines.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 25, 35);

        BuildGridColumns();

        dgvMachines.CellPainting += DgvMachines_CellPainting;
        dgvMachines.CellFormatting += DgvMachines_CellFormatting;
        dgvMachines.CellDoubleClick += (s, e) => OpenSelectedMachine();
        dgvMachines.MouseDown += DgvMachines_MouseDown;

        pnlWorkstationsView.Controls.Add(dgvMachines);
        pnlWorkstationsView.Controls.Add(pnlFilterBar);
        pnlWorkstationsView.Controls.Add(pnlDashboard);
        pnlFilterBar.SendToBack();
        dgvMachines.BringToFront();
    }

    private Button CreateFilterChip(string text, bool active, EventHandler onClick)
    {
        var btn = new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 8.5f, active ? FontStyle.Bold : FontStyle.Regular),
            BackColor = active ? Color.FromArgb(0, 120, 215) : Color.White,
            ForeColor = active ? Color.White : Color.FromArgb(40, 50, 65),
            FlatStyle = FlatStyle.Flat,
            Height = 26,
            AutoSize = true,
            Padding = new Padding(8, 0, 8, 0),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = active ? Color.FromArgb(0, 100, 190) : Color.FromArgb(200, 215, 230);
        btn.Click += onClick;
        return btn;
    }

    private void ApplyStatusFilter(string filter)
    {
        currentFilterStatus = filter;
        btnFilterAll.BackColor = filter == "ALL" ? Color.FromArgb(0, 120, 215) : Color.White;
        btnFilterAll.ForeColor = filter == "ALL" ? Color.White : Color.FromArgb(40, 50, 65);

        btnFilterInUse.BackColor = filter == "IN_USE" ? Color.FromArgb(0, 120, 215) : Color.White;
        btnFilterInUse.ForeColor = filter == "IN_USE" ? Color.White : Color.FromArgb(40, 50, 65);

        btnFilterOffline.BackColor = filter == "OFFLINE" ? Color.FromArgb(0, 120, 215) : Color.White;
        btnFilterOffline.ForeColor = filter == "OFFLINE" ? Color.White : Color.FromArgb(40, 50, 65);

        FilterWorkstationsList();
    }

    private void FilterWorkstationsList()
    {
        string kw = txtSearchMachine.Text.Trim().ToLower();
        CurrencyManager? cm = (CurrencyManager?)BindingContext?[dgvMachines.DataSource ?? dgvMachines.Rows];
        cm?.SuspendBinding();

        foreach (DataGridViewRow row in dgvMachines.Rows)
        {
            string name = row.Cells["colName"].Value?.ToString()?.ToLower() ?? "";
            string user = row.Cells["colUser"].Value?.ToString()?.ToLower() ?? "";
            string status = row.Cells["colStatus"].Value?.ToString() ?? "";

            bool matchKw = string.IsNullOrEmpty(kw) || name.Contains(kw) || user.Contains(kw);
            bool matchStatus = currentFilterStatus switch
            {
                "IN_USE" => (status == "Đang sử dụng"),
                "OFFLINE" => (status is "Mất kết nối" or "Cảnh báo"),
                _ => true
            };

            row.Visible = matchKw && matchStatus;
        }

        cm?.ResumeBinding();
    }

    private void BuildGridColumns()
    {
        // Định dạng cột lấp đầy toàn bộ màn hình theo tỉ lệ FillWeight (khớp 100% chiều rộng màn hình, không để trống)
        (string header, float weight, int minWidth, string name, DataGridViewContentAlignment align)[] cols =
        [
            ("Tên máy",        10f, 110, "colName",    DataGridViewContentAlignment.MiddleLeft),
            ("Tình trạng",      9f, 100, "colStatus",  DataGridViewContentAlignment.MiddleCenter),
            ("Tên người dùng", 11f, 110, "colUser",    DataGridViewContentAlignment.MiddleLeft),
            ("COMBO",           6f,  70, "colCombo",   DataGridViewContentAlignment.MiddleCenter),
            ("Bắt đầu",         8f,  85, "colStart",   DataGridViewContentAlignment.MiddleCenter),
            ("Đã dùng",         7f,  75, "colUsed",    DataGridViewContentAlignment.MiddleCenter),
            ("Còn lại",         7f,  75, "colRemain",  DataGridViewContentAlignment.MiddleCenter),
            ("Số tiền",         8f,  85, "colAmount",  DataGridViewContentAlignment.MiddleRight),
            ("Ngày",            9f,  95, "colDate",    DataGridViewContentAlignment.MiddleCenter),
            ("Phiên bản",       7f,  75, "colVersion", DataGridViewContentAlignment.MiddleCenter),
            ("Nhóm máy",       12f, 130, "colGroup",   DataGridViewContentAlignment.MiddleLeft),
            ("Ghi chú",        14f, 120, "colNote",    DataGridViewContentAlignment.MiddleLeft)
        ];

        dgvMachines.Columns.Clear();
        foreach (var (header, weight, minWidth, name, align) in cols)
        {
            var col = new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                Name = name,
                FillWeight = weight,
                MinimumWidth = minWidth,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.False,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = align,
                    Padding = align switch
                    {
                        DataGridViewContentAlignment.MiddleRight => new Padding(0, 0, 8, 0),
                        DataGridViewContentAlignment.MiddleLeft  => new Padding(8, 0, 0, 0),
                        _                                        => new Padding(0)
                    }
                }
            };
            dgvMachines.Columns.Add(col);
        }
    }

    private void DgvMachines_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        // ── Xử lý vẽ Header (RowIndex == -1): XÓA HOÀN TOÀN MÀU XANH KHI CLICK CỘT ──
        if (e.RowIndex == -1)
        {
            // 1. Luôn tô màu nền header chuẩn (xám sáng), không bao giờ bị đổi màu xanh
            using var bgBrush = new SolidBrush(ClrGridHeaderBg);
            e.Graphics?.FillRectangle(bgBrush, e.CellBounds);

            // 2. Vẽ đường viền phân cách cột
            using var borderPen = new Pen(ClrGridLine, 1f);
            e.Graphics?.DrawRectangle(borderPen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 1, e.CellBounds.Height - 1);

            // 3. Vẽ text header màu xám đậm chuẩn
            if (e.ColumnIndex >= 0 && e.ColumnIndex < dgvMachines.Columns.Count)
            {
                var headerText = dgvMachines.Columns[e.ColumnIndex].HeaderText;
                var align = dgvMachines.Columns[e.ColumnIndex].DefaultCellStyle.Alignment;
                using var textBrush = new SolidBrush(Color.FromArgb(40, 45, 55));
                using var headerFont = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold);
                using var sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = align switch
                    {
                        DataGridViewContentAlignment.MiddleRight => StringAlignment.Far,
                        DataGridViewContentAlignment.MiddleCenter => StringAlignment.Center,
                        _ => StringAlignment.Near
                    }
                };
                var padLeft = align == DataGridViewContentAlignment.MiddleLeft ? 6 : 2;
                var padRight = align == DataGridViewContentAlignment.MiddleRight ? 8 : 2;
                var textBounds = new Rectangle(
                    e.CellBounds.Left + padLeft,
                    e.CellBounds.Top,
                    e.CellBounds.Width - padLeft - padRight,
                    e.CellBounds.Height);
                e.Graphics?.DrawString(headerText, headerFont, textBrush, textBounds, sf);
            }

            e.Handled = true;
            return;
        }

        var colNameCol = dgvMachines.Columns["colName"];
        if (colNameCol != null && e.ColumnIndex == colNameCol.Index)
        {
            var status = dgvMachines.Rows[e.RowIndex].Cells["colStatus"].Value?.ToString() ?? "";
            var name = e.Value?.ToString() ?? "";

            // Nền của cell colName theo trạng thái
            Color bgColor = status switch
            {
                "Đang sử dụng"             => ClrRowInUseBg,
                "Mất kết nối" or "Cảnh báo" => ClrRowOfflineBg,
                _                           => ClrRowIdleBg
            };
            Color fgColor = status switch
            {
                "Đang sử dụng"             => ClrRowInUseFg,
                "Mất kết nối" or "Cảnh báo" => ClrRowOfflineFg,
                _                           => ClrRowIdleFg
            };

            using var bgBrush = new SolidBrush(bgColor);
            e.Graphics?.FillRectangle(bgBrush, e.CellBounds);

            Bitmap icon = status switch
            {
                "Mất kết nối" => bmpMonitorOffline,
                "Cảnh báo"    => bmpMonitorWarning,
                _             => bmpMonitorOnline
            };

            int iconY = e.CellBounds.Top + (e.CellBounds.Height - 16) / 2;
            int iconX = e.CellBounds.Left + 5;
            e.Graphics?.DrawImage(icon, iconX, iconY, 16, 16);

            var textRect = new Rectangle(iconX + 22, e.CellBounds.Top, e.CellBounds.Width - 27, e.CellBounds.Height);
            using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var brush = new SolidBrush(fgColor);
            using var sf = new StringFormat { LineAlignment = StringAlignment.Center };
            e.Graphics?.DrawString(name, font, brush, textRect, sf);

            using var pen = new Pen(ClrGridLine);
            e.Graphics?.DrawRectangle(pen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 1, e.CellBounds.Height - 1);

            e.Handled = true;
        }
    }

    private void DgvMachines_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var colName = dgvMachines.Columns[e.ColumnIndex].Name;
        var row = dgvMachines.Rows[e.RowIndex];
        var status = row.Cells["colStatus"].Value?.ToString() ?? "";

        // ── Tô màu TOÀN BỘ HÀNG theo trạng thái ──────────────────────────────
        Color rowBg, rowFg;
        if (status == "Đang sử dụng")
        {
            rowBg = ClrRowInUseBg;
            rowFg = ClrRowInUseFg;
        }
        else if (status is "Mất kết nối" or "Cảnh báo")
        {
            rowBg = ClrRowOfflineBg;
            rowFg = ClrRowOfflineFg;
        }
        else // Trống / Sẵn sàng
        {
            rowBg = ClrRowIdleBg;
            rowFg = ClrRowIdleFg;
        }

        // Áp dụng cho tất cả cột (trừ colName vẽ bằng CellPainting)
        if (colName != "colName")
        {
            e.CellStyle.BackColor = rowBg;
            e.CellStyle.ForeColor = rowFg;
            e.CellStyle.SelectionBackColor = rowBg;
            e.CellStyle.SelectionForeColor = rowFg;
        }

        // Cột Tình trạng: in đậm
        if (colName == "colStatus")
        {
            e.CellStyle.Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold);
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            e.FormattingApplied = true;
        }

        // Cột Đã dùng: giữ màu chữ đỏ nhạt trên nền hàng (chỉ cho hàng đang dùng)
        if (colName == "colUsed" && status == "Đang sử dụng")
        {
            e.CellStyle.ForeColor = Color.FromArgb(255, 230, 180);
            e.CellStyle.SelectionForeColor = Color.FromArgb(255, 230, 180);
            e.CellStyle.Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold);
        }

        // Cột Còn lại: giữ màu chữ vàng nhạt trên nền hàng (chỉ cho hàng đang dùng)
        if (colName == "colRemain" && status == "Đang sử dụng")
        {
            e.CellStyle.ForeColor = Color.FromArgb(180, 230, 255);
            e.CellStyle.SelectionForeColor = Color.FromArgb(180, 230, 255);
            e.CellStyle.Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold);
        }

        if (colName == "colAmount" && e.Value != null)
        {
            if (decimal.TryParse(e.Value.ToString()?.Replace(",", ""), out decimal amt))
            {
                e.Value = amt.ToString("#,##0");
                e.FormattingApplied = true;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5. TAB 1: VIEW TÀI KHOẢN HỘI VIÊN (Members Management)
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildMembersView()
    {
        pnlMembersView = new Panel { Dock = DockStyle.Fill, Visible = false };

        // ── Toolbar trên cùng ──────────────────────────────────────────────────
        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = Color.FromArgb(246, 249, 253),
            Padding = new Padding(8, 6, 8, 6)
        };
        toolbar.Paint += (s, e) =>
        {
            using var pen = new Pen(ClrBannerBorder, 1f);
            e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
        };

        // Dropdown nhóm người dùng: Thay thế "Thẻ đăng quán" bằng "Tài khoản", thêm "VIP" và "Nhân viên"
        cboMemberGroup = new ComboBox
        {
            Location = new Point(8, 8),
            Width = 130,
            Font = new Font("Segoe UI", 9f),
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Standard
        };
        cboMemberGroup.Items.AddRange(["Tài khoản", "Hội viên", "VIP", "Nhân viên", "Khách"]);
        cboMemberGroup.SelectedIndex = 0;
        cboMemberGroup.SelectedIndexChanged += async (s, e) => await LoadMembersAsync(txtMemberSearch.Text);

        // Search box
        txtMemberSearch = new TextBox
        {
            Location = new Point(146, 8),
            Width = 160,
            PlaceholderText = "Tìm tài khoản...",
            Font = new Font("Segoe UI", 9f),
            BorderStyle = BorderStyle.FixedSingle
        };

        // Nút Tìm
        var btnSearchMember = new Button
        {
            Text = "Tìm",
            Location = new Point(312, 8),
            Width = 55,
            Height = 28,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnSearchMember.FlatAppearance.BorderSize = 0;
        btnSearchMember.Click += async (s, e) => await LoadMembersAsync(txtMemberSearch.Text);

        // Nút Làm mới - Đặt ngay sau nút Tìm (tránh đè chữ)
        var btnRefreshMembers = new Button
        {
            Text = "Làm mới",
            Location = new Point(373, 8),
            Width = 75,
            Height = 28,
            BackColor = Color.FromArgb(235, 242, 250),
            ForeColor = Color.FromArgb(40, 60, 85),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnRefreshMembers.FlatAppearance.BorderColor = ClrBannerBorder;
        btnRefreshMembers.Click += async (s, e) => await LoadMembersAsync(txtMemberSearch.Text);

        // Label tổng số lượng
        lblMembersTotal = new Label
        {
            Text = "Đang tải dữ liệu...",
            Location = new Point(460, 13),
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 9f),
            ForeColor = Color.FromArgb(40, 80, 140)
        };

        // Label hiển thị số lượng
        lblMembersPage = new Label
        {
            Text = "",
            Location = new Point(610, 13),
            AutoSize = true,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(90, 105, 120)
        };

        // Nút thêm hội viên
        var btnAddMem = new Button
        {
            Text = "➕  Thêm hội viên",
            Font = new Font("Segoe UI Semibold", 9f),
            BackColor = Color.FromArgb(0, 150, 70),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Height = 30,
            Width = 140,
            Location = new Point(740, 7),
            Cursor = Cursors.Hand
        };
        btnAddMem.FlatAppearance.BorderSize = 0;
        btnAddMem.Click += (s, e) =>
        {
            using var form = new CreateMemberForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _ = LoadMembersAsync(txtMemberSearch.Text);
            }
        };

        var btnTopUp = new Button
        {
            Text = "💰  Nạp tiền",
            Font = new Font("Segoe UI Semibold", 9f),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Height = 30,
            Width = 105,
            Location = new Point(888, 7),
            Cursor = Cursors.Hand
        };
        btnTopUp.FlatAppearance.BorderSize = 0;
        btnTopUp.Click += (s, e) => OpenTopUpDialog();

        txtMemberSearch.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LoadMembersAsync(txtMemberSearch.Text);
            }
        };

        toolbar.Controls.AddRange([
            cboMemberGroup, txtMemberSearch, btnSearchMember, btnRefreshMembers,
            lblMembersTotal, lblMembersPage, btnAddMem, btnTopUp
        ]);

        // ── DataGridView hội viên ──────────────────────────────────────────────
        dgvMembers = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            GridColor = ClrGridLine,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.Single,
            Font = new Font("Segoe UI", 9f),
            ColumnHeadersHeight = 32,
            RowTemplate = { Height = 28 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgvMembers.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ClrGridHeaderBg,
            ForeColor = Color.FromArgb(40, 45, 55),
            SelectionBackColor = ClrGridHeaderBg,
            SelectionForeColor = Color.FromArgb(40, 45, 55),
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
            WrapMode = DataGridViewTriState.False
        };
        dgvMembers.EnableHeadersVisualStyles = false;
        dgvMembers.AllowUserToResizeColumns = false;
        dgvMembers.AllowUserToResizeRows = false;
        dgvMembers.AllowUserToOrderColumns = false;
        dgvMembers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        // Cột tự động lấp đầy toàn màn hình theo tỉ lệ FillWeight
        (string header, float weight, int minWidth)[] memCols =
        [
            ("Tên đăng nhập",   13f, 120),
            ("Tên",             12f, 110),
            ("Số CCCD",         10f, 100),
            ("Điện thoại",      10f, 100),
            ("Số tiền còn lại", 11f, 100),
            ("Nhóm người dùng", 12f, 110),
            ("Email",           14f, 120),
            ("Ghi chú",         18f, 130),
        ];
        foreach (var (header, weight, minWidth) in memCols)
        {
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = minWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(8, 0, 0, 0)
                }
            });
        }

        pnlMembersView.Controls.Add(dgvMembers);
        pnlMembersView.Controls.Add(toolbar);
        toolbar.SendToBack();
        dgvMembers.BringToFront();
    }

    private async Task LoadMembersAsync(string? search = null)
    {
        try
        {
            var result = await ApiClient.Instance.GetMembersAsync(search);
            dgvMembers.Rows.Clear();

            string selectedGroup = cboMemberGroup?.SelectedItem?.ToString() ?? "Tài khoản";
            int matchCount = 0;

            foreach (var member in result.Data)
            {
                var maskedId = member.Id.ToString();
                if (maskedId.Length > 4)
                    maskedId = new string('*', Math.Max(0, maskedId.Length - 4)) + maskedId[^4..];

                string group = DetermineUserGroup(member);

                // Lọc theo nhóm nếu người dùng chọn một nhóm cụ thể
                if (selectedGroup != "Tài khoản" && !string.Equals(group, selectedGroup, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                matchCount++;
                dgvMembers.Rows.Add(
                    member.Username,
                    member.FullName ?? "",
                    maskedId,
                    member.Phone ?? "",
                    member.Balance.ToString("N0"),
                    group,
                    "",
                    member.Status == "ACTIVE" ? "Hoạt động" : "Đã khóa");
            }

            if (selectedGroup == "Tài khoản")
            {
                lblMembersTotal.Text = $"Tổng tài khoản: {result.Total}";
                lblMembersPage.Text = $"Hiển thị: {matchCount} / {result.Total}";
            }
            else
            {
                lblMembersTotal.Text = $"Số lượng {selectedGroup}: {matchCount}";
                lblMembersPage.Text = $"Hiển thị: {matchCount} / {result.Total}";
            }
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException or TaskCanceledException)
        {
            dgvMembers.Rows.Clear();
            lblMembersTotal.Text = "Không tải được dữ liệu";
            lblMembersPage.Text = "";
            MessageBox.Show(ex.Message, "Không tải được dữ liệu tài khoản",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string DetermineUserGroup(UserDto member)
    {
        if (member.Role is "ADMIN" or "STAFF" ||
            member.Username.StartsWith("thungan", StringComparison.OrdinalIgnoreCase) ||
            member.Username.StartsWith("admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Nhân viên";
        }

        if (member.Username.Contains("vip", StringComparison.OrdinalIgnoreCase) || member.Balance >= 100_000)
        {
            return "VIP";
        }

        if (member.Username.StartsWith("khach", StringComparison.OrdinalIgnoreCase) ||
            member.Username.Contains("het_tien", StringComparison.OrdinalIgnoreCase) ||
            member.Username.StartsWith("guest", StringComparison.OrdinalIgnoreCase))
        {
            return "Khách";
        }

        return "Hội viên";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. TAB 2: VIEW NHẬT KÝ HỆ THỐNG (System Logs)
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildSystemLogsView()
    {
        pnlSystemLogsView = new Panel { Dock = DockStyle.Fill, Visible = false };
        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            GridColor = ClrGridLine,
            Font = new Font("Segoe UI", 9f),
            ColumnHeadersHeight = 32,
            RowTemplate = { Height = 28 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ClrGridHeaderBg,
            ForeColor = Color.FromArgb(40, 45, 55),
            SelectionBackColor = ClrGridHeaderBg,
            SelectionForeColor = Color.FromArgb(40, 45, 55),
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
            WrapMode = DataGridViewTriState.False
        };
        dgv.EnableHeadersVisualStyles = false;
        dgv.AllowUserToResizeColumns = false;
        dgv.AllowUserToResizeRows = false;
        dgv.AllowUserToOrderColumns = false;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        (string header, float weight, int minWidth)[] logCols =
        [
            ("Thời gian",                 13f, 120),
            ("Máy trạm / Nguồn",          14f, 130),
            ("Nội dung sự kiện hệ thống", 44f, 280),
            ("Mức độ",                    10f,  90),
            ("Người vận hành",            19f, 130)
        ];
        foreach (var (header, weight, minWidth) in logCols)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = minWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        object[][] logs =
        [
            ["11:00:15", "SVIP-07",    "Đăng nhập phiên chơi hội viên 0345767374", "INFO", "admin_vt3"],
            ["10:55:02", "STADIUM-3",  "Mất tín hiệu Heartbeat từ CSM Client (Cảnh báo)", "WARNING", "System"],
            ["10:48:20", "SVIP-15",    "Máy trạm ngắt kết nối mạng LAN", "ERROR", "System"],
            ["10:38:59", "STADIUM-8",  "Mở máy trả trước nạp 50,000 đ", "INFO", "thungan_ca1"],
            ["09:36:53", "STADIUM-10", "Khách hàng kết thúc phiên chơi và thanh toán", "INFO", "thungan_ca1"]
        ];
        foreach (var l in logs) dgv.Rows.Add(l);
        pnlSystemLogsView.Controls.Add(dgv);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 7. TAB 3: VIEW NHẬT KÝ GIAO DỊCH (Transaction Logs)
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildTransactionsView()
    {
        pnlTransactionsView = new Panel { Dock = DockStyle.Fill, Visible = false };
        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = Color.FromArgb(246, 249, 253),
            Padding = new Padding(10, 6, 10, 6)
        };
        toolbar.Paint += (s, e) =>
        {
            using var pen = new Pen(ClrBannerBorder);
            e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
        };

        // Panel chứa bộ lọc ngày và nút bấm bên trái (không bao giờ đè lên nhau)
        var flpLeft = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 3, 0, 0)
        };

        var lblRange = new Label
        {
            Text = "Khoảng ngày:",
            AutoSize = true,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(71, 85, 105),
            Margin = new Padding(0, 7, 8, 0)
        };

        dtpTransactionsFrom = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today.AddDays(-30),
            Width = 115,
            Font = new Font("Segoe UI", 9f),
            Margin = new Padding(0, 4, 10, 0)
        };
        var lblDash = new Label
        {
            Text = "—",
            AutoSize = true,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(148, 163, 184),
            Margin = new Padding(0, 7, 10, 0)
        };
        dtpTransactionsTo = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today,
            Width = 115,
            Font = new Font("Segoe UI", 9f),
            Margin = new Padding(0, 4, 14, 0)
        };
        var btnRefresh = new Button
        {
            Text = "🔍 Lọc giao dịch",
            Width = 125,
            Height = 29,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 3, 0, 0)
        };
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Click += async (s, e) => await LoadTransactionsAsync();

        flpLeft.Controls.AddRange([lblRange, dtpTransactionsFrom, lblDash, dtpTransactionsTo, btnRefresh]);

        // Panel hiển thị thống kê KPI bên phải
        var flpRight = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 7, 12, 0)
        };

        lblTransactionsTotal = new Label
        {
            Text = "📊 Chưa tải giao dịch",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59),
            Margin = new Padding(0, 0, 24, 0)
        };

        lblTransSummaryRevenue = new Label
        {
            Text = "💰 Doanh thu nạp: 0 VNĐ",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(22, 163, 74),
            Margin = new Padding(0, 0, 0, 0)
        };

        flpRight.Controls.AddRange([lblTransactionsTotal, lblTransSummaryRevenue]);

        toolbar.Controls.Add(flpRight);
        toolbar.Controls.Add(flpLeft);

        dgvTransactions = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            CellBorderStyle = DataGridViewCellBorderStyle.Single,
            GridColor = ClrGridLine,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9f),
            ColumnHeadersHeight = 34,
            RowTemplate = { Height = 32 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        EnableDoubleBuffered(dgvTransactions);
        dgvTransactions.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ClrGridHeaderBg,
            ForeColor = Color.FromArgb(40, 45, 55),
            SelectionBackColor = ClrGridHeaderBg,
            SelectionForeColor = Color.FromArgb(40, 45, 55),
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
            WrapMode = DataGridViewTriState.False
        };
        dgvTransactions.RowsDefaultCellStyle.BackColor = Color.White;
        dgvTransactions.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgvTransactions.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 254);
        dgvTransactions.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        dgvTransactions.EnableHeadersVisualStyles = false;
        dgvTransactions.AllowUserToResizeColumns = false;
        dgvTransactions.AllowUserToResizeRows = false;
        dgvTransactions.AllowUserToOrderColumns = false;
        dgvTransactions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        (string name, string header, float weight, int minWidth)[] transCols =
        [
            ("code",    "Mã giao dịch",   13f, 130),
            ("time",    "Thời gian",      14f, 140),
            ("member",  "Khách hàng",     13f, 120),
            ("type",    "Loại giao dịch", 12f, 115),
            ("amount",  "Số tiền (VNĐ)",  13f, 120),
            ("payment", "Phương thức",    13f, 115),
            ("staff",   "Nhân viên",      11f, 100),
            ("order",   "Mã đơn",         11f, 95)
        ];
        foreach (var (cName, cHeader, cWeight, cMinWidth) in transCols)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = cName,
                HeaderText = cHeader,
                FillWeight = cWeight,
                MinimumWidth = cMinWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            if (cName == "amount")
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            dgvTransactions.Columns.Add(col);
        }

        dgvTransactions.CellPainting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var colName = dgvTransactions.Columns[e.ColumnIndex].Name;
            var val = e.Value?.ToString() ?? string.Empty;

            bool isHandled = false;
            if (colName == "type")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                var (bg, bdr, fg) = val switch
                {
                    "Nạp tiền" => (Color.FromArgb(220, 252, 231), Color.FromArgb(134, 239, 172), Color.FromArgb(21, 128, 61)),
                    "Tiền giờ" => (Color.FromArgb(224, 242, 254), Color.FromArgb(125, 211, 252), Color.FromArgb(3, 105, 161)),
                    "Dịch vụ" => (Color.FromArgb(254, 243, 199), Color.FromArgb(253, 230, 138), Color.FromArgb(180, 83, 9)),
                    "Hoàn tiền" => (Color.FromArgb(254, 226, 226), Color.FromArgb(252, 165, 165), Color.FromArgb(185, 28, 28)),
                    _ => (Color.FromArgb(241, 245, 249), Color.FromArgb(203, 213, 225), Color.FromArgb(71, 85, 105))
                };
                DrawPillBadge(e.Graphics!, e.CellBounds, val, bg, bdr, fg, FontBadge);
                isHandled = true;
            }
            else if (colName == "amount")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                Color fg = val.StartsWith('+') ? Color.FromArgb(22, 163, 74)
                         : val.StartsWith('-') ? Color.FromArgb(220, 38, 38)
                         : Color.FromArgb(15, 23, 42);

                var bounds = e.CellBounds;
                bounds.Width -= 10;
                TextRenderer.DrawText(e.Graphics!, val, FontAmountBold, bounds, fg,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                isHandled = true;
            }
            else if (colName == "code")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                var bounds = e.CellBounds;
                bounds.X += 8;
                bounds.Width -= 8;
                TextRenderer.DrawText(e.Graphics!, val, FontMono, bounds, Color.FromArgb(3, 105, 161),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                isHandled = true;
            }
            else if (colName == "payment" && (val == "Tiền mặt" || val == "Chuyển khoản" || val.Contains("QR", StringComparison.OrdinalIgnoreCase)))
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                if (val == "Tiền mặt")
                {
                    DrawPillBadge(e.Graphics!, e.CellBounds, "💵 Tiền mặt",
                        Color.FromArgb(240, 253, 244), Color.FromArgb(187, 247, 208), Color.FromArgb(22, 101, 52), FontBadge);
                }
                else
                {
                    DrawPillBadge(e.Graphics!, e.CellBounds, "💳 " + val,
                        Color.FromArgb(245, 243, 255), Color.FromArgb(221, 214, 254), Color.FromArgb(109, 40, 217), FontBadge);
                }
                isHandled = true;
            }
            else if (colName == "order" && !string.IsNullOrEmpty(val) && val != "-")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                var bounds = e.CellBounds;
                bounds.X += 8;
                bounds.Width -= 8;
                TextRenderer.DrawText(e.Graphics!, val, FontMono, bounds, Color.FromArgb(100, 116, 139),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                isHandled = true;
            }

            if (isHandled)
            {
                DrawCellGridBorder(e.Graphics!, e.CellBounds, dgvTransactions.GridColor);
                e.Handled = true;
            }
        };

        pnlTransactionsView.Controls.Add(dgvTransactions);
        pnlTransactionsView.Controls.Add(toolbar);
        toolbar.SendToBack();
        dgvTransactions.BringToFront();
    }

    private async Task LoadTransactionsAsync()
    {
        try
        {
            if (dtpTransactionsFrom.Value.Date > dtpTransactionsTo.Value.Date)
            {
                MessageBox.Show("Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.",
                    "Khoảng ngày không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = await ApiClient.Instance.GetTransactionsAsync(
                dtpTransactionsFrom.Value.Date,
                dtpTransactionsTo.Value.Date);
            dgvTransactions.Rows.Clear();
            long totalTopUp = 0;
            foreach (var transaction in result.Data)
            {
                var (type, signedAmount) = transaction.Type switch
                {
                    "TOP_UP" => ("Nạp tiền", transaction.Amount),
                    "GAME_FEE" => ("Tiền giờ", -transaction.Amount),
                    "FOOD_ORDER" => ("Dịch vụ", -transaction.Amount),
                    "REFUND" => ("Hoàn tiền", transaction.Amount),
                    _ => (transaction.Type, transaction.Amount)
                };

                if (transaction.Type == "TOP_UP" || signedAmount > 0)
                {
                    totalTopUp += (long)transaction.Amount;
                }

                dgvTransactions.Rows.Add(
                    transaction.TransCode,
                    transaction.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    transaction.MemberName ?? "Khách vãng lai",
                    type,
                    signedAmount.ToString("+#,##0;-#,##0;0"),
                    transaction.PaymentMethod ?? "-",
                    transaction.StaffName ?? "-",
                    transaction.OrderCode ?? "-");
            }

            lblTransactionsTotal.Text = $"📊 {result.Data.Count} giao dịch";
            lblTransSummaryRevenue.Text = $"💰 Doanh thu nạp: {totalTopUp:#,##0} VNĐ";
        }
        catch (Exception ex)
        {
            dgvTransactions.Rows.Clear();
            lblTransactionsTotal.Text = "Không tải được giao dịch";
            lblTransSummaryRevenue.Text = "💰 Doanh thu nạp: 0 VNĐ";
            MessageBox.Show("Không tải được lịch sử giao dịch từ server:\n" + ex.Message,
                "Lỗi tải giao dịch", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 8. TAB 4: VIEW NHÓM MÁY (Machine Groups)
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildMachineGroupsView()
    {
        pnlMachineGroupsView = new Panel { Dock = DockStyle.Fill, Visible = false };
        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            GridColor = ClrGridLine,
            Font = new Font("Segoe UI", 9f),
            ColumnHeadersHeight = 32,
            RowTemplate = { Height = 28 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ClrGridHeaderBg,
            ForeColor = Color.FromArgb(40, 45, 55),
            SelectionBackColor = ClrGridHeaderBg,
            SelectionForeColor = Color.FromArgb(40, 45, 55),
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
            WrapMode = DataGridViewTriState.False
        };
        dgv.EnableHeadersVisualStyles = false;
        dgv.AllowUserToResizeColumns = false;
        dgv.AllowUserToResizeRows = false;
        dgv.AllowUserToOrderColumns = false;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        (string header, float weight, int minWidth)[] mgCols =
        [
            ("Tên phân khu / Nhóm máy",    20f, 160),
            ("Giá giờ chơi",               15f, 120),
            ("Số lượng máy trạm",          15f, 120),
            ("Thông số phần cứng (Specs)", 50f, 280)
        ];
        foreach (var (header, weight, minWidth) in mgCols)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = minWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        object[][] groups =
        [
            ["THI ĐẤU", "20,000 đ/giờ", "10 máy", "Core i7 14700K | RTX 4070 Ti | Màn hình 360Hz 27 inch | Bàn phím cơ Quang học"],
            ["VIP",     "15,000 đ/giờ", "25 máy", "Core i5 13400F | RTX 4060 | Màn hình 240Hz 27 inch cong | Ghế công thái học"],
            ["SVIP",    "18,000 đ/giờ", "14 máy", "Core i7 13700F | RTX 4060 Ti | Màn hình 280Hz Fast IPS | Phòng riêng chống ồn"],
            ["STANDARD","10,000 đ/giờ", "15 máy", "Core i3 12100F | GTX 1660 Super | Màn hình 165Hz 24 inch"]
        ];
        foreach (var g in groups) dgv.Rows.Add(g);
        pnlMachineGroupsView.Controls.Add(dgv);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 9. TAB 5: VIEW NHÓM NGƯỜI DÙNG (User Groups)
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildUserGroupsView()
    {
        pnlUserGroupsView = new Panel { Dock = DockStyle.Fill, Visible = false };

        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = Color.FromArgb(246, 249, 253),
            Padding = new Padding(12, 8, 12, 8)
        };
        toolbar.Paint += (s, e) =>
        {
            using var pen = new Pen(ClrBannerBorder);
            e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
        };

        var lblTitle = new Label
        {
            Text = "👑 QUẢN LÝ NHÓM NGƯỜI DÙNG & HẠNG HỘI VIÊN",
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            AutoSize = true,
            Location = new Point(12, 13)
        };
        var lblDesc = new Label
        {
            Text = "— Cấu hình chính sách chiết khấu, ưu đãi nạp và phân quyền điều khiển máy trạm",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Location = new Point(360, 15)
        };
        toolbar.Controls.AddRange([lblTitle, lblDesc]);

        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            CellBorderStyle = DataGridViewCellBorderStyle.Single,
            GridColor = ClrGridLine,
            Font = new Font("Segoe UI", 9f),
            ColumnHeadersHeight = 34,
            RowTemplate = { Height = 36 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        EnableDoubleBuffered(dgv);
        dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ClrGridHeaderBg,
            ForeColor = Color.FromArgb(40, 45, 55),
            SelectionBackColor = ClrGridHeaderBg,
            SelectionForeColor = Color.FromArgb(40, 45, 55),
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
            WrapMode = DataGridViewTriState.False
        };
        dgv.RowsDefaultCellStyle.BackColor = Color.White;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 254);
        dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        dgv.EnableHeadersVisualStyles = false;
        dgv.AllowUserToResizeColumns = false;
        dgv.AllowUserToResizeRows = false;
        dgv.AllowUserToOrderColumns = false;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        (string header, float weight, int minWidth)[] ugCols =
        [
            ("Nhóm người dùng",       22f, 160),
            ("Khuyến mãi / Giảm giá", 20f, 150),
            ("Quyền hạn hệ thống",    25f, 180),
            ("Mô tả điều kiện",       33f, 200)
        ];
        foreach (var (header, weight, minWidth) in ugCols)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = minWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        object[][] uGroups =
        [
            ["Khách vãng lai",  "0%",   "Chơi trả sau tại quầy", "Khách chơi không đăng ký tài khoản"],
            ["Hội viên Thường", "5%",   "Đăng nhập tài khoản, gọi món", "Tích lũy nạp từ 50,000 đ"],
            ["Hội viên VIP",    "10%",  "Ưu tiên giữ máy, tặng nước giờ vàng", "Tích lũy nạp từ 500,000 đ"],
            ["Hội viên SVIP",   "15%",  "Miễn phí 1 giờ chơi đêm, giảm 10% F&B", "Tích lũy nạp từ 2,000,000 đ"],
            ["Quản trị viên",   "100%", "Toàn quyền điều khiển hệ thống", "Admin / Thu ngân quản lý ca"]
        ];
        foreach (var ug in uGroups) dgv.Rows.Add(ug);

        dgv.CellPainting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (e.ColumnIndex != 0 && e.ColumnIndex != 1) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            var val = e.Value?.ToString() ?? string.Empty;
            if (e.ColumnIndex == 0)
            {
                var (bg, bdr, fg, label) = val switch
                {
                    "Khách vãng lai" => (Color.FromArgb(241, 245, 249), Color.FromArgb(203, 213, 225), Color.FromArgb(71, 85, 105), "Khách vãng lai"),
                    "Hội viên Thường" => (Color.FromArgb(224, 242, 254), Color.FromArgb(125, 211, 252), Color.FromArgb(3, 105, 161), "Hội viên Thường"),
                    "Hội viên VIP" => (Color.FromArgb(254, 243, 199), Color.FromArgb(252, 211, 77), Color.FromArgb(180, 83, 9), "Hội viên VIP ⭐"),
                    "Hội viên SVIP" => (Color.FromArgb(243, 232, 255), Color.FromArgb(216, 180, 254), Color.FromArgb(126, 34, 206), "Hội viên SVIP 💎"),
                    "Quản trị viên" => (Color.FromArgb(254, 226, 226), Color.FromArgb(252, 165, 165), Color.FromArgb(185, 28, 28), "Quản trị viên 🛡️"),
                    _ => (Color.FromArgb(241, 245, 249), Color.FromArgb(203, 213, 225), Color.FromArgb(71, 85, 105), val)
                };
                DrawPillBadge(e.Graphics!, e.CellBounds, label, bg, bdr, fg, FontBadge);
            }
            else if (e.ColumnIndex == 1)
            {
                if (val == "0%")
                {
                    DrawPillBadge(e.Graphics!, e.CellBounds, "Không giảm",
                        Color.FromArgb(241, 245, 249), Color.FromArgb(226, 232, 240), Color.FromArgb(148, 163, 184), FontBadge);
                }
                else if (val == "100%")
                {
                    DrawPillBadge(e.Graphics!, e.CellBounds, "TOÀN QUYỀN (100%)",
                        Color.FromArgb(237, 233, 254), Color.FromArgb(196, 181, 253), Color.FromArgb(109, 40, 217), FontBadge);
                }
                else
                {
                    DrawPillBadge(e.Graphics!, e.CellBounds, $"GIẢM {val}",
                        Color.FromArgb(220, 252, 231), Color.FromArgb(134, 239, 172), Color.FromArgb(21, 128, 61), FontBadge);
                }
            }

            DrawCellGridBorder(e.Graphics!, e.CellBounds, dgv.GridColor);
            e.Handled = true;
        };

        pnlUserGroupsView.Controls.Add(dgv);
        pnlUserGroupsView.Controls.Add(toolbar);
        toolbar.SendToBack();
        dgv.BringToFront();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 10. TAB 6: VIEW BÁN THẺ (Prepaid Cards)
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildPrepaidCardsView()
    {
        pnlPrepaidCardsView = new Panel { Dock = DockStyle.Fill, Visible = false };

        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = Color.FromArgb(246, 249, 253),
            Padding = new Padding(12, 8, 12, 8)
        };
        toolbar.Paint += (s, e) =>
        {
            using var pen = new Pen(ClrBannerBorder);
            e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
        };

        var btnNewCard = new Button
        {
            Text = "＋ Xuất thẻ nạp mới",
            Location = new Point(12, 8),
            Width = 145,
            Height = 30,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnNewCard.FlatAppearance.BorderSize = 0;
        btnNewCard.Click += (s, e) =>
        {
            MessageBox.Show("Chức năng tạo lô thẻ nạp trả trước đang sẵn sàng kết nối máy in nhiệt.",
                "Xuất thẻ nạp", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        var lblCardSummary = new Label
        {
            Text = "💳 Tổng số: 4 thẻ (3 chưa nạp, 1 đã nạp) | Mã hóa PIN an toàn 128-bit",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(71, 85, 105),
            AutoSize = true,
            Location = new Point(170, 13)
        };
        toolbar.Controls.AddRange([btnNewCard, lblCardSummary]);

        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            CellBorderStyle = DataGridViewCellBorderStyle.Single,
            GridColor = ClrGridLine,
            Font = new Font("Segoe UI", 9f),
            ColumnHeadersHeight = 34,
            RowTemplate = { Height = 34 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        EnableDoubleBuffered(dgv);
        dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ClrGridHeaderBg,
            ForeColor = Color.FromArgb(40, 45, 55),
            SelectionBackColor = ClrGridHeaderBg,
            SelectionForeColor = Color.FromArgb(40, 45, 55),
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
            WrapMode = DataGridViewTriState.False
        };
        dgv.RowsDefaultCellStyle.BackColor = Color.White;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 254);
        dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        dgv.EnableHeadersVisualStyles = false;
        dgv.AllowUserToResizeColumns = false;
        dgv.AllowUserToResizeRows = false;
        dgv.AllowUserToOrderColumns = false;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        (string header, float weight, int minWidth)[] cardCols =
        [
            ("Số seri thẻ",    24f, 150),
            ("Mã nạp (PIN)",   24f, 150),
            ("Mệnh giá thẻ",   18f, 120),
            ("Trạng thái thẻ", 16f, 110),
            ("Ngày xuất thẻ",  18f, 130)
        ];
        foreach (var (header, weight, minWidth) in cardCols)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = minWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        object[][] cards =
        [
            ["VT3-CARD-00129", "8374-9182-4412", "50,000 đ",  "Đã nạp", "14/11/2026 10:15"],
            ["VT3-CARD-00130", "1928-3847-5591", "20,000 đ",  "Chưa nạp", "14/11/2026 10:30"],
            ["VT3-CARD-00131", "5512-8839-1123", "100,000 đ", "Chưa nạp", "14/11/2026 10:45"],
            ["VT3-CARD-00132", "9912-3344-7788", "200,000 đ", "Chưa nạp", "14/11/2026 11:00"]
        ];
        foreach (var c in cards) dgv.Rows.Add(c);

        dgv.CellPainting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            var val = e.Value?.ToString() ?? string.Empty;
            if (e.ColumnIndex == 0) // Số seri
            {
                var bounds = e.CellBounds;
                bounds.X += 8;
                bounds.Width -= 8;
                TextRenderer.DrawText(e.Graphics!, val, FontMonoBold, bounds, Color.FromArgb(15, 23, 42),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
            else if (e.ColumnIndex == 1) // PIN
            {
                var bounds = e.CellBounds;
                bounds.X += 8;
                bounds.Width -= 8;
                TextRenderer.DrawText(e.Graphics!, val, FontMonoBold, bounds, Color.FromArgb(2, 132, 199),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
            else if (e.ColumnIndex == 2) // Mệnh giá thẻ
            {
                var bounds = e.CellBounds;
                bounds.X += 8;
                bounds.Width -= 8;
                TextRenderer.DrawText(e.Graphics!, val, FontAmountBold, bounds, Color.FromArgb(13, 148, 136),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
            else if (e.ColumnIndex == 3) // Trạng thái thẻ
            {
                if (val == "Chưa nạp")
                {
                    DrawPillBadge(e.Graphics!, e.CellBounds, "● Chưa nạp",
                        Color.FromArgb(220, 252, 231), Color.FromArgb(134, 239, 172), Color.FromArgb(21, 128, 61), FontBadge);
                }
                else
                {
                    DrawPillBadge(e.Graphics!, e.CellBounds, "✓ Đã nạp",
                        Color.FromArgb(241, 245, 249), Color.FromArgb(203, 213, 225), Color.FromArgb(100, 116, 139), FontBadge);
                }
            }
            else if (e.ColumnIndex == 4) // Ngày xuất thẻ
            {
                var bounds = e.CellBounds;
                bounds.X += 8;
                bounds.Width -= 8;
                TextRenderer.DrawText(e.Graphics!, val, FontBadge, bounds, Color.FromArgb(100, 116, 139),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }

            DrawCellGridBorder(e.Graphics!, e.CellBounds, dgv.GridColor);
            e.Handled = true;
        };

        pnlPrepaidCardsView.Controls.Add(dgv);
        pnlPrepaidCardsView.Controls.Add(toolbar);
        toolbar.SendToBack();
        dgv.BringToFront();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 11. TAB 7: VIEW QUẢN LÝ ỨNG DỤNG (Games & Apps Launcher)
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildAppsView()
    {
        pnlAppsView = new Panel { Dock = DockStyle.Fill, Visible = false };
        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            GridColor = ClrGridLine,
            Font = new Font("Segoe UI", 9f),
            ColumnHeadersHeight = 32,
            RowTemplate = { Height = 28 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ClrGridHeaderBg,
            ForeColor = Color.FromArgb(40, 45, 55),
            SelectionBackColor = ClrGridHeaderBg,
            SelectionForeColor = Color.FromArgb(40, 45, 55),
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
            WrapMode = DataGridViewTriState.False
        };
        dgv.EnableHeadersVisualStyles = false;
        dgv.AllowUserToResizeColumns = false;
        dgv.AllowUserToResizeRows = false;
        dgv.AllowUserToOrderColumns = false;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        (string header, float weight, int minWidth)[] appCols =
        [
            ("Tên trò chơi / Ứng dụng", 35f, 200),
            ("Thể loại",                15f, 120),
            ("Phiên bản hiện tại",      15f, 120),
            ("CSM Click Update",        20f, 160),
            ("Lượt mở trong ngày",      15f, 120)
        ];
        foreach (var (header, weight, minWidth) in appCols)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = minWidth,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        object[][] games =
        [
            ["Liên Minh Huyền Thoại (LoL)", "MOBA", "14.22.1", "Đã cập nhật (Mới nhất)", "42 lượt"],
            ["PUBG: BATTLEGROUNDS",         "Battle Royale", "32.1.0", "Đã cập nhật", "28 lượt"],
            ["Valorant",                   "FPS", "9.09", "Đã cập nhật", "35 lượt"],
            ["Counter-Strike 2 (CS2)",     "FPS", "1.40.2.1", "Đã cập nhật", "31 lượt"],
            ["EA Sports FC Online (FO4)",  "Thể thao", "Live Update", "Đã cập nhật", "25 lượt"],
            ["Steam Client",               "Nền tảng", "Latest", "Đã cập nhật", "48 lượt"],
            ["Discord Voice",              "Giao tiếp", "Latest", "Đã cập nhật", "45 lượt"]
        ];
        foreach (var gm in games) dgv.Rows.Add(gm);
        pnlAppsView.Controls.Add(dgv);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 12. CONTEXT MENU & RIGHT-CLICK
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildContextMenu()
    {
        cmsMachine = new ContextMenuStrip { Font = new Font("Segoe UI", 9f) };
        cmsMachine.Items.Add(CreateContextItem("open", "▶  Mở máy / Tính giờ", OpenSelectedMachine));
        cmsMachine.Items.Add(CreateContextItem("open-member", "👤  Mở máy bằng tài khoản hội viên", OpenSelectedMachine));
        cmsMachine.Items.Add(CreateContextItem("open-admin", "🛡️  Mở máy quyền Admin / Bảo trì", OpenSelectedMachine));
        cmsMachine.Items.Add(CreateContextItem("reserve", "📅  Đặt trước máy", () => MessageBox.Show("Đang mở chức năng đặt trước máy trạm...", "Đặt trước", MessageBoxButtons.OK, MessageBoxIcon.Information)));
        cmsMachine.Items.Add(CreateContextItem("close", "⏹  Đóng phiên máy", CloseSelectedSession));
        cmsMachine.Items.Add(CreateContextItem("topup", "💰  Nạp tiền vào tài khoản", OpenTopUpDialog));
        cmsMachine.Items.Add(CreateContextItem("service", "🛎️  Gọi dịch vụ Bar & Bếp", OpenServiceDialog));
        cmsMachine.Items.Add(CreateContextItem("transfer", "🔄  Chuyển máy trạm", TransferMachine));
        cmsMachine.Items.Add(CreateContextItem("lock", "🔒  Khóa máy trạm", LockWorkstation));
        cmsMachine.Items.Add(CreateContextItem("message", "💬  Gửi tin nhắn đến máy trạm...", SendMessageToMachine));
        cmsMachine.Items.Add(CreateContextItem("combo", "🎫  Đổi gói cước / Combo", () => MessageBox.Show("Chọn gói cước hoặc Combo cho phiên đang chơi.", "Đổi gói cước", MessageBoxButtons.OK, MessageBoxIcon.Information)));
        cmsMachine.Items.Add(new ToolStripSeparator());
        cmsMachine.Items.Add(CreateContextItem("shutdown", "⚡  Tắt máy trạm", ShutdownWorkstation));
        cmsMachine.Items.Add(CreateContextItem("restart", "🔁  Khởi động lại máy", RestartWorkstation));
        cmsMachine.Items.Add(CreateContextItem("details", "📋  Xem chi tiết thông số máy", ShowMachineDetails));
        cmsMachine.Opening += CmsMachine_Opening;
    }

    private ToolStripMenuItem CreateContextItem(string key, string text, Action action)
    {
        var item = new ToolStripMenuItem(text);
        item.Tag = key;
        item.Click += (_, _) => action();
        return item;
    }

    private void CmsMachine_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        bool hasSession = dgvMachines.SelectedRows.Count > 0
            && dgvMachines.SelectedRows[0].Cells["colStatus"].Value?.ToString() == "Đang sử dụng";

        var contextKeys = new HashSet<string>(hasSession
            ? ["close", "topup", "lock", "message"]
            : ["open", "open-member", "open-admin", "reserve"]);

        foreach (ToolStripItem item in cmsMachine.Items)
        {
            if (item is ToolStripSeparator)
            {
                item.Visible = true;
                continue;
            }

            var key = item.Tag?.ToString();
            item.Visible = key == "shutdown" || key == "restart" || (!hasSession && key == "details")
                || contextKeys.Contains(key ?? "");
        }

        NormalizeContextSeparators();
    }

    private void NormalizeContextSeparators()
    {
        for (int i = 0; i < cmsMachine.Items.Count; i++)
        {
            var separator = cmsMachine.Items[i] as ToolStripSeparator;
            if (separator is null) continue;

            bool hasVisibleBefore = false;
            for (int j = i - 1; j >= 0 && !hasVisibleBefore; j--)
                hasVisibleBefore = cmsMachine.Items[j].Visible;

            bool hasVisibleAfter = false;
            for (int j = i + 1; j < cmsMachine.Items.Count && !hasVisibleAfter; j++)
                hasVisibleAfter = cmsMachine.Items[j].Visible;

            separator.Visible = hasVisibleBefore && hasVisibleAfter;
        }

        ToolStripSeparator? previousSeparator = null;
        for (int i = 0; i < cmsMachine.Items.Count; i++)
        {
            if (cmsMachine.Items[i] is not ToolStripSeparator separator)
            {
                previousSeparator = null;
                continue;
            }

            if (!separator.Visible)
                continue;

            if (previousSeparator is not null)
                previousSeparator.Visible = false;
            previousSeparator = separator;
        }
    }

    private void DgvMachines_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            var hit = dgvMachines.HitTest(e.X, e.Y);
            if (hit.RowIndex >= 0)
            {
                dgvMachines.ClearSelection();
                dgvMachines.Rows[hit.RowIndex].Selected = true;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 13. BOTTOM STATUS BAR
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildBottomStatusBar()
    {
        statusStrip = new StatusStrip
        {
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(242, 245, 249),
            Font = new Font("Segoe UI", 8.75f),
            SizingGrip = false
        };

        statusStrip.Paint += (s, e) =>
        {
            using var pen = new Pen(ClrBannerBorder, 1f);
            e.Graphics.DrawLine(pen, 0, 0, statusStrip.Width, 0);
        };

        lblStatusLeft = new ToolStripStatusLabel
        {
            Text = $"F1: Hỗ trợ | {DateTime.Now:dd.MM.yyyy} | HỘ KINH DOANH | Phone: 0762****240 | Email: k****@gmail.com",
            ForeColor = Color.FromArgb(60, 70, 85),
            TextAlign = ContentAlignment.MiddleLeft,
            Spring = true
        };

        lblStatusRight = new ToolStripStatusLabel
        {
            Text = "Số thành viên:367   Số máy trạm:49   Đang sử dụng:24   Bán điện:0   Mất kết nối:25   Tỉ lệ:48%",
            ForeColor = Color.FromArgb(40, 50, 65),
            Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight
        };

        statusStrip.Items.AddRange([lblStatusLeft, lblStatusRight]);
        Controls.Add(statusStrip);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 14. DATA LOADING — DỮ LIỆU CHUẨN 100% NHƯ ẢNH 2
    // ─────────────────────────────────────────────────────────────────────────
    private async void LoadCsmMachineData()
    {
        if (_isLoadingMachines) return;
        _isLoadingMachines = true;
        try
        {
            var workstations = await ApiClient.Instance.GetWorkstationsAsync();
            dgvMachines.Rows.Clear();

            foreach (var workstation in workstations)
            {
                var status = workstation.Status switch
                {
                    "IN_USE" => "Đang sử dụng",
                    "OFFLINE" => "Mất kết nối",
                    "MAINTENANCE" => "Cảnh báo",
                    _ => "Sẵn sàng"
                };

                var session = workstation.CurrentSession;
                var startTime = session is null ? "--:--:--" : session.StartTime.ToString("HH:mm:ss");
                var used = session is null ? "00:00" : TimeSpan.FromMinutes(session.MinutesElapsed).ToString(@"hh\:mm");
                var remaining = session is null ? "00:00" : "00:00";
                var amount = session is null ? "0" : session.CostSoFar.ToString("N0");
                var userName = session is null ? "" : session.MemberName ?? "Khách vãng lai";

                var rowIndex = dgvMachines.Rows.Add(new object[]
                {
                    workstation.MachineCode,
                    status,
                    userName,
                    "",
                    startTime,
                    used,
                    remaining,
                    amount,
                    DateTime.Now.ToString("dd-MMM-yyyy"),
                    "2.1.4",
                    workstation.GroupName,
                    ""
                });
                dgvMachines.Rows[rowIndex].Tag = new WorkstationSelection(workstation.Id, session?.SessionId ?? 0);
            }

            if (dgvMachines.Rows.Count > 0)
                dgvMachines.Rows[0].Selected = true;
        }
        catch (Exception ex)
        {
            dgvMachines.Rows.Clear();
            MessageBox.Show("Không tải được danh sách máy trạm từ server:\n" + ex.Message, "Lỗi kết nối server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _isLoadingMachines = false;
        }

        await RefreshDashboardAsync();
        UpdateStatusBarMetrics();
    }

    private async Task RefreshWorkstationsListAsync()
    {
        var workstations = await ApiClient.Instance.GetWorkstationsAsync();
        dgvMachines.Rows.Clear();

        foreach (var workstation in workstations)
        {
            var status = workstation.Status switch
            {
                "IN_USE" => "Đang sử dụng",
                "OFFLINE" => "Mất kết nối",
                "MAINTENANCE" => "Cảnh báo",
                _ => "Sẵn sàng"
            };

            var session = workstation.CurrentSession;
            var startTime = session is null ? "--:--:--" : session.StartTime.ToString("HH:mm:ss");
            var used = session is null ? "00:00" : TimeSpan.FromMinutes(session.MinutesElapsed).ToString(@"hh\:mm");
            var remaining = session is null ? "00:00" : "00:00";
            var amount = session is null ? "0" : session.CostSoFar.ToString("N0");
            var userName = session is null ? "" : session.MemberName ?? "Khách vãng lai";

            var rowIndex = dgvMachines.Rows.Add(new object[]
            {
                workstation.MachineCode,
                status,
                userName,
                "",
                startTime,
                used,
                remaining,
                amount,
                DateTime.Now.ToString("dd-MMM-yyyy"),
                "2.1.4",
                workstation.GroupName,
                ""
            });
            dgvMachines.Rows[rowIndex].Tag = new WorkstationSelection(workstation.Id, session?.SessionId ?? 0);
        }

        if (dgvMachines.Rows.Count > 0)
            dgvMachines.Rows[0].Selected = true;
    }

    private async Task RefreshDashboardAsync()
    {
        try
        {
            var workstations = await ApiClient.Instance.GetWorkstationsAsync();
            var online = workstations.Count(w => w.Status != "OFFLINE" && w.Status != "MAINTENANCE");
            var inUse = workstations.Count(w => w.Status == "IN_USE");
            var offline = workstations.Count(w => w.Status == "OFFLINE" || w.Status == "MAINTENANCE");
            var total = workstations.Count;

            SetDashboardValue(lblDashboardOnline, online);
            SetDashboardValue(lblDashboardInUse, inUse);
            SetDashboardValue(lblDashboardOffline, offline);
            SetDashboardValue(lblDashboardTotal, total);

            btnFilterAll.Text = $"Tất cả ({total})";
            btnFilterInUse.Text = $"Đang dùng ({inUse})";
            btnFilterOffline.Text = $"Mất kết nối ({offline})";
        }
        catch
        {
            SetDashboardValue(lblDashboardOnline, 0);
            SetDashboardValue(lblDashboardInUse, 0);
            SetDashboardValue(lblDashboardOffline, 0);
            SetDashboardValue(lblDashboardTotal, dgvMachines.Rows.Count);
        }
    }

    private static void SetDashboardValue(Label? label, int value)
    {
        if (label is null) return;
        label.Text = value.ToString();
    }

    private sealed record WorkstationSelection(int WorkstationId, int SessionId);

    private void UpdateStatusBarMetrics()
    {
        int total = dgvMachines.Rows.Count;
        int inUse = 0;
        int ready = 0;
        int disconnected = 0;

        foreach (DataGridViewRow row in dgvMachines.Rows)
        {
            var st = row.Cells["colStatus"].Value?.ToString();
            if (st == "Đang sử dụng") inUse++;
            else if (st is "Sẵn sàng" or "Trống") ready++;
            else if (st is "Mất kết nối" or "Cảnh báo") disconnected++;
        }

        double rate = total > 0 ? (double)inUse / total * 100.0 : 0;

        lblStatusRight.Text =
            $"Số thành viên:367   " +
            $"Số máy trạm:{total}   " +
            $"Đang sử dụng:{inUse}   " +
            $"Bán điện:0   " +
            $"Mất kết nối:{disconnected}   " +
            $"Tỉ lệ:{rate:0}%";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 15. TIMERS
    // ─────────────────────────────────────────────────────────────────────────
    private void SetupTimers()
    {
        timerClock = new System.Windows.Forms.Timer { Interval = 1_000 };
        timerClock.Tick += (s, e) =>
        {
            lblStatusLeft.Text = $"F1: Hỗ trợ | {DateTime.Now:dd.MM.yyyy} | HỘ KINH DOANH | Phone: 0762****240 | Email: k****@gmail.com";
        };
        timerClock.Start();

        timerLiveCounters = new System.Windows.Forms.Timer { Interval = 60_000 };
        timerLiveCounters.Tick += (s, e) =>
        {
            foreach (DataGridViewRow row in dgvMachines.Rows)
            {
                if (row.Cells["colStatus"].Value?.ToString() == "Đang sử dụng")
                {
                    if (TimeSpan.TryParse(row.Cells["colUsed"].Value?.ToString(), out var used))
                    {
                        row.Cells["colUsed"].Value = (used + TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
                    }
                    if (TimeSpan.TryParse(row.Cells["colRemain"].Value?.ToString(), out var remain) && remain.TotalMinutes > 0)
                    {
                        var newRemain = remain - TimeSpan.FromMinutes(1);
                        row.Cells["colRemain"].Value = (newRemain > TimeSpan.Zero ? newRemain : TimeSpan.Zero).ToString(@"hh\:mm");
                    }
                }
            }
        };
        timerLiveCounters.Start();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 16. CHỨC NĂNG NGHIỆP VỤ (Mở máy, Nạp tiền, Đóng phiên, Khóa, v.v.)
    // ─────────────────────────────────────────────────────────────────────────
    private async void OpenSelectedMachine()
    {
        if (dgvMachines.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một máy trạm trong danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var row = dgvMachines.SelectedRows[0];
        var workSelection = row.Tag as WorkstationSelection ?? new WorkstationSelection(0, 0);
        var machineId = workSelection.WorkstationId;
        string name = row.Cells["colName"].Value?.ToString() ?? "";
        string status = row.Cells["colStatus"].Value?.ToString() ?? "";
        string group = row.Cells["colGroup"].Value?.ToString() ?? "Standard";

        if (status == "Đang sử dụng")
        {
            MessageBox.Show($"Máy {name} đang có người sử dụng!\nNgười dùng: {row.Cells["colUser"].Value}\nThời gian đã dùng: {row.Cells["colUsed"].Value}",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        decimal rate = group.Contains("THI ĐẤU") ? 20000m : group.Contains("VIP") ? 15000m : 10000m;
        using var form = new OpenMachineForm(name, group, rate, machineId);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            await LoadMembersAsync(txtMemberSearch.Text);
            await RefreshWorkstationsListAsync();
            UpdateStatusBarMetrics();
            MessageBox.Show($"Đã kích hoạt máy trạm {name} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void CloseSelectedSession()
    {
        if (dgvMachines.SelectedRows.Count == 0) return;
        var row = dgvMachines.SelectedRows[0];
        var workSelection = row.Tag as WorkstationSelection ?? new WorkstationSelection(0, 0);
        var sessionId = workSelection.SessionId;
        string name = row.Cells["colName"].Value?.ToString() ?? "";
        string status = row.Cells["colStatus"].Value?.ToString() ?? "";

        if (status != "Đang sử dụng")
        {
            MessageBox.Show($"Máy {name} hiện không trong phiên sử dụng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var res = MessageBox.Show($"Xác nhận kết thúc phiên làm việc và khóa máy [{name}]?", "Xác nhận đóng phiên", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (res == DialogResult.Yes)
        {
            try
            {
                var closeResult = await ApiClient.Instance.CloseSessionAsync(sessionId);
                row.Cells["colStatus"].Value = "Mất kết nối";
                row.Cells["colUser"].Value = "";
                row.Cells["colUsed"].Value = "00:00";
                row.Cells["colRemain"].Value = "00:00";
                await RefreshWorkstationsListAsync();
                UpdateStatusBarMetrics();
                MessageBox.Show($"Đã kết thúc phiên và tất toán tiền cho máy {name}.\nMã GD: {closeResult.TransCode}", "Đóng phiên thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không đóng phiên được: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OpenTopUpDialog()
    {
        string? selectedUser = null;
        decimal? currentBal = null;

        if (selectedTab == 1 && dgvMembers.SelectedRows.Count > 0)
        {
            var row = dgvMembers.SelectedRows[0];
            selectedUser = row.Cells[0].Value?.ToString();
            var balStr = row.Cells[4].Value?.ToString()?.Replace(" đ", "")?.Replace(",", "")?.Trim();
            if (decimal.TryParse(balStr, out var b)) currentBal = b;
        }
        else if (dgvMachines.SelectedRows.Count > 0)
        {
            var user = dgvMachines.SelectedRows[0].Cells["colUser"].Value?.ToString()?.Trim();
            if (!string.IsNullOrWhiteSpace(user) && user != "-" && !user.StartsWith("Khách", StringComparison.OrdinalIgnoreCase))
            {
                selectedUser = user;
            }
        }

        using var form = new TopUpForm(selectedUser, currentBal);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _ = LoadMembersAsync(txtMemberSearch.Text);
            LoadCsmMachineData();
        }
    }

    private void OpenServiceDialog()
    {
        string selectedMachine = "STADIUM-1";
        if (dgvMachines.SelectedRows.Count > 0)
        {
            selectedMachine = dgvMachines.SelectedRows[0].Cells["colName"].Value?.ToString() ?? "STADIUM-1";
        }
        using var form = new FoodOrderForm(selectedMachine);
        form.ShowDialog(this);
    }

    private void OpenReportDialog()
    {
        using var form = new RevenueReportForm();
        form.ShowDialog(this);
    }

    private void OpenOnlineTransactions()
    {
        MessageBox.Show("Cổng thanh toán & Giao dịch trực tuyến VIP:\n- Đang lắng nghe webhook ngân hàng (VietQR / MoMo / VNPay)\n- Trạng thái kết nối máy chủ CyberZone: ĐANG HOẠT ĐỘNG (Xanh lá)",
            "Giao dịch Online", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LockProgram()
    {
        var confirm = MessageBox.Show("Khóa màn hình làm việc và yêu cầu đăng nhập lại?", "Khóa chương trình", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm == DialogResult.Yes)
        {
            Hide();
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK
                || ApiClient.Instance.CurrentUser?.Username != _currentUser
                || ApiClient.Instance.CurrentUser?.Role != _currentRole)
            {
                Close();
                return;
            }

            Show();
        }
    }

    private void TransferMachine()
    {
        if (dgvMachines.SelectedRows.Count == 0) return;
        var name = dgvMachines.SelectedRows[0].Cells["colName"].Value;
        MessageBox.Show($"Chọn máy đích để chuyển phiên hoạt động từ [{name}]...", "Chuyển máy", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LockWorkstation()
    {
        if (dgvMachines.SelectedRows.Count == 0) return;
        var name = dgvMachines.SelectedRows[0].Cells["colName"].Value;
        MessageBox.Show($"Đã gửi lệnh khóa màn hình bảo vệ tới máy [{name}].\n(Tính năng điều khiển từ xa qua client agent sẽ được tích hợp trong phiên bản tiếp theo.)", "Khóa máy", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void ShutdownWorkstation()
    {
        if (dgvMachines.SelectedRows.Count == 0) return;
        var row = dgvMachines.SelectedRows[0];
        var name = row.Cells["colName"].Value;
        var workSelection = row.Tag as WorkstationSelection ?? new WorkstationSelection(0, 0);
        if (workSelection.WorkstationId <= 0) return;

        var res = MessageBox.Show($"Đặt máy [{name}] về trạng thái OFFLINE (tắt nguồn)?", "Tắt máy", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (res != DialogResult.Yes) return;

        try
        {
            await ApiClient.Instance.UpdateWorkstationStatusAsync(workSelection.WorkstationId, "OFFLINE");
            await RefreshWorkstationsListAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể cập nhật trạng thái máy: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void RestartWorkstation()
    {
        if (dgvMachines.SelectedRows.Count == 0) return;
        var row = dgvMachines.SelectedRows[0];
        var name = row.Cells["colName"].Value;
        var workSelection = row.Tag as WorkstationSelection ?? new WorkstationSelection(0, 0);
        if (workSelection.WorkstationId <= 0) return;

        var res = MessageBox.Show($"Xác nhận đặt máy [{name}] về trạng thái khởi động lại (OFFLINE)?", "Khởi động lại", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (res != DialogResult.Yes) return;

        try
        {
            await ApiClient.Instance.RestartWorkstationAsync(workSelection.WorkstationId);
            await RefreshWorkstationsListAsync();
            MessageBox.Show($"Đã gửi lệnh khởi động lại máy [{name}].", "Khởi động lại", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể gửi lệnh khởi động lại: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SendMessageToMachine()
    {
        if (dgvMachines.SelectedRows.Count == 0) return;
        var name = dgvMachines.SelectedRows[0].Cells["colName"].Value;
        MessageBox.Show($"Nhập nội dung thông báo gửi đến máy [{name}]:\n[V-T3 CyberZone thông báo: Quý khách còn 15 phút, vui lòng nạp thêm giờ chơi]", "Gửi thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowMachineDetails()
    {
        if (dgvMachines.SelectedRows.Count == 0) return;
        var row = dgvMachines.SelectedRows[0];
        MessageBox.Show(
            $"Tên máy: {row.Cells["colName"].Value}\n" +
            $"Trạng thái: {row.Cells["colStatus"].Value}\n" +
            $"Tài khoản: {row.Cells["colUser"].Value}\n" +
            $"Nhóm: {row.Cells["colGroup"].Value}\n" +
            $"Thời gian đã dùng: {row.Cells["colUsed"].Value}\n" +
            $"Thời gian còn lại: {row.Cells["colRemain"].Value}\n" +
            $"Số tiền: {row.Cells["colAmount"].Value} đ\n" +
            $"Phiên bản CSM Client: {row.Cells["colVersion"].Value}\n" +
            $"Ghi chú: {row.Cells["colNote"].Value}",
            "Chi tiết máy trạm", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowHelp()
    {
        MessageBox.Show(
            "Phím tắt nhanh:\n" +
            "- F1: Trợ giúp & hướng dẫn\n" +
            "- F5: Tải lại dữ liệu máy trạm\n" +
            "- Double click vào máy: Mở máy hoặc xem chi tiết\n" +
            "- Chuột phải vào máy: Menu tác vụ nhanh (Nạp tiền, Đóng phiên, Tắt máy...)",
            "Hướng dẫn phím tắt", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DỌN DẸP TÀI NGUYÊN
    // ─────────────────────────────────────────────────────────────────────────
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timerClock?.Dispose();
        timerLiveCounters?.Dispose();
        bmpMonitorOnline?.Dispose();
        bmpMonitorOffline?.Dispose();
        bmpMonitorWarning?.Dispose();
        ApiClient.Instance.Logout();
        base.OnFormClosed(e);
        Application.Exit();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        typeof(DataGridView)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(dgvMachines, true);
    }
}
