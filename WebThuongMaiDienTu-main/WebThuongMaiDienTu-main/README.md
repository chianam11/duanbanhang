# ShopMVC

README này dành cho 2 mục đích:

1. giúp người mới kéo dự án về máy khác biết chính xác cần cài gì
2. giúp AI assistant đọc repo và kiểm tra môi trường đúng trước khi chạy

## Nếu bạn là AI assistant

Khi mở repo này trên máy mới, hãy làm theo thứ tự sau trước khi cố chạy dự án:

1. Xác nhận đang mở đúng thư mục chứa `ShopMVC.sln`
2. Kiểm tra `.NET SDK 9` và `.NET SDK 10`
3. Kiểm tra SQL Server LocalDB nếu chạy local trên Windows
4. Nếu không có LocalDB, đề xuất dùng Docker
5. Chỉ sau đó mới chạy `dotnet restore`, `dotnet build`, `dotnet run`

Các lệnh kiểm tra nên chạy:

```powershell
dotnet --list-sdks
dotnet --list-runtimes
sqllocaldb info
docker --version
docker compose version
```

Nếu thiếu gì, hãy báo người dùng cài đúng các thành phần trong mục `Cần cài trên máy mới`.

## Tổng quan nhanh

- Tên dự án: `ShopMVC`
- Stack chính: `ASP.NET Core MVC`, `Entity Framework Core`, `ASP.NET Identity`, `SignalR`, `Serilog`
- App web: target `net9.0`
- Test project: target `net10.0`
- Database mặc định khi chạy local: `SQL Server LocalDB`
- Database khi chạy Docker: `SQL Server 2022 container`
- App tự chạy migration và seed dữ liệu khi startup qua `DbSeeder.SeedAsync()`

## Cần cài trên máy mới

### Bắt buộc nếu muốn chạy đầy đủ solution

1. `.NET SDK 9.x`
2. `.NET SDK 10.x`
3. `SQL Server LocalDB` hoặc `SQL Server Express`

Lý do:

- project web `ShopMVC` target `net9.0`
- project test `ShopMVC.Tests` target `net10.0`
- cấu hình mặc định trong `appsettings.json` dùng `Server=(localdb)\MSSQLLocalDB`

### Bắt buộc nếu muốn chạy bằng Docker

1. `Docker Desktop`
2. bật `docker compose`

### Khuyến nghị cho dev trên Windows

1. `Visual Studio 2022` với workload `ASP.NET and web development`
2. hoặc `VS Code` + `C# Dev Kit`
3. `Git`

## Kiểm tra môi trường sau khi cài

### .NET

```powershell
dotnet --list-sdks
```

Kỳ vọng có cả dòng `9.x` và `10.x`.

### LocalDB

```powershell
sqllocaldb info
```

Kỳ vọng thấy instance `MSSQLLocalDB`.

Nếu chưa chạy:

```powershell
sqllocaldb start MSSQLLocalDB
```

### Docker

```powershell
docker --version
docker compose version
```

## Cấu trúc repo quan trọng

```text
ShopMVC.sln
ShopMVC/
  Program.cs
  appsettings.json
  Data/
    AppDbContext.cs
    DbSeeder.cs
  Hubs/
  Controllers/
  Areas/
  wwwroot/
ShopMVC.Tests/
```

Các file cần đọc đầu tiên:

- `ShopMVC/Program.cs`
- `ShopMVC/appsettings.json`
- `ShopMVC/Data/AppDbContext.cs`
- `ShopMVC/Data/DbSeeder.cs`
- `docker-compose.yml`

## Chạy local trên Windows

### Bước 1: restore

```powershell
dotnet restore ShopMVC.sln
```

### Bước 2: build

```powershell
dotnet build ShopMVC.sln
```

### Bước 3: chạy LocalDB

```powershell
sqllocaldb start MSSQLLocalDB
```

### Bước 4: chạy web app

