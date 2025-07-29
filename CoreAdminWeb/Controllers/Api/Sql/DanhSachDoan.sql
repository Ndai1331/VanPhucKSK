select sksk.ma_luot_kham, ct.code, u.last_name, u.first_name, u.ngay_sinh, u.gioi_tinh,  
ts.ten_benh, ts.tien_su_gia_dinh,
tl.chieu_cao, tl.can_nang, tl.bmi, tl.mach, tl.huyet_ap,
ck.kq_nk_tuan_hoan, ck.kq_nk_ho_hap, ck.kq_nk_tieu_hoa, ck.kq_nk_than_tiet_nieu, ck.kq_nk_noi_tiet,
ck.kq_nk_co_xuong_khop, ck.kq_nk_than_kinh, ck.kq_nk_tam_than, ck.kq_ngoai_khoa,
spk.ket_qua, ck.benh_mat, ck.benh_tai_mui_hong, ck.benh_rhm, ck.kq_da_lieu,
kl.benh_tat_ket_luan, kl.de_nghi, plsk.name as phan_loai_suc_khoe,
-- Gộp tất cả kết quả cận lâm sàng thành một cột, phân cách bằng dấu |
STRING_AGG(CONCAT(cls.ten_cls, ': ', cls.ket_qua_cls), ' | ') AS can_lam_sang_results

from SoKhamSucKhoe sksk 
Left join kham_suc_khoe_cong_ty ct on ct.id = sksk.MaDotKham
Left join custom_users u on u.ma_benh_nhan = sksk.ma_benh_nhan 
Left join kham_suc_khoe_tien_su ts on ts.ma_luot_kham = sksk.ma_luot_kham
Left join kham_suc_khoe_the_luc tl on tl.ma_luot_kham = sksk.ma_luot_kham
Left join kham_suc_khoe_kham_chuyen_khoa ck on ck.ma_luot_kham = sksk.ma_luot_kham
Left join kham_suc_khoe_san_phu_khoa spk on spk.ma_luot_kham = sksk.ma_luot_kham
Left join kham_suc_khoe_ket_luan kl on kl.ma_luot_kham = sksk.ma_luot_kham
Left join phan_loai_suc_khoe plsk on kl.phan_loai_suc_khoe = plsk.id
Left join kham_suc_khoe_can_lam_sang cls on cls.ma_luot_kham = sksk.ma_luot_kham
where sksk.MaDotKham = @maDotKham
GROUP BY sksk.id, sksk.ma_luot_kham, ct.code, u.last_name, u.first_name, u.ngay_sinh, u.gioi_tinh,  
ts.ten_benh, ts.tien_su_gia_dinh,
tl.chieu_cao, tl.can_nang, tl.bmi, tl.mach, tl.huyet_ap,
ck.kq_nk_tuan_hoan, ck.kq_nk_ho_hap, ck.kq_nk_tieu_hoa, ck.kq_nk_than_tiet_nieu, ck.kq_nk_noi_tiet,
ck.kq_nk_co_xuong_khop, ck.kq_nk_than_kinh, ck.kq_nk_tam_than, ck.kq_ngoai_khoa,
spk.ket_qua, ck.benh_mat, ck.benh_tai_mui_hong, ck.benh_rhm, ck.kq_da_lieu,
kl.benh_tat_ket_luan, kl.de_nghi, plsk.name
ORDER BY sksk.id
OFFSET @offset ROWS 
FETCH NEXT @limit ROWS ONLY;

-- Alternative using FOR XML PATH (commented out)
-- STUFF((SELECT ' | ' + cls.ten_cls + ': ' + cls.ket_qua_cls 
--        FROM kham_suc_khoe_can_lam_sang cls 
--        WHERE cls.ma_luot_kham = sksk.ma_luot_kham 
--        FOR XML PATH('')), 1, 3, '') AS can_lam_sang_results