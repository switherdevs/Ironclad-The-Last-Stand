using UnityEngine;
using System.Collections;

public class EnemyBurst : BaseEnemy
{
    [Header("--- BURST LINE SETTINGS ---")]
    [SerializeField] private float burstDelay = 0.15f;
    [SerializeField] private float spawnOffsetDistance = 1.0f;
    private AudioSource Amthanh;
    [SerializeField]
    private AudioClip Shoot;

    private Animator ChaosTerminator_animator;
    private Vector3 viTriKhungHinhTruoc;
    protected override void Awake()
    {
        base.Awake();
        ChaosTerminator_animator = GetComponentInChildren<Animator>();
        Amthanh = GetComponent<AudioSource>();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void HandleMovement()
    {
        base.HandleMovement();

        if (ChaosTerminator_animator != null)
        {
            // Block toàn bộ animation logic khi đang tấn công
            if (currentState == EnemyState.Attacking) return;

            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            if (dangDiChuyen)
                ChaosTerminator_animator.SetBool("ChaosTerminator_isWalking", true);
            else
                ChaosTerminator_animator.SetBool("ChaosTerminator_isWalking", false);
        }

        viTriKhungHinhTruoc = transform.position;
    }

    protected override void ExecuteAttackPattern()
    {
        currentState = EnemyState.Attacking;
        StartCoroutine(BurstFireRoutine());
    }

    private IEnumerator BurstFireRoutine()
    {
        // ── [SỬA ĐỔI] Khi vào trạng thái tấn công: Đứng yên chuẩn bị bắn ──
        if (ChaosTerminator_animator != null)
            ChaosTerminator_animator.SetBool("ChaosTerminator_isWalking", false);

        for (int i = 0; i < 3; i++)
        {
            // ── [SỬA ĐỔI] Đạn sắp ra: Kích hoạt ngay animation bắn chớp nhoáng ──
            if (ChaosTerminator_animator != null)
                ChaosTerminator_animator.SetBool("ChaosTerminator_isShooting", true);
            Amthanh.PlayOneShot(Shoot);
            FireSingleLinearProjectile();
            yield return new WaitForSeconds(burstDelay);
        }
        ChaosTerminator_animator.SetBool("ChaosTerminator_isShooting", false);

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