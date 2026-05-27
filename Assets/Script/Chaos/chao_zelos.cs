using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Movement (2.5D)")]
    public float moveSpeed = 5f;

    [Header("Attack Settings")]
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackRate = 1f;
    private float lastAttackTime;

    [Header("Melee Hitbox (Inspector)")]
    // Kéo chính xác GameObject con (Hitbox) vào đây để bật/tắt ô tích trong Inspector
    public GameObject attackHitboxObject;
    public float hitboxActiveDuration = 0.2f;

    private Transform currentTarget;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null) return;

        // Cấu hình chuẩn vật lý 2.5D
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.WakeUp();

        // Đầu game tắt hẳn ô tích của GameObject Hitbox ngoài Inspector
        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        if (rb.IsSleeping()) rb.WakeUp();

        FindNearestTarget();

        if (currentTarget == null)
        {
            MoveLeft25D();
        }
        else
        {
            HandleCombat25D();
        }
    }

    // ===== DI CHUYỂN SANG TRÁI MẶC ĐỊNH =====
    void MoveLeft25D()
    {
        rb.linearVelocity = new Vector2(-moveSpeed, 0f);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false; // Tùy thuộc vào Sprite gốc của bạn, mặc định hướng trái là false hoặc true
        }

        // Xoay hướng của Hitbox sang bên trái (X âm)
        RotateHitbox(-1f);
    }

    // ===== XỬ LÝ CHIẾN ĐẤU =====
    void HandleCombat25D()
    {
        float distToTarget = Vector2.Distance(transform.position, currentTarget.position);

        if (distToTarget > attackRange)
        {
            MoveToTarget25D();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            if (Time.time >= lastAttackTime + attackRate)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    // ===== ĐUỔI ĐỊCH =====
    void MoveToTarget25D()
    {
        Vector2 direction = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        float directionX = currentTarget.position.x - transform.position.x;
        float moveDir = directionX > 0 ? 1f : -1f;

        // Lật hình ảnh dựa theo hướng di chuyển
        if (spriteRenderer != null)
        {
            // Nếu đi bên phải (moveDir > 0) thì flipX = true (hoặc ngược lại tùy Sprite gốc)
            spriteRenderer.flipX = (moveDir > 0);
        }

        // Xoay hướng của Hitbox theo hướng mục tiêu
        RotateHitbox(moveDir);
    }

    // ===== ÉP HITBOX LUÔN Ở PHÍA TRƯỚC MẶT =====
    void RotateHitbox(float moveDir)
    {
        if (attackHitboxObject == null) return;

        // Lấy vị trí X hiện tại (luôn lấy số dương để tính khoảng cách trước mặt)
        float currentPosX = Mathf.Abs(attackHitboxObject.transform.localPosition.x);

        // Nếu đi sang trái (moveDir = -1), vị trí X của hitbox phải là số ÂM. Đi sang phải là số DƯƠNG.
        float newPosX = currentPosX * moveDir;

        attackHitboxObject.transform.localPosition = new Vector3(newPosX, attackHitboxObject.transform.localPosition.y, attackHitboxObject.transform.localPosition.z);
    }

    // ===== TÌM MỤC TIÊU TAG "Phechinh" GẦN NHẤT =====
    void FindNearestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Phechinh");

        if (targets == null || targets.Length == 0)
        {
            currentTarget = null;
            return;
        }

        GameObject nearestTarget = null;
        float minDistance = Mathf.Infinity;
        Vector2 currentPosition = transform.position;

        foreach (GameObject t in targets)
        {
            float dist = Vector2.Distance(t.transform.position, currentPosition);
            if (dist < minDistance && dist <= detectionRange)
            {
                minDistance = dist;
                nearestTarget = t;
            }
        }

        if (nearestTarget != null) currentTarget = nearestTarget.transform;
        else currentTarget = null;
    }

    // ===== VUNG ĐÒN (BẬT TẮT GAME OBJECT) =====
    void Attack()
    {
        if (attackHitboxObject == null) return;
        StartCoroutine(TriggerHitboxRoutine());
    }

    IEnumerator TriggerHitboxRoutine()
    {
        // Bật ô tích xanh ngoài Inspector (Hiện Object lên)
        attackHitboxObject.SetActive(true);

        yield return new WaitForSeconds(hitboxActiveDuration);

        // Tắt ô tích xanh ngoài Inspector (Ẩn Object đi)
        attackHitboxObject.SetActive(false);
    }
}