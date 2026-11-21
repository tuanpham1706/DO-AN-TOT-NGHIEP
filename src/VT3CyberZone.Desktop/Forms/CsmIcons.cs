namespace VT3CyberZone.Desktop.Forms;

using System.Drawing;
using System.Drawing.Drawing2D;

/// <summary>
/// Trình tạo icon chuẩn phong cách CSM (Cyber Station Manager) sắc nét bằng GDI+
/// </summary>
public static class CsmIcons
{
    /// <summary>Logo thương hiệu V-T3 CyberZone.</summary>
    public static Bitmap CreateBrandLogo(int w = 58, int h = 46)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using var shield = new GraphicsPath();
        shield.AddPolygon(
        [
            new PointF(w * 0.08f, h * 0.08f),
            new PointF(w * 0.92f, h * 0.08f),
            new PointF(w * 0.86f, h * 0.72f),
            new PointF(w * 0.50f, h * 0.96f),
            new PointF(w * 0.14f, h * 0.72f)
        ]);
        using var shieldBrush = new SolidBrush(Color.FromArgb(0, 43, 92));
        g.FillPath(shieldBrush, shield);

        using var cyanPen = new Pen(Color.FromArgb(38, 196, 220), 2f);
        g.DrawPath(cyanPen, shield);

        using var cyanBrush = new SolidBrush(Color.FromArgb(38, 196, 220));
        using var whiteBrush = new SolidBrush(Color.White);
        using var font = new Font("Segoe UI Black", 14f, FontStyle.Bold);
        using var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString("V", font, whiteBrush, new RectangleF(4, 3, w - 8, h - 12), sf);
        g.FillEllipse(cyanBrush, w * 0.67f, h * 0.17f, w * 0.22f, h * 0.22f);

