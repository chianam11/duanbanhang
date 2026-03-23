# 🚀 ShopMVC Deployment Guide

## Production Deployment for ASP.NET Core 9.0

---

## **1. Server Requirements**

### Minimum Specifications
- **OS**: Windows Server 2019+ / Ubuntu 20.04+ / CentOS 8+
- **CPU**: 2 cores @ 2.0 GHz
- **RAM**: 4 GB minimum (8 GB recommended)
- **Storage**: 40 GB SSD
- **.NET Runtime**: .NET 9.0 Runtime

### Recommended (for 1000+ concurrent users)
- **CPU**: 4 cores @ 2.5 GHz
- **RAM**: 16 GB
- **Storage**: 100 GB SSD + separate storage for uploads

---

## **2. Database Setup (SQL Server)**

### Create Production Database
```sql
-- On Production SQL Server
CREATE DATABASE [ShopMVC_Production]
 CONTAINMENT = NONE
 ON  PRIMARY 
   ( NAME = N'ShopMVC_Production_dat', 
     FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\ShopMVC_Production.mdf' , 
     SIZE = 1GB , 
     FILEGROWTH = 256MB)
GO

USE [ShopMVC_Production]
GO

-- Create Login for application
CREATE LOGIN [shopmvc_app] WITH PASSWORD='YOUR_STRONG_PASSWORD123!@#'
GO

-- Create Database User
CREATE USER [shopmvc_app] FOR LOGIN [shopmvc_app]
GO

-- Grant Permissions
ALTER ROLE [db_owner] ADD MEMBER [shopmvc_app]
GO
```

### Connection String
```
Server=YOUR_SERVER_IP;Database=ShopMVC_Production;User Id=shopmvc_app;Password=YOUR_STRONG_PASSWORD123!@#;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
```

---

## **3. Deployment via Docker**

### Build Docker Image
```bash
# From project root
docker build -f Dockerfile -t shopmvc:1.0 .
```

### Run Container
```bash
docker run -d \
  --name shopmvc-app \
  -p 443:443 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=sql-server;Database=ShopMVC_Production;User Id=sa;Password=YOUR_PASSWORD;" \
  -v /etc/ssl/certs/certificate.pfx:/app/certificate.pfx:ro \
  shopmvc:1.0
```

### Docker Compose (Recommended)
```yaml
version: '3.8'

services:
  app:
    image: shopmvc:1.0
    ports:
      - "443:443"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=ShopMVC_Production;User Id=sa;Password=YOUR_PASSWORD;"
    depends_on:
      - sqlserver
    volumes:
      - /etc/ssl/certs/certificate.pfx:/app/certificate.pfx:ro
      - /var/log/shopmvc:/app/logs
    restart: unless-stopped

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      SA_PASSWORD: YOUR_SA_PASSWORD
      ACCEPT_EULA: Y
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    restart: unless-stopped

volumes:
  sqlserver-data:
```

---

## **4. IIS Deployment (Windows)**

### Prerequisites
- Install .NET 9.0 Hosting Bundle
- Enable Application Request Routing (ARR)

### Configuration

#### Step 1: Publish Application
```bash
cd ShopMVC
dotnet publish -c Release -o ./publish
```

#### Step 2: Create IIS Site
```powershell
# PowerShell (Admin)
New-Website -Name "ShopMVC" `
  -PhysicalPath "C:\inetpub\ShopMVC" `
  -HostHeader "yourdomain.com" `
  -Port 443 `
  -Protocol https `
  -SslFlags 0
```

#### Step 3: Configure Application Pool
```powershell
# Set to "No Managed Code"
Set-ItemProperty "IIS:\AppPools\ShopMVC" -Name "RuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\ShopMVC" -Name "ManagedRuntimeVersion" -Value ""
```

#### Step 4: SSL Certificate
- Import certificate in IIS Server Certificates
- Bind to HTTPS site

---

## **5. Linux (Ubuntu) Deployment**

### Install .NET Runtime
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
sudo chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0

# Add to PATH
echo 'export PATH=$PATH:/home/user/.dotnet' >> ~/.bashrc
source ~/.bashrc
```

### Create Systemd Service
```bash
sudo nano /etc/systemd/system/shopmvc.service
```

```ini
[Unit]
Description=ShopMVC E-Commerce Platform
After=network.target

[Service]
Type=notify
User=shopmvc
WorkingDirectory=/var/www/shopmvc
ExecStart=/home/user/.dotnet/dotnet /var/www/shopmvc/ShopMVC.dll
Restart=on-failure
RestartSec=10
StandardOutput=journal
StandardError=journal
SyslogIdentifier=shopmvc

[Install]
WantedBy=multi-user.target
```

