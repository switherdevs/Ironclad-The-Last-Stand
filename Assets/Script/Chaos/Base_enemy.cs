using UnityEngine;
using System.Collections;
public abstract class BaseEnemy : MonoBehaviour

{

    public enum EnemyState { Chasing, Holding, Attacking, Cooldown }



    [Header("--- BASE MOVEMENT ---")]

    [SerializeField] protected float moveSpeed = 5f;

    [SerializeField] protected float attackRange = 4f;

    [SerializeField] protected float leftMoveBias = 0.6f;



    [Header("--- DETECTION SETTINGS ---")]

    [SerializeField] protected float detectionRange = 8f;



    [Header("--- BASE HEALTH ---")]

    [SerializeField] protected int normalHP = 20;



    [Header("--- BASE ATTACK ---")]

    [SerializeField] protected float attackCooldown = 3f;

    [SerializeField] protected float projectileSpeed = 10f;

    [SerializeField] protected GameObject projectilePrefab;

    public Health_chaos deads;



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



        // SỬA TẠI ĐÂY: Tự động quét và lấy script Health_chaos trên chính nó hoặc lớp con để tránh lỗi quên kéo thả ngoài Inspector

        if (deads == null)

        {

            deads = GetComponent<Health_chaos>();

        }

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



    protected void FindPriorityTarget()

    {

        Vector2 currentPosition = transform.position;



        GameObject[] sannhaTargets = GameObject.FindGameObjectsWithTag(TAG_SAN_NHA);

        if (sannhaTargets != null && sannhaTargets.Length > 0)

        {

            targetTransform = GetNearestInDetectionRange(sannhaTargets, currentPosition);

            if (targetTransform != null) return;

        }



        GameObject[] phechinhTargets = GameObject.FindGameObjectsWithTag(TAG_PHE_CHINH);

        if (phechinhTargets != null && phechinhTargets.Length > 0)

        {

            targetTransform = GetNearestInDetectionRange(phechinhTargets, currentPosition);

            return;

        }



        targetTransform = null;

    }



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

        // SỬA TẠI ĐÂY: Thêm điều kiện kiểm tra an toàn (deads != null) trước khi đọc biến Deadre.

        // Nếu quái chết (Deadre == true), đứng khựng vận tốc về 0 và thoát hàm không cho di chuyển tiếp.

        if (deads != null && deads.Deadre)

        {

            rb.linearVelocity = Vector2.zero;

            return;

        }



        Vector2 targetVelocity = Vector2.zero;



        if (targetTransform != null)

        {

            Vector2 directionToTarget = (targetTransform.position - transform.position);

            float distance = directionToTarget.magnitude;

            Vector2 normalDir = directionToTarget.normalized;



            if (distance > attackRange)

            {

                targetVelocity = normalDir * moveSpeed;

            }

            else

            {

                rb.linearVelocity = Vector2.zero;

                return;

            }

        }

        else

        {

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

        // SỬA TẠI ĐÂY: Chặn hoàn toàn việc chuyển trạng thái tấn công nếu quái đã chết

        if (deads != null && deads.Deadre) return;



        if (currentState == EnemyState.Chasing && Time.time >= nextAttackTime && targetTransform != null)

        {

            float distanceToTarget = Vector2.Distance(transform.position, targetTransform.position);



            if (distanceToTarget <= attackRange)

            {

                ExecuteAttackPattern();

            }

        }

    }



    public virtual void TakeDamage(int damage)

    {

        currentHP -= damage;

        currentHP = Mathf.Clamp(currentHP, 0, currentMaxHP);

    }





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

        Gizmos.DrawWireSphere(transform.position, attackRange);

    }

}

