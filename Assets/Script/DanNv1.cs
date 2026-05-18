using UnityEngine;

public class DanNV1 : MonoBehaviour
{
    public khodanan cauHinhDan; // Kéo file ScriptableObject khodanan vào đây trên Prefab
    public int satThuong = 10;
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
            // Phòng hờ nếu quên kéo file cấu hình ngoài Unity
            demThoiGian = 3f;
            daKichHoat = true;
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (cauHinhDan == null) return;

        // Đạn bay thẳng theo tốc độ trong khodanan
        transform.Translate(Vector2.right * cauHinhDan.tocDobay * Time.deltaTime, Space.Self);

        demThoiGian -= Time.deltaTime;
        if (demThoiGian <= 0f)
        {
            daKichHoat = false;
            // CHUẨN POOLING:chỉ ẩn đi để trả về kho
            gameObject.SetActive(false);
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Xử lý trừ máu quái tại đây (nếu có script máu)
            Debug.Log("Trúng quái!");
            // Lập tức ẩn đạn đi để trả về kho Pooling
            daKichHoat = false;
            gameObject.SetActive(false);
        }
    }
}