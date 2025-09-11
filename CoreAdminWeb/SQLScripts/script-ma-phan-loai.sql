CREATE UNIQUE INDEX IX_phan_loai_suc_khoe_ma
ON dbo.phan_loai_suc_khoe(code);

-- Drop trigger cũ trước khi tạo mới
DROP TRIGGER IF EXISTS dbo.trg_upsert_pl_suc_khoe;


CREATE TRIGGER dbo.trg_upsert_pl_suc_khoe
ON dbo.kham_suc_khoe_kham_chuyen_khoa
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF (TRIGGER_NESTLEVEL() > 1) RETURN;

    -- Chỉ chạy khi có thay đổi các trường ma_pl_*
    IF (UPDATE(ma_pl_nk_tuan_hoan) OR UPDATE(ma_pl_rhm) OR UPDATE(ma_pl_mat) OR UPDATE(ma_pl_tmh) OR 
        UPDATE(ma_pl_nk_ho_hap) OR UPDATE(ma_pl_nk_tieu_hoa) OR UPDATE(ma_pl_nk_than_tiet_nieu) OR 
        UPDATE(ma_pl_nk_noi_tiet) OR UPDATE(ma_pl_nk_co_xuong_khop) OR UPDATE(ma_pl_nk_than_kinh) OR 
        UPDATE(ma_pl_nk_tam_than) OR UPDATE(ma_pl_ngoai_khoa) OR UPDATE(ma_pl_da_lieu))
    BEGIN
        ;WITH src AS (
            SELECT
                i.id,
                -- Tra cứu id theo mã; nếu mã NULL hoặc không khớp thì kết quả NULL
                p1.id  AS id_pl_nk_tuan_hoan,
                p2.id  AS id_pl_rhm,
                p3.id  AS id_pl_mat,
                p4.id  AS id_pl_tmh,
                p5.id  AS id_pl_nk_ho_hap,
                p6.id  AS id_pl_nk_tieu_hoa,
                p7.id  AS id_pl_nk_than_tiet_nieu,
                p8.id  AS id_pl_nk_noi_tiet,
                p9.id  AS id_pl_nk_co_xuong_khop,
                p10.id AS id_pl_nk_than_kinh,
                p11.id AS id_pl_nk_tam_than,
                p12.id AS id_pl_ngoai_khoa,
                p13.id AS id_pl_da_lieu
            FROM inserted i
            LEFT JOIN dbo.phan_loai_suc_khoe p1  ON p1.code  = i.ma_pl_nk_tuan_hoan
            LEFT JOIN dbo.phan_loai_suc_khoe p2  ON p2.code  = i.ma_pl_rhm
            LEFT JOIN dbo.phan_loai_suc_khoe p3  ON p3.code  = i.ma_pl_mat
            LEFT JOIN dbo.phan_loai_suc_khoe p4  ON p4.code  = i.ma_pl_tmh
            LEFT JOIN dbo.phan_loai_suc_khoe p5  ON p5.code  = i.ma_pl_nk_ho_hap
            LEFT JOIN dbo.phan_loai_suc_khoe p6  ON p6.code  = i.ma_pl_nk_tieu_hoa
            LEFT JOIN dbo.phan_loai_suc_khoe p7  ON p7.code  = i.ma_pl_nk_than_tiet_nieu
            LEFT JOIN dbo.phan_loai_suc_khoe p8  ON p8.code  = i.ma_pl_nk_noi_tiet
            LEFT JOIN dbo.phan_loai_suc_khoe p9  ON p9.code  = i.ma_pl_nk_co_xuong_khop
            LEFT JOIN dbo.phan_loai_suc_khoe p10 ON p10.code = i.ma_pl_nk_than_kinh
            LEFT JOIN dbo.phan_loai_suc_khoe p11 ON p11.code = i.ma_pl_nk_tam_than
            LEFT JOIN dbo.phan_loai_suc_khoe p12 ON p12.code = i.ma_pl_ngoai_khoa
            LEFT JOIN dbo.phan_loai_suc_khoe p13 ON p13.code = i.ma_pl_da_lieu
        )
        UPDATE k SET
            k.pl_nk_tuan_hoan        = s.id_pl_nk_tuan_hoan,
            k.pl_rhm                 = s.id_pl_rhm,
            k.pl_mat                 = s.id_pl_mat,
            k.pl_tmh                 = s.id_pl_tmh,
            k.pl_nk_ho_hap           = s.id_pl_nk_ho_hap,
            k.pl_nk_tieu_hoa         = s.id_pl_nk_tieu_hoa,
            k.pl_nk_than_tiet_nieu   = s.id_pl_nk_than_tiet_nieu,
            k.pl_nk_noi_tiet         = s.id_pl_nk_noi_tiet,
            k.pl_nk_co_xuong_khop    = s.id_pl_nk_co_xuong_khop,
            k.pl_nk_than_kinh        = s.id_pl_nk_than_kinh,
            k.pl_nk_tam_than         = s.id_pl_nk_tam_than,
            k.pl_ngoai_khoa          = s.id_pl_ngoai_khoa,
            k.pl_da_lieu             = s.id_pl_da_lieu
        FROM dbo.kham_suc_khoe_kham_chuyen_khoa k
        JOIN src s ON s.id = k.id
        WHERE 
            ISNULL(k.pl_nk_tuan_hoan, -1) <> ISNULL(s.id_pl_nk_tuan_hoan, -1) OR
            ISNULL(k.pl_rhm, -1) <> ISNULL(s.id_pl_rhm, -1) OR
            ISNULL(k.pl_mat, -1) <> ISNULL(s.id_pl_mat, -1) OR
            ISNULL(k.pl_tmh, -1) <> ISNULL(s.id_pl_tmh, -1) OR
            ISNULL(k.pl_nk_ho_hap, -1) <> ISNULL(s.id_pl_nk_ho_hap, -1) OR
            ISNULL(k.pl_nk_tieu_hoa, -1) <> ISNULL(s.id_pl_nk_tieu_hoa, -1) OR
            ISNULL(k.pl_nk_than_tiet_nieu, -1) <> ISNULL(s.id_pl_nk_than_tiet_nieu, -1) OR
            ISNULL(k.pl_nk_noi_tiet, -1) <> ISNULL(s.id_pl_nk_noi_tiet, -1) OR
            ISNULL(k.pl_nk_co_xuong_khop, -1) <> ISNULL(s.id_pl_nk_co_xuong_khop, -1) OR
            ISNULL(k.pl_nk_than_kinh, -1) <> ISNULL(s.id_pl_nk_than_kinh, -1) OR
            ISNULL(k.pl_nk_tam_than, -1) <> ISNULL(s.id_pl_nk_tam_than, -1) OR
            ISNULL(k.pl_ngoai_khoa, -1) <> ISNULL(s.id_pl_ngoai_khoa, -1) OR
            ISNULL(k.pl_da_lieu, -1) <> ISNULL(s.id_pl_da_lieu, -1);
    END
