using UnityEngine;

public class SentryBullet : MonoBehaviour
{
    public float tocDoBay = 15f;
    public float satThuong = 25f;
    [Tooltip("Số lượng quái tối đa viên đạn có thể xuyên qua trước khi tự hủy.")]
    public int soLuongXuyenThauToiDa = 3;

    private int soLuongQuaiDaXuyenQua = 0;

    private void Update()
    {
        // Đạn tự bay về phía trước dựa theo hướng nòng súng lúc sinh ra
        transform.Translate(Vector3.right * tocDoBay * Time.deltaTime);
    }

    // 🌟 THUẬT TOÁN XUYÊN THẤU BẰNG TRIGGER
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Gọi hàm trừ máu quái tại đây (ví dụ con quái dùng script EnemyHealth)
            // collision.GetComponent<EnemyHealth>().TruMau(satThuong);

            Debug.Log("Đạn Sentry xuyên qua: " + collision.name);

            soLuongQuaiDaXuyenQua++;

            // Chỉ tự hủy khi đã xiên đủ số lượng quái quy định
            if (soLuongQuaiDaXuyenQua >= soLuongXuyenThauToiDa)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnBecameInvisible()
    {
        // Tự động xóa viên đạn khi bay ra ngoài màn hình để tránh nghẽn RAM
        Destroy(gameObject);
    }
}