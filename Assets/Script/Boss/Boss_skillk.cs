using UnityEngine;
using System.Collections;

public abstract class BossSkill : ScriptableObject
{
    [Header("--- BASE SKILL SETTINGS ---")]
    [Tooltip("ID độc nhất để hệ thống nhận diện chống lặp đòn (Ví dụ: 1, 2, 3)")]
    [SerializeField] private int skillID;

    public int SkillID => skillID;

    /// <summary>
    /// Kích hoạt Skill. Coroutine này phải chạy hoàn chỉnh và chỉ kết thúc khi Skill đã hoàn thành 100%.
    /// </summary>
    public abstract IEnumerator ExecuteSkillRoutine(BossController boss);
}