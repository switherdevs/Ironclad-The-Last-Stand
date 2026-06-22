using System.Collections.Generic;
using UnityEngine;

public class DanNv4 : MonoBehaviour
{
    public khodanan cauHinhDan;
    [HideInInspector] public int satThuong = 100;

    [Header("Cấu hình Bắn Lan (AOE)")]
    public bool laDanBanLan = true;
    public float banKinhNoLan = 2.5f;

    [Header("--- CẤU HÌNH XUYÊN THẤU ---")]
    [Tooltip("Điền tên các Prefab chủng lính KHÔNG THỂ bắn xuyên qua (Ví dụ: Titan, Chaos_boss)")]
    public string[] danhSachChungKhongXuyenQua;

    private float demThoiGian;
    private bool daKichHoat = false;
    private TrailRenderer line;

    private void Awake()
    {
        line = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        if (line != null) line.enabled = true;

        if (cauHinhDan != null)
            demThoiGian = cauHinhDan.Duytri;
        else
            demThoiGian = 3f;

        daKichHoat = true;
    }

    void Update()
    {
        if (!daKichHoat) return;
        if (cauHinhDan == null) return;

        transform.Translate(Vector2.right * cauHinhDan.tocDobay * Time.deltaTime, Space.Self);

        demThoiGian -= Time.deltaTime;
        if (demThoiGian <= 0f)
        {
            ThanhCongTraDan();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 1. Kiểm tra xem chủng lính va chạm có nằm trong danh sách KHÔNG cho xuyên qua không
            bool biChanLai = false;
            string tenVatTheVaCham = collision.gameObject.name;

            if (danhSachChungKhongXuyenQua != null)
            {
                foreach (string tenChungLinh in danhSachChungKhongXuyenQua)
                {
                    // Dùng Contains để né lỗi chữ "(Clone)" sau tên Prefab khi sinh ra trong cụm ma trận
                    if (!string.IsNullOrEmpty(tenChungLinh) && tenVatTheVaCham.Contains(tenChungLinh))
                    {
                        biChanLai = true;
                        break;
                    }
                }
            }

            // 2. Phân nhánh xử lý logic Sát thương
            if (biChanLai)
            {
                // 🔥 TRƯỜNG HỢP 1: Chạm trúng chủng lính BỊ CHẶN -> Kích hoạt NỔ LAN (AOE)
                if (laDanBanLan)
                {
                    Collider2D[] xungQuanh = Physics2D.OverlapCircleAll(transform.position, banKinhNoLan);
                    List<Health_chaos> danhSachQuaiDaTrungDan = new List<Health_chaos>();

                    foreach (Collider2D vatThe in xungQuanh)
                    {
                        if (vatThe.CompareTag("Enemy"))
                        {
                            Health_chaos mau = vatThe.GetComponent<Health_chaos>() ?? vatThe.GetComponentInParent<Health_chaos>();

                            if (mau != null && !danhSachQuaiDaTrungDan.Contains(mau))
                            {
                                mau.TakeDamage(satThuong);
                                danhSachQuaiDaTrungDan.Add(mau);
                            }
                        }
                    }
                }
                else
                {
                    // Nếu không bật tính năng bắn lan, chỉ gây sát thương đơn cho mục tiêu chặn này
                    Health_chaos mauQuai = collision.GetComponent<Health_chaos>() ?? collision.GetComponentInParent<Health_chaos>();
                    if (mauQuai != null) mauQuai.TakeDamage(satThuong);
                }

                // Chạm quái to/bị chặn -> Đạn nổ chốt hạ và biến mất (Trả về pool)
                ThanhCongTraDan();
            }
            else
            {
                // ✨ TRƯỜNG HỢP 2: Chạm quái thường -> XUYÊN QUA: Chỉ trừ máu đơn mục tiêu con đó, đạn bay tiếp
                Health_chaos mauQuai = collision.GetComponent<Health_chaos>() ?? collision.GetComponentInParent<Health_chaos>();
                if (mauQuai != null) mauQuai.TakeDamage(satThuong);
            }
        }
    }

    void ThanhCongTraDan()
    {
        if (line != null) line.enabled = false;
        daKichHoat = false;
        gameObject.SetActive(false);
    }

    public void KichHoatVienDan()
    {
        daKichHoat = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, banKinhNoLan);
    }
}