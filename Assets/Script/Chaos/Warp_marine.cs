using System.Collections;
using UnityEngine;

public class EnemyHolding : BaseEnemy
{
    [Header("--- HOLDING SPECIFIC ---")]
    [SerializeField] private GameObject holdingObjectPrefab;
    [SerializeField] private float holdingDuration = 5f;
    [SerializeField] private float scalePerSecond = 0.5f;
    [SerializeField] private float maxScaleTime = 5f;
    [SerializeField] private int holdingHPMultiplier = 2;
    [SerializeField] private float spawnOffsetDistance = 1.2f;
    [SerializeField] private GameObject Light2;

    private GameObject spawnedHoldingObj;
    private Animator WarpMarine_animator;
    private Vector3 viTriKhungHinhTruoc;

    protected override void Awake()
    {
        base.Awake();
        WarpMarine_animator = GetComponentInChildren<Animator>();
    }

    protected override void Start()
    {
        base.Start();
        Light2.SetActive(false);
    }

    protected override void HandleMovement()
    {
        base.HandleMovement();

        if (currentState == EnemyState.Attacking || currentState == EnemyState.Cooldown)
        {
            viTriKhungHinhTruoc = transform.position;
            return;
        }

        if (WarpMarine_animator != null && WarpMarine_animator.gameObject.activeInHierarchy)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            if (dangDiChuyen)
                WarpMarine_animator.CrossFade("Move_warpmarine", 0.1f, 0);
            else
                WarpMarine_animator.CrossFade("idle_warpmarine", 0.1f, 0);
        }

        viTriKhungHinhTruoc = transform.position;
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

        while (timer < holdingDuration)
        {
            if (spawnedHoldingObj != null && timer < maxScaleTime)
                spawnedHoldingObj.transform.localScale += Vector3.one * (scalePerSecond * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        currentState = EnemyState.Attacking;

        if (spawnedHoldingObj != null) Destroy(spawnedHoldingObj);

        if (WarpMarine_animator != null && WarpMarine_animator.gameObject.activeInHierarchy)
            WarpMarine_animator.CrossFade("Shoot_warp", 0.02f, 0);
            Light2.SetActive(true);

        FireProjectile();

        currentMaxHP = normalHP;
        currentHP = Mathf.Clamp(currentHP, 0, currentMaxHP);

        currentState = EnemyState.Cooldown;
        nextAttackTime = Time.time + attackCooldown;

        yield return new WaitForSeconds(0.3f);
        Light2.SetActive(false);


        if (WarpMarine_animator != null && WarpMarine_animator.gameObject.activeInHierarchy)
            WarpMarine_animator.CrossFade("idle_warpmarine", 0.2f, 0);

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
            projSR.sortingOrder = spriteRenderer.sortingOrder + 2;

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.bodyType = RigidbodyType2D.Dynamic;
            projRb.gravityScale = 0f;
            projRb.linearVelocity = fireDirection * projectileSpeed;
        }
    }
}