using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    private Animator animator; // Đổi tên biến viết thường cho đúng chuẩn C#
    public enum BossState { Spawn, Idle, UsingSkill, Cooldown, Dead }

    [Header("--- BOSS STATE ---")]
    [SerializeField] private BossState currentState = BossState.Spawn;

    [Header("--- ANIMATION TIMING ---")]
    [Tooltip("Thời gian diễn ra của Animation Spawn (giây) trước khi Boss vào trạng thái hoạt động")]
    [SerializeField] private float spawnAnimationDuration = 2.0f;

    [Header("--- SKILL SYSTEM CONFIG ---")]
    [SerializeField] private float skillCooldown = 3f;
    [SerializeField] private int maxConsecutiveUse = 2;

    [Header("--- SKILLS POOL ---")]
    [Tooltip("Thêm các file ScriptableObject Skill vào danh sách này (Skill 1, Skill 2, Skill 3...)")]
    [SerializeField] private List<BossSkill> availableSkills = new List<BossSkill>();

    private BossSkill currentActiveSkill;

    // Lưu trữ lịch sử sử dụng kỹ năng để chạy thuật toán Anti-Repeat System
    private int lastSkillID = -1;
    private int consecutiveCount = 0;

    private Rigidbody2D rb;

    // Biến toàn cục chứa mục tiêu để Skill có thể truy cập nếu cần
    [HideInInspector] public Transform TargetTransform;

    public BossState CurrentState => currentState;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        ConfigurePhysics();
    }

    private void Start()
    {
        // Khởi động vòng lặp AI chính của Boss
        StartCoroutine(BossAILoop());
    }

    private void FixedUpdate()
    {
        if (currentState == BossState.Dead || rb == null) return;

        if (rb.IsSleeping()) rb.WakeUp();

        // Luôn luôn khóa chặt vận tốc bằng 0 để Boss đứng yên tại chỗ
        rb.linearVelocity = Vector2.zero;

        // Quét tìm kẻ địch gần nhất để làm mục tiêu kiểm tra khoảng cách tung chiêu
        FindPriorityTarget();
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

    private void FindPriorityTarget()
    {
        Vector2 currentPosition = transform.position;

        // Ưu tiên: Tìm Phechinh
        GameObject[] phechinhTargets = GameObject.FindGameObjectsWithTag("Phechinh");
        if (phechinhTargets != null && phechinhTargets.Length > 0)
        {
            TargetTransform = GetNearest(phechinhTargets, currentPosition);
            return;
        }

        TargetTransform = null;
    }

    private Transform GetNearest(GameObject[] group, Vector2 currentPos)
    {
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject obj in group)
        {
            if (obj == null) continue;
            float dist = Vector2.Distance(obj.transform.position, currentPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = obj;
            }
        }
        return nearest != null ? nearest.transform : null;
    }

    private IEnumerator BossAILoop()
    {
        // 1. GIAI ĐOẠN XUẤT HIỆN (SPAWN): Diễn 1 lần duy nhất khi vừa sinh ra
        currentState = BossState.Spawn;
        if (animator != null)
        {
            animator.SetTrigger("Spawn");
        }
        // Đóng băng AI, chờ Animation Spawn hoàn thành 100%
        yield return new WaitForSeconds(spawnAnimationDuration);

        // 2. GIAI ĐOẠN COOLDOWN ĐẦU TRẬN: Nghỉ ngơi một chút trước khi bắt đầu quét mục tiêu
        currentState = BossState.Cooldown;
        if (animator != null)
        {
            animator.SetBool("idle", true);
            animator.SetBool("Skill_1", false);
        }
        yield return new WaitForSeconds(skillCooldown);

        // VÒNG LẶP AI CHÍNH
        while (currentState != BossState.Dead)
        {
            // Thiết lập trạng thái Idle khi ở ngoài tầm đánh hoặc đang tìm kiếm
            if (currentState != BossState.Idle && currentState != BossState.Cooldown)
            {
                currentState = BossState.Idle;
                if (animator != null)
                {
                    animator.SetBool("idle", true);
                    animator.SetBool("Skill_1", false);
                }
            }

            // Lấy thông số tầm đánh của Skill 1
            float attackRange = 8f;
            if (availableSkills != null && availableSkills.Count > 0 && availableSkills[0] is Skill1_MeteorShower meteor)
            {
                attackRange = meteor.GetDetectionRange();
            }

            // ĐIỀU KIỆN KÍCH HOẠT: Có mục tiêu và mục tiêu lọt vào tầm đánh
            if (TargetTransform != null && Vector2.Distance(transform.position, TargetTransform.position) <= attackRange)
            {
                BossSkill selectedSkill = SelectValidRandomSkill();

                if (selectedSkill != null)
                {
                    currentState = BossState.UsingSkill;

                    // Cập nhật Animator: Tắt Idle, Bật trạng thái tấn công
                    if (animator != null)
                    {
                        animator.SetBool("idle", false);
                        animator.SetBool("Skill_1", true);
                    }

                    currentActiveSkill = selectedSkill;

                    // Xử lý logic Anti-Repeat chống lặp đòn
                    if (selectedSkill.SkillID == lastSkillID) consecutiveCount++;
                    else { lastSkillID = selectedSkill.SkillID; consecutiveCount = 1; }

                    // Chạy Skill và đợi cho tới khi chiêu thức kết thúc hoàn toàn
                    yield return StartCoroutine(selectedSkill.ExecuteSkillRoutine(this));

                    currentActiveSkill = null;
                }

                // Chuyển sang Cooldown sau khi xả chiêu xong
                currentState = BossState.Cooldown;
                if (animator != null)
                {
                    animator.SetBool("idle", true); // Đưa về thế thủ chuẩn bị hồi chiêu
                    animator.SetBool("Skill_1", false);
                }

                yield return new WaitForSeconds(skillCooldown);
            }

            // Vòng lặp chờ tối ưu hiệu năng (Tránh đứng khung hình)
            yield return new WaitForSeconds(0.1f);
        }
    }

    private BossSkill SelectValidRandomSkill()
    {
        if (availableSkills == null || availableSkills.Count == 0) return null;
        List<BossSkill> validSkills = new List<BossSkill>(availableSkills);

        if (consecutiveCount >= maxConsecutiveUse && lastSkillID != -1)
        {
            validSkills.RemoveAll(skill => skill.SkillID == lastSkillID);
        }

        if (validSkills.Count > 0)
        {
            return validSkills[Random.Range(0, validSkills.Count)];
        }
        return null;
    }

    public void OnBossDeath()
    {
        currentState = BossState.Dead;
        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }
        StopAllCoroutines();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Destroy(gameObject, 1.5f); // Trì hoãn hủy Object một chút để kịp diễn hoạt xong hoạt ảnh chết
    }

    private void OnDrawGizmosSelected()
    {
        if (availableSkills != null)
        {
            foreach (var skill in availableSkills)
            {
                if (skill is Skill1_MeteorShower meteorShowerSkill)
                {
                    meteorShowerSkill.DrawGizmos(transform.position);
                }
            }
        }
    }
}