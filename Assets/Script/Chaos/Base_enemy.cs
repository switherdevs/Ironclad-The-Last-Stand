using UnityEngine;
using System.Collections;

public abstract class BaseEnemy : MonoBehaviour
{
    public enum EnemyState { Chasing, Holding, Attacking, Cooldown }

    [Header("--- BASE MOVEMENT ---")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float keepDistance = 4f;
    [SerializeField] protected float leftMoveBias = 0.6f;

    [Header("--- DETECTION SETTINGS ---")]
    [SerializeField] protected float detectionRange = 8f;
    [Header("--- BASE HEALTH ---")]
    [SerializeField] protected int normalHP = 20;

    [Header("--- BASE ATTACK ---")]
    [SerializeField] protected float attackCooldown = 3f;
    [SerializeField] protected float projectileSpeed = 10f;
    [SerializeField] protected GameObject projectilePrefab;

    protected EnemyState currentState = EnemyState.Chasing;
    protected Transform targetTransform;
    protected float nextAttackTime;
    protected int currentMaxHP;
    protected int currentHP;

    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;

    protected const string TAG_PHE_CHINH = "Phechinh";
    protected const string TAG_SAN_NHA = "Sannha";

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        currentMaxHP = normalHP;
        currentHP = currentMaxHP;
        ConfigurePhysics2D();
    }

    protected virtual void Update()
    {
        HandleStateTransition();
    }

    protected virtual void FixedUpdate()
    {
        if (rb == null) return;
        if (rb.IsSleeping()) rb.WakeUp();

        // Quét tìm mục tiêu có phân cấp ưu tiên và GIỚI HẠN TẦM
        FindPriorityTarget();

        if (currentState == EnemyState.Holding || currentState == EnemyState.Attacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        HandleMovement();
    }

    private void ConfigurePhysics2D()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.linearDamping = 5f;
        }
    }

    /// <summary>
    /// Thuật toán ưu tiên kết hợp bộ lọc tầm quét tối đa (detectionRange)
    /// </summary>
    protected void FindPriorityTarget()
    {
        Vector2 currentPosition = transform.position;

        // ƯU TIÊN 1: Quét mục tiêu "Sannha" xem có con nào lọt vào tầm quét không
        GameObject[] sannhaTargets = GameObject.FindGameObjectsWithTag(TAG_SAN_NHA);
        if (sannhaTargets != null && sannhaTargets.Length > 0)
        {
            targetTransform = GetNearestInDetectionRange(sannhaTargets, currentPosition);
            if (targetTransform != null) return;
        }
        // ƯU TIÊN 2: Nếu không có Sannha trong tầm, quét "Phechinh" trong tầm
        GameObject[] phechinhTargets = GameObject.FindGameObjectsWithTag(TAG_PHE_CHINH);
        if (phechinhTargets != null && phechinhTargets.Length > 0)
        {
            targetTransform = GetNearestInDetectionRange(phechinhTargets, currentPosition);
            return;
        }

        // Không có ai lọt vào tầm quét cả
        targetTransform = null;
    }

    // Hàm lọc lấy Object gần nhất NHƯNG phải nhỏ hơn hoặc bằng detectionRange
    private Transform GetNearestInDetectionRange(GameObject[] group, Vector2 currentPos)
    {
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject obj in group)
        {
            if (obj == null) continue;
            float dist = Vector2.Distance(obj.transform.position, currentPos);

            if (dist < minDistance && dist <= detectionRange)
            {
                minDistance = dist;
                nearest = obj;
            }
        }
        return nearest != null ? nearest.transform : null;
    }

    protected virtual void HandleMovement()
    {
        Vector2 targetVelocity = Vector2.zero;

        if (targetTransform != null)
        {
            Vector2 directionToTarget = (targetTransform.position - transform.position);
            float distance = directionToTarget.magnitude;
            Vector2 normalDir = directionToTarget.normalized;

            float stopThreshold = 0.2f;

            if (distance > keepDistance + stopThreshold)
            {
                targetVelocity = normalDir * moveSpeed;
            }
            else if (distance < keepDistance - stopThreshold)
            {
                targetVelocity = -normalDir * (moveSpeed * 1.2f);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }
        else
        {
            // Nếu không có mục tiêu lọt vào tầm quét -> Cứ lững thững hành quân qua trái màn hình
            targetVelocity = Vector2.left * moveSpeed * leftMoveBias;
        }

        rb.linearVelocity = targetVelocity;

        if (rb.linearVelocity.x != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = rb.linearVelocity.x > 0;
        }
        else if (targetTransform != null && spriteRenderer != null)
        {
            spriteRenderer.flipX = targetTransform.position.x > transform.position.x;
        }
    }

    protected virtual void HandleStateTransition()
    {
        if (currentState == EnemyState.Chasing && Time.time >= nextAttackTime && targetTransform != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, targetTransform.position);
            if (distanceToTarget <= (keepDistance + 0.5f))
            {
                ExecuteAttackPattern();
            }
        }
    }
    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, currentMaxHP);
        if (currentHP <= 0) Die();
    }

    protected virtual void Die() => Destroy(gameObject);

    protected abstract void ExecuteAttackPattern();

    protected float GetLookDirection()
    {
        if (spriteRenderer != null)
        {
            return spriteRenderer.flipX ? 1f : -1f;
        }
        return -1f;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, keepDistance);
    }
}