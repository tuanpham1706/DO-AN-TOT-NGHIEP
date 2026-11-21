USE vt3_cyberzone_db;

UPDATE users
SET password_hash = '$2a$11$Bmv3SGBuC21OVbIj9uyPR.z3oL0d1lzrlWRYcQDmtQXVShyZjvBnS'
WHERE username IN ('admin_vt3', 'thungan_ca1', 'gamer_pro', 'khach_vip01', 'het_tien01');
