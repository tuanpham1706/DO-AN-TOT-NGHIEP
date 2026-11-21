using VT3CyberZone.Desktop.Api;

namespace VT3CyberZone.Desktop.Forms;

internal sealed class ShopRegistrationForm : Form
{
    private readonly TextBox _shopName = new();
    private readonly TextBox _ownerName = new();
    private readonly TextBox _username = new();
    private readonly TextBox _email = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _password = new();
    private readonly TextBox _confirmPassword = new();
    private readonly Button _submit = new();

    public ShopRegistrationForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Đăng ký quán CyberZone";
        Size = new Size(480, 700);
        MinimumSize = Size;
        MaximumSize = Size;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 9.5f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.FromArgb(10, 25, 60) };
        header.Controls.Add(new Label
        {
            Text = "ĐĂNG KÝ QUÁN NET",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        });

        AddField("Tên quán", _shopName, 84);
        AddField("Họ tên chủ quán", _ownerName, 150);
        AddField("Tên đăng nhập quản lý", _username, 216);
        AddField("Email", _email, 282);
        AddField("Số điện thoại (không bắt buộc)", _phone, 348);
        AddField("Mật khẩu (ít nhất 8 ký tự)", _password, 414, password: true);
        AddField("Nhập lại mật khẩu", _confirmPassword, 480, password: true);

        var note = new Label
        {
            Text = "Quán sẽ ở trạng thái chờ duyệt. Sau khi được duyệt, bạn có thể đăng nhập app.",
            Bounds = new Rectangle(24, 544, 420, 34),
            ForeColor = Color.FromArgb(80, 90, 110)
        };
        _submit.Text = "GỬI ĐĂNG KÝ";
        _submit.Bounds = new Rectangle(24, 584, 420, 38);
        _submit.BackColor = Color.FromArgb(0, 120, 215);
        _submit.ForeColor = Color.White;
        _submit.FlatStyle = FlatStyle.Flat;
        _submit.FlatAppearance.BorderSize = 0;
        _submit.Click += Submit_Click;

        Controls.AddRange([header, note, _submit]);
    }

    private void AddField(string title, TextBox textBox, int top, bool password = false)
    {
        Controls.Add(new Label
        {
            Text = title,
            Bounds = new Rectangle(24, top, 420, 20),
            ForeColor = Color.FromArgb(40, 50, 70)
        });
        textBox.Bounds = new Rectangle(24, top + 22, 420, 30);
        textBox.Font = new Font("Segoe UI", 10f);
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.UseSystemPasswordChar = password;
        Controls.Add(textBox);
    }

    private async void Submit_Click(object? sender, EventArgs e)
    {
        if (!string.Equals(_password.Text, _confirmPassword.Text, StringComparison.Ordinal))
        {
            MessageBox.Show("Mật khẩu nhập lại không khớp.", "Kiểm tra thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _submit.Enabled = false;
        _submit.Text = "ĐANG GỬI...";
        try
        {
            var result = await ApiClient.Instance.RegisterShopAsync(
                _shopName.Text,
                _ownerName.Text,
                _username.Text,
                _email.Text,
                _phone.Text,
                _password.Text);
            MessageBox.Show(result.Message, "Đăng ký thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không gửi được đăng ký quán:\n" + ex.Message,
                "Lỗi đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _submit.Enabled = true;
            _submit.Text = "GỬI ĐĂNG KÝ";
        }
    }
}
