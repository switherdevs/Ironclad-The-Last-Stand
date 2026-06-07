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

    private void Awake()
    {
        explosionCollider = GetComponent<CircleCollider2D>();
        explosionCollider.isTrigger = true;
    }

    public void Initialize(int explosionDamage, float explosionRadius, float explosionDuration)
    {
        damage = explosionDamage;
        radius = explosionRadius;
        duration = explosionDuration;

        explosionCollider.radius = radius;

        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Phechinh") || other.CompareTag("Sannha")) && !hitTargets.Contains(other))
        {
            hitTargets.Add(other);

            // Kiểm tra xem đối tượng bị trúng nổ có script PhechinhHealth (file ETTH.cs) không
            var healthComponent = other.GetComponent<Health_phechinh>();
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(damage);
                Debug.Log($"💥 Charger nổ gây {damage} ST lên {other.name} (Phechinh)!");
            }
            else if (other.CompareTag("Sannha"))
            {
                // Nếu trúng Sân Nhà mà chưa có script máu riêng, tạm thời log ra để không bị lỗi game
                Debug.Log($"💥 Sân nhà bị dính đòn nổ! Gây {damage} sát thương.");
            }
        }
    }
}