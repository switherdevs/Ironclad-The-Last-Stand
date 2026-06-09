using UnityEngine;
using System.Collections;

public class EnemyMelee : BaseEnemy
{
    [Header("--- MELEE SPECIFIC (CẬN CHIẾN) ---")]
    [SerializeField] private GameObject attackHitboxObject;
    [SerializeField] private float hitboxActiveDuration = 0.2f;

    // ── [ANIMATION] Khai báo Animator ───────────────────────────────────────
    private Animator ChaoZelos_animator;
    private Vector3 viTriKhungHinhTruoc;
    // ────────────────────────────────────────────────────────────────────────

    protected override void Start()
    {
        base.Start();

        if (attackHitboxObject != null)
            attackHitboxObject.SetActive(false);

        attackRange = 1.2f;

        // ── [ANIMATION] Lấy Animator từ children ────────────────────────────
        ChaoZelos_animator = GetComponentInChildren<Animator>();
        // ────────────────────────────────────────────────────────────────────
    }

    protected override void HandleMovement()
    {
        base.HandleMovement();
        RotateHitbox(GetLookDirection());

        // ── [ANIMATION] SỬA: Thay thế SetBool bằng CrossFade gọi tên State di chuyển hoặc đứng yên ──
        if (ChaoZelos_animator != null)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            if (dangDiChuyen)
            {
                ChaoZelos_animator.CrossFade("ChaoZelos_walk", 0.1f);
            }
            else
            {
                // Nếu quái đang ở trạng thái Attacking thì không ép nó chạy Idle để tránh lỗi ngắt đòn đánh
                if (currentState != EnemyState.Attacking)
                {
                    ChaoZelos_animator.CrossFade("ChaoZelos_idle", 0.1f);
                }
            }
        }
        viTriKhungHinhTruoc = transform.position;
        // ────────────────────────────────────────────────────────────────────
    }

    private void RotateHitbox(float lookDir)
    {
        if (attackHitboxObject == null) return;

        float currentPosX = Mathf.Abs(attackHitboxObject.transform.localPosition.x);
        float newPosX = currentPosX * lookDir;

        attackHitboxObject.transform.localPosition = new Vector3(
            newPosX,
            attackHitboxObject.transform.localPosition.y,
            attackHitboxObject.transform.localPosition.z
        );
    }

    protected override void ExecuteAttackPattern()
    {
        currentState = EnemyState.Attacking;
        StartCoroutine(TriggerHitboxRoutine());
    }

    private IEnumerator TriggerHitboxRoutine()
    {
        // ── [ANIMATION] SỬA: Thay thế SetTrigger bằng CrossFade gọi đòn tấn công cận chiến ──
        if (ChaoZelos_animator != null)
            ChaoZelos_animator.CrossFade("ChaoZelos_Attack", 0.05f);
        // ────────────────────────────────────────────────────────────────────

        if (attackHitboxObject != null)
            attackHitboxObject.SetActive(true);

        yield return new WaitForSeconds(hitboxActiveDuration);

        if (attackHitboxObject != null)
            attackHitboxObject.SetActive(false);

        nextAttackTime = Time.time + attackCooldown;
        currentState = EnemyState.Chasing;
    }
}