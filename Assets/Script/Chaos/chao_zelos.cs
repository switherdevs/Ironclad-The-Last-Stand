using UnityEngine;
using System.Collections;

public class EnemyMelee : Test_base
{
    [Header("--- MELEE SPECIFIC (CẬN CHIẾN) ---")]
    [SerializeField] private GameObject attackHitboxObject;
    [SerializeField] private float hitboxActiveDuration = 0.2f;
    private Animator Animator;
    protected override void Awake()
    {
        base.Awake();
        Animator = GetComponentInChildren<Animator>();
    }
    protected override void Start()
    {
        base.Start(); // Gọi Start của lớp Base để cài đặt máu và vật lý

        // Ẩn hitbox lúc đầu game
        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(false);
        }

        // Ép khoảng cách giữ chân của quái cận chiến về sát Player
    }

    protected override void HandleMovement()
    {
        // Chạy logic di chuyển tiếp cận mục tiêu của lớp Base
        base.HandleMovement();
        if(Animator != null)
        {
            Animator.SetBool("ChaoZelos_isWalking", true);

        }
        // Xoay hướng của Hitbox cận chiến luôn ở phía trước mặt quái dựa theo Sprite đang lật hướng nào
        RotateHitbox(GetLookDirection());
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
        // Chuyển trạng thái để đứng im vung kiếm

        currentState = EnemyState.Attacking;
        StartCoroutine(TriggerHitboxRoutine());
    }

    private IEnumerator TriggerHitboxRoutine()
    {

        if (Animator != null)
        {
            Animator.SetBool("ChaoZelos_isWalking", false);

        }

        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(true);
        }

        if(Animator != null) 
        {
            Animator.SetBool("ChaoZelos_doAttack", true);

        }

        yield return new WaitForSeconds(hitboxActiveDuration);
        Animator.SetBool("ChaoZelos_doAttack", false);

        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(false);
        }

        // Kết thúc vung kiếm, đưa vào cooldown và hồi phục di chuyển
        nextAttackTime = Time.time + attackCooldown;
        currentState = EnemyState.Chasing;
    }
}