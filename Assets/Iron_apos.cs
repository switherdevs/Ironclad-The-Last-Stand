using UnityEngine;
using System;
using System.Collections;

public class ChaplainSkillManager : MonoBehaviour
{
    // C# Event: Đài phát thanh toàn cục (true = bật buff, false = tắt buff)
    public static event Action<bool> OnChaplainBuffChanged;

    [Header("--- CẤU HÌNH SKILL CHAPLAIN ---")]
    [Tooltip("Thời gian tồn tại của hiệu ứng buff hỏa lực (giây).")]
    public float thoiGianBuff = 5f;

    // Hàm gọi khi người chơi NHẤN NÚT UI CHAPLAIN
    public void KichHoatSkillChaplain()
    {
        StopAllCoroutines();
        StartCoroutine(ChuoiKichHoatBuffRoutine());
    }

    IEnumerator ChuoiKichHoatBuffRoutine()
    {
        // 1. Phát tín hiệu BẬT BUFF cho tất cả lính đang sống trên Map
        if (OnChaplainBuffChanged != null)
        {
            OnChaplainBuffChanged.Invoke(true);
        }
        Debug.Log("<color=yellow>[Chaplain]</color> Litanies of Battle! Toàn bộ lính được tăng hỏa lực!");

        // 2. Chờ hết thời gian quy định
        yield return new WaitForSeconds(thoiGianBuff);

        // 3. Phát tín hiệu TẮT BUFF để lính tự trả chỉ số về mức nâng cấp cũ
        if (OnChaplainBuffChanged != null)
        {
            OnChaplainBuffChanged.Invoke(false);
        }
        Debug.Log("<color=white>[Chaplain]</color> Hết thời gian buff. Chỉ số lính trở về bình thường.");
    }
}