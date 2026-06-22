using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    private Animator animator;

    [Header("--- ANIMATION TIMING ---")]
    [Tooltip("Thời gian diễn ra của Animation Spawn (giây) trước khi Boss vào trạng thái hoạt động")]
    [SerializeField] private float spawnAnimationDuration = 2.0f;

    [Header("--- SKILL SYSTEM CONFIG ---")]
    [Tooltip("Thời gian nghỉ giữa các lần ra đòn")]
    [SerializeField] private float skillCooldown = 3f;

    [Header("--- SKILLS POOL ---")]
    [Tooltip("Thêm các file ScriptableObject Skill vào danh sách này (Skill 1, Skill 2, Skill 3...)")]
    [SerializeField] private List<BossSkill> availableSkills = new List<BossSkill>();

    [Header("--- HỆ THỐNG ÂM THANH BOSS ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip attackSound;

    // ĐỒNG BỘ: Đã chuyển sang script máu mới của bạn
    private Health_boss Heal;
    private Rigidbody2D rb;

    // Biến toàn cục chứa mục tiêu để Skill có thể truy cập nếu cần
    [HideInInspector] public Transform TargetTransform;

    private void Awake()
    {
        Heal = GetComponent<Health_boss>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // FIX: Chỉ tự động lấy Component nếu bạn quên kéo thả AudioSource trong Inspector
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        ConfigurePhysics();
    }

    private void Start()
    {
        // Khởi động vòng lặp AI chính của Boss
        StartCoroutine(BossAILoop());
    }

    private void FixedUpdate()
    {
        // Kiểm tra theo hệ thống máu Health_boss mới
        if (Heal != null && Heal.Deadre_boss) return;
        if (rb == null) return;

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
        if (animator != null)
        {
            animator.SetTrigger("Spawn");
        }

        // PHÁT ÂM THANH XUẤT HIỆN: Lệnh PlayOneShot đã chuẩn xác!
        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        // Đóng băng AI, chờ Animation Spawn hoàn thành 100%
        yield return new WaitForSeconds(spawnAnimationDuration);

        // 2. GIAI ĐOẠN COOLDOWN ĐẦU TRẬN
        if (animator != null)
        {
            animator.SetBool("idle", true);
            animator.SetBool("Skill_1", false);
        }
        yield return new WaitForSeconds(skillCooldown);

        // VÒNG LẶP AI CHÍNH: CHẠY LIÊN TỤC KHÔNG GIỚI HẠN
        while (true)
        {
            // Nếu Boss chết -> Thoát hẳn vòng lặp AI ngay lập tức
            if (Heal != null && Heal.Deadre_boss)
            {
                if (animator != null)
                {
                    animator.SetBool("idle", false);
                    animator.SetBool("Skill_1", false);
                }
                yield break;
            }

            // Đưa trạng thái Animator về thế thủ (Idle) trước khi quét mục tiêu
            if (animator != null && !animator.GetBool("idle"))
            {
                animator.SetBool("idle", true);
                animator.SetBool("Skill_1", false);
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
                BossSkill selectedSkill = SelectRandomSkill();

                if (selectedSkill != null)
                {
                    // Cập nhật Animator: Vào trạng thái tấn công
                    if (animator != null)
                    {
                        animator.SetBool("idle", false);
                        animator.SetBool("Skill_1", true);
                    }

                    // PHÁT ÂM THANH TẤN CÔNG: Lệnh PlayOneShot tiếp tục làm tốt nhiệm vụ phát 1 lần ở đây!
                    if (audioSource != null && attackSound != null)
                    {
                        audioSource.PlayOneShot(attackSound);
                    }

                    // Yield trực tiếp vào Coroutine của Skill. Khi Skill chạy xong, code tự động chạy tiếp!
                    yield return StartCoroutine(selectedSkill.ExecuteSkillRoutine(this));
                }

                // Kiểm tra lại trạng thái sống chết sau khi tung chiêu kết thúc
                if (Heal != null && Heal.Deadre_boss) yield break;

                // Đưa về thế thủ chuẩn bị hồi chiêu sau khi xả chiêu xong
                if (animator != null)
                {
                    animator.SetBool("idle", true);
                    animator.SetBool("Skill_1", false);
                }

                // Quá trình hồi chiêu (Cooldown) giữa các đòn đánh
                float cooldownTimer = 0f;
                while (cooldownTimer < skillCooldown)
                {
                    if (Heal != null && Heal.Deadre_boss) yield break;
                    cooldownTimer += 0.1f;
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // Giảm tải hiệu năng hệ thống tránh tràn ram/đứng khung hình
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Đơn giản hóa việc chọn Skill, lấy ngẫu nhiên liên tục không giới hạn điều kiện cũ
    private BossSkill SelectRandomSkill()
    {
        if (availableSkills == null || availableSkills.Count == 0) return null;
        return availableSkills[Random.Range(0, availableSkills.Count)];
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