-- =========================================================================
-- KHỞI TẠO CƠ SỞ DỮ LIỆU: V-T3 CYBERZONE & V-T3 FOOD
-- =========================================================================
DROP DATABASE IF EXISTS vt3_cyberzone_db;
CREATE DATABASE vt3_cyberzone_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE vt3_cyberzone_db;

-- =========================================================================
-- PHÂN HỆ 1: QUẢN TRỊ & V-T3 CYBERZONE (QUẢN LÝ PHÒNG MÁY)
-- =========================================================================

-- 1. Bảng Tài khoản (Admin, Thu ngân, Hội viên)
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100),
    phone VARCHAR(15),
    role ENUM('ADMIN', 'STAFF', 'MEMBER') NOT NULL DEFAULT 'MEMBER',
    balance DECIMAL(12, 2) NOT NULL DEFAULT 0.00,
    status ENUM('ACTIVE', 'LOCKED') NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Bảng Nhóm máy (Thường, VIP, Thi đấu)
CREATE TABLE machine_groups (
    id INT AUTO_INCREMENT PRIMARY KEY,
    group_name VARCHAR(50) NOT NULL UNIQUE,
    hourly_rate DECIMAL(10, 2) NOT NULL,
    specs TEXT
);

-- 3. Bảng Máy trạm (Workstations)
CREATE TABLE workstations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    machine_code VARCHAR(20) NOT NULL UNIQUE,
    group_id INT NOT NULL,
    ip_address VARCHAR(45),
    status ENUM('OFFLINE', 'IDLE', 'IN_USE', 'MAINTENANCE') NOT NULL DEFAULT 'OFFLINE',
    FOREIGN KEY (group_id) REFERENCES machine_groups(id) ON DELETE RESTRICT
);

-- 4. Bảng Phiên chơi (Usage Sessions)
CREATE TABLE usage_sessions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NULL, -- NULL nếu là khách vãng lai mở máy trả sau
    workstation_id INT NOT NULL,
    start_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    end_time DATETIME NULL,
    total_cost DECIMAL(12, 2) DEFAULT 0.00,
    status ENUM('ACTIVE', 'CLOSED') NOT NULL DEFAULT 'ACTIVE',
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL,
    FOREIGN KEY (workstation_id) REFERENCES workstations(id) ON DELETE RESTRICT
);

-- =========================================================================
-- PHÂN HỆ 2: V-T3 FOOD & QUẢN LÝ KHO ĐỊNH LƯỢNG (F&B)
-- =========================================================================

-- 5. Bảng Danh mục thực đơn
CREATE TABLE categories (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    display_order INT DEFAULT 1
);

-- 6. Bảng Món ăn / Sản phẩm
CREATE TABLE products (
    id INT AUTO_INCREMENT PRIMARY KEY,
    category_id INT NOT NULL,
    name VARCHAR(150) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    image_url VARCHAR(255),
    is_available BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT
);

-- 7. Bảng Nguyên vật liệu tồn kho (Ingredients)
CREATE TABLE ingredients (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    unit VARCHAR(30) NOT NULL, -- gói, quả, gram, lon, chai
    stock_quantity DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    min_threshold DECIMAL(10, 2) NOT NULL DEFAULT 5.00 -- Mức cảnh báo sắp hết hàng
);

-- 8. Bảng Định lượng công thức món (Product Recipes / BOM)
CREATE TABLE product_recipes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    ingredient_id INT NOT NULL,
    quantity_required DECIMAL(10, 2) NOT NULL, -- Số lượng nguyên liệu cần cho 1 phần
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (ingredient_id) REFERENCES ingredients(id) ON DELETE RESTRICT
);

-- 9. Bảng Đơn đặt đồ ăn (Orders)
CREATE TABLE orders (
    id INT AUTO_INCREMENT PRIMARY KEY,
    order_code VARCHAR(20) NOT NULL UNIQUE, -- VD: VT3F-0001
    user_id INT NULL,
    workstation_id INT NOT NULL, -- Đặt từ máy trạm nào để nhân viên bưng ra
    total_amount DECIMAL(12, 2) NOT NULL,
    payment_method ENUM('CASH', 'ACCOUNT_BALANCE') NOT NULL,
    order_status ENUM('PENDING', 'PREPARING', 'COMPLETED', 'CANCELLED') NOT NULL DEFAULT 'PENDING',
    note VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL,
    FOREIGN KEY (workstation_id) REFERENCES workstations(id) ON DELETE RESTRICT
);

