using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    public enum BossState { Idle, UsingSkill, Cooldown, Dead }

    [Header("--- BOSS STATE ---")]
    [SerializeField] private BossState currentState = BossState.Idle;

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
        rb = GetComponent<Rigidbody2D>();
        ConfigurePhysics();
    }

    private void Start()
    {
        currentState = BossState.Cooldown;
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

        // Ưu tiên 1: Tìm Sannha
        GameObject[] sannhaTargets = GameObject.FindGameObjectsWithTag("Sannha");
        if (sannhaTargets != null && sannhaTargets.Length > 0)
        {
            TargetTransform = GetNearest(sannhaTargets, currentPosition);
            if (TargetTransform != null) return;
        }

        // Ưu tiên 2: Tìm Phechinh
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
        // Chờ cooldown ban đầu khi vừa vào game
        yield return new WaitForSeconds(skillCooldown);

        while (currentState != BossState.Dead)
        {
            currentState = BossState.Idle;

            // Lấy thông số tầm đánh của Skill 1 để làm điều kiện kích hoạt đòn đánh
            float attackRange = 8f;
            if (availableSkills != null && availableSkills.Count > 0 && availableSkills[0] is Skill1_MeteorShower meteor)
            {
                attackRange = meteor.GetDetectionRange();
            }

            // ĐIỀU KIỆN KÍCH HOẠT: Có mục tiêu và mục tiêu lọt vào tầm đánh vòng tròn đỏ
            if (TargetTransform != null && Vector2.Distance(transform.position, TargetTransform.position) <= attackRange)
            {
                BossSkill selectedSkill = SelectValidRandomSkill();

                if (selectedSkill != null)
                {
                    currentState = BossState.UsingSkill;
                    currentActiveSkill = selectedSkill;

                    // Xử lý logic Anti-Repeat chống lặp đòn
                    if (selectedSkill.SkillID == lastSkillID) consecutiveCount++;
                    else { lastSkillID = selectedSkill.SkillID; consecutiveCount = 1; }

                    // Chạy Skill và đợi cho tới khi thiên thạch rơi xong 100%
                    yield return StartCoroutine(selectedSkill.ExecuteSkillRoutine(this));

                    currentActiveSkill = null;
                }

                // Đổi trạng thái sang Cooldown nghỉ ngơi sau khi xả chiêu
                currentState = BossState.Cooldown;
                yield return new WaitForSeconds(skillCooldown);
            }

            // Vòng lặp chờ tối ưu hiệu năng (tránh treo luồng game khi không có địch)
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
        StopAllCoroutines();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (availableSkills != null)
        {
            foreach (var skill in availableSkills)
            {
                if (skill is Skill1_MeteorShower meteorShowerSkill)
                {
                    // Vẽ vùng Box Area xanh ngọc dựa trên vị trí đứng yên của Boss
                    meteorShowerSkill.DrawGizmos(transform.position);
                }
            }
        }
    }
}