END;


-- Drop trigger cũ trước khi tạo mới
DROP TRIGGER IF EXISTS dbo.trg_upsert_ma_from_pl;


CREATE TRIGGER dbo.trg_upsert_ma_from_pl
ON dbo.kham_suc_khoe_kham_chuyen_khoa
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF (TRIGGER_NESTLEVEL() > 1) RETURN;

    -- Chỉ chạy khi có thay đổi các trường pl_* và KHÔNG có thay đổi các trường ma_pl_*
    IF ((UPDATE(pl_nk_tuan_hoan) OR UPDATE(pl_rhm) OR UPDATE(pl_mat) OR UPDATE(pl_tmh) OR 
         UPDATE(pl_nk_ho_hap) OR UPDATE(pl_nk_tieu_hoa) OR UPDATE(pl_nk_than_tiet_nieu) OR 
         UPDATE(pl_nk_noi_tiet) OR UPDATE(pl_nk_co_xuong_khop) OR UPDATE(pl_nk_than_kinh) OR 
         UPDATE(pl_nk_tam_than) OR UPDATE(pl_ngoai_khoa) OR UPDATE(pl_da_lieu)) AND
        NOT (UPDATE(ma_pl_nk_tuan_hoan) OR UPDATE(ma_pl_rhm) OR UPDATE(ma_pl_mat) OR UPDATE(ma_pl_tmh) OR 
             UPDATE(ma_pl_nk_ho_hap) OR UPDATE(ma_pl_nk_tieu_hoa) OR UPDATE(ma_pl_nk_than_tiet_nieu) OR 
             UPDATE(ma_pl_nk_noi_tiet) OR UPDATE(ma_pl_nk_co_xuong_khop) OR UPDATE(ma_pl_nk_than_kinh) OR 
             UPDATE(ma_pl_nk_tam_than) OR UPDATE(ma_pl_ngoai_khoa) OR UPDATE(ma_pl_da_lieu)))
    BEGIN
        ;WITH src AS (
            SELECT
                i.id,
                m1.code  AS ma_pl_nk_tuan_hoan,
                m2.code  AS ma_pl_rhm,
                m3.code  AS ma_pl_mat,
                m4.code  AS ma_pl_tmh,
                m5.code  AS ma_pl_nk_ho_hap,
                m6.code  AS ma_pl_nk_tieu_hoa,
                m7.code  AS ma_pl_nk_than_tiet_nieu,
                m8.code  AS ma_pl_nk_noi_tiet,
                m9.code  AS ma_pl_nk_co_xuong_khop,
                m10.code AS ma_pl_nk_than_kinh,
                m11.code AS ma_pl_nk_tam_than,
                m12.code AS ma_pl_ngoai_khoa,
                m13.code AS ma_pl_da_lieu
            FROM inserted i
            LEFT JOIN dbo.phan_loai_suc_khoe m1  ON m1.id  = i.pl_nk_tuan_hoan
            LEFT JOIN dbo.phan_loai_suc_khoe m2  ON m2.id  = i.pl_rhm
            LEFT JOIN dbo.phan_loai_suc_khoe m3  ON m3.id  = i.pl_mat
            LEFT JOIN dbo.phan_loai_suc_khoe m4  ON m4.id  = i.pl_tmh
            LEFT JOIN dbo.phan_loai_suc_khoe m5  ON m5.id  = i.pl_nk_ho_hap
            LEFT JOIN dbo.phan_loai_suc_khoe m6  ON m6.id  = i.pl_nk_tieu_hoa
            LEFT JOIN dbo.phan_loai_suc_khoe m7  ON m7.id  = i.pl_nk_than_tiet_nieu
            LEFT JOIN dbo.phan_loai_suc_khoe m8  ON m8.id  = i.pl_nk_noi_tiet
            LEFT JOIN dbo.phan_loai_suc_khoe m9  ON m9.id  = i.pl_nk_co_xuong_khop
            LEFT JOIN dbo.phan_loai_suc_khoe m10 ON m10.id = i.pl_nk_than_kinh
            LEFT JOIN dbo.phan_loai_suc_khoe m11 ON m11.id = i.pl_nk_tam_than
            LEFT JOIN dbo.phan_loai_suc_khoe m12 ON m12.id = i.pl_ngoai_khoa
            LEFT JOIN dbo.phan_loai_suc_khoe m13 ON m13.id = i.pl_da_lieu
        )
        UPDATE k
        SET
            k.ma_pl_nk_tuan_hoan      = s.ma_pl_nk_tuan_hoan,
            k.ma_pl_rhm               = s.ma_pl_rhm,
            k.ma_pl_mat               = s.ma_pl_mat,
            k.ma_pl_tmh               = s.ma_pl_tmh,
            k.ma_pl_nk_ho_hap         = s.ma_pl_nk_ho_hap,
            k.ma_pl_nk_tieu_hoa       = s.ma_pl_nk_tieu_hoa,
            k.ma_pl_nk_than_tiet_nieu = s.ma_pl_nk_than_tiet_nieu,
            k.ma_pl_nk_noi_tiet       = s.ma_pl_nk_noi_tiet,
            k.ma_pl_nk_co_xuong_khop  = s.ma_pl_nk_co_xuong_khop,
            k.ma_pl_nk_than_kinh      = s.ma_pl_nk_than_kinh,
            k.ma_pl_nk_tam_than       = s.ma_pl_nk_tam_than,
            k.ma_pl_ngoai_khoa        = s.ma_pl_ngoai_khoa,
            k.ma_pl_da_lieu           = s.ma_pl_da_lieu
        FROM dbo.kham_suc_khoe_kham_chuyen_khoa k
        JOIN src s ON s.id = k.id
        WHERE
            ISNULL(k.ma_pl_nk_tuan_hoan,      N'') <> ISNULL(s.ma_pl_nk_tuan_hoan,      N'') OR
            ISNULL(k.ma_pl_rhm,               N'') <> ISNULL(s.ma_pl_rhm,               N'') OR
            ISNULL(k.ma_pl_mat,               N'') <> ISNULL(s.ma_pl_mat,               N'') OR
            ISNULL(k.ma_pl_tmh,               N'') <> ISNULL(s.ma_pl_tmh,               N'') OR
            ISNULL(k.ma_pl_nk_ho_hap,         N'') <> ISNULL(s.ma_pl_nk_ho_hap,         N'') OR
            ISNULL(k.ma_pl_nk_tieu_hoa,       N'') <> ISNULL(s.ma_pl_nk_tieu_hoa,       N'') OR
            ISNULL(k.ma_pl_nk_than_tiet_nieu, N'') <> ISNULL(s.ma_pl_nk_than_tiet_nieu, N'') OR
            ISNULL(k.ma_pl_nk_noi_tiet,       N'') <> ISNULL(s.ma_pl_nk_noi_tiet,       N'') OR
            ISNULL(k.ma_pl_nk_co_xuong_khop,  N'') <> ISNULL(s.ma_pl_nk_co_xuong_khop,  N'') OR
            ISNULL(k.ma_pl_nk_than_kinh,      N'') <> ISNULL(s.ma_pl_nk_than_kinh,      N'') OR
            ISNULL(k.ma_pl_nk_tam_than,       N'') <> ISNULL(s.ma_pl_nk_tam_than,       N'') OR
            ISNULL(k.ma_pl_ngoai_khoa,        N'') <> ISNULL(s.ma_pl_ngoai_khoa,        N'') OR
            ISNULL(k.ma_pl_da_lieu,           N'') <> ISNULL(s.ma_pl_da_lieu,           N'');
    END
