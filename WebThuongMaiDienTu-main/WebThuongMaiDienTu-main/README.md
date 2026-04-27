# ShopMVC

Repo này đã được chuẩn hóa theo hướng:

- chạy nhanh bằng `Docker Compose`
- phát triển lâu dài bằng `Dev Container`

Nếu bạn đổi máy thường xuyên, cách này là ổn nhất vì chỉ cần cài:

- `Docker Desktop`
- nếu code bằng VS Code: thêm `VS Code` + extension `Dev Containers`

## Cách chuẩn nhất

Có 2 chế độ dùng:

### 1. Chỉ chạy dự án

Dùng `Docker Compose`.

### 2. Code + chạy + test trong cùng môi trường

Dùng `Dev Container`.

Đây là lựa chọn nên dùng lâu dài vì không cần cài tay `.NET SDK`, `LocalDB`, `SQL Server` trên từng máy host.

## Chạy nhanh bằng Docker Compose

### Bước 1: vào thư mục solution

```powershell
cd E:\duanbanhang\WebThuongMaiDienTu-main\WebThuongMaiDienTu-main
```

### Bước 2: tạo file `.env`

```powershell
Copy-Item .env.example .env
```

Bạn có thể giữ nguyên mặc định, hoặc đổi:

- `SA_PASSWORD`
- `APP_PORT`
- `SQL_PORT`

Ví dụ:

```env
SA_PASSWORD=YourStrong!Passw0rd
APP_PORT=8080
SQL_PORT=1433
```

### Bước 3: build và chạy

```powershell
docker compose up --build
```

### Bước 4: mở trình duyệt

- App: `http://localhost:8080`
- Swagger: `http://localhost:8080/api-docs`

### Dừng dịch vụ

```powershell
docker compose down
```

Muốn xóa luôn volume database:

```powershell
docker compose down -v
```

## Phát triển bằng Dev Container

### Yêu cầu

- `Docker Desktop`
- `VS Code`
- extension `Dev Containers`

### Cách mở

1. Mở thư mục repo trong VS Code
2. Chạy lệnh: `Dev Containers: Reopen in Container`
3. Chờ container build xong

Sau khi vào container:

- workspace nằm ở `/workspace`
- SQL Server chạy cùng network nội bộ
- connection string đã được set sẵn cho môi trường dev
- `dotnet restore ShopMVC.sln` chạy tự động sau khi tạo container

### Chạy app trong Dev Container

Mở terminal trong VS Code và chạy:

```bash
dotnet run --project ShopMVC/ShopMVC.csproj --launch-profile http
```

Mặc định app sẽ chạy theo profile trong project. Nếu muốn ép URL rõ ràng:

```bash
ASPNETCORE_URLS=http://0.0.0.0:5018 dotnet run --project ShopMVC/ShopMVC.csproj --launch-profile http
```

Nếu cần forward port trong VS Code, forward port tương ứng mà terminal đang log ra.

## Cấu trúc môi trường

### Docker runtime

- `Dockerfile`: build/publish app ASP.NET Core
- `docker-compose.yml`: chạy `shopmvc` + `sqlserver`

### Dev environment

- `.devcontainer/Dockerfile`: môi trường dev có `.NET SDK 9` và `.NET SDK 10`
- `.devcontainer/docker-compose.yml`: container dev + SQL Server
- `.devcontainer/devcontainer.json`: cấu hình cho VS Code

## Tài khoản mặc định

Được seed khi app khởi động:

- Admin: `admin@shopmvc.local / Admin@123`
- Nhân viên:
  - `employee1@shopmvc.local / Employee@123`
  - `employee2@shopmvc.local / Employee@123`
  - `employee3@shopmvc.local / Employee@123`

## Lệnh hay dùng

### Docker Compose

```powershell
docker compose up --build
docker compose down
docker compose down -v
docker compose logs -f shopmvc
docker compose logs -f sqlserver
```

### Trong Dev Container

```bash
dotnet restore ShopMVC.sln
dotnet build ShopMVC.sln
dotnet test ShopMVC.sln
dotnet run --project ShopMVC/ShopMVC.csproj --launch-profile http
```

## Ghi chú kỹ thuật

- App runtime target `net9.0`
- Test project target `net10.0`
- Dev container đã cài cả SDK 9 và 10 để không lệch môi trường
- SQL Server trong Docker dùng volume riêng để giữ dữ liệu
- App tự chạy migration/seed khi startup qua `DbSeeder.SeedAsync()`

## Khi nào nên dùng cách nào

### Dùng Docker Compose nếu:

- bạn chỉ cần chạy dự án
- bạn không muốn cài SDK trên máy

### Dùng Dev Container nếu:

- bạn code thường xuyên
- bạn muốn môi trường dev giống nhau trên mọi máy
- bạn muốn giảm tối đa lỗi kiểu “máy này chạy, máy kia lỗi”

## Nếu Docker báo lỗi

### Port bị chiếm

Đổi trong `.env`:

```env
APP_PORT=8081
SQL_PORT=1434
```

Sau đó chạy lại:

```powershell
docker compose up --build
```

### SQL không lên

Xem log:

```powershell
docker compose logs -f sqlserver
```

### App không lên dù SQL đã chạy

Xem log:

```powershell
docker compose logs -f shopmvc
```

## Kết luận

Nếu chỉ cần dùng:

```powershell
docker compose up --build
```

Nếu muốn làm việc lâu dài:

- mở repo bằng `Dev Container`
- code, build, test ngay trong container
