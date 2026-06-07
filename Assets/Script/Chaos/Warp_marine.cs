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

        // ── [ANIMATION] Cập nhật isWalking theo vị trí thực tế ──────────────
        if (WarpMarine_animator != null)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            WarpMarine_animator.SetBool("WarpMarine_isWalking", dangDiChuyen);
        }
        viTriKhungHinhTruoc = transform.position;
        // ────────────────────────────────────────────────────────────────────
    }

    protected override void ExecuteAttackPattern()
    {
        currentState = EnemyState.Holding;
        StartCoroutine(HoldingRoutine());
    }

    private IEnumerator HoldingRoutine()
    {
        int hpDeficit = currentMaxHP - currentHP;
        currentMaxHP = normalHP * holdingHPMultiplier;
        currentHP = currentMaxHP - hpDeficit;

        if (holdingObjectPrefab != null)
        {
            Vector3 holdingSpawnPos = transform.position + new Vector3(GetLookDirection() * 0.5f, 0f, 0f);
            spawnedHoldingObj = Instantiate(holdingObjectPrefab, holdingSpawnPos, Quaternion.identity, transform);

            SpriteRenderer holdingSR = spawnedHoldingObj.GetComponent<SpriteRenderer>();
            if (holdingSR != null && spriteRenderer != null)
            {
                holdingSR.sortingOrder = spriteRenderer.sortingOrder + 1;
            }
        }

        // ── [ANIMATION] Bật isCharging khi bắt đầu nén năng lượng ──────────
        // isCharging giữ nguyên suốt holdingDuration, đồng bộ với hiệu ứng scale
        if (WarpMarine_animator != null)
            WarpMarine_animator.SetBool("WarpMarine_isCharging", true);
        // ────────────────────────────────────────────────────────────────────

        float elapsed = 0f;
        float scaleTimer = 0f;

        while (elapsed < holdingDuration)
        {
            elapsed += Time.deltaTime;
            scaleTimer += Time.deltaTime;

            if (scaleTimer >= 1f && elapsed <= maxScaleTime && spawnedHoldingObj != null)
            {
                spawnedHoldingObj.transform.localScale += new Vector3(scalePerSecond, scalePerSecond, 0f);
                scaleTimer = 0f;
            }
            yield return null;
        }

        // ── [ANIMATION] Tắt isCharging, kích hoạt isShooting khi phóng ──────
        if (WarpMarine_animator != null)
        {
            WarpMarine_animator.SetBool("WarpMarine_isCharging", false);
            WarpMarine_animator.SetTrigger("WarpMarine_doShoot");
        }
        // ────────────────────────────────────────────────────────────────────

        currentState = EnemyState.Attacking;

        if (spawnedHoldingObj != null) Destroy(spawnedHoldingObj);

        FireProjectile();

        currentMaxHP = normalHP;
        currentHP = Mathf.Clamp(currentHP, 0, currentMaxHP);

        currentState = EnemyState.Cooldown;
        nextAttackTime = Time.time + attackCooldown;
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