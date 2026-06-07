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

        // ── [ANIMATION] Cập nhật isWalking theo vị trí thực tế ──────────────
        if (ChaoZelos_animator != null)
        {
            bool dangDiChuyen = transform.position != viTriKhungHinhTruoc;
            ChaoZelos_animator.SetBool("ChaoZelos_isWalking", dangDiChuyen);
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
        // ── [ANIMATION] Kích hoạt animation tấn công cận chiến ──────────────
        if (ChaoZelos_animator != null)
            ChaoZelos_animator.SetTrigger("ChaoZelos_doAttack");
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