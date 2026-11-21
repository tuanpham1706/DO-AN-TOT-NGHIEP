using VT3CyberZone.Desktop.Api;

namespace VT3CyberZone.Desktop.Forms;

public sealed class CreateMemberForm : Form
{
    private readonly TextBox _username = new();
    private readonly TextBox _fullName = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _password = new();
    private readonly TextBox _birthDate = new();
    private readonly TextBox _citizenId = new();
    private readonly TextBox _address = new();
    private readonly PictureBox _avatar = new();
    private readonly Button _createButton = new();

    public CreateMemberForm()
    {
        Text = "Hội viên";
        ClientSize = new Size(820, 440);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 9f);
        BackColor = Color.FromArgb(247, 249, 252);

        var title = new Label
        {
            Text = "THÔNG TIN HỘI VIÊN",
            Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 55, 85),
            Location = new Point(18, 12),
            AutoSize = true
        };
        Controls.Add(title);

        var left = new GroupBox
        {
            Text = "Thông tin cá nhân",
            Location = new Point(18, 42),
            Size = new Size(470, 330),
            BackColor = Color.White
        };
        var right = new GroupBox
        {
            Text = "Tài khoản",
            Location = new Point(498, 42),
            Size = new Size(304, 330),
            BackColor = Color.White
        };

        // Hàng 1
        AddFormField(left, "Tên", _fullName, 18, 28, 210);
        AddFormField(left, "Ngày sinh", _birthDate, 238, 28, 210);

        // Hàng 2
        AddFormField(left, "Số CCCD", _citizenId, 18, 82, 210);
        AddFormField(left, "Địa chỉ", _address, 238, 82, 210);

        // Hàng 3
        AddFormField(left, "Điện thoại", _phone, 18, 136, 210);

        var memberGroup = new ComboBox
        {
            Location = new Point(238, 158),
            Size = new Size(210, 26),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        memberGroup.Items.AddRange(["Hội viên", "Hội viên VIP", "Hội viên SVIP"]);
        memberGroup.SelectedIndex = 0;
        left.Controls.Add(new Label { Text = "Nhóm người dùng", Location = new Point(238, 136), AutoSize = true });
        left.Controls.Add(memberGroup);

        // Hàng 4
        var allowed = new CheckBox { Text = "Cho phép sử dụng", Location = new Point(18, 200), AutoSize = true, Checked = true };
        left.Controls.Add(allowed);

        // Ảnh đại diện
        left.Controls.Add(new Label { Text = "Ảnh đại diện", Location = new Point(238, 196), AutoSize = true });
        _avatar.Location = new Point(238, 218);
        _avatar.Size = new Size(110, 88);
        _avatar.BorderStyle = BorderStyle.FixedSingle;
        _avatar.SizeMode = PictureBoxSizeMode.Zoom;
        _avatar.BackColor = Color.FromArgb(230, 235, 242);
        left.Controls.Add(_avatar);

        var addPhoto = new Button { Text = "Thêm ảnh", Location = new Point(356, 246), Size = new Size(80, 32), FlatStyle = FlatStyle.Flat };
        addPhoto.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog { Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                using var stream = new MemoryStream(File.ReadAllBytes(dialog.FileName));
                _avatar.Image = new Bitmap(stream);
            }
        };
        left.Controls.Add(addPhoto);

        // Tài khoản bên phải
        right.Controls.Add(new Label { Text = "Tên đăng nhập *", Location = new Point(18, 28), AutoSize = true });
        _username.Location = new Point(18, 50);
        _username.Size = new Size(270, 26);
        right.Controls.Add(_username);

        right.Controls.Add(new Label { Text = "Mật khẩu ban đầu *", Location = new Point(18, 84), AutoSize = true });
        _password.Location = new Point(18, 106);
        _password.Size = new Size(270, 26);
        _password.UseSystemPasswordChar = true;
        right.Controls.Add(_password);

        var balanceTitle = new Label
        {
            Text = "Tài khoản tiền tệ",
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
            Location = new Point(18, 144),
            AutoSize = true
        };
        right.Controls.Add(balanceTitle);
        AddReadOnlyCurrency(right, "Tài khoản chính", "0 đ", 18, 168);
        AddReadOnlyCurrency(right, "Tài khoản phụ", "0 đ", 18, 192);
        AddReadOnlyCurrency(right, "Số tiền đã nạp", "0 đ", 18, 216);
        AddReadOnlyCurrency(right, "Số tiền dịch vụ", "0 đ", 18, 240);
        AddReadOnlyCurrency(right, "Số tiền khả dụng", "0 đ", 18, 264);
        AddReadOnlyCurrency(right, "Tổng nạp", "0 đ", 18, 288);

        Controls.AddRange([left, right]);

        var cancelButton = new Button
        {
            Text = "Đóng",
            Location = new Point(602, 388),
            Size = new Size(90, 36),
            BackColor = Color.FromArgb(220, 225, 232),
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.Cancel
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        _createButton.Text = "Thêm";
        _createButton.Location = new Point(702, 388);
        _createButton.Size = new Size(100, 36);
        _createButton.BackColor = Color.FromArgb(0, 150, 70);
        _createButton.ForeColor = Color.White;
        _createButton.FlatStyle = FlatStyle.Flat;
        _createButton.FlatAppearance.BorderSize = 0;
        _createButton.Click += CreateButton_Click;
        Controls.AddRange([cancelButton, _createButton]);

        AcceptButton = _createButton;
        CancelButton = cancelButton;
    }

    private static void AddFormField(Control parent, string label, TextBox box, int left, int top, int width)
    {
        parent.Controls.Add(new Label { Text = label, Location = new Point(left, top), AutoSize = true });
        box.Location = new Point(left, top + 22);
        box.Size = new Size(width, 26);
        parent.Controls.Add(box);
    }

    private static void AddReadOnlyCurrency(Control parent, string label, string value, int left, int top)
    {
        parent.Controls.Add(new Label
        {
            Text = label,
            Location = new Point(left, top),
            Width = 155,
            Font = new Font("Segoe UI", 8.5f)
        });
        parent.Controls.Add(new TextBox
        {
            Text = value,
            Location = new Point(180, top - 2),
            Width = 105,
            ReadOnly = true,
            TextAlign = HorizontalAlignment.Right
        });
    }

    private async void CreateButton_Click(object? sender, EventArgs e)
    {
        var username = _username.Text.Trim();
        var password = _password.Text;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Tên đăng nhập và mật khẩu ban đầu là bắt buộc.", "Thiếu thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (password.Length < 6)
        {
            MessageBox.Show("Mật khẩu ban đầu phải có ít nhất 6 ký tự.", "Mật khẩu chưa hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _createButton.Enabled = false;
        _createButton.Text = "Đang lưu...";
        try
        {
            var result = await ApiClient.Instance.CreateMemberAsync(
                username,
                password,
                string.IsNullOrWhiteSpace(_fullName.Text) ? null : _fullName.Text.Trim(),
                string.IsNullOrWhiteSpace(_phone.Text) ? null : _phone.Text.Trim());
            MessageBox.Show($"{result.Message}\nID hội viên: {result.Id}\nSố dư ban đầu: 0 đồng.",
                "Đã lưu lên server", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ApiException ex)
        {
            MessageBox.Show(ex.Message, "Không tạo được hội viên", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (HttpRequestException)
        {
            MessageBox.Show("Không kết nối được server.", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (TaskCanceledException)
        {
            MessageBox.Show("Server không phản hồi trong thời gian chờ.", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (!IsDisposed)
            {
                _createButton.Enabled = true;
                _createButton.Text = "Thêm";
            }
        }
    }
}
