using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class ChargerExplosion : MonoBehaviour
{
    private int damage;
    private float duration;
    private float radius;

    private readonly HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();
    private CircleCollider2D explosionCollider;
    private TrailRenderer line;

    private void Awake()
    {
        // ĐÃ SỬA: Lấy tất cả linh kiện ở Awake để OnEnable không bị lỗi NullReference
        explosionCollider = GetComponent<CircleCollider2D>();
        explosionCollider.isTrigger = true;

        line = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        // An toàn 100%, không bao giờ bị báo lỗi đỏ nữa
        if (line != null)
        {
            line.enabled = true;
        }
    }

    public void Initialize(int explosionDamage, float explosionRadius, float explosionDuration)
    {
        damage = explosionDamage;
        radius = explosionRadius;
        duration = explosionDuration;

        if (explosionCollider != null)
        {
            explosionCollider.radius = radius;
        }

        // Tự động xóa quả nổ sau khi hết thời gian duration
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu mục tiêu không thuộc phe chính hoặc sân nhà, hoặc đã dính sát thương rồi thì bỏ qua
        if (!other.CompareTag("Phechinh") && !other.CompareTag("Sannha")) return;
        if (hitTargets.Contains(other)) return;

        // Lưu mục tiêu vào danh sách đã trúng đòn để không bị đa sát thương (tránh nhâsn dame)
        hitTargets.Add(other);

        // Kiểm tra gây sát thương cho Phechinh
        if (other.CompareTag("Phechinh"))
        {
            var healthComponent = other.GetComponent<Health_phechinh>();
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(damage);
                Debug.Log($"💥 Charger nổ gây {damage} ST lên {other.name} (Phechinh)!");
            }
        }
        // Kiểm tra gây sát thương cho Sân nhà
        else if (other.CompareTag("Sannha"))
        {
            // Nếu sau này Sân Nhà của bạn có script máu riêng (ví dụ: Health_sannha), hãy sửa tương tự như Phechinh ở đây
            Debug.Log($"💥 Sân nhà [{other.name}] bị dính đòn nổ! Gây {damage} sát thương.");
        }
        Destroy(gameObject);
    }

    // ĐÃ THÊM: Khi quả nổ bị hủy hoàn toàn khỏi game, ta mới dọn dẹp sạch sẽ danh sách tránh rác bộ nhớ
    private void OnDestroy()
    {
        hitTargets.Clear();
    }
}