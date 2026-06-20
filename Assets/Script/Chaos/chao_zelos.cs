using UnityEngine;
using System.Collections;

public class EnemyMelee : BaseEnemy
{
    [Header("--- MELEE SPECIFIC (CẬN CHIẾN) ---")]
    [SerializeField] private GameObject attackHitboxObject;
    [SerializeField] private float hitboxActiveDuration = 0.2f;
    private Animator Animator;
    private AudioSource Amthanh;

    // ⭐ ĐÃ NÂNG CẤP: Thay thế clip đơn bằng mảng (Thêm bao nhiêu âm thanh vào Inspector tùy ý)
    [Header("--- HỆ THỐNG ÂM THANH NGẪU NHIÊN ---")]
    [SerializeField] private AudioClip[] danhSachTiengChem;

    protected override void Awake()
    {
        base.Awake();
        Amthanh = GetComponent<AudioSource>();
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
    }

    protected override void HandleMovement()
    {
        // Chạy logic di chuyển tiếp cận mục tiêu của lớp Base
        base.HandleMovement();
        if (Animator != null)
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

        // ⭐ ĐÃ NÂNG CẤP: Gọi hàm phát âm thanh ngẫu nhiên ngay khi quái vung kiếm
        PhatAmThanhChemNgauNhien();

        if (attackHitboxObject != null)
        {
            attackHitboxObject.SetActive(true);
        }

        if (Animator != null)
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

    // ⭐ HÀM BỔ TRỢ: Tự động tính toán và chọn ngẫu nhiên file âm thanh trong danh sách
    private void PhatAmThanhChemNgauNhien()
    {
        if (Amthanh != null && danhSachTiengChem != null && danhSachTiengChem.Length > 0)
        {
            // Lấy ra một vị trí ngẫu nhiên từ vị trí 0 đến hết chiều dài danh sách
            int viTriNgauNhien = Random.Range(0, danhSachTiengChem.Length);

            // Lấy file âm thanh tại vị trí đó ra
            AudioClip clipDuocChon = danhSachTiengChem[viTriNgauNhien];

            // Phát âm thanh (Nếu file đó không bị trống)
            if (clipDuocChon != null)
            {
                Amthanh.PlayOneShot(clipDuocChon);
            }
        }
    }
}