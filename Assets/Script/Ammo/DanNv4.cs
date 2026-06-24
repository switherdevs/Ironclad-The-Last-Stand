using System.Collections.Generic;
using UnityEngine;

public class DanNv4 : MonoBehaviour
{
    public khodanan cauHinhDan;
    [HideInInspector] public int satThuong = 100;

    [Header("Cấu hình Bắn Lan (AOE)")]
    public bool laDanBanLan = true;
    public float banKinhNoLan = 2.5f;

    [Header("--- HIỆU ỨNG VA CHẠM ---")]
    [Tooltip("Prefab hiệu ứng sẽ được tạo ra tại vị trí viên đạn va chạm")]
    public GameObject prefabHieuUngNo;

    [Header("--- CẤU HÌNH XUYÊN THẤU ---")]
    [Tooltip("Điền tên các Prefab chủng lính KHÔNG THỂ bắn xuyên qua")]
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
            bool biChanLai = false;
            string tenVatTheVaCham = collision.gameObject.name;

            if (danhSachChungKhongXuyenQua != null)
            {
                foreach (string tenChungLinh in danhSachChungKhongXuyenQua)
                {
                    if (!string.IsNullOrEmpty(tenChungLinh) && tenVatTheVaCham.Contains(tenChungLinh))
                    {
                        biChanLai = true;
                        break;
                    }
                }
            }

            if (biChanLai)
            {
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
                    Health_chaos mauQuai = collision.GetComponent<Health_chaos>() ?? collision.GetComponentInParent<Health_chaos>();
                    if (mauQuai != null) mauQuai.TakeDamage(satThuong);
                }

                TaoHieuUngVaCham(); // Gọi hiệu ứng trước khi trả đạn về pool
                ThanhCongTraDan();
            }
            else
            {
                Health_chaos mauQuai = collision.GetComponent<Health_chaos>() ?? collision.GetComponentInParent<Health_chaos>();
                if (mauQuai != null) mauQuai.TakeDamage(satThuong);
            }
        }
    }

    // Hàm tạo hiệu ứng tại vị trí va chạm
    void TaoHieuUngVaCham()
    {
        if (prefabHieuUngNo != null)
        {
            // Tạo hiệu ứng tại vị trí hiện tại của viên đạn, giữ nguyên góc xoay
            Instantiate(prefabHieuUngNo, transform.position, transform.rotation);
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