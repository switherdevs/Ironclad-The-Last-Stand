using UnityEngine;
using System.Collections;

public class EnemyBurst : BaseEnemy
{
    [Header("--- BURST LINE SETTINGS ---")]
    [SerializeField] private float burstDelay = 0.15f; // Thời gian giãn cách giữa mỗi viên (giây)
    [SerializeField] private float spawnOffsetDistance = 1.0f; // Đẩy điểm bắn ra trước mặt

    protected override void ExecuteAttackPattern()
    {
        currentState = EnemyState.Attacking;
        // Sử dụng Coroutine để bắn tuần tự theo thời gian
        StartCoroutine(BurstAttackRoutine());
    }

    private IEnumerator BurstAttackRoutine()
    {
        for (int i = 0; i < 3; i++)
        {
            if (targetTransform != null)
            {
                FireSingleLinearProjectile();
            }
            yield return new WaitForSeconds(burstDelay);
        }

        nextAttackTime = Time.time + attackCooldown;
        currentState = EnemyState.Chasing;
    }

    private void FireSingleLinearProjectile()
    {
        if (projectilePrefab == null || targetTransform == null) return;

        // Khóa Vector hướng bay thẳng về phía Player
        Vector2 fireDirection = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

        // Đẩy điểm xuất phát đạn ra trước mặt quái
        Vector3 spawnPosition = transform.position + (Vector3)(fireDirection * spawnOffsetDistance);

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.Euler(0f, 0f, angle));

        SpriteRenderer projSR = projectile.GetComponent<SpriteRenderer>();
        if (projSR != null && spriteRenderer != null)
        {
            projSR.sortingOrder = spriteRenderer.sortingOrder + 2;
        }

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.bodyType = RigidbodyType2D.Dynamic;
            projRb.gravityScale = 0f;
            projRb.linearVelocity = fireDirection * projectileSpeed;
        }
    }
}