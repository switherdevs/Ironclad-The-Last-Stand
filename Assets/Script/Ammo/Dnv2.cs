using UnityEngine;
using System.Collections.Generic;
public class Dannv2 : MonoBehaviour
{
    public khodanan cauHinhDan;
    [HideInInspector] public int satThuong = 100; // Sẽ được nhân vật nạp đam vào khi bắn

    private float demThoiGian;
    private bool daKichHoat = false;

    public void KichHoatVienDan()
    {
        if (cauHinhDan != null)
        {
            demThoiGian = cauHinhDan.Duytri;
        }
        else
        {
            demThoiGian = 3f; // Thời gian tự hủy dự phòng nếu thiếu cấu hình
        }
        daKichHoat = true;
    }

    void Update()
    {
        if (!daKichHoat) return;

        // Lấy tốc độ từ cấu hình, nếu không có thì mặc định bằng 8f
        float tocDo = (cauHinhDan != null) ? cauHinhDan.tocDobay : 8f;

        // Xác định hướng bay dựa trên hướng mặt của viên đạn
        float huongDi = (transform.right.x >= 0) ? 1f : -1f;

        // Di chuyển đạn theo trục thế giới độc lập
        transform.Translate(Vector3.right * huongDi * tocDo * Time.deltaTime, Space.World);

        // Tính thời gian tự hủy trả về kho pooling
        demThoiGian -= Time.deltaTime;
        if (demThoiGian <= 0f)
        {
            ThanhCongTraDan();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi chạm trúng mục tiêu có Tag là Enemy
        if (collision.CompareTag("Enemy"))
        {
            Health_chaos mauQuai = collision.GetComponent<Health_chaos>();
            if (mauQuai == null) mauQuai = collision.GetComponentInParent<Health_chaos>();

            if (mauQuai != null)
            {
                // Gây sát thương đơn mục tiêu (lượng sát thương lớn từ Nhân Vật 4 truyền sang)
                mauQuai.TakeDamage(satThuong);
                Debug.Log($"🎯 Đạn đánh trúng {collision.name}, gây {satThuong} sát thương đơn!");
            }

            // Biến mất ngay sau khi chạm mục tiêu
            ThanhCongTraDan();
        }
    }

    void ThanhCongTraDan()
    {
        daKichHoat = false;
        gameObject.SetActive(false);
    }
}