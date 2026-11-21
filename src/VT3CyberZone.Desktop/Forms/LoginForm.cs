using System.Drawing.Drawing2D;
using VT3CyberZone.Desktop.Api;

namespace VT3CyberZone.Desktop.Forms;

public class LoginForm : Form
{
    // ── Controls ──────────────────────────────────────────────────────────────
    private Panel pnlHeader = null!;
    private Panel pnlCard = null!;
    private Label lblLogo = null!;
    private Label lblSubtitle = null!;
    private Label lblUsername = null!;
    private Label lblPassword = null!;
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Button btnLogin = null!;
    private Label lblError = null!;
    private Label lblVersion = null!;
    private LinkLabel lnkRegisterShop = null!;

    // ── Colors ────────────────────────────────────────────────────────────────
    static readonly Color NavyBlue = Color.FromArgb(10, 25, 60);
    static readonly Color AccentBlue = Color.FromArgb(0, 120, 215);
    static readonly Color CardBg = Color.FromArgb(245, 247, 250);

    public LoginForm()
    {
        InitializeComponent();
        SetupEvents();
    }

    private void InitializeComponent()
    {
        // ── Form ──────────────────────────────────────────────────────────────
        Text = "V-T3 CyberZone — Đăng nhập";
        Size = new Size(440, 560);
        MinimumSize = new Size(440, 560);
        MaximumSize = new Size(440, 560);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = NavyBlue;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 9f);
        Icon = SystemIcons.Shield;

        // ── Header Panel ──────────────────────────────────────────────────────
        pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 180,
            BackColor = NavyBlue
        };

        lblLogo = new Label
        {
            Text = "V-T3",
            Font = new Font("Segoe UI Black", 42f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds = new Rectangle(0, 20, 440, 80)
        };

        lblSubtitle = new Label
        {
            Text = "CYBERZONE — QUẢN LÝ PHÒNG NET",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 160, 255),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds = new Rectangle(0, 100, 440, 28)
        };

        // Đường kẻ trang trí
        var divider = new Label
        {
            Bounds = new Rectangle(80, 136, 280, 2),
            BackColor = AccentBlue,
            Text = ""
        };

        pnlHeader.Controls.AddRange([lblLogo, lblSubtitle, divider]);

        // ── Card Panel ────────────────────────────────────────────────────────
        pnlCard = new Panel
        {
            Bounds = new Rectangle(30, 190, 380, 320),
            BackColor = CardBg,
            Padding = new Padding(30)
        };
        MakeRoundedPanel(pnlCard, 12);

        // Tiêu đề card
        var lblTitle = new Label
        {
            Text = "Đăng nhập hệ thống",
            Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold),
            ForeColor = NavyBlue,
            Bounds = new Rectangle(30, 20, 320, 32),
            AutoSize = false
        };

        lblUsername = new Label
        {
            Text = "Tên đăng nhập",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 80),
            Bounds = new Rectangle(30, 68, 320, 20),
            AutoSize = false
        };

        txtUsername = new TextBox
        {
            Bounds = new Rectangle(30, 90, 320, 36),
            Font = new Font("Segoe UI", 11f),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            PlaceholderText = "Nhập tên đăng nhập..."
        };

        lblPassword = new Label
        {
            Text = "Mật khẩu",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 80),
            Bounds = new Rectangle(30, 140, 320, 20),
            AutoSize = false
        };

        txtPassword = new TextBox
        {
            Bounds = new Rectangle(30, 162, 320, 36),
            Font = new Font("Segoe UI", 11f),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            UseSystemPasswordChar = true,
            PlaceholderText = "Nhập mật khẩu..."
        };

        btnLogin = new Button
        {
            Text = "🔓  ĐĂNG NHẬP",
            Bounds = new Rectangle(30, 218, 320, 46),
            Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
            BackColor = AccentBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;

        lblError = new Label
        {
            Text = "",
            Bounds = new Rectangle(30, 272, 320, 24),
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.Crimson,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Visible = false
        };
        lnkRegisterShop = new LinkLabel
        {
            Text = "Chủ quán? Đăng ký quán mới",
            Bounds = new Rectangle(30, 298, 320, 20),
            Font = new Font("Segoe UI", 9f),
            TextAlign = ContentAlignment.MiddleCenter,
            LinkColor = AccentBlue
        };
        lnkRegisterShop.LinkClicked += (_, _) =>
        {
            using var form = new ShopRegistrationForm();
            form.ShowDialog(this);
        };

        pnlCard.Controls.AddRange([lblTitle, lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, lblError, lnkRegisterShop]);

        // ── Version ───────────────────────────────────────────────────────────
        lblVersion = new Label
        {
            Text = "V-T3 CyberZone v1.0  |  © 2026 V-T3 Team",
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.FromArgb(100, 130, 170),
            Bounds = new Rectangle(0, 525, 440, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false
        };

        Controls.AddRange([pnlHeader, pnlCard, lblVersion]);
    }

    private void SetupEvents()
    {
        btnLogin.Click += BtnLogin_Click;
        txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(s!, e); };
        txtUsername.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPassword.Focus(); };

        // Hover effect cho nút
        btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(0, 100, 190);
        btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(0, 120, 215);
    }

    private async void BtnLogin_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            ShowError("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!");
            return;
        }

        btnLogin.Enabled = false;
        btnLogin.Text = "⏳  Đang đăng nhập...";
        lblError.Visible = false;

        try
        {
            await ApiClient.Instance.LoginAsync(txtUsername.Text.Trim(), txtPassword.Text);
            if (ApiClient.Instance.CurrentUser?.Role is not ("ADMIN" or "STAFF"))
            {
                ApiClient.Instance.Logout();
                ShowError("Tài khoản hội viên không thể đăng nhập màn hình quản lý.");
                return;
            }

            await ApiClient.Instance.ActivateShopAsync();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ApiException ex)
        {
            ShowError(ex.Message);
        }
        catch (HttpRequestException)
        {
            ShowError("Không kết nối được server. Hãy kiểm tra mạng LAN và địa chỉ trong ServerAddress.txt.");
        }
        catch (TaskCanceledException)
        {
            ShowError("Server không phản hồi trong thời gian chờ. Hãy kiểm tra server và mạng LAN.");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            btnLogin.Enabled = true;
            btnLogin.Text = "🔓  ĐĂNG NHẬP";
        }
    }

    private void ShowError(string msg)
    {
        lblError.Text = msg;
        lblError.Visible = true;
    }

    // Bo góc panel
    private static void MakeRoundedPanel(Panel panel, int radius)
    {
        var path = new GraphicsPath();
        var rect = new Rectangle(0, 0, panel.Width, panel.Height);
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        panel.Region = new Region(path);
    }
}
