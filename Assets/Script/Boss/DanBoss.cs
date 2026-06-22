using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CircleCollider2D))]
public class BossDamageArea : MonoBehaviour
{
    private CircleCollider2D circleCollider;
    private int damage;
    private bool hasDealtDamage = false;
    private AudioSource Amthanh;
    [SerializeField] private AudioClip TiengNo;

    private void Awake()
    {
        Amthanh = GetComponent<AudioSource>();
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.enabled = false; // Tạm khóa collider khi chưa được kích hoạt
    }

    public IEnumerator InitializeDamageAreaRoutine(int dmgValue, float radius, float duration)
    {
        damage = dmgValue;
        circleCollider.radius = radius;
        circleCollider.enabled = true; // Mở collider ra để quét Trigger trong 1 Frame duy nhất
        Amthanh.PlayOneShot(TiengNo);

        yield return new WaitForFixedUpdate(); // Chờ hệ thống vật lý 2D cập nhật xong Frame đó

        circleCollider.enabled = false; // Tắt ngay lập tức để bảo đảm cơ chế CHỈ gây sát thương 1 lần duy nhất

        // Giữ lại Object tồn tại trên màn hình đủ thời gian Duration (để chạy Effect nổ/Anim nếu có) trước khi tự hủy
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasDealtDamage) return;

        // Kiểm tra xem mục tiêu có đúng là Phechinh hay Sannha không
        if (collision.CompareTag("Phechinh") || collision.CompareTag("Sannha"))
        {
            // Thay đổi "PhechinhHealth" hoặc "BaseHealth" tương ứng với tên script quản lý HP trong dự án của bạn
            var healthComponent = collision.GetComponent<Health_phechinh>();
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(damage);
                hasDealtDamage = true; // Khóa bảo vệ chống trùng lặp dữ liệu sát thương
            }
        }
    }
}