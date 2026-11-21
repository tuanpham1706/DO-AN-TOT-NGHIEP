using VT3CyberZone.Desktop.Api;

namespace VT3CyberZone.Desktop.Forms;

internal sealed class RevenueReportForm : Form
{
    private readonly DateTimePicker _from = new();
    private readonly DateTimePicker _to = new();
    private readonly Label _totalRevenue = new();
    private readonly Label _gameRevenue = new();
    private readonly Label _foodRevenue = new();
    private readonly Label _status = new();
    private readonly DataGridView _dailyGrid = new();
    private readonly Button _loadButton = new();

    public RevenueReportForm()
    {
        InitializeComponent();
        Shown += async (_, _) => await LoadReportAsync();
    }

    private void InitializeComponent()
    {
        Text = "Báo cáo doanh thu";
        Size = new Size(860, 560);
        MinimumSize = new Size(760, 480);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9f);

        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 74,
            BackColor = Color.FromArgb(246, 249, 253),
            Padding = new Padding(12)
        };
        toolbar.Controls.Add(new Label
        {
            Text = "Từ ngày",
            AutoSize = true,
            Location = new Point(12, 8)
        });
        _from.Format = DateTimePickerFormat.Short;
        _from.Value = DateTime.Today.AddDays(-6);
        _from.Location = new Point(12, 30);
        _from.Width = 130;

        toolbar.Controls.Add(new Label
        {
            Text = "Đến ngày",
            AutoSize = true,
            Location = new Point(154, 8)
        });
        _to.Format = DateTimePickerFormat.Short;
        _to.Value = DateTime.Today;
        _to.Location = new Point(154, 30);
        _to.Width = 130;

        _loadButton.Text = "Xem báo cáo";
        _loadButton.Location = new Point(296, 29);
        _loadButton.Size = new Size(120, 28);
        _loadButton.BackColor = Color.FromArgb(0, 120, 215);
        _loadButton.ForeColor = Color.White;
        _loadButton.FlatStyle = FlatStyle.Flat;
        _loadButton.FlatAppearance.BorderSize = 0;
        _loadButton.Click += async (_, _) => await LoadReportAsync();
        toolbar.Controls.AddRange([_from, _to, _loadButton]);

        var summary = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 96,
            Padding = new Padding(12, 10, 12, 8),
            WrapContents = false,
            BackColor = Color.White
        };
        summary.Controls.Add(CreateSummaryCard("Tổng doanh thu", _totalRevenue, Color.FromArgb(124, 58, 237)));
        summary.Controls.Add(CreateSummaryCard("Tiền giờ", _gameRevenue, Color.FromArgb(0, 120, 215)));
        summary.Controls.Add(CreateSummaryCard("Dịch vụ", _foodRevenue, Color.FromArgb(22, 163, 74)));

        _dailyGrid.Dock = DockStyle.Fill;
        _dailyGrid.AllowUserToAddRows = false;
        _dailyGrid.ReadOnly = true;
        _dailyGrid.RowHeadersVisible = false;
        _dailyGrid.BackgroundColor = Color.White;
        _dailyGrid.GridColor = Color.FromArgb(226, 230, 236);
        _dailyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _dailyGrid.ColumnHeadersHeight = 30;
        _dailyGrid.Columns.Add("date", "Ngày");
        _dailyGrid.Columns.Add("game", "Tiền giờ (VNĐ)");
        _dailyGrid.Columns.Add("food", "Dịch vụ (VNĐ)");
        _dailyGrid.Columns.Add("total", "Tổng (VNĐ)");

        _status.Dock = DockStyle.Bottom;
        _status.Height = 26;
        _status.Padding = new Padding(12, 4, 0, 0);
        _status.ForeColor = Color.FromArgb(80, 90, 110);

        Controls.Add(_dailyGrid);
        Controls.Add(_status);
        Controls.Add(summary);
        Controls.Add(toolbar);
    }

    private static Panel CreateSummaryCard(string title, Label value, Color color)
    {
        var card = new Panel
        {
            Width = 250,
            Height = 72,
            BackColor = color,
            Margin = new Padding(0, 0, 12, 0),
            Padding = new Padding(12, 8, 8, 6)
        };
        card.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Location = new Point(12, 8)
        });
        value.Text = "0 đ";
        value.AutoSize = true;
        value.ForeColor = Color.White;
        value.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
        value.Location = new Point(12, 30);
        card.Controls.Add(value);
        return card;
    }

    private async Task LoadReportAsync()
    {
        if (_from.Value.Date > _to.Value.Date)
        {
            MessageBox.Show("Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.",
                "Khoảng ngày không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _loadButton.Enabled = false;
        _status.Text = "Đang tải dữ liệu từ server...";
        try
        {
            var report = await ApiClient.Instance.GetRevenueAsync(_from.Value.Date, _to.Value.Date);
            _totalRevenue.Text = $"{report.TotalRevenue:N0} đ";
            _gameRevenue.Text = $"{report.GameFeeRevenue:N0} đ";
            _foodRevenue.Text = $"{report.FoodRevenue:N0} đ";
            _dailyGrid.Rows.Clear();
            foreach (var day in report.ByDay)
            {
                _dailyGrid.Rows.Add(
                    day.Date,
                    day.GameFee.ToString("N0"),
                    day.Food.ToString("N0"),
                    day.Total.ToString("N0"));
            }

            _status.Text = $"Đã tải {report.ByDay.Count} ngày ({_from.Value:dd/MM/yyyy} – {_to.Value:dd/MM/yyyy}).";
        }
        catch (Exception ex)
        {
            _status.Text = "Không tải được báo cáo.";
            MessageBox.Show("Không tải được báo cáo doanh thu:\n" + ex.Message,
                "Lỗi báo cáo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _loadButton.Enabled = true;
        }
    }
}