END;





DROP TRIGGER IF EXISTS dbo.trg_sync_ma_id_kssk_spk;


CREATE OR ALTER TRIGGER dbo.trg_sync_ma_id_kssk_spk
ON dbo.kham_suc_khoe_san_phu_khoa
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF (TRIGGER_NESTLEVEL() > 1) RETURN;

    IF (UPDATE(ma_phan_loai))
    BEGIN
        ;WITH src AS (
            SELECT i.id, p.id AS new_id, p.code AS new_ma
            FROM inserted i
            LEFT JOIN dbo.phan_loai_suc_khoe p ON p.code = i.ma_phan_loai
        )
        UPDATE k
           SET k.phan_loai = s.new_id
        FROM dbo.kham_suc_khoe_san_phu_khoa k
        JOIN src s ON s.id = k.id
        WHERE ISNULL(k.phan_loai, -1) <> ISNULL(s.new_id, -1);
    END

    IF (NOT UPDATE(ma_phan_loai) AND UPDATE(phan_loai))
    BEGIN
        ;WITH src AS (
            SELECT i.id, p.code AS new_ma
            FROM inserted i
            LEFT JOIN dbo.phan_loai_suc_khoe p ON p.id = i.phan_loai
        )
        UPDATE k
           SET k.ma_phan_loai = s.new_ma
        FROM dbo.kham_suc_khoe_san_phu_khoa k
        JOIN src s ON s.id = k.id
        WHERE ISNULL(k.ma_phan_loai, N'') <> ISNULL(s.new_ma, N'');
    END