```powershell
dotnet run --project ShopMVC\ShopMVC.csproj --launch-profile http
```

App mặc định chạy tại:

- `http://localhost:5018`

Swagger UI:

- `http://localhost:5018/api-docs`

## Chạy bằng Docker

```powershell
docker compose up --build
```

Service mặc định:

- web app: `http://localhost:8080`
- SQL Server: `localhost:1433`

Lưu ý:

- `docker-compose.yml` đang dùng password mẫu cho SQL Server
- nếu đổi password thì phải đổi đồng thời trong biến `SA_PASSWORD` và `ConnectionStrings__DefaultConnection`

## Test

```powershell
dotnet test ShopMVC.sln
```

Vì test project target `net10.0`, máy thiếu `.NET SDK 10` sẽ lỗi build test.

## Tài khoản seed mặc định

Được tạo trong `DbSeeder` và `Program.cs` khi app startup:

- Admin: `admin@shopmvc.local / Admin@123`
- Nhân viên:
  - `employee1@shopmvc.local / Employee@123`
  - `employee2@shopmvc.local / Employee@123`
  - `employee3@shopmvc.local / Employee@123`

## Những điểm kỹ thuật quan trọng

- App dùng `UseSqlServer(...)`, không chạy bằng SQLite theo cấu hình mặc định
- Database migration được gọi tự động lúc startup
- Dữ liệu mẫu được seed tự động lúc startup
- SignalR hub được map tại `/chatHub`
- launch profile HTTP dùng port `5018`
- Docker map port `8080 -> 80`

## Lỗi thường gặp

### 1. Thiếu .NET 9 hoặc .NET 10

Triệu chứng:

- `The current .NET SDK does not support targeting...`
- `It was not possible to find any compatible framework version`

Cách xử lý:

- cài `.NET SDK 9.x`
- cài `.NET SDK 10.x`

### 2. Không có LocalDB

Triệu chứng:

- lỗi kết nối tới `(localdb)\MSSQLLocalDB`

Cách xử lý:

- cài `SQL Server LocalDB`
- hoặc đổi sang SQL Server/connection string khác
- hoặc chạy bằng Docker

### 3. Port 5018 đã bị chiếm

Triệu chứng:

- `Failed to bind to address http://127.0.0.1:5018`

Cách xử lý:

```powershell
netstat -ano | findstr :5018
taskkill /PID <PID> /F
```

Hoặc đổi port trong:

- `ShopMVC/Properties/launchSettings.json`

### 4. Build được nhưng web không lên

Hãy kiểm tra:

1. LocalDB đã start chưa
2. connection string trong `ShopMVC/appsettings.json` có đúng không
3. log trong thư mục `ShopMVC/logs/`

### 5. Docker lên nhưng app không kết nối được DB

Hãy kiểm tra:

1. container `sqlserver` đã healthy chưa
2. password trong `docker-compose.yml` có khớp không
3. app đang dùng đúng connection string `Server=sqlserver;...`

## Lệnh nhanh nên dùng

```powershell
dotnet restore ShopMVC.sln
dotnet build ShopMVC.sln
dotnet run --project ShopMVC\ShopMVC.csproj --launch-profile http
dotnet test ShopMVC.sln
sqllocaldb start MSSQLLocalDB
docker compose up --build
```

## Gợi ý workflow cho AI trên máy mới

Nếu AI cần hỗ trợ bootstrap repo này, trình tự tốt nhất là:

1. đọc `README.md`
2. kiểm tra `dotnet --list-sdks`
3. kiểm tra `sqllocaldb info`
4. đọc `ShopMVC/Program.cs`
5. đọc `ShopMVC/appsettings.json`
6. chạy `dotnet restore`
7. chạy `dotnet build`
8. chạy app hoặc `docker compose up`

## Tài liệu liên quan

- `DEVELOPMENT_GUIDE.md`
- `FEATURES_SUMMARY.md`
- `QUICK_START.md`
- `DEPLOYMENT.md`
