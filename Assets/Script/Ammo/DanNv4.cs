using System.Collections.Generic;
using UnityEngine;

public class DanNv4 : MonoBehaviour
{
    public khodanan cauHinhDan;
    public int satThuong = 100;

    [Header("Cấu hình Bắn Lan (AOE)")]
    public bool laDanBanLan = true;     // Tích chọn ngoài Inspector cho đạn Titan
    public float banKinhNoLan = 2.5f;   // Phạm vi nổ lan xung quanh (tính bằng ô)

    private float demThoiGian;
    private bool daKichHoat = false;

    public void KichHoatVienDan()
    {
        if (cauHinhDan != null)
        {
            demThoiGian = cauHinhDan.Duytri;
            daKichHoat = true;
        }
        else
        {
            demThoiGian = 3f;
            daKichHoat = true;
        }
    }

    void Update()
    {
        if (!daKichHoat) return;
        if (cauHinhDan == null) return;

        transform.Translate(Vector2.right * cauHinhDan.tocDobay * Time.deltaTime, Space.Self);

        demThoiGian -= Time.deltaTime;
        if (demThoiGian <= 0f)
        {
            daKichHoat = false;
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (laDanBanLan)
            {
                Collider2D[] XungQuanh = Physics2D.OverlapCircleAll(transform.position, banKinhNoLan);

                // Tạo một danh sách để lưu các script máu đã được xử lý, tránh trừ máu trùng lặp
                List<Health_chaos> danhSachQuaiDaTrungDan = new List<Health_chaos>();

                foreach (Collider2D vatThe in XungQuanh)
                {
                    if (vatThe.CompareTag("Enemy"))
                    {
                        // 1. Thử kiểm tra script Health_phechinh
                        Health_chaos mauDich = vatThe.GetComponent<Health_chaos>();
                        if (mauDich == null) mauDich = vatThe.GetComponentInParent<Health_chaos>();

                        if (mauDich != null)
                        {
                            // Nếu con quái này CHƯA từng bị trừ HP trong vụ nổ này
                            if (!danhSachQuaiDaTrungDan.Contains(mauDich))
                            {
                                mauDich.TakeDamage(satThuong);
                                danhSachQuaiDaTrungDan.Add(mauDich); // Đánh dấu đã xử lý
                            }
                            continue;
                        }

                        // 2. Thử kiểm tra script Health_chaos
                        Health_chaos mauChaos = vatThe.GetComponent<Health_chaos>();
                        if (mauChaos == null) mauChaos = vatThe.GetComponentInParent<Health_chaos>();

                        if (mauChaos != null)
                        {
                            if (!danhSachQuaiDaTrungDan.Contains(mauChaos))
                            {
                                mauChaos.TakeDamage(satThuong);
                                danhSachQuaiDaTrungDan.Add(mauChaos);
                                Debug.Log($"💥 Nổ lan (Chaos) trúng {vatThe.name} gây {satThuong} sát thương thực tế!");
                            }
                        }
                    }
                }
            }
            else
            {
                // LOGIC BẮN ĐƠN MỤC TIÊU CŨ
                Health_chaos mauQuai = collision.GetComponent<Health_chaos>();
                if (mauQuai == null) mauQuai = collision.GetComponentInParent<Health_chaos>();

                if (mauQuai != null)
                {
                    mauQuai.TakeDamage(satThuong);
                }
            }

            // Tắt đạn trả về kho Pooling
            daKichHoat = false;
            gameObject.SetActive(false);
        }
    }

    // Hàm vẽ vòng tròn biểu diễn phạm vi nổ ngoài Scene (Chỉ nhìn thấy trong cửa sổ Editor)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, banKinhNoLan);
    }
}