END;







DROP TRIGGER IF EXISTS dbo.trg_sync_ma_id_kssk_tl;

CREATE OR ALTER TRIGGER dbo.trg_sync_ma_id_kssk_tl
ON dbo.kham_suc_khoe_the_luc
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF (TRIGGER_NESTLEVEL() > 1) RETURN;

    IF (UPDATE(ma_phan_loai))
    BEGIN
        ;WITH src AS (
            SELECT i.id, p.id AS new_id, p.code AS new_ma
            FROM inserted i
            LEFT JOIN dbo.phan_loai_suc_khoe p ON p.code = i.ma_phan_loai
        )
        UPDATE k
           SET k.phan_loai = s.new_id
        FROM dbo.kham_suc_khoe_the_luc k
        JOIN src s ON s.id = k.id
        WHERE ISNULL(k.phan_loai, -1) <> ISNULL(s.new_id, -1);
    END

    IF (NOT UPDATE(ma_phan_loai) AND UPDATE(phan_loai))
    BEGIN
        ;WITH src AS (
            SELECT i.id, p.code AS new_ma
            FROM inserted i
            LEFT JOIN dbo.phan_loai_suc_khoe p ON p.id = i.phan_loai
        )
        UPDATE k
           SET k.ma_phan_loai = s.new_ma
        FROM dbo.kham_suc_khoe_the_luc k
        JOIN src s ON s.id = k.id
        WHERE ISNULL(k.ma_phan_loai, N'') <> ISNULL(s.new_ma, N'');
    END
