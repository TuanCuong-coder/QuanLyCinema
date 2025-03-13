CREATE DATABASE QLCinema;

go
USE QLCinema;

--bảng thể loại phim
CREATE TABLE theloai (
    id INT IDENTITY(1,1) PRIMARY KEY,       
    ten NVARCHAR(255) NOT NULL                 
);

--bảng người dùng
CREATE TABLE nguoidung (
    id INT IDENTITY(1,1) PRIMARY KEY,    
    username NVARCHAR(255) NOT NULL,           
    password NVARCHAR(255) NOT NULL,             
    email NVARCHAR(255) NOT NULL,              
    sodienthoai NVARCHAR(20) NOT NULL          
);

--bảng hóa đơn
CREATE TABLE hoadon (
    id INT IDENTITY(1,1) PRIMARY KEY,        
    userid INT NOT NULL,                  
    ngaytao DATETIME NOT NULL,               
    trangthai INT DEFAULT 0,                   -- Trạng thái hóa đơn (0: Chưa thanh toán, 1: Đã thanh toán)
    FOREIGN KEY (userid) REFERENCES nguoidung(id)
);

--bảng phim
CREATE TABLE phim (
    id INT IDENTITY(1,1) PRIMARY KEY,         
    tenphim NVARCHAR(255) NOT NULL,              
    theloaiid INT NOT NULL,                    
    thoigian DATETIME NOT NULL,               
    giave DECIMAL(10, 2) NOT NULL DEFAULT 100000, -- Giá vé của phim
    mota TEXT,                                
    hinhanh NVARCHAR(555),                    
    FOREIGN KEY (theloaiid) REFERENCES theloai(id) 
);

-- bảng ghế
CREATE TABLE ghe (
    id INT IDENTITY(1,1) PRIMARY KEY,        
    phimid INT NOT NULL,                       
    vitri nvarchar (50) not null,             
    trangthai INT DEFAULT 0,                   -- Trạng thái ghế (0: Trống, 1: Đang giữ, 2: Đã đặt)
    hoadonid INT NULL,                        
	thoigianGiu datetime ,
    FOREIGN KEY (phimid) REFERENCES phim(id), 
    FOREIGN KEY (hoadonid) REFERENCES hoadon(id) 
);

-- Trigger thêm ghế tự động khi tạo phim
go
-- Tạo lại trigger
CREATE TRIGGER ghe_mac_dinh_sau_phim
ON phim
AFTER INSERT
AS
BEGIN
    DECLARE @phimid INT;
    DECLARE @i INT = 1;
    
    -- Duyệt qua tất cả các phim được chèn vào bảng
    DECLARE phim_cursor CURSOR FOR 
    SELECT id FROM inserted;
    
    OPEN phim_cursor;
    FETCH NEXT FROM phim_cursor INTO @phimid;
    
    -- Tạo 10 ghế cho mỗi phim
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @i = 1;  -- Đặt lại giá trị i cho mỗi phim
        
        -- Tạo 10 ghế cho phim hiện tại
        WHILE @i <= 10
        BEGIN
            INSERT INTO ghe (phimid, vitri) 
            VALUES (@phimid, CONCAT('ghe ', @i));
            SET @i = @i + 1;
        END
        
        FETCH NEXT FROM phim_cursor INTO @phimid;
    END
    
    CLOSE phim_cursor;
    DEALLOCATE phim_cursor;
END;
GO

--bảng chi tiết hóa đơn
CREATE TABLE chitiethoadon (
    id INT IDENTITY(1,1) PRIMARY KEY,         
    hoadonid INT NOT NULL,                    
    gheid INT NOT NULL,                        
    gia DECIMAL(10, 2) NOT NULL,               
    FOREIGN KEY (hoadonid) REFERENCES hoadon(id), 
    FOREIGN KEY (gheid) REFERENCES ghe(id)     
);

--bảng món ăn
CREATE TABLE doan (
    id INT IDENTITY(1,1) PRIMARY KEY,      
    ten NVARCHAR(255) NOT NULL,              
    gia DECIMAL(10, 2) NOT NULL,               
    hinhanh NVARCHAR(555)                       
);

--bảng chi tiết món ăn trong hóa đơn
CREATE TABLE chitietdoan (
    id INT IDENTITY(1,1) PRIMARY KEY,        
    hoadonid INT NOT NULL,                 
    doandid INT NOT NULL,                      
    soluong INT NOT NULL,                     
    gia DECIMAL(10, 2) NOT NULL,               
    FOREIGN KEY (hoadonid) REFERENCES hoadon(id),
    FOREIGN KEY (doandid) REFERENCES doan(id) 
);