### Start Service
```bash
sudo systemctl daemon-reload
sudo systemctl enable shopmvc.service
sudo systemctl start shopmvc.service
sudo systemctl status shopmvc.service
```

---

## **6. Nginx Configuration (Reverse Proxy)**

```nginx
upstream shopmvc_backend {
    server localhost:5000;
    server localhost:5001;
    keepalive 32;
}

server {
    listen 80;
    listen [::]:80;
    server_name yourdomain.com www.yourdomain.com;
    
    # Redirect to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;

    # SSL Configuration
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    # Proxy Configuration
    location / {
        proxy_pass http://shopmvc_backend;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_buffer_size 128k;
        proxy_buffers 4 256k;
        proxy_busy_buffers_size 256k;
        proxy_cache_bypass $http_upgrade;
    }

    # WebSocket Support (for SignalR)
    location /chat {
        proxy_pass http://shopmvc_backend;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }

    # Static Files
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, max-age=31536000";
    }
}
```

---

## **7. SSL Certificate Setup**

### Using Let's Encrypt (Free)
```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx

# Get Certificate
sudo certbot certonly --nginx -d yourdomain.com -d www.yourdomain.com

# Auto-renewal
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer
```

### Using selbst-signed (Testing Only)
```bash
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes
```

---

## **8. Environmental Variables**

Create `.env` file or export variables:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Server=...;Database=ShopMVC_Production;User Id=sa;Password=..."
export Kestrel__Endpoints__Https__Certificate__Path=/etc/ssl/certs/certificate.pfx
export Kestrel__Endpoints__Https__Certificate__Password=YOUR_PASSWORD
export Logging__LogLevel__Default=Information
```

---

## **9. Monitoring & Logging**

### Application Insights (Azure)
```json
// appsettings.Production.json
{
  "ApplicationInsights": {
    "InstrumentationKey": "YOUR_KEY_HERE"
  }
}
```

### Log File Location
- **Windows**: `C:\Logs\ShopMVC\production-2024-03-17.txt`
- **Linux**: `/var/log/shopmvc/production-2024-03-17.txt`

### View Logs
```bash
# Linux
sudo tail -f /var/log/shopmvc/production-*.txt

# Windows
Get-Content C:\Logs\ShopMVC\production-*.txt -Tail 100
```

---

## **10. Performance Tuning**

### Application Configuration
```csharp
// Program.cs
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100MB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddResponseCaching();
builder.Services.AddHttpCacheHeaders(); // Requires package
```

### Database Optimization
```sql
-- Create Indexes
CREATE INDEX idx_donhang_userid ON DonHang(UserId);
CREATE INDEX idx_donhang_trangthai ON DonHang(TrangThai);
CREATE INDEX idx_sanpham_danhmuc ON SanPham(IdDanhMuc);
CREATE INDEX idx_sanpham_tonkho ON SanPham(TonKho);
```

---

## **11. Backup Strategy**

### Automated SQL Server Backups
```sql
-- Full backup daily
BACKUP DATABASE [ShopMVC_Production]
TO DISK = N'\\backup-server\backups\ShopMVC_Full_$(DATE).bak'
WITH COMPRESSION, INIT

-- Transaction log backup hourly
BACKUP LOG [ShopMVC_Production]
TO DISK = N'\\backup-server\backups\ShopMVC_Log_$(TIMESTAMP).trn'
WITH COMPRESSION
```

### Upload Backups
```bash
# Linux cron job
0 2 * * * /usr/bin/mysqldump --all-databases | gzip > /backups/db_$(date +\%Y\%m\%d).sql.gz
0 */6 * * * aws s3 sync /backups/ s3://your-backup-bucket/
```

---

## **12. Health Check & Monitoring**

### Health Check Endpoint
```
GET http://yourdomain.com/health
```

### Monitor Database Connection
```bash
curl -X GET http://yourdomain.com/api/health/db -H "Authorization: Bearer YOUR_TOKEN"
```

---

## **Troubleshooting**

### Application won't start
```bash
# Check logs
sudo journalctl -u shopmvc.service -n 100

# Test connection
dotnet /var/www/shopmvc/ShopMVC.dll
```

### Database connection failed
```bash
# Test SQL Server connectivity
telnet sql-server 1433
```

### High memory usage
```bash
# Restart application
sudo systemctl restart shopmvc.service

# Check garbage collection
dotnet /app/ShopMVC.dll --gc-server=true
```

---

## **Support & Contact**

For deployment issues, contact: support@shopmvc.local
