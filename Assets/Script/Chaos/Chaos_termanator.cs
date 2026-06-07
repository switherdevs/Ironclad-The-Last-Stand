using UnityEngine;
using System.Collections;

public class EnemyBurst : BaseEnemy
{
    [Header("--- BURST LINE SETTINGS ---")]
    [SerializeField] private float burstDelay = 0.15f;
    [SerializeField] private float spawnOffsetDistance = 1.0f;

    // ── [ANIMATION] Khai báo Animator ───────────────────────────────────────
    private Animator ChaosTerminator_animator;
    private Vector3 viTriKhungHinhTruoc;
    // ────────────────────────────────────────────────────────────────────────

    protected override void Start()
    {
        base.Start();

        // ── [ANIMATION] Lấy Animator từ children ────────────────────────────
        ChaosTerminator_animator = GetComponentInChildren<Animator>();
        // ────────────────────────────────────────────────────────────────────
    }

    protected override void HandleMovement()
    {
        base.HandleMovement();

        // ── [ANIMATION] Cập nhật isWalking theo vị trí thực tế ──────────────
        if (ChaosTerminator_animator != null)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            ChaosTerminator_animator.SetBool("ChaosTerminator_isWalking", dangDiChuyen);
        }
        viTriKhungHinhTruoc = transform.position;
        // ────────────────────────────────────────────────────────────────────
    }

    protected override void ExecuteAttackPattern()
    {
        currentState = EnemyState.Attacking;
        StartCoroutine(BurstAttackRoutine());
    }

    private IEnumerator BurstAttackRoutine()
    {
        // ── [ANIMATION] Bật isShooting khi bắt đầu loạt bắn ────────────────
        if (ChaosTerminator_animator != null)
            ChaosTerminator_animator.SetBool("ChaosTerminator_isShooting", true);
        // ────────────────────────────────────────────────────────────────────

        for (int i = 0; i < 3; i++)
        {
            if (targetTransform != null)
                FireSingleLinearProjectile();

            yield return new WaitForSeconds(burstDelay);
        }

        // ── [ANIMATION] Tắt isShooting sau khi bắn xong loạt ───────────────
        if (ChaosTerminator_animator != null)
            ChaosTerminator_animator.SetBool("ChaosTerminator_isShooting", false);
        // ────────────────────────────────────────────────────────────────────

        nextAttackTime = Time.time + attackCooldown;
        currentState = EnemyState.Chasing;
    }

    private void FireSingleLinearProjectile()
    {
        if (projectilePrefab == null || targetTransform == null) return;

        Vector2 fireDirection = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

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