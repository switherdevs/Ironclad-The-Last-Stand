using System.Collections.Generic;
using UnityEngine;

public class DanNv4 : MonoBehaviour
{
    public khodanan cauHinhDan;
    [HideInInspector] public int satThuong = 100;

    [Header("Cấu hình Bắn Lan (AOE)")]
    public bool laDanBanLan = true;
    public float banKinhNoLan = 2.5f;

    private float demThoiGian;
    private bool daKichHoat = false;
    private TrailRenderer line;

    // SỬA: Dùng Awake thay cho Start để đảm bảo line luôn sẵn sàng trước khi dùng
    private void Awake()
    {
        line = GetComponent<TrailRenderer>();
    }

    // SỬA: Reset toàn bộ đạn tại đây để Pooling hoạt động hoàn hảo
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

            ThanhCongTraDan();
        }
    }

    void ThanhCongTraDan()
    {
        if (line != null) line.enabled = false;
        daKichHoat = false;
        gameObject.SetActive(false);
    }

    // Giữ lại để các script khác không bị lỗi thiếu phương thức
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