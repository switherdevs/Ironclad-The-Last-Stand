using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float lifeTime = 5f;
  
    private Rigidbody2D rb;
    private Transform target;
    private float speed;
    private float curveAmount; // Độ cong của quỹ đạo bay
    private bool isCurveBullet = false;

    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    /// <summary>
    /// Hàm khởi tạo thông số đạn bay từ quái vật bắn ra.
    /// </summary>
    /// <param name="targetTransform">Mục tiêu hướng tới</param>
    /// <param name="bulletSpeed">Tốc độ đạn</param>
    /// <param name="curve">Độ cong (0: bay thẳng, số khác 0: bay cong)</param>
    public void InitializeProjectile(Transform targetTransform, float bulletSpeed, float curve)
    {
        target = targetTransform;
        speed = bulletSpeed;
        curveAmount = curve;

        // Nếu độ cong khác 0 thì kích hoạt cơ chế đạn bay đường cong
        isCurveBullet = Mathf.Abs(curve) > 0.01f;

        // Nếu là đạn bay thẳng thông thường, gán vận tốc ngay lập tức để tối ưu
        if (!isCurveBullet && target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
    }

    private void FixedUpdate()
    {
        // Logic điều khiển đạn bay đường cong bằng lực ép vật lý (Steering Force)
        if (isCurveBullet && target != null)
        {
            Vector2 directionToTarget = (Vector2)target.position - rb.position;
            directionToTarget.Normalize();

            // Tính toán lực xoáy vuông góc với hướng đi để tạo độ cong bọc lót hai bên
            Vector2 curveDirection = Vector2.Perpendicular(directionToTarget) * curveAmount;

            // Tổng hợp hướng bay mới kết hợp giữa hướng mục tiêu và độ cong lượn
            Vector2 finalVelocity = (directionToTarget + curveDirection).normalized * speed;

            rb.linearVelocity = finalVelocity;

            // Xoay đầu mũi mũi tên viên đạn theo hướng vận tốc thực tế
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Phechinh"))
        {
            Destroy(gameObject);
        }
    }
}