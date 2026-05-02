# ShopMVC ERD

ERD ben duoi duoc rut ra tu `AppDbContext` va cac model hien tai.

- Mo file nay trong VS Code/GitHub neu renderer cua ban ho tro Mermaid.
- Neu can export anh, copy block `mermaid` vao `https://mermaid.live`.
- So do tap trung vao bang nghiep vu chinh. Cac bang Identity chi gom `AspNetUsers` de de nhin.

```mermaid
erDiagram
    AspNetUsers {
        string Id PK
        string UserName
        string Email
        string HoTen
        string DiaChiMacDinh
    }

    DanhMucs {
        int Id PK
        string Ten
        string Slug
        string MoTa
        int DanhMucChaId FK
        int ThuTu
        bool HienThi
        string IconUrl
    }

    ThuongHieus {
        int Id PK
        string Ten
        string MoTa
        string LogoUrl
        string Slug
        bool HienThi
    }

    SanPhams {
        int Id PK
        string Ten
        string DisplaySuffix
        string MoTaNgan
        string MoTaChiTiet
        decimal Gia
        decimal GiaKhuyenMai
        int TonKho
        bool LaNoiBat
        int TrangThai
        datetime NgayTao
        datetime NgayCapNhat
        int ParentId FK
        string Mau
        string ThuocTinh2
        string SKU
        bool IsActive
        int IdDanhMuc FK
        int IdThuongHieu FK
    }

    AnhSanPhams {
        int Id PK
        int IdSanPham FK
        string Url
        bool LaAnhChinh
        int ThuTu
    }

    ChiTietSanPhams {
        int Id PK
        int SanPhamId FK
        string TenChiTiet
        decimal Gia
        int SoLuongKho
    }

    Vouchers {
        int Id PK
        string Code
        string Ten
        double PhanTramGiam
        decimal GiamToiDa
        decimal GiamTrucTiep
        datetime NgayBatDau
        datetime NgayHetHan
        int SoLanSuDungToiDa
        int SoLanDaSuDung
        bool IsActive
        bool IsFlashSale
    }

    VoucherThuongHieus {
        int VoucherId PK, FK
        int ThuongHieuId PK, FK
    }

    VoucherDanhMucs {
        int VoucherId PK, FK
        int DanhMucId PK, FK
    }

    VoucherSanPhams {
        int VoucherId PK, FK
        int SanPhamId PK, FK
        decimal GiaGiam
        int SoLuongPhanBo
        int SoLuongDaBan
    }

    DonHangs {
        int Id PK
        string MaDon
        string UserId FK
        string HoTenNhan
        string DienThoaiNhan
        string DiaChiNhan
        decimal PhiVanChuyen
        decimal TienGiam
        decimal TongTruocGiam
        decimal TongThanhToan
        int PhuongThucThanhToan
        int TrangThai
        datetime NgayDat
        datetime NgayCapNhat
        int VoucherId FK
        string VoucherCode
    }

    DonHangChiTiets {
        int Id PK
        int IdDonHang FK
        int IdSanPham FK
        int SoLuong
        decimal DonGia
        decimal ThanhTien
    }

    OrderStatusHistories {
        int Id PK
        int OrderId FK
        int FromStatus
        int ToStatus
        string ReasonCode
        string Note
        string ChangedByUserId
        datetime ChangedAtUtc
        string MetadataJson
        bool IsOverride
    }

    DonHangNotes {
        int Id PK
        int MaDH FK
        string NoiDung
        string CreatedBy
        datetime CreatedAt
    }

    DonHangLogs {
        int Id PK
        int MaDH FK
        int From
        int To
        string ByUser
        datetime At
    }

    DanhGias {
        int Id PK
        int IdSanPham FK
        int IdDonHang FK
        string UserId FK
        int SoSao
        string NoiDung
        string HinhAnh
        bool HienThiTen
        datetime NgayTao
        int TrangThai
        bool LaNoiBat
    }

    ChatSessions {
        int Id PK
        string UserConnectionId
        string UserId FK
        datetime ThoiGianTao
        bool DaDong
        int SanPhamId FK
    }

    ChatMessages {
        int Id PK
        int ChatSessionId FK
        int Sender
        string NoiDung
        datetime ThoiGian
    }

    Banners {
        int Id PK
        string TenBanner
        string HinhAnh
        int ThuTu
        bool HienThi
    }

    SystemSettings {
        int Id PK
        string SettingKey
        string SettingValue
        datetime UpdatedAt
    }

    DanhMucs ||--o{ SanPhams : phan_loai
    ThuongHieus ||--o{ SanPhams : thuoc_thuong_hieu
    SanPhams ||--o{ AnhSanPhams : co_anh
    SanPhams ||--o{ ChiTietSanPhams : co_bien_the
    SanPhams ||--o{ SanPhams : parent_child
    DanhMucs ||--o{ DanhMucs : parent_child

    AspNetUsers ||--o{ DonHangs : dat_hang
    Vouchers o|--o{ DonHangs : ap_dung
    DonHangs ||--|{ DonHangChiTiets : gom
    SanPhams ||--o{ DonHangChiTiets : duoc_mua
    DonHangs ||--o{ OrderStatusHistories : lich_su_trang_thai
    DonHangs ||--o{ DonHangNotes : ghi_chu
    DonHangs ||--o{ DonHangLogs : log

    Vouchers ||--o{ VoucherThuongHieus : map
    ThuongHieus ||--o{ VoucherThuongHieus : map
    Vouchers ||--o{ VoucherDanhMucs : map
    DanhMucs ||--o{ VoucherDanhMucs : map
    Vouchers ||--o{ VoucherSanPhams : flash_sale
    SanPhams ||--o{ VoucherSanPhams : flash_sale

    SanPhams ||--o{ DanhGias : duoc_danh_gia
    DonHangs ||--o{ DanhGias : nguon_don_hang
    AspNetUsers ||--o{ DanhGias : viet_danh_gia

    AspNetUsers ||--o{ ChatSessions : mo_chat
    SanPhams o|--o{ ChatSessions : context_san_pham
    ChatSessions ||--|{ ChatMessages : gom_tin_nhan
```

## Nhom bang chinh

- Danh muc / thuong hieu / san pham: `DanhMucs`, `ThuongHieus`, `SanPhams`, `AnhSanPhams`, `ChiTietSanPhams`
- Ban hang: `DonHangs`, `DonHangChiTiets`, `OrderStatusHistories`, `DonHangNotes`, `DonHangLogs`
- Khuyen mai: `Vouchers`, `VoucherThuongHieus`, `VoucherDanhMucs`, `VoucherSanPhams`
- Trai nghiem nguoi dung: `DanhGias`, `ChatSessions`, `ChatMessages`, `Banners`
- Cau hinh he thong: `SystemSettings`

## Neu muon chup anh dep nhanh

1. Mo file nay trong Markdown preview neu editor render Mermaid.
2. Hoac copy sang `https://mermaid.live`.
3. Chon `Actions` -> `Download PNG` / `Download SVG`.