-- Dữ liệu mẫu

-- Thể loại phim
INSERT INTO theloai (ten) VALUES ('Tình cảm'), ('Phiêu lưu'), ('Kinh dị'), ('Hài'), ('Hành động');

-- Người dùng
INSERT INTO nguoidung (username, password, email, sodienthoai)
VALUES
('h', '1', 'cuong@gmail.com', '0123456789'),
('lethao', 'password456', 'lethao@gmail.com', '0987654321'),
('hoangbao', 'password789', 'hoangbao@gmail.com', '0912345678');

-- Phim (kích hoạt trigger tạo ghế)
INSERT INTO phim (tenphim, theloaiid, thoigian, giave, mota, hinhanh) VALUES
('Chuyện Tình Mùa Thu', 1, '2024-12-20 18:00', 100000, 'Câu chuyện tình yêu đầy lãng mạn.', 'tinhcam1.jpg'),
('Cuộc Phiêu Lưu Rừng Xanh', 2, '2024-12-21 20:00', 120000, 'Hành trình khám phá rừng xanh.', 'phieuluu1.jpg'),
('Ngôi Nhà Ma Ám', 3, '2024-12-22 22:00', 150000, 'Ngôi nhà kỳ bí với những bí mật đáng sợ.', 'kinhdi1.jpg'),
('Siêu Cười', 4, '2024-12-23 19:00', 90000, 'Bộ phim hài làm bạn cười đến cuối.', 'hai1.jpg'),
('Đấu Trường Sinh Tử', 5, '2024-12-24 21:00', 200000, 'Cuộc chiến sống còn đầy kịch tính.', 'hanhdong1.jpg'),
('Hẹn Hò Lần Đầu', 1, '2024-12-25 17:00', 80000, 'Câu chuyện ngọt ngào về lần hẹn đầu tiên.', 'tinhcam2.jpg'),
('Phi Thuyền Không Gian', 2, '2024-12-26 20:00', 180000, 'Khám phá vũ trụ trên phi thuyền hiện đại.', 'phieuluu2.jpg'),
('Đêm Kinh Hoàng', 3, '2024-12-27 23:00', 170000, 'Đêm đầy ám ảnh và kinh hoàng.', 'kinhdi2.jpg'),
('Hài Đặc Biệt', 4, '2024-12-28 18:00', 95000, 'Cười xuyên suốt với dàn diễn viên hài nổi tiếng.', 'hai2.jpg'),
('Chiến Binh Bất Bại', 5, '2024-12-29 20:00', 220000, 'Bộ phim hành động đỉnh cao.', 'hanhdong2.jpg');

-- Hóa đơn
INSERT INTO hoadon (userid, ngaytao, trangthai) VALUES
(1, '2024-12-10 15:30', 1),
(2, '2024-12-11 16:00', 0),
(3, '2024-12-12 14:45', 1);


-- Chi tiết hóa đơn cho ghế
-- hoadonid: Mã hóa đơn
-- gheid: Mã ghế
-- gia: Giá vé tại thời điểm đặt

INSERT INTO chitiethoadon (hoadonid, gheid, gia) VALUES
(1, 1, 100000), -- Ghế 1, Phim 1
(1, 2, 100000), -- Ghế 2, Phim 1
(2, 3, 120000), -- Ghế 3, Phim 2
(3, 4, 150000); -- Ghế 4, Phim 3


-- Món ăn
INSERT INTO doan (ten, gia, hinhanh) VALUES
('Bắp rang bơ', 25000, '1.jpg'),
('Nước ngọt', 15000, '2.jpg'),
('Gà rán', 50000, '3.jpg');


-- Chi tiết hóa đơn cho đồ ăn
-- hoadonid: Mã hóa đơn
-- doandid: Mã món ăn
-- soluong: Số lượng món ăn
-- gia: Giá món ăn tại thời điểm đặt

INSERT INTO chitietdoan (hoadonid, doandid, soluong, gia) VALUES
(1, 1, 2, 25000), -- 2 bắp rang bơ
(1, 2, 1, 15000), -- 1 nước ngọt
(2, 3, 1, 50000), -- 1 gà rán
(3, 1, 3, 25000); -- 3 bắp rang bơ