-- 10. Bảng Chi tiết đơn đồ ăn (Order Items)
CREATE TABLE order_items (
    id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT
);

-- =========================================================================
-- PHÂN HỆ 3: TÀI CHÍNH & LỊCH SỬ DÒNG TIỀN CHUNG
-- =========================================================================

-- 11. Bảng Lịch sử giao dịch (Transactions)
CREATE TABLE transactions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    trans_code VARCHAR(25) NOT NULL UNIQUE, -- VD: VT3C-0001
    user_id INT NULL,
    staff_id INT NULL, -- Thu ngân thực hiện giao dịch
    order_id INT NULL, -- Liên kết với đơn V-T3 Food nếu là giao dịch mua đồ ăn
    type ENUM('TOP_UP', 'GAME_FEE', 'FOOD_ORDER', 'REFUND') NOT NULL,
    amount DECIMAL(12, 2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL,
    FOREIGN KEY (staff_id) REFERENCES users(id) ON DELETE SET NULL,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE SET NULL
);

-- =========================================================================
-- CHÈN DỮ LIỆU MẪU (SAMPLE DATA CHO DEMO ĐỒ ÁN)
-- =========================================================================

-- 1. Thêm Users (Mật khẩu mẫu giả định là '123456' đã mã hóa BCrypt)
INSERT INTO users (username, password_hash, full_name, phone, role, balance, status) VALUES
('admin_vt3', '$2a$11$Bmv3SGBuC21OVbIj9uyPR.z3oL0d1lzrlWRYcQDmtQXVShyZjvBnS', 'Quản Trị Viên V-T3', '0905111222', 'ADMIN', 0, 'ACTIVE'),
('thungan_ca1', '$2a$11$Bmv3SGBuC21OVbIj9uyPR.z3oL0d1lzrlWRYcQDmtQXVShyZjvBnS', 'Thu Ngân Ca Sáng', '0905333444', 'STAFF', 0, 'ACTIVE'),
('gamer_pro', '$2a$11$Bmv3SGBuC21OVbIj9uyPR.z3oL0d1lzrlWRYcQDmtQXVShyZjvBnS', 'Nguyễn Văn A', '0912000111', 'MEMBER', 45000.00, 'ACTIVE'),
('khach_vip01', '$2a$11$Bmv3SGBuC21OVbIj9uyPR.z3oL0d1lzrlWRYcQDmtQXVShyZjvBnS', 'Trần Thị B', '0912000222', 'MEMBER', 120000.00, 'ACTIVE'),
('het_tien01', '$2a$11$Bmv3SGBuC21OVbIj9uyPR.z3oL0d1lzrlWRYcQDmtQXVShyZjvBnS', 'Lê Văn C', '0912000333', 'MEMBER', 0.00, 'ACTIVE');

-- 2. Thêm Nhóm máy (Machine Groups)
INSERT INTO machine_groups (group_name, hourly_rate, specs) VALUES
('Standard Zone', 10000.00, 'Core i5-12400F | RAM 16GB | RTX 3060 | Màn 180Hz'),
('VIP CyberZone', 15000.00, 'Core i5-13400F | RAM 32GB | RTX 4060 Ti | Màn 240Hz | Gear Cơ'),
('Pro Arena (Thi Đấu)', 20000.00, 'Core i7-14700K | RAM 32GB | RTX 4070 | Màn Zowie 360Hz');

-- 3. Thêm Máy trạm (Workstations - Phù hợp để test trên 2 Laptop Client + các máy ảo trên sơ đồ)
INSERT INTO workstations (machine_code, group_id, ip_address, status) VALUES
('VT3-MAY01', 1, '192.168.1.11', 'IN_USE'),      -- Laptop 2 dùng mã này để test
('VT3-MAY02', 2, '192.168.1.12', 'IDLE'),        -- Laptop 3 dùng mã này để test
('VT3-MAY03', 1, '192.168.1.13', 'OFFLINE'),
('VT3-VIP01', 2, '192.168.1.21', 'IN_USE'),
('VT3-VIP02', 2, '192.168.1.22', 'MAINTENANCE'),
('VT3-PRO01', 3, '192.168.1.31', 'IDLE');

-- 4. Thêm Phiên chơi đang hoạt động
INSERT INTO usage_sessions (user_id, workstation_id, start_time, end_time, total_cost, status) VALUES
(3, 1, DATE_SUB(NOW(), INTERVAL 45 MINUTE), NULL, 7500.00, 'ACTIVE'),
(4, 4, DATE_SUB(NOW(), INTERVAL 30 MINUTE), NULL, 7500.00, 'ACTIVE');

-- 5. Thêm Danh mục V-T3 Food
INSERT INTO categories (name, display_order) VALUES
('Mì & Cơm Chế Biến', 1),
('Nước Giải Khát', 2),
('Đồ Ăn Vặt (Snack)', 3),
('Thẻ Game & Dịch Vụ', 4);

-- 6. Thêm Món ăn V-T3 Food
INSERT INTO products (category_id, name, price, image_url, is_available) VALUES
(1, 'Mì Trứng Bò V-T3', 35000.00, '/images/food/mi-trung-bo.jpg', TRUE),
(1, 'Mì Xào Xúc Xích Đôi', 30000.00, '/images/food/mi-xuc-xich.jpg', TRUE),
(1, 'Cơm Chiên Trứng Ốp La', 35000.00, '/images/food/com-chien-trung.jpg', TRUE),
(2, 'Sting Dâu Lon 330ml', 15000.00, '/images/food/sting-dau.jpg', TRUE),
(2, 'Bò Húc (RedBull) Lon', 20000.00, '/images/food/redbull.jpg', TRUE),
(2, 'Trà Đào Cam Sả V-T3', 25000.00, '/images/food/tra-dao.jpg', TRUE),
(3, 'Khoai Tây Chiên Lắc Phô Mai', 25000.00, '/images/food/khoai-tay.jpg', TRUE),
(4, 'Thẻ Zing / Garena 50K', 50000.00, '/images/food/the-game-50k.jpg', TRUE);

-- 7. Thêm Nguyên vật liệu trong kho
INSERT INTO ingredients (name, unit, stock_quantity, min_threshold) VALUES
('Vắt mì Hảo Hảo', 'gói', 100.00, 20.00),
('Trứng gà tươi', 'quả', 50.00, 10.00),
('Thịt bò thái lát', 'gram', 2000.00, 300.00),
('Xúc xích Đức', 'cây', 60.00, 10.00),
('Sting Dâu (Lon)', 'lon', 48.00, 12.00),
('Nước tăng lực RedBull', 'lon', 24.00, 6.00);

-- 8. Thiết lập Công thức Định lượng (BOM) cho món ăn
-- Món 1: Mì Trứng Bò V-T3 = 1 vắt mì + 1 quả trứng + 50g thịt bò
INSERT INTO product_recipes (product_id, ingredient_id, quantity_required) VALUES
(1, 1, 1.00),
(1, 2, 1.00),
(1, 3, 50.00),
-- Món 2: Mì Xào Xúc Xích Đôi = 1 vắt mì + 2 cây xúc xích
(2, 1, 1.00),
(2, 4, 2.00),
-- Món 4: Sting Dâu = 1 lon Sting trong kho
(4, 5, 1.00),
-- Món 5: Bò Húc = 1 lon RedBull trong kho
(5, 6, 1.00);

-- 9. Thêm Đơn đặt hàng mẫu trên V-T3 Food
INSERT INTO orders (order_code, user_id, workstation_id, total_amount, payment_method, order_status, note) VALUES
('VT3F-0001', 3, 1, 50000.00, 'ACCOUNT_BALANCE', 'COMPLETED', 'Mì không hành, Sting nhiều đá'),
('VT3F-0002', 4, 4, 35000.00, 'CASH', 'PENDING', 'Làm cay vừa giúp mình');

-- 10. Thêm Chi tiết đơn hàng
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
(1, 1, 1, 35000.00), -- 1 Mì Trứng Bò
(1, 4, 1, 15000.00), -- 1 Sting Dâu
(2, 1, 1, 35000.00); -- 1 Mì Trứng Bò

-- 11. Thêm Lịch sử giao dịch mẫu
INSERT INTO transactions (trans_code, user_id, staff_id, order_id, type, amount) VALUES
('VT3C-0001', 3, 2, NULL, 'TOP_UP', 100000.00),
('VT3C-0002', 4, 2, NULL, 'TOP_UP', 120000.00),
('VT3C-0003', 3, 2, 1, 'FOOD_ORDER', 50000.00);
