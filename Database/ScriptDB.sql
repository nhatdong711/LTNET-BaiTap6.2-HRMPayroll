
IF DB_ID(N'HRMDB') IS NULL
    CREATE DATABASE HRMDB;
GO
USE HRMDB;
GO

CREATE TABLE PhongBan (
    MaPB  INT IDENTITY(1,1) PRIMARY KEY,
    TenPB NVARCHAR(100) NOT NULL,
    Mota  NVARCHAR(255)
);

CREATE TABLE NhanVien (
    MaNV        INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    NgaySinh    DATETIME,
    CCCD        VARCHAR(20) UNIQUE,
    MaPB        INT FOREIGN KEY REFERENCES PhongBan(MaPB),
    LuongCoBan  DECIMAL(18,2) NOT NULL CHECK (LuongCoBan > 0),
    HeSoLuong   FLOAT NOT NULL DEFAULT 1.0
);

CREATE TABLE BangChamCong (
    MaCong     INT IDENTITY(1,1) PRIMARY KEY,
    MaNV       INT NOT NULL FOREIGN KEY REFERENCES NhanVien(MaNV),
    ThangNam   VARCHAR(7) NOT NULL,                 -- ví dụ '11/2026'
    SoNgayCong FLOAT NOT NULL DEFAULT 0 CHECK (SoNgayCong >= 0 AND SoNgayCong <= 31),
    SoGioOT    FLOAT NOT NULL DEFAULT 0 CHECK (SoGioOT >= 0),
    DaChot     BIT NOT NULL DEFAULT 0,             -- 1 = đã chốt lương, khóa chấm công
    CONSTRAINT UQ_ChamCong UNIQUE (MaNV, ThangNam)
);

CREATE TABLE BangLuong (
    MaLuong  INT IDENTITY(1,1) PRIMARY KEY,
    MaNV     INT NOT NULL FOREIGN KEY REFERENCES NhanVien(MaNV),
    ThangNam VARCHAR(7) NOT NULL,
    ThucLinh DECIMAL(18,2) NOT NULL,
    CONSTRAINT UQ_BangLuong UNIQUE (MaNV, ThangNam)
);
GO

INSERT INTO PhongBan (TenPB, Mota) VALUES
(N'Phòng Kỹ thuật',   N'Phát triển và vận hành hệ thống'),
(N'Phòng Kinh doanh', N'Bán hàng, chăm sóc khách hàng'),
(N'Phòng Kế toán',    N'Tài chính, tiền lương'),
(N'Phòng Nhân sự',    N'Tuyển dụng, đào tạo');

INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) VALUES
(N'Nguyễn Văn Hùng',  '1990-03-12', '001090012345', 1, 12000000, 2.34),
(N'Trần Thị Mai',     '1995-07-25', '001195023456', 1, 10000000, 1.86),
(N'Lê Quốc Bảo',      '1998-11-02', '001098034567', 1,  9000000, 1.00),
(N'Phạm Ngọc Lan',    '1993-01-18', '001193045678', 2,  8500000, 1.67),
(N'Hoàng Minh Tuấn',  '1996-05-30', '001096056789', 2,  8000000, 1.00),
(N'Đỗ Thu Trang',     '1992-09-09', '001192067890', 3, 11000000, 2.10),
(N'Vũ Đức Anh',       '1997-12-21', '001097078901', 3,  8000000, 1.00),
(N'Bùi Hải Yến',      '1994-04-14', '001194089012', 4,  9500000, 1.50);

INSERT INTO BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT, DaChot) VALUES
(1, '10/2026', 22, 10, 1), (2, '10/2026', 21, 4, 1), (3, '10/2026', 22, 0, 1), (4, '10/2026', 20, 6, 1),
(5, '10/2026', 22, 12, 1), (6, '10/2026', 22, 2, 1), (7, '10/2026', 19, 0, 1), (8, '10/2026', 21.5, 3, 1);

INSERT INTO BangLuong (MaNV, ThangNam, ThucLinh)
SELECT cc.MaNV, cc.ThangNam,
       CAST(ROUND((nv.LuongCoBan * nv.HeSoLuong / 22) * cc.SoNgayCong + cc.SoGioOT * 50000, 0) AS DECIMAL(18,2))
FROM BangChamCong cc JOIN NhanVien nv ON cc.MaNV = nv.MaNV
WHERE cc.ThangNam = '10/2026';
GO
