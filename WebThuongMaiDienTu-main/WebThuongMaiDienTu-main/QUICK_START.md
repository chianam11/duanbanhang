# Quick Start Guide - ShopMVC

Bắt đầu nhanh trong 5 phút!

## ⚡ Installation (Windows)

### 1. Prerequisites
```bash
# Check .NET version
dotnet --version
# Should be 9.0 or higher

# Check SQL Server LocalDB
sqllocaldb info
# Should show: MSSQLLocalDB
```

### 2. Clone & Setup
```bash
git clone https://github.com/your-username/ShopMVC.git
cd ShopMVC/WebThuongMaiDienTu-main/WebThuongMaiDienTu-main/ShopMVC
```

### 3. Run Application
```bash
# Restore packages
dotnet restore

# Apply migrations
dotnet ef database update

# Run server
dotnet run
```

👉 Open browser: http://localhost:5018

---

## 🔐 Default Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@shopmvc.com | Admin@123 |
| Staff | staff@shopmvc.com | Staff123 |
| User | user@shopmvc.com | User123 |

---

## 🗺️ Project Navigation

### Important Files
- `Program.cs` - Application entry point
- `appsettings.json` - Configuration
- `Data/AppDbContext.cs` - Database context
- `Data/DbSeeder.cs` - Seed data

### Important Folders
```
ShopMVC/
├── Controllers/         ← Create new controllers here
├── Controllers/Api/     ← Create new API endpoints here
├── Services/            ← Business logic
├── Models/              ← Data models
├── Views/               ← HTML templates
└── wwwroot/             ← CSS, JS, images
```

---

## 🔨 Common Commands

```bash
# Build
dotnet build

# Run
dotnet run

# Create migration
dotnet ef migrations add YourMigrationName

# Update database
dotnet ef database update

# Run tests
dotnet test

# Clean build
dotnet clean
```

---

## 🚀 Key Features to Try

1. **Browse Products**
   - http://localhost:5018/ → View products

2. **Add to Cart**
   - Click "Add to Cart" button

3. **Admin Dashboard**
   - Login with admin account
   - http://localhost:5018/admin

4. **Create API Request**
   - GET http://localhost:5018/api/products
   - Use Postman or cURL

---

## ⚙️ Configuration

### Database Connection
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ShopMVC;Trusted_Connection=True;"
  }
}
```

### Email (Optional)
```json
{
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

---

## 🐛 Troubleshooting

### Error: Cannot connect to database
```bash
# Start LocalDB
sqllocaldb start MSSQLLocalDB

# Check connection
dotnet ef database update
```

### Port 5018 already in use
```bash
# Change port in: Properties/launchSettings.json
# Change "applicationUrl" value
```

### Build failed
```bash
# Clean and rebuild
dotnet clean
dotnet restore  
dotnet build
```

---

## 📚 Next Steps

1. ✅ Read [README.md](README.md)
2. ✅ Read [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md)
3. ✅ Check [FEATURES_SUMMARY.md](FEATURES_SUMMARY.md)
4. ✅ Explore code in `Controllers/` and `Services/`
5. ✅ Run tests: `dotnet test`

---

## 🎯 Learning Path

- **Week 1-2**: Understand project structure
- **Week 3-4**: Add new controller/service
- **Week 5**: Write unit tests
- **Week 6**: Deploy with Docker
- **Week 7-8**: Complete capstone project

---

## 💡 Tips

- Use Visual Studio / VS Code for development
- Set breakpoints and debug with F5
- Check `logs/` folder for application logs
- Use Postman for API testing
- Always write tests for new features

---

**Happy Coding! 🚀**