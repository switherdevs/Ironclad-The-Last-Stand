using System.Collections;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EnemyHolding : BaseEnemy
{
    [Header("--- HOLDING SPECIFIC ---")]
    [SerializeField] private GameObject holdingObjectPrefab;
    [SerializeField] private float holdingDuration = 5f;
    [SerializeField] private float scalePerSecond = 0.5f;
    [SerializeField] private float maxScaleTime = 5f;
    [SerializeField] private int holdingHPMultiplier = 2;
    [SerializeField] private float spawnOffsetDistance = 1.2f;

    private GameObject spawnedHoldingObj;

    // ── [ANIMATION] Khai báo Animator ───────────────────────────────────────
    private Animator WarpMarine_animator;
    private Vector3 viTriKhungHinhTruoc;
    // ────────────────────────────────────────────────────────────────────────

    protected override void Start()
    {
        base.Start();

        // ── [ANIMATION] Lấy Animator từ children ────────────────────────────
        WarpMarine_animator = GetComponentInChildren<Animator>();
        // ────────────────────────────────────────────────────────────────────
    }

    protected override void HandleMovement()
    {
        base.HandleMovement();

        // ── [ANIMATION] SỬA: Thay đổi hoàn toàn hệ thống SetBool di chuyển bằng CrossFade ──
        if (WarpMarine_animator != null)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            if (dangDiChuyen)
            {
                WarpMarine_animator.CrossFade("WarpMarine_walk", 0.1f);
            }
            else
            {
                // Ngăn chặn việc đè trạng thái đứng yên khi đang gồng hoặc đang bắn
                if (currentState != EnemyState.Attacking && currentState != EnemyState.Cooldown)
                {
                    WarpMarine_animator.CrossFade("WarpMarine_idle", 0.1f);
                }
            }
        }
        viTriKhungHinhTruoc = transform.position;
        // ────────────────────────────────────────────────────────────────────
    }

    protected override void ExecuteAttackPattern()
    {
        currentState = EnemyState.Attacking;
        StartCoroutine(HoldingRoutine());
    }

    private IEnumerator HoldingRoutine()
    {
        Vector2 fireDirection = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
        Vector3 spawnPosition = transform.position + (Vector3)(fireDirection * spawnOffsetDistance);

        if (holdingObjectPrefab != null)
        {
            spawnedHoldingObj = Instantiate(holdingObjectPrefab, spawnPosition, Quaternion.identity);
            spawnedHoldingObj.transform.SetParent(transform);
        }

        currentMaxHP = normalHP * holdingHPMultiplier;
        currentHP = currentMaxHP;

        float timer = 0f;

        // ── [ANIMATION] SỬA GIAI ĐOẠN 1: Bắt đầu nén năng lượng tụ đạn ──
        if (WarpMarine_animator != null)
            WarpMarine_animator.CrossFade("WarpMarine_holding", 0.1f);
        // ────────────────────────────────────────────────────────────────────

        while (timer < holdingDuration)
        {
            if (spawnedHoldingObj != null && timer < maxScaleTime)
            {
                spawnedHoldingObj.transform.localScale += Vector3.one * (scalePerSecond * Time.deltaTime);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        currentState = EnemyState.Attacking;

        if (spawnedHoldingObj != null) Destroy(spawnedHoldingObj);

        // ── [ANIMATION] SỬA GIAI ĐOẠN 2: Ngay lập tức giải phóng đòn bắn (Hòa trộn cực nhanh 0.02s giúp nối tiếp đòn gồng siêu mượt) ──
        if (WarpMarine_animator != null)
            WarpMarine_animator.CrossFade("WarpMarine_fire", 0.02f);
        // ────────────────────────────────────────────────────────────────────

        FireProjectile();

        currentMaxHP = normalHP;
        currentHP = Mathf.Clamp(currentHP, 0, currentMaxHP);

        currentState = EnemyState.Cooldown;
        nextAttackTime = Time.time + attackCooldown;

        // Trả lại trạng thái Chasing sau khi diễn hoạt hoạt ảnh bắn kết thúc mượt mà
        yield return new WaitForSeconds(0.3f);
        currentState = EnemyState.Chasing;
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || targetTransform == null) return;

        Vector2 fireDirection = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;

        Vector3 spawnPosition = transform.position + (Vector3)(fireDirection * spawnOffsetDistance);

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
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