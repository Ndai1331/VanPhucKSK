CREATE VIEW QLCLViewBaoCaoKiemTraHauKiemATTP AS
SELECT
    FORMAT(qlcl.ngay_kiem_tra, 'yyyy-MM') AS thang,
    qlcl.province,
    qlcl.ward,
    COUNT(DISTINCT qlcl.dot_kiem_tra) AS tong_dot_kiem_tra,
    COUNT(*) AS tong_co_so_kiem_tra,
    SUM(CASE WHEN qlcl.tinh_hinh_vi_pham = 1 THEN 1 ELSE 0 END) AS so_vi_pham,
    SUM(CASE WHEN qlcl.tinh_hinh_vi_pham = 2 THEN 1 ELSE 0 END) AS so_chap_hanh,
    SUM(CASE WHEN qlcl.ket_qua_kiem_tra = 1 THEN 1 ELSE 0 END) AS so_dat,
    SUM(CASE WHEN qlcl.ket_qua_kiem_tra = 2 THEN 1 ELSE 0 END) AS so_khong_dat
FROM
    QLCLKiemTraHauKiemATTP qlcl
LEFT JOIN QLCLDotKiemTraHauKiemATTP dk ON dk.id = qlcl.dot_kiem_tra
GROUP BY
    FORMAT(qlcl.ngay_kiem_tra, 'yyyy-MM'),
    qlcl.province,
    qlcl.ward;


INSERT INTO custom_collections (collection, hidden, singleton,archive_app_filter,collapse,versioning)
VALUES ('QLCLViewBaoCaoKiemTraHauKiemATTP', 0, 0, 1, 0, 0);

INSERT INTO custom_fields (
    collection, field, interface, readonly, hidden, sort, width, translations, required
) VALUES
('QLCLViewBaoCaoKiemTraHauKiemATTP', 'id', 'input', 0, 0, 1, 'full', '[{"language":"vi-VN","translation":"ID"}]', 0),
('QLCLViewBaoCaoKiemTraHauKiemATTP', 'thang', 'input', 0, 0, 2, 'full', '[{"language":"vi-VN","translation":"Tháng"}]', 0),
('QLCLViewBaoCaoKiemTraHauKiemATTP', 'province', 'input', 0, 0, 3, 'full', '[{"language":"vi-VN","translation":"Tỉnh/Thành"}]', 0),
('QLCLViewBaoCaoKiemTraHauKiemATTP', 'ward', 'input', 0, 0, 4, 'full', '[{"language":"vi-VN","translation":"Phường/Xã"}]', 0),
('QLCLViewBaoCaoKiemTraHauKiemATTP', 'tong_dot_kiem_tra', 'input', 0, 0, 5, 'full', '[{"language":"vi-VN","translation":"Tổng đợt kiểm tra"}]', 0),
('QLCLViewBaoCaoKiemTraHauKiemATTP', 'tong_co_so_kiem_tra', 'input', 0, 0, 6, 'full', '[{"language":"vi-VN","translation":"Tổng cơ sở kiểm tra"}]', 0),
('QLCLViewBaoCaoKiemTraHauKiemATTP', 'so_vi_pham', 'input', 0, 0, 7, 'full', '[{"language":"vi-VN","translation":"Số vi phạm"}]', 0),
('QLCLViewBaoCaoKiemTraHauKiemATTP', 'so_chap_hanh', 'input', 0, 0, 8, 'full', '[{"language":"vi-VN","translation":"Số chấp hành"}]', 0),
('QLCLViewBaoCaoKiemTraHauKiemATTP', 'so_dat', 'input', 0, 0, 9, 'full', '[{"language":"vi-VN","translation":"Số đạt"}]', 0),
('QLCLViewBaoCaoKiemTraHauKiemATTP', 'so_khong_dat', 'input', 0, 0, 10, 'full', '[{"language":"vi-VN","translation":"Số không đạt"}]', 0);
