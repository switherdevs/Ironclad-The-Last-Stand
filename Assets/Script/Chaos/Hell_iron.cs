using GLTFast.Schema;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCharger : MonoBehaviour
{
    public enum ChargerState { FastRangedRun, Exploding, MeleeChasing, MeleeAttacking, Dead }

    [Header("--- CURRENT STATE ---")]
    [SerializeField] private ChargerState currentChargerState = ChargerState.FastRangedRun;

    [Header("--- PHASE 1: RANGED RUN ---")]
    [SerializeField] private float fastMoveSpeed = 12f;
    [SerializeField] private float leftMoveBias = 0.6f;
    [SerializeField] private float detectionRange = 16f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private GameObject projectilePrefab;

    // 🎯 THÊM OBJECT RỖNG LÀM NÒNG SÚNG (Giữ nguyên toàn bộ các biến xung quanh)
    [Tooltip("Kéo Object rỗng làm nòng súng bắn đạn vào đây")]
    [SerializeField] private Transform firePoint;

    [Header("--- DETONATION OPTIMIZATION (INSPECTOR) ---")]
    [Tooltip("X = Forward/Backward, Y = Up/Down")]
    [SerializeField] private Vector2 hitboxOffset = new Vector2(1.6f, 0f);
    [SerializeField] private float chargeHitboxRadius = 1.8f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private int explosionDamage = 40;
    [SerializeField] private float explosionDuration = 0.5f;

    [Header("--- PHASE 2: MELEE ---")]
    [SerializeField] private float meleeMoveSpeed = 5f;
    [SerializeField] private float meleeAttackRange = 1.3f;
    [SerializeField] private int meleeDamage = 15;
    [SerializeField] private float meleeAttackCooldown = 1.2f;
    [SerializeField] private GameObject meleeHitboxObject;
    [SerializeField] private float meleeHitboxDuration = 0.2f;
    private AudioSource Amthanh;
    [SerializeField]
    private AudioClip Shoot;
    private Animator animator;

    private bool hasExploded = false;
    private float nextAttackTime;
    private float nextMeleeAttackTime;

    private Rigidbody2D rb;
    private Collider2D monsterCollider;
    private Transform targetTransform;
    private Vector2 lastLookDirection = Vector2.left;
    private Health_chaos mauChaos;

    private const string TAG_PHE_CHINH = "Phechinh";
    private const string TAG_SAN_NHA = "Sannha";

    private void Awake()
    {
        mauChaos = GetComponent<Health_chaos>();
        rb = GetComponent<Rigidbody2D>();
        monsterCollider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();
        Amthanh = GetComponent<AudioSource>();
        ConfigurePhysics();
    }

    private void Start()
    {
        if (meleeHitboxObject != null) meleeHitboxObject.SetActive(false);
        nextAttackTime = Time.time;
    }

    private void Update()
    {
        if (mauChaos.Deadre)
        {
            return;
        }

        if (currentChargerState == ChargerState.Dead) return;

        HandleCoreLogicTransitions();
        HandleRangedShooting();
    }

    private void FixedUpdate()
    {
        // CHỖ SỬA YÊU CẦU: Triệt tiêu vận tốc ngay khi hết máu để ngăn hành vi trượt đi vô tận
        if (mauChaos.Deadre)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }
        if (currentChargerState == ChargerState.Dead || rb == null) return;


        if (rb.IsSleeping()) rb.WakeUp();
        if (currentChargerState == ChargerState.FastRangedRun || currentChargerState == ChargerState.MeleeChasing)
        {
            FindClosestPriorityTarget();
        }

        HandlePhysicsMovement();
    }

    private void ConfigurePhysics()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.linearDamping = 0f;
        }
    }

    private void FindClosestPriorityTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        Transform nearestSannha = null;
        Transform nearestPhechinh = null;
        float minDistSannha = Mathf.Infinity;
        float minDistPhechinh = Mathf.Infinity;

        foreach (var col in colliders)
        {
            if (col == null) continue;
            float dist = Vector2.Distance(transform.position, col.transform.position);

            if (col.CompareTag(TAG_SAN_NHA) && dist < minDistSannha)
            {
                minDistSannha = dist;
                nearestSannha = col.transform;
            }
            else if (col.CompareTag(TAG_PHE_CHINH) && dist < minDistPhechinh)
            {
                minDistPhechinh = dist;
                minDistPhechinh = dist;
                nearestPhechinh = col.transform;
            }
        }
        targetTransform = (nearestSannha != null) ? nearestSannha : nearestPhechinh;
    }

    private void HandleRangedShooting()
    {
        if (mauChaos.Deadre)
        {
            return;
        }
        if (currentChargerState != ChargerState.FastRangedRun || hasExploded || targetTransform == null) return;

        if (Time.time >= nextAttackTime)
        {
            if (projectilePrefab != null)
            {
                // 1. Tính toán vị trí
                Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;

                // 2. Kích hoạt đồng bộ Animation, Âm thanh và Tạo đạn tại cùng 1 thời điểm
                animator.SetTrigger("Chao_dead_Shoot");
                Amthanh.PlayOneShot(Shoot);
                GameObject proj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

                Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
                if (projRb != null)
                {
                    Vector2 shootDir = (targetTransform.position - spawnPosition).normalized;
                    projRb.linearVelocity = shootDir * projectileSpeed;

                    float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
                    proj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }

            // Xóa lệnh Amthanh.PlayOneShot(Shoot) dư thừa ở dưới này
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void HandleCoreLogicTransitions()
    {
        if (mauChaos.Deadre)
        {
            return;
        }
        switch (currentChargerState)
        {
            case ChargerState.Exploding:
                hasExploded = true;
                if (rb != null) rb.simulated = true;
                if (monsterCollider != null) monsterCollider.enabled = true;

                currentChargerState = ChargerState.MeleeChasing;
                break;

            case ChargerState.MeleeChasing:
                if (targetTransform != null)
                {
                    float dist = Vector2.Distance(transform.position, targetTransform.position);
                    if (dist <= meleeAttackRange && Time.time >= nextMeleeAttackTime)
                    {
                        currentChargerState = ChargerState.MeleeAttacking;
                        StartCoroutine(MeleeAttackRoutine());
                    }
                }
                break;
        }
    }

    private void HandlePhysicsMovement()
    {
        switch (currentChargerState)
        {
            case ChargerState.FastRangedRun:
                if (targetTransform != null)
                {
                    Vector2 moveDir = (targetTransform.position - transform.position).normalized;
                    lastLookDirection = moveDir;

                    Vector2 checkCenter = CalculateHitboxCenter(moveDir);
                    Collider2D[] cols = Physics2D.OverlapCircleAll(checkCenter, chargeHitboxRadius);
                    bool hitTarget = false;

                    foreach (var c in cols)
                    {
                        if (c != null && c.gameObject != this.gameObject)
                        {
                            if (c.CompareTag(TAG_PHE_CHINH) || c.CompareTag(TAG_SAN_NHA))
                            {
                                hitTarget = true;
                                break;
                            }
                        }
                    }

                    if (hitTarget)
                    {
                        if (rb != null) rb.simulated = false;
                        if (monsterCollider != null) monsterCollider.enabled = false;
                        ExecuteAoEExplosion();
                    }
                    else
                    {
                        rb.linearVelocity = moveDir * fastMoveSpeed;
                    }
                }
                else
                {
                    lastLookDirection = Vector2.left;
                    rb.linearVelocity = Vector2.left * fastMoveSpeed * leftMoveBias;
                }
                break;

            case ChargerState.Exploding:
            case ChargerState.MeleeAttacking:
                rb.linearVelocity = Vector2.zero;
                break;

            case ChargerState.MeleeChasing:
                if (targetTransform != null)
                {
                    float dist = Vector2.Distance(transform.position, targetTransform.position);
                    if (dist <= meleeAttackRange)
                    {
                        rb.linearVelocity = Vector2.zero;
                    }
                    else
                    {
                        Vector2 dir = (targetTransform.position - transform.position).normalized;
                        rb.linearVelocity = dir * meleeMoveSpeed;
                    }
                }
                else
                {
                    rb.linearVelocity = Vector2.left * meleeMoveSpeed * leftMoveBias;
                }

                RotateMeleeHitbox();
                break;
        }
        if (rb.linearVelocity.magnitude > 0.15f)
        {
            animator.SetBool("Chaos_dead_run", true);
        }
        else
        {
            animator.SetBool("Chaos_dead_run", false);
        }
    }

    private Vector2 CalculateHitboxCenter(Vector2 moveDir)
    {
        Vector2 forward = moveDir.normalized;
        Vector2 up = new Vector2(-forward.y, forward.x);
        return (Vector2)transform.position + (forward * hitboxOffset.x) + (up * hitboxOffset.y);
    }

    private void ExecuteAoEExplosion()
    {
        currentChargerState = ChargerState.Exploding;

        if (explosionPrefab != null)
        {
            GameObject expGo = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            ChargerExplosion expScript = expGo.GetComponent<ChargerExplosion>();
            if (expScript != null)
            {
                expScript.Initialize(explosionDamage, explosionRadius, explosionDuration);
            }
        }

        HandleCoreLogicTransitions();
    }

    private IEnumerator MeleeAttackRoutine()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("chao_dead_attack", true);
        if (meleeHitboxObject != null)
        {
            meleeHitboxObject.SetActive(true);

            Collider2D[] hitTargets = Physics2D.OverlapBoxAll(meleeHitboxObject.transform.position, new Vector2(1.5f, 1.5f), 0f);
            foreach (var col in hitTargets)
            {
                if (col != null && (col.CompareTag(TAG_PHE_CHINH) || col.CompareTag(TAG_SAN_NHA)))
                {
                    var health = col.GetComponent<Health_phechinh>();
                    if (health != null)
                    {
                        // 🛠️ ĐÃ SỬA LỖI CS0414: Đưa biến meleeDamage vào sử dụng thực tế để làm sạch Console
                        health.TakeDamage(meleeDamage);
                    }
                }
            }
        }
        animator.SetBool("chao_dead_attack", false);

        yield return new WaitForSeconds(meleeHitboxDuration);

        if (meleeHitboxObject != null) meleeHitboxObject.SetActive(false);

        nextMeleeAttackTime = Time.time + meleeAttackCooldown;
        currentChargerState = ChargerState.MeleeChasing;
    }

    private void RotateMeleeHitbox()
    {
        if (meleeHitboxObject == null) return;
        float lookDir = (targetTransform != null && targetTransform.position.x < transform.position.x) ? -1f : 1f;
        float currentPosX = Mathf.Abs(meleeHitboxObject.transform.localPosition.x);
        meleeHitboxObject.transform.localPosition = new Vector3(currentPosX * lookDir, meleeHitboxObject.transform.localPosition.y, meleeHitboxObject.transform.localPosition.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 dir = (targetTransform != null) ? (Vector2)(targetTransform.position - transform.position).normalized : lastLookDirection;
        Vector2 debugCenter = CalculateHitboxCenter(dir);

        Gizmos.DrawWireSphere(debugCenter, chargeHitboxRadius);
    }
}