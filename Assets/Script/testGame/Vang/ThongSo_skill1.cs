using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Skill1_MeteorShower", menuName = "Boss Skills/Skill 1: Meteor Shower")]
public class Skill1_MeteorShower : BossSkill
{
    [Header("--- AREA SPAWN (DỰA THEO BOSS) ---")]
    [Tooltip("Vị trí của tâm hộp Spawn (Tính theo độ lệch Offset so với con Boss)")]
    [SerializeField] private Vector2 areaOffsetFromBoss = new Vector2(-4f, 2f);
    [Tooltip("Chiều rộng (X) và Chiều cao (Y) của vùng thiên thạch")]
    [SerializeField] private Vector2 areaSize = new Vector2(6f, 4f);

    [Tooltip("Tầm đánh của Boss. Muốn đánh TOÀN MAP thì hãy điền một số thật lớn (Ví dụ: 9999)")]
    [SerializeField] private float triggerAttackRange = 9999f;

    [Header("--- METEOR COUNT ---")]
    [SerializeField] private int minMeteorCount = 3;
    [SerializeField] private int maxMeteorCount = 6;

    [Header("--- METEOR MOVEMENT ---")]
    [Tooltip("Góc rơi của thiên thạch (Tính bằng độ). \n- 270: Rơi thẳng đứng từ trên xuống\n- 210: Xiên góc 7h (mặc định cũ)\n- 330: Xiên góc 5h (từ trái qua phải)")]
    [Range(0f, 360f)]
    [SerializeField] private float meteorDropAngle = 210f;
    [SerializeField] private float meteorSpeed = 15f;
    [SerializeField] private GameObject meteorPrefab;

    [Header("--- IMPACT DELAY ---")]
    [SerializeField] private float minImpactDelay = 2f;
    [SerializeField] private float maxImpactDelay = 3f;

    [Header("--- DAMAGE AREA ---")]
    [SerializeField] private float damageRadius = 1.5f;
    [SerializeField] private float damageDuration = 0.5f;
    [SerializeField] private int damageValue = 25;
    [SerializeField] private GameObject damageAreaPrefab;

    // Tính toán Vector hướng dựa theo số độ (Góc) mà bạn thiết lập trên Inspector
    private Vector2 GetDirectionFromAngle()
    {
        float radians = meteorDropAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }

    public float GetDetectionRange() => triggerAttackRange;

    public override IEnumerator ExecuteSkillRoutine(BossController boss)
    {
        int spawnCount = Random.Range(minMeteorCount, maxMeteorCount + 1);
        List<Coroutine> activeMeteors = new List<Coroutine>(spawnCount);

        Vector2 absoluteCenter = (Vector2)boss.transform.position + areaOffsetFromBoss;

        for (int i = 0; i < spawnCount; i++)
        {
            float randomX = Random.Range(absoluteCenter.x - areaSize.x / 2f, absoluteCenter.x + areaSize.x / 2f);
            float randomY = Random.Range(absoluteCenter.y - areaSize.y / 2f, absoluteCenter.y + areaSize.y / 2f);
            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);

            if (meteorPrefab != null)
            {
                // 🔥 CẢI TIẾN: Tự động xoay Prefab viên thiên thạch nhìn về đúng hướng nó đang bay cho đẹp mắt
                Quaternion spawnRotation = Quaternion.Euler(0f, 0f, meteorDropAngle);
                GameObject meteorGo = Instantiate(meteorPrefab, spawnPos, spawnRotation);

                float randomDelay = Random.Range(minImpactDelay, maxImpactDelay);

                Coroutine meteorRoutine = boss.StartCoroutine(MeteorMovementRoutine(meteorGo, randomDelay));
                activeMeteors.Add(meteorRoutine);
            }
        }

        foreach (var routine in activeMeteors)
        {
            if (routine != null) yield return routine;
        }
    }

    private IEnumerator MeteorMovementRoutine(GameObject meteor, float delay)
    {
        float elapsedTime = 0f;
        Transform meteorTransform = meteor.transform;

        // Lấy hướng bay thực tế từ số độ bạn cấu hình
        Vector3 moveDirection = (Vector3)GetDirectionFromAngle();

        while (elapsedTime < delay)
        {
            if (meteorTransform == null) break;

            // Di chuyển theo hướng góc đã chọn
            meteorTransform.Translate(moveDirection * (meteorSpeed * Time.deltaTime), Space.World);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (meteorTransform != null)
        {
            Vector3 finalImpactPos = meteorTransform.position;
            Destroy(meteor);

            if (damageAreaPrefab != null)
            {
                GameObject damageAreaGo = Instantiate(damageAreaPrefab, finalImpactPos, Quaternion.identity);
                BossDamageArea damageScript = damageAreaGo.GetComponent<BossDamageArea>();
                if (damageScript != null)
                {
                    yield return damageScript.InitializeDamageAreaRoutine(damageValue, damageRadius, damageDuration);
                }
                else Destroy(damageAreaGo);
            }
        }
    }

    public void DrawGizmos(Vector3 bossPosition)
    {
        Vector2 absoluteCenter = (Vector2)bossPosition + areaOffsetFromBoss;

        // Vẽ hộp Box Area xanh ngọc
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(absoluteCenter, areaSize);

        // Vẽ tầm đánh vòng tròn đỏ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bossPosition, triggerAttackRange);

        // 🔥 ĐÃ THÊM: Vẽ một vài mũi tên ngắn từ đỉnh hộp để minh họa hướng rơi của thiên thạch ngoài Scene
        Gizmos.color = Color.yellow;
        Vector3 arrowStart = new Vector3(absoluteCenter.x, absoluteCenter.y + areaSize.y / 2f, 0f);
        Vector3 arrowDir = (Vector3)GetDirectionFromAngle() * 1.5f;
        Gizmos.DrawRay(arrowStart, arrowDir);
    }
}