        return bmp;
    }

    /// <summary>Icon máy trạm đang hoạt động (Màn hình xanh)</summary>
    public static Bitmap CreateMonitorOnline(int w = 16, int h = 16)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;

        // Vỏ màn hình
        using var penFrame = new Pen(Color.FromArgb(50, 90, 140), 1f);
        using var brushScreen = new SolidBrush(Color.FromArgb(70, 160, 245));
        var rectScreen = new Rectangle(1, 1, w - 3, h - 6);
        g.FillRectangle(brushScreen, rectScreen);
        g.DrawRectangle(penFrame, rectScreen);

        // Chân đế
        using var penStand = new Pen(Color.FromArgb(80, 100, 120), 1.5f);
        g.DrawLine(penStand, w / 2, h - 5, w / 2, h - 2);
        g.DrawLine(penStand, w / 2 - 3, h - 2, w / 2 + 3, h - 2);

        // Ánh sáng màn hình
        using var brushGlow = new SolidBrush(Color.FromArgb(180, 225, 255));
        g.FillRectangle(brushGlow, new Rectangle(3, 3, w - 7, 2));

        return bmp;
    }

    /// <summary>Icon máy trạm mất kết nối (Màn hình đỏ có dấu X)</summary>
    public static Bitmap CreateMonitorOffline(int w = 16, int h = 16)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;

        // Vỏ màn hình xám nhạt
        using var penFrame = new Pen(Color.FromArgb(120, 120, 130), 1f);
        using var brushScreen = new SolidBrush(Color.FromArgb(235, 238, 242));
        var rectScreen = new Rectangle(1, 1, w - 3, h - 6);
        g.FillRectangle(brushScreen, rectScreen);
        g.DrawRectangle(penFrame, rectScreen);

        // Chân đế
        using var penStand = new Pen(Color.FromArgb(120, 120, 130), 1.5f);
        g.DrawLine(penStand, w / 2, h - 5, w / 2, h - 2);
        g.DrawLine(penStand, w / 2 - 3, h - 2, w / 2 + 3, h - 2);

        // Dấu X đỏ tươi nổi bật
        using var penX = new Pen(Color.FromArgb(220, 20, 20), 2f);
        g.DrawLine(penX, 3, 3, w - 5, h - 8);
        g.DrawLine(penX, w - 5, 3, 3, h - 8);

        return bmp;
    }

    /// <summary>Icon máy trạm cảnh báo (Màn hình vàng cam có dấu !)</summary>
    public static Bitmap CreateMonitorWarning(int w = 16, int h = 16)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;

        // Màn hình vàng
        using var penFrame = new Pen(Color.FromArgb(200, 140, 0), 1f);
        using var brushScreen = new SolidBrush(Color.FromArgb(255, 200, 40));
        var rectScreen = new Rectangle(1, 1, w - 3, h - 6);
        g.FillRectangle(brushScreen, rectScreen);
        g.DrawRectangle(penFrame, rectScreen);

        // Chân đế
        using var penStand = new Pen(Color.FromArgb(120, 120, 130), 1.5f);
        g.DrawLine(penStand, w / 2, h - 5, w / 2, h - 2);
        g.DrawLine(penStand, w / 2 - 3, h - 2, w / 2 + 3, h - 2);

        // Dấu chấm than
        using var brushExcl = new SolidBrush(Color.FromArgb(160, 40, 0));
        g.FillRectangle(brushExcl, new Rectangle(w / 2 - 1, 3, 2, 4));
        g.FillRectangle(brushExcl, new Rectangle(w / 2 - 1, 8, 2, 2));

        return bmp;
    }

    /// <summary>Icon Dịch vụ tại quầy (Khay nước / chuông phục vụ xanh cyan)</summary>
    public static Bitmap CreateServiceIcon(int w = 24, int h = 24)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(Color.FromArgb(0, 175, 235));
        using var pen = new Pen(Color.FromArgb(0, 130, 190), 1.5f);

        // Cái ly / khay phục vụ
        g.FillPolygon(brush, new PointF[]
        {
            new PointF(4, 4),
            new PointF(20, 4),
            new PointF(18, 16),
            new PointF(13, 20),
            new PointF(11, 20),
            new PointF(6, 16)
        });
        g.DrawPolygon(pen, new PointF[]
        {
            new PointF(4, 4),
            new PointF(20, 4),
            new PointF(18, 16),
            new PointF(13, 20),
            new PointF(11, 20),
            new PointF(6, 16)
        });

        // Điểm sáng
        using var brushWhite = new SolidBrush(Color.FromArgb(200, Color.White));
        g.FillEllipse(brushWhite, 7, 7, 3, 6);

        return bmp;
    }

    /// <summary>Icon Khóa chương trình (Ổ khóa xanh dương)</summary>
    public static Bitmap CreateLockIcon(int w = 24, int h = 24)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Vòng cung khóa
        using var penShackle = new Pen(Color.FromArgb(0, 130, 220), 2.5f);
        g.DrawArc(penShackle, 6, 3, 12, 12, 180, 180);

        // Thân ổ khóa
        using var brushBody = new SolidBrush(Color.FromArgb(0, 150, 240));
        g.FillRectangle(brushBody, 4, 10, 16, 12);
        using var penBody = new Pen(Color.FromArgb(0, 110, 190), 1.2f);
        g.DrawRectangle(penBody, 4, 10, 16, 12);

        // Lỗ khóa trắng
        using var brushHole = new SolidBrush(Color.White);
        g.FillEllipse(brushHole, 10, 13, 4, 4);
        g.FillRectangle(brushHole, 11, 15, 2, 4);

        return bmp;
    }

    /// <summary>Icon Báo cáo (Biểu đồ cột tròn màu xanh)</summary>
    public static Bitmap CreateReportIcon(int w = 24, int h = 24)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Nền hình tròn xanh cyan
        using var brushCircle = new SolidBrush(Color.FromArgb(0, 165, 235));
        g.FillEllipse(brushCircle, 2, 2, w - 4, h - 4);

        // Các cột biểu đồ màu trắng
        using var brushBar = new SolidBrush(Color.White);
        g.FillRectangle(brushBar, 6, 13, 3, 6);
        g.FillRectangle(brushBar, 10.5f, 9, 3, 10);
        g.FillRectangle(brushBar, 15, 6, 3, 13);

        return bmp;
    }

    /// <summary>Icon Giao dịch Online (Huy hiệu xanh lá VIP)</summary>
    public static Bitmap CreateVipBadge(int w = 28, int h = 18)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Khối xanh lá VIP
        using var brush = new SolidBrush(Color.FromArgb(16, 170, 75));
        var path = new GraphicsPath();
        int r = 3;
        var rect = new Rectangle(0, 0, w - 1, h - 1);
        path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
        path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
        path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);

        using var font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("VIP", font, Brushes.White, new RectangleF(0, 0, w, h), sf);

        return bmp;
    }

    /// <summary>Icon Badge Khóa xanh lá chuẩn CSM (Nền xanh lá bo góc, ổ khóa trắng bên trong)</summary>
    public static Bitmap CreateGreenLockBadge(int w = 24, int h = 24)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var brushBg = new SolidBrush(Color.FromArgb(39, 174, 96));
        using var path = new GraphicsPath();
        int r = 4;
        var rect = new Rectangle(1, 1, w - 2, h - 2);
        path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
        path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
        path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
        path.CloseFigure();
        g.FillPath(brushBg, path);

        // Quai khóa trắng
        using var penShackle = new Pen(Color.White, 2f);
        g.DrawArc(penShackle, 6.5f, 4.5f, 10, 8, 180, 180);

        // Thân ổ khóa trắng
        using var brushBody = new SolidBrush(Color.White);
        g.FillRectangle(brushBody, 5.5f, 8.5f, 12, 10);

        // Lỗ khóa xanh lá
        using var brushHole = new SolidBrush(Color.FromArgb(39, 174, 96));
        g.FillEllipse(brushHole, 10f, 11f, 3, 3);
        g.FillRectangle(brushHole, 10.5f, 12.5f, 2, 3);

        return bmp;
    }

    /// <summary>Icon toolbar chức năng màu xanh dương tươi (chuẩn CSM Billing)</summary>
    public static Bitmap CreateToolbarIcon(string name, int w = 18, int h = 18)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var colorBlue = Color.FromArgb(0, 140, 235);
        using var brush = new SolidBrush(colorBlue);
        using var pen = new Pen(colorBlue, 1.5f);

        switch (name)
        {
            case "Dịch vụ tại quầy":
                g.FillEllipse(brush, 3, 3, 5, 5);
                g.FillPie(brush, 0, 8, 11, 9, 180, 180);
                g.FillEllipse(brush, 9, 2, 6, 6);
                g.FillPie(brush, 6, 7, 12, 10, 180, 180);
                break;

            case "Nhật ký giao dịch":
                g.DrawArc(pen, 2, 2, 14, 14, 45, 270);
                g.FillPolygon(brush, [new PointF(12, 3), new PointF(16, 5), new PointF(13, 8)]);
                using (var font = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString("$", font, brush, new RectangleF(1, 1, 16, 16), sf);
                }
                break;

            case "Nhóm máy":
                g.FillRectangle(brush, 1, 2, 11, 8);
                g.FillRectangle(Brushes.White, 2, 3, 9, 6);
                g.DrawLine(pen, 6, 10, 6, 13);
                g.DrawLine(pen, 3, 13, 9, 13);
                g.FillRectangle(brush, 8, 6, 9, 7);
                g.FillRectangle(Brushes.White, 9, 7, 7, 5);
                break;

            case "Nhóm người dùng":
                g.FillEllipse(brush, 3, 3, 5, 5);
                g.FillPie(brush, 0, 8, 11, 9, 180, 180);
                g.FillEllipse(brush, 9, 2, 6, 6);
                g.FillPie(brush, 6, 7, 12, 10, 180, 180);
                break;

            case "Bán thẻ":
                g.FillPolygon(brush, [new PointF(2, 6), new PointF(9, 2), new PointF(16, 6)]);
                g.FillRectangle(brush, 3, 6, 12, 10);
                g.FillRectangle(Brushes.White, 6, 9, 6, 7);
                using (var penWhite = new Pen(Color.White, 1f))
                {
                    g.DrawLine(penWhite, 5, 6, 5, 9);
                    g.DrawLine(penWhite, 9, 6, 9, 9);
                    g.DrawLine(penWhite, 13, 6, 13, 9);
                }
                break;

            case "Quản chế Website":
                g.DrawRectangle(pen, 1, 2, 15, 13);
                g.FillRectangle(brush, 1, 2, 15, 4);
                g.FillEllipse(Brushes.White, 3, 3, 2, 2);
                g.FillEllipse(Brushes.White, 6, 3, 2, 2);
                g.FillRectangle(brush, 3, 8, 11, 2);
                g.FillRectangle(brush, 3, 11, 7, 2);
                break;

            case "Nhật ký Website":
            default:
                g.DrawEllipse(pen, 1, 1, 15, 15);
                g.DrawLine(pen, 1, 8.5f, 16, 8.5f);
                g.DrawArc(pen, 4, 1, 9, 15, 0, 360);
                break;
        }

        return bmp;
    }

    /// <summary>Tạo icon cho thanh Tabbar (Máy trạm, Tài khoản, Nhật ký...)</summary>
    public static Bitmap CreateTabIcon(string name, int w = 18, int h = 18)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        switch (name)
        {
            case "Máy trạm":
                using (var brush = new SolidBrush(Color.FromArgb(0, 150, 240)))
                {
                    g.FillRectangle(brush, 2, 2, 14, 10);
                    g.FillRectangle(Brushes.White, 3, 3, 12, 8);
                    g.FillRectangle(brush, 5, 5, 8, 4);
                }
                using (var pen = new Pen(Color.FromArgb(0, 130, 200), 1.5f))
                {
                    g.DrawLine(pen, 9, 12, 9, 15);
                    g.DrawLine(pen, 6, 15, 12, 15);
                }
                break;

            case "Tài khoản":
                using (var brush = new SolidBrush(Color.FromArgb(0, 150, 240)))
                {
                    g.FillEllipse(brush, 6, 2, 6, 6);
                    g.FillPie(brush, 3, 7, 12, 11, 180, 180);
                }
                break;

            case "Nhật ký hệ thống":
                using (var pen = new Pen(Color.FromArgb(0, 150, 240), 1.8f))
                {
                    g.DrawLines(pen, new Point[]
                    {
                        new(1, 9), new(4, 9), new(6, 4), new(9, 14), new(12, 6), new(14, 9), new(17, 9)
                    });
                }
                break;

            case "Nhật ký giao dịch":
                using (var brush = new SolidBrush(Color.FromArgb(0, 150, 240)))
                {
                    g.DrawEllipse(new Pen(brush, 1.5f), 2, 2, 14, 14);
                    using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("$", font, brush, new RectangleF(0, 0, 18, 18), sf);
                }
                break;

            case "Nhóm máy":
                using (var brush = new SolidBrush(Color.FromArgb(0, 150, 240)))
                {
                    g.FillRectangle(brush, 1, 1, 9, 7);
                    g.FillRectangle(brush, 8, 6, 9, 7);
                    g.FillRectangle(Brushes.White, 2, 2, 7, 5);
                    g.FillRectangle(Brushes.White, 9, 7, 7, 5);
                }
                break;

            case "Nhóm người dùng":
                using (var brush = new SolidBrush(Color.FromArgb(0, 150, 240)))
                {
                    g.FillEllipse(brush, 4, 3, 5, 5);
                    g.FillPie(brush, 1, 8, 11, 9, 180, 180);
                    g.FillEllipse(brush, 10, 4, 4, 4);
                    g.FillPie(brush, 7, 9, 10, 8, 180, 180);
                }
                break;

            case "Bán thẻ":
                using (var brush = new SolidBrush(Color.FromArgb(0, 150, 240)))
                {
                    // Mặt tiền kiosk / cửa hàng
                    g.FillPolygon(brush, new Point[] { new(2, 6), new(9, 2), new(16, 6) });
                    g.FillRectangle(brush, 3, 6, 12, 10);
                    g.FillRectangle(Brushes.White, 6, 9, 6, 7);
                }
                break;

            case "Quản lý ứng dụng":
                using (var brush = new SolidBrush(Color.FromArgb(0, 150, 240)))
                {
                    g.FillRectangle(brush, 2, 2, 6, 6);
                    g.FillRectangle(brush, 10, 2, 6, 6);
                    g.FillRectangle(brush, 2, 10, 6, 6);
                    g.FillRectangle(brush, 10, 10, 6, 6);
                }
                break;
        }

        return bmp;
    }

    /// <summary>Icon tìm kiếm (Kính lúp sắc nét AntiAlias chuẩn UI hiện đại)</summary>
    public static Bitmap CreateSearchIcon(int w = 16, int h = 16, Color? color = null)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var clr = color ?? Color.FromArgb(120, 135, 160);
        using var pen = new Pen(clr, 1.8f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        // Vòng tròn kính lúp
        g.DrawEllipse(pen, 2.5f, 2.5f, 7.5f, 7.5f);
        // Cán kính lúp chéo 45 độ
        g.DrawLine(pen, 8.5f, 8.5f, 13.5f, 13.5f);

        return bmp;
    }

    /// <summary>Icon tiền mặt (2 tờ tiền xanh xếp chồng chuẩn CSM)</summary>
    public static Bitmap CreateCashPaymentIcon(int w = 32, int h = 24)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Tờ tiền dưới
        using var brushDark = new SolidBrush(Color.FromArgb(20, 140, 225));
        g.FillRectangle(brushDark, 3, 2, 22, 14);

        // Tờ tiền trên
        using var brushLight = new SolidBrush(Color.FromArgb(50, 180, 255));
        g.FillRectangle(brushLight, 7, 7, 22, 14);

        // Viền và chi tiết tờ tiền trên
        using var penLine = new Pen(Color.FromArgb(20, 120, 200), 1f);
        g.DrawRectangle(penLine, 7, 7, 22, 14);
        using var penWhite = new Pen(Color.FromArgb(200, 240, 255), 1.2f);
        g.DrawEllipse(penWhite, 14, 10, 8, 8);
        using var brushWhite = new SolidBrush(Color.White);
        g.FillEllipse(brushWhite, 16.5f, 12.5f, 3f, 3f);

        return bmp;
    }

    /// <summary>Icon thẻ ngân hàng (Chuyển khoản)</summary>
    public static Bitmap CreateCardPaymentIcon(int w = 32, int h = 24)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var brushCard = new SolidBrush(Color.FromArgb(40, 170, 235));
        g.FillRectangle(brushCard, 4, 4, 24, 16);

        // Dải từ
        using var brushStrip = new SolidBrush(Color.FromArgb(20, 110, 170));
        g.FillRectangle(brushStrip, 4, 8, 24, 4);

        // Chip vàng nhỏ
        using var brushChip = new SolidBrush(Color.FromArgb(255, 215, 0));
        g.FillRectangle(brushChip, 7, 14, 5, 3.5f);

        return bmp;
    }

    /// <summary>Icon QR thanh toán</summary>
    public static Bitmap CreateQrPaymentIcon(int w = 32, int h = 32)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Ô vuông cam góc trên trái
        using var penOrange = new Pen(Color.FromArgb(240, 150, 60), 2f);
        g.DrawRectangle(penOrange, 4, 4, 8, 8);
        g.FillRectangle(new SolidBrush(Color.FromArgb(240, 150, 60)), 7, 7, 3, 3);

        // Ô vuông cam góc dưới trái
        g.DrawRectangle(penOrange, 4, 18, 8, 8);
        g.FillRectangle(new SolidBrush(Color.FromArgb(240, 150, 60)), 7, 21, 3, 3);

        // Ô vuông góc trên phải
        using var penBlue = new Pen(Color.FromArgb(100, 170, 230), 2f);
        g.DrawRectangle(penBlue, 18, 4, 8, 8);
        g.FillRectangle(new SolidBrush(Color.FromArgb(100, 170, 230)), 21, 7, 3, 3);

        // Vòng tròn tích xanh góc dưới phải
        using var brushGreen = new SolidBrush(Color.FromArgb(46, 204, 113));
        g.FillEllipse(brushGreen, 17, 17, 11, 11);
        using var penCheck = new Pen(Color.White, 1.8f);
        g.DrawLines(penCheck, [new PointF(20, 22), new PointF(22, 25), new PointF(25, 20)]);

        return bmp;
    }

    /// <summary>Icon máy in nhỏ</summary>
    public static Bitmap CreatePrinterIcon(int w = 16, int h = 16)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var brushBody = new SolidBrush(Color.FromArgb(140, 150, 165));
        // Thân máy in
        g.FillRectangle(brushBody, 2, 5, 12, 7);
        // Giấy trên
        using var brushPaper = new SolidBrush(Color.White);
        g.FillRectangle(brushPaper, 4, 2, 8, 4);
        // Giấy dưới ra
        g.FillRectangle(brushPaper, 4, 10, 8, 4);
        using var pen = new Pen(Color.FromArgb(100, 110, 125), 1f);
        g.DrawRectangle(pen, 2, 5, 12, 7);

        return bmp;
    }

    /// <summary>Icon bút chì chỉnh sửa</summary>
    public static Bitmap CreatePencilIcon(int w = 16, int h = 16)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var pen = new Pen(Color.FromArgb(0, 140, 235), 1.6f);
        g.DrawLine(pen, 3, 13, 12, 4);
        g.DrawLine(pen, 12, 4, 13, 5);
        g.DrawLine(pen, 3, 13, 2, 14);

        return bmp;
    }
}
