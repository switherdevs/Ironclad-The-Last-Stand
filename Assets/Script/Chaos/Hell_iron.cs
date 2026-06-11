using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCharger : MonoBehaviour
{
    public enum ChargerState { FastRangedRun, Exploding, MeleeChasing, MeleeAttacking, Dead }

    [Header("--- CURRENT STATE ---")]
    [SerializeField] private ChargerState currentChargerState = ChargerState.FastRangedRun;
    public Health_chaos deads;

    [Header("--- PHASE 1: RANGED RUN ---")]
    [SerializeField] private float fastMoveSpeed = 12f;
    [SerializeField] private float leftMoveBias = 0.6f;
    [SerializeField] private float detectionRange = 16f;
    [SerializeField] private int normalHP = 20;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private GameObject projectilePrefab;

    [Header("--- DETONATION OPTIMIZATION (INSPECTOR) ---")]
    [Tooltip("X = Forward/Backward, Y = Up/Down")]
    [SerializeField] private Vector2 hitboxOffset = new Vector2(1.6f, 0f);
    [SerializeField] private float chargeHitboxRadius = 1.8f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private int explosionDamage = 40;
    [SerializeField] private float explosionDuration = 0.5f;

    [Header("--- PHASE 2: MELEE ATTACK ---")]
    [SerializeField] private float meleeMoveSpeed = 5f;
    [SerializeField] private float meleeAttackRange = 1.5f;
    //[SerializeField] private int meleeDamage = 15;
    [SerializeField] private float meleeAttackCooldown = 1.5f;
    [SerializeField] private float hitboxActiveDuration = 0.2f;
    [SerializeField] private GameObject meleeHitboxObject;

    private int currentHP;
    private float nextAttackTime;
    private float nextMeleeAttackTime;
    private Rigidbody2D rb;
    private Transform targetTransform;
    private SpriteRenderer spriteRenderer;
    private Animator HellIron_animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        HellIron_animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        currentHP = normalHP;
        if (meleeHitboxObject != null) meleeHitboxObject.SetActive(false);
        FindTarget();
    }

    private void Update()
    {
        if (currentChargerState == ChargerState.Dead) return;

        if (targetTransform == null)
        {
            FindTarget();
            if (targetTransform == null)
            {
                rb.linearVelocity = Vector2.zero;
                // SỬA: Gọi trạng thái đứng yên
                if (HellIron_animator != null) HellIron_animator.CrossFade("Chaos_dead_idle", 0.1f);
                return;
            }
        }

        FlipSprite();

        switch (currentChargerState)
        {
            case ChargerState.FastRangedRun:
                HandleFastRangedRun();
                break;
            case ChargerState.MeleeChasing:
                HandleMeleeChasing();
                break;
            case ChargerState.MeleeAttacking:
                break;
        }
    }

    private void FindTarget()
    {
        GameObject target = GameObject.FindWithTag("Phechinh");
        if (target != null) targetTransform = target.transform;
    }

    private void FlipSprite()
    {
        if (targetTransform == null || spriteRenderer == null) return;
        if (targetTransform.position.x < transform.position.x)
            spriteRenderer.transform.localScale = new Vector3(-1f, 1f, 1f);
        else
            spriteRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void HandleFastRangedRun()
    {
        // SỬA: Ép chạy hoạt ảnh lao lên siêu tốc bằng CrossFade
        if (HellIron_animator != null) HellIron_animator.SetBool("Chaos_dead_run", true);

        float distance = Vector2.Distance(transform.position, targetTransform.position);

        if (distance <= chargeHitboxRadius + 0.5f)
        {
            StartCoroutine(DetonateRoutine());
            return;
        }

        Vector2 direction = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
        direction.y += (targetTransform.position.y > transform.position.y) ? leftMoveBias : -leftMoveBias;
        direction = direction.normalized;

        rb.linearVelocity = direction * fastMoveSpeed;

        if (Time.time >= nextAttackTime && distance <= detectionRange)
        {
            FireRangedProjectile();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void FireRangedProjectile()
    {
        if (projectilePrefab == null) return;
        Vector2 dir = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 spawnPos = transform.position + (Vector3)(dir * 1.2f);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.bodyType = RigidbodyType2D.Dynamic;
            projRb.gravityScale = 0f;
            projRb.linearVelocity = dir * projectileSpeed;
        }
    }

    private IEnumerator DetonateRoutine()
    {
        currentChargerState = ChargerState.Exploding;

        rb.linearVelocity = Vector2.zero;

        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(exp, explosionDuration);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Phechinh"))
            {
                Health_phechinh hp = hit.GetComponent<Health_phechinh>();
                if (hp != null) hp.TakeDamage(explosionDamage);
            }
        }

        yield return new WaitForSeconds(0.1f);
        currentChargerState = ChargerState.MeleeChasing;
    }

    private void HandleMeleeChasing()
    {
        // SỬA: Khi đổi sang cận chiến đi bộ, gọi hoạt ảnh di chuyển bộ (tận dụng lại run hoặc walk tùy bạn cấu hình, ở đây giữ đi bộ/chạy mượt)
        if (HellIron_animator != null) HellIron_animator.SetBool("Chaos_dead_run", true);

        float distance = Vector2.Distance(transform.position, targetTransform.position);

        if (distance <= meleeAttackRange)
        {
            rb.linearVelocity = Vector2.zero;
            if (Time.time >= nextMeleeAttackTime)
            {
                StartCoroutine(MeleeAttackRoutine());
            }
            return;
        }

        Vector2 direction = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * meleeMoveSpeed;
    }

    private IEnumerator MeleeAttackRoutine()
    {
        currentChargerState = ChargerState.MeleeAttacking;
        if (HellIron_animator != null) HellIron_animator.SetBool("Chaos_dead_run", false);

        // SỬA: Đập búa cận chiến mượt mà
        if (HellIron_animator != null) HellIron_animator.SetBool("chao_dead_attack", true);

        RotateMeleeHitbox();
        if (meleeHitboxObject != null) meleeHitboxObject.SetActive(true);

        yield return new WaitForSeconds(hitboxActiveDuration);

        if (meleeHitboxObject != null) meleeHitboxObject.SetActive(false);

        nextMeleeAttackTime = Time.time + meleeAttackCooldown;
        currentChargerState = ChargerState.MeleeChasing;
        if (HellIron_animator != null) HellIron_animator.SetBool("chao_dead_attack", false);

    }

    private void RotateMeleeHitbox()
    {
        if (meleeHitboxObject == null) return;
        float lookDir = (targetTransform != null && targetTransform.position.x < transform.position.x) ? -1f : 1f;
        float currentPosX = Mathf.Abs(meleeHitboxObject.transform.localPosition.x);
        meleeHitboxObject.transform.localPosition = new Vector3(currentPosX * lookDir, meleeHitboxObject.transform.localPosition.y, meleeHitboxObject.transform.localPosition.z);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, normalHP);
        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        currentChargerState = ChargerState.Dead;

        // KHÔNG ĐỤNG ĐẾN ĐOẠN ANIMATION CHẾT THEO YÊU CẦU
        if (HellIron_animator != null)
            HellIron_animator.SetBool("Chaos_dead_run", false);
    }
    // ── [DEBUG] Hiển thị phạm vi nổ đi theo Offset trong cửa sổ Scene ───────
    private void OnDrawGizmosSelected()
    {
        // Tính toán vị trí tâm nổ thực tế dựa trên Offset
        // Cần nhân với localScale.x để offset quay hướng theo quái
        Vector3 pointOffset = transform.position + new Vector3(hitboxOffset.x * transform.localScale.x, hitboxOffset.y, 0);

        // Vẽ vòng tròn phạm vi nổ (Màu đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pointOffset, explosionRadius);

        // Vẽ tâm nổ (Màu vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pointOffset, 0.1f);

        // Vẽ đường nối từ tâm quái đến tâm nổ để dễ quan sát (Màu trắng)
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, pointOffset);
    }
    // ────────────────────────────────────────────────────────────────────────
}