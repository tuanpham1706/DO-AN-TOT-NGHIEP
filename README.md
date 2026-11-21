# ⚡ V-T3 CyberZone — Hệ thống Quản lý Phòng Net

> Đồ án môn học — Hệ thống quản lý phòng máy tích hợp F&B

## 🚀 Công nghệ

| Layer | Công nghệ |
|---|---|
| Desktop App | C# WinForms .NET 10 |
| Backend API | ASP.NET Core 10 + SignalR |
| Database | MySQL 8 + Entity Framework Core |
| Web nhúng | HTML/CSS/JS + WebView2 |

## 📁 Cấu trúc

```
V-T3 CyberZone/
├── src/
│   ├── VT3CyberZone.Server/    ← Backend API + SignalR Hub
│   └── VT3CyberZone.Desktop/   ← WinForms Client App
├── vt3_cyberzone_db.sql         ← Database schema + seed data
└── VT3CyberZone.sln
```

## ⚙️ Cài đặt

### 1. Database

> **Lưu ý:** File SQL có lệnh xóa và tạo lại `vt3_cyberzone_db`. Chỉ chạy trên database mới hoặc khi bạn chắc chắn muốn xóa dữ liệu cũ.

```bash
mysql -u root -p < vt3_cyberzone_db.sql
```

### 2. Chạy server cho mạng LAN nhỏ (3–4 máy)

Chọn một máy làm server, cài .NET 10 SDK và MySQL 8 trên máy đó. Các máy client và server cần kết nối cùng mạng LAN; chỉ máy server cần kết nối MySQL.

Mở PowerShell tại thư mục dự án, cấu hình chuỗi kết nối và khóa ký JWT bằng User Secrets (không đưa mật khẩu hoặc khóa vào Git):

```powershell
cd src\VT3CyberZone.Server
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=vt3_cyberzone_db;User=root;Password=THAY_BANG_MAT_KHAU_MYSQL"
$bytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)
dotnet user-secrets set "Jwt:Key" $key
dotnet run --launch-profile http
```

Server lắng nghe trên `http://0.0.0.0:5121`. Trên máy server, chạy `ipconfig` để tìm IPv4 LAN, ví dụ `192.168.1.10`. Các máy khác truy cập API tại `http://192.168.1.10:5121`; kiểm tra kết nối database bằng `http://192.168.1.10:5121/health` (trả về `{"status":"ok"}` khi kết nối được).

Sau khi đã cài MySQL và tạo database, khởi động server bằng cách nhấp đúp `Chay-Server-LAN.bat` ở thư mục gốc dự án. Nhập mật khẩu MySQL khi được hỏi; mật khẩu được nhập ẩn và không lưu vào file dự án. Cửa sổ lệnh cần được giữ mở khi đang sử dụng server.

#### Cổng quản trị nền tảng và đăng ký quán

Tạo tài khoản quản trị nền tảng riêng (không dùng tài khoản `admin_vt3` của quán). Cấu hình hai giá trị trước khi chạy server; ở lần khởi động tiếp theo server sẽ tạo tài khoản nếu chưa có quản trị viên nền tảng:

```powershell
cd src\VT3CyberZone.Server
dotnet user-secrets set "PlatformAdmin:Username" "platform_admin"
dotnet user-secrets set "PlatformAdmin:Password" "THAY_BANG_MAT_KHAU_MANH"
```

Sau khi chạy server, quản trị nền tảng đăng nhập tại `http://localhost:5121/admin`. Chủ quán đăng ký tại `http://localhost:5121/shop` hoặc từ liên kết đăng ký ở màn hình đăng nhập desktop; tài khoản quán chỉ dùng được sau khi được duyệt. Dashboard nền tảng hiển thị danh sách quán, lượt tải/kích hoạt và các khoản phí thuê bao do quản trị viên ghi nhận. Doanh thu bán hàng và khách hàng của quán chỉ hiển thị trong desktop app đã đăng nhập vào quán đó.

Để phục vụ bộ cài từ cổng `/shop`, cấu hình đường dẫn đến bộ cài đã publish trên server. Nếu chưa cấu hình, chức năng tải app sẽ báo rằng bộ cài chưa sẵn sàng:

```powershell
dotnet user-secrets set "ClientPackage:Path" "C:\VT3\Releases\VT3CyberZone-Setup.exe"
dotnet user-secrets set "ClientPackage:Version" "1.0.0"
```

Việc nâng cấp database cũ được thực hiện khi server khởi động và giữ lại dữ liệu hiện có; không chạy lại file SQL khởi tạo trên database đang dùng.

### 3. Dùng ứng dụng trên các máy khách

Ứng dụng khách gọi API trên máy server; không kết nối MySQL trực tiếp và không cần cài MySQL trên từng máy. Trước khi chạy desktop, mở `src\VT3CyberZone.Desktop\ServerAddress.txt` và đặt địa chỉ LAN của máy server, ví dụ `http://192.168.1.40:5121`. Build ứng dụng desktop; `ServerAddress.txt` sẽ được chép cạnh file chạy. Đăng nhập bằng tài khoản nhân viên, mở tab hội viên và chọn **Thêm hội viên** để lưu tài khoản mới lên database server. Có thể tìm hội viên, nạp tiền từ máy thu ngân; máy khách khác bấm **Làm mới** để tải dữ liệu mới nhất.

Database mẫu hiện có mật khẩu đăng nhập `123456`. Nếu database đã được tạo từ bản SQL cũ, chạy `CapNhat-MatKhau-Demo.bat` một lần rồi nhập mật khẩu MySQL root để sửa hash sai của các tài khoản demo. Không chạy lại file SQL khởi tạo chỉ để sửa mật khẩu vì file đó xóa và tạo lại toàn bộ database.

Nếu Windows Firewall chặn kết nối, mở PowerShell **Run as administrator** trên máy server và chỉ cho phép cổng API trong mạng Private:

```powershell
New-NetFirewallRule -DisplayName "VT3 CyberZone API" -Direction Inbound -Protocol TCP -LocalPort 5121 -Action Allow -Profile Private
```

Không cần mở cổng MySQL cho các máy client. Khi triển khai ngoài môi trường demo, dùng HTTPS và đặt khóa JWT/mật khẩu DB trong biến môi trường hoặc kho bí mật phù hợp.

### 4. Chạy Desktop App
```bash
cd src/VT3CyberZone.Desktop
dotnet run
```

> **Tình trạng tích hợp:** Đăng nhập, đăng ký quán, kích hoạt quán đã duyệt, quản lý hội viên (danh sách/tìm kiếm/tạo/nạp tiền), danh sách máy trạm và mở/đóng phiên đã kết nối server. Tab **Nhật ký giao dịch** tải dữ liệu từ `GET /api/transactions`; báo cáo doanh thu dùng `GET /api/reports/revenue` và tính doanh thu từ giao dịch tiền giờ/dịch vụ, không tính nạp số dư là doanh thu. Đơn hàng F&B và Client Agent/heartbeat vẫn cần được hoàn thiện để ghi nhận dữ liệu vận hành đầy đủ.

## 🔑 Tài khoản demo

| Username | Password | Role |
|---|---|---|
| admin_vt3 | 123456 | ADMIN |
| thungan_ca1 | 123456 | STAFF |
| gamer_pro | 123456 | MEMBER |
