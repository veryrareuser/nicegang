-- ============================================================
--  NICE.EAT  –  Database Schema & Seed Data (SQLite Edition)
-- ============================================================

-- Drop existing tables (if re-importing)
DROP TABLE IF EXISTS orders;
DROP TABLE IF EXISTS menu_items;
DROP TABLE IF EXISTS restaurants;
DROP TABLE IF EXISTS users;

-- ──────────────────────────────────────────────────────────
--  USERS
-- ──────────────────────────────────────────────────────────
CREATE TABLE users (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    username        TEXT NOT NULL UNIQUE,
    email           TEXT NOT NULL UNIQUE,
    user_type       TEXT NOT NULL DEFAULT 'Basic' /* Basic or Premium */,
    loyalty_points  INTEGER NOT NULL DEFAULT 0,
    discount_rate   REAL NOT NULL DEFAULT 0.00 /* e.g. 0.10 = 10% */,
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ──────────────────────────────────────────────────────────
--  RESTAURANTS
-- ──────────────────────────────────────────────────────────
CREATE TABLE restaurants (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    name          TEXT NOT NULL,
    address       TEXT NOT NULL,
    spicy_level   INTEGER NOT NULL DEFAULT 0 /* 0=not spicy, 5=very spicy */,
    is_vegan      INTEGER NOT NULL DEFAULT 0,
    average_price REAL NOT NULL,
    mood_tags     TEXT NOT NULL DEFAULT '' /* comma-separated: happy,bored,angry,... */,
    rating        REAL NOT NULL DEFAULT 0.0,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ──────────────────────────────────────────────────────────
--  MENU ITEMS
-- ──────────────────────────────────────────────────────────
CREATE TABLE menu_items (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    restaurant_id INTEGER NOT NULL,
    name          TEXT NOT NULL,
    price         REAL NOT NULL,
    spicy_level   INTEGER NOT NULL DEFAULT 0,
    is_vegan      INTEGER NOT NULL DEFAULT 0,
    category      TEXT NOT NULL DEFAULT 'Main',
    item_type     TEXT NOT NULL DEFAULT 'Food' /* Food or Drink */,
    portion_size  TEXT NOT NULL DEFAULT 'Regular' /* For Food: Small/Regular/Large */,
    is_cold       INTEGER NOT NULL DEFAULT 1 /* For Drink: 1=cold, 0=hot */,
    volume_ml     INTEGER NOT NULL DEFAULT 300 /* For Drink: milliliters */,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (restaurant_id) REFERENCES restaurants(id) ON DELETE CASCADE
);

-- ──────────────────────────────────────────────────────────
--  ORDERS
-- ──────────────────────────────────────────────────────────
CREATE TABLE orders (
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id          INTEGER NOT NULL,
    restaurant_id    INTEGER NOT NULL,
    menu_item_id     INTEGER NOT NULL,
    ordered_at       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    note             TEXT DEFAULT NULL,
    order_type       TEXT NOT NULL DEFAULT 'DineIn' /* DineIn or Delivery */,
    delivery_address TEXT DEFAULT NULL,
    delivery_fee     REAL NOT NULL DEFAULT 0.00,
    FOREIGN KEY (user_id)       REFERENCES users(id)       ON DELETE CASCADE,
    FOREIGN KEY (restaurant_id) REFERENCES restaurants(id) ON DELETE CASCADE,
    FOREIGN KEY (menu_item_id)  REFERENCES menu_items(id) ON DELETE CASCADE
);

-- ============================================================
--  SEED DATA
-- ============================================================

-- Users: 1 Basic + 1 Premium
INSERT INTO users (username, email, user_type, loyalty_points, discount_rate) VALUES
    ('demo_user',    'demo@niceeat.id',    'Basic',   0,   0.00),
    ('vip_member',   'vip@niceeat.id',     'Premium', 500, 0.10);

-- 10 Restaurants
INSERT INTO restaurants (name, address, spicy_level, is_vegan, average_price, mood_tags, rating) VALUES
    ('Warung Pedas Bahagia',  'Jl. Merdeka No.1, Jakarta',    4, 0, 25000,  'angry,adventurous',    4.5),
    ('Green Garden Resto',   'Jl. Sudirman No.22, Bandung',  1, 1, 35000,  'happy,stressed,calm',  4.2),
    ('Noodle Kingdom',       'Jl. Gatot Subroto No.7, Jkt', 2, 0, 20000,  'bored,happy',          4.0),
    ('Dapur Ibu Cantik',     'Jl. Pahlawan No.45, Depok',   0, 0, 15000,  'happy,calm',           4.3),
    ('Spice Route',          'Jl. Asia Afrika No.11, Bdg',  5, 0, 30000,  'angry,adventurous',    4.7),
    ('The Vegan Corner',     'Jl. Kebon Sirih No.3, Jkt',   0, 1, 40000,  'happy,stressed',       4.1),
    ('Bakso Beranak',        'Jl. Margonda Raya No.9, Dpk', 2, 0, 12000,  'bored,sad',            3.9),
    ('Sate Pak Jono',        'Jl. Diponegoro No.17, Sby',   1, 0, 18000,  'happy,bored',          4.4),
    ('FusionFire Kitchen',   'Jl. Thamrin No.5, Jakarta',   3, 0, 50000,  'adventurous,happy',    4.6),
    ('Zen Salad Bar',        'Jl. Rasuna Said No.30, Jkt',  0, 1, 45000,  'stressed,calm,happy',  4.0);

-- Menu Items
INSERT INTO menu_items (restaurant_id, name, price, spicy_level, is_vegan, category, item_type, portion_size, is_cold, volume_ml) VALUES
    (1, 'Ayam Geprek Super Pedas',  20000, 5, 0, 'Main',   'Food',  'Regular', 1, 300),
    (1, 'Nasi Goreng Pedas',        15000, 4, 0, 'Main',   'Food',  'Regular', 1, 300),
    (1, 'Es Teh Manis',              5000, 0, 1, 'Drink',  'Drink', 'Regular', 1, 400),
    (2, 'Gado-Gado Spesial',        30000, 1, 1, 'Main',   'Food',  'Large',   1, 300),
    (2, 'Smoothie Alpukat',         20000, 0, 1, 'Drink',  'Drink', 'Regular', 1, 350),
    (2, 'Salad Buah Segar',         25000, 0, 1, 'Dessert','Food',  'Regular', 1, 300),
    (3, 'Mie Ayam Komplit',         18000, 2, 0, 'Main',   'Food',  'Regular', 1, 300),
    (3, 'Pangsit Goreng',           10000, 1, 0, 'Snack',  'Food',  'Small',   1, 300),
    (3, 'Jus Jeruk',                 7000, 0, 1, 'Drink',  'Drink', 'Regular', 1, 300),
    (4, 'Nasi Uduk Lengkap',        13000, 0, 0, 'Main',   'Food',  'Regular', 1, 300),
    (4, 'Tempe Orek',                5000, 1, 1, 'Side',   'Food',  'Small',   1, 300),
    (4, 'Sayur Asem',                8000, 0, 1, 'Side',   'Food',  'Regular', 1, 300),
    (5, 'Rendang Daging Sapi',      28000, 5, 0, 'Main',   'Food',  'Regular', 1, 300),
    (5, 'Sambal Goreng Ati',        20000, 4, 0, 'Main',   'Food',  'Regular', 1, 300),
    (5, 'Teh Tarik',                 8000, 0, 0, 'Drink',  'Drink', 'Regular', 0, 250),
    (6, 'Buddha Bowl',              38000, 0, 1, 'Main',   'Food',  'Large',   1, 300),
    (6, 'Vegan Burger',             35000, 1, 1, 'Main',   'Food',  'Regular', 1, 300),
    (6, 'Green Detox Juice',        20000, 0, 1, 'Drink',  'Drink', 'Regular', 1, 400),
    (7, 'Bakso Jumbo Isi 3',        10000, 2, 0, 'Main',   'Food',  'Large',   1, 300),
    (7, 'Mie Bakso Spesial',        12000, 2, 0, 'Main',   'Food',  'Regular', 1, 300),
    (7, 'Es Jeruk Peras',            5000, 0, 1, 'Drink',  'Drink', 'Regular', 1, 300),
    (8, 'Sate Ayam 10 Tusuk',       15000, 1, 0, 'Main',   'Food',  'Regular', 1, 300),
    (8, 'Lontong Sayur',            10000, 0, 0, 'Side',   'Food',  'Regular', 1, 300),
    (8, 'Es Campur',                 8000, 0, 1, 'Drink',  'Drink', 'Regular', 1, 350),
    (9, 'Wagyu Beef Sushi Roll',    45000, 3, 0, 'Main',   'Food',  'Regular', 1, 300),
    (9, 'Crispy Tofu Tacos',        30000, 2, 1, 'Main',   'Food',  'Regular', 1, 300),
    (9, 'Matcha Latte',             25000, 0, 1, 'Drink',  'Drink', 'Regular', 0, 300),
    (10, 'Quinoa Salad',            40000, 0, 1, 'Main',   'Food',  'Regular', 1, 300),
    (10, 'Avocado Toast',           35000, 0, 1, 'Main',   'Food',  'Regular', 1, 300),
    (10, 'Cucumber Lemon Water',    15000, 0, 1, 'Drink',  'Drink', 'Regular', 1, 500);

-- Seed demo orders
INSERT INTO orders (user_id, restaurant_id, menu_item_id, note, order_type, delivery_address, delivery_fee) VALUES
    (1, 3,  7,  'Tambah kecap',      'DineIn',   NULL, 0),
    (1, 3,  7,  NULL,                 'DineIn',   NULL, 0),
    (1, 1,  1,  'Level 3 aja ya',    'DineIn',   NULL, 0),
    (1, 8,  22, NULL,                 'Delivery', 'Jl. Margonda No.5', 5000),
    (1, 8,  22, 'Minta sambal banyak','Delivery', 'Jl. Margonda No.5', 5000),
    (1, 3,  8,  NULL,                 'DineIn',   NULL, 0),
    (2, 5,  13, 'Extra nasi',        'DineIn',   NULL, 0),
    (2, 9,  25, NULL,                 'Delivery', 'Jl. Thamrin No.88', 8000);
