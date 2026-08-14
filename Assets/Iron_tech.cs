using UnityEngine;

public class TechmarineSkillController : MonoBehaviour
{
    [Header("--- QUẢN LÝ TRỤ SÚNG TRÊN MAP ---")]
    [Tooltip("Kéo Object Trụ súng Sentry đang nằm gục sẵn trên Hierarchy vào đây.")]
    public TechmarineSentry truSungSentryTrenMap;

    // Hàm gọi khi người chơi NHẤN NÚT UI TECHMARINE
    public void KichHoatSkillTechmarine()
    {
        if (truSungSentryTrenMap != null)
        {
            // Đánh thức trụ súng dậy
            truSungSentryTrenMap.ThucDayVaKichHoatSentry();
        }
        else
        {
            Debug.LogError("[Techmarine UI] Chưa kéo Trụ Súng ngoài Map vào ô Tru Sung Sentry Tren Map!");
        }
    }
}