END;




DROP TRIGGER IF EXISTS dbo.trg_sync_ma_id_kssk_kl;
CREATE OR ALTER TRIGGER dbo.trg_sync_ma_id_kssk_kl
ON dbo.kham_suc_khoe_ket_luan
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF (TRIGGER_NESTLEVEL() > 1) RETURN;

    IF (UPDATE(ma_phan_loai_suc_khoe))
    BEGIN
        ;WITH src AS (
            SELECT i.id, p.id AS new_id, p.code AS new_ma
            FROM inserted i
            LEFT JOIN dbo.phan_loai_suc_khoe p ON p.code = i.ma_phan_loai_suc_khoe
        )
        UPDATE k
           SET k.phan_loai_suc_khoe = s.new_id
        FROM dbo.kham_suc_khoe_ket_luan k
        JOIN src s ON s.id = k.id
        WHERE ISNULL(k.phan_loai_suc_khoe, -1) <> ISNULL(s.new_id, -1);
    END

    IF (NOT UPDATE(ma_phan_loai_suc_khoe) AND UPDATE(phan_loai_suc_khoe))
    BEGIN
        ;WITH src AS (
            SELECT i.id, p.code AS new_ma
            FROM inserted i
            LEFT JOIN dbo.phan_loai_suc_khoe p ON p.id = i.phan_loai_suc_khoe
        )
        UPDATE k
           SET k.ma_phan_loai_suc_khoe = s.new_ma
        FROM dbo.kham_suc_khoe_ket_luan k
        JOIN src s ON s.id = k.id
        WHERE ISNULL(k.ma_phan_loai_suc_khoe, N'') <> ISNULL(s.new_ma, N'');
    END
END;