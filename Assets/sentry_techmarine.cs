using UnityEngine;
using System.Collections;

public class TechmarineSentry : MonoBehaviour
{
    [Header("--- TRẠNG THÁI HOẠT ĐỘNG ---")]
    public bool dangHoatDong = false;

    [Header("--- CẤU HÌNH CHIẾN ĐẤU ---")]
    public float thoiGianThucDay = 10f; // Súng hoạt động trong bao lâu trước khi gục lại
    public float tamQuetQuai = 8f;
    public float tocDoXoayNong = 5f;
    public float tocDoBan = 0.2f;

    [Header("--- THÀNH PHẦN KẾT NỐI ---")]
    public Transform nongSungXoay;   // Kéo Pivot của nòng súng vào đây để nó xoay hướng theo quái
    public Transform viTriBanDan;    // Điểm sinh đầu đạn ở đầu nòng súng
    public GameObject prefabDanXuyenThau; // Prefab viên đạn có chứa script SentryBullet
    public Animator sentryAnimator;  // Animator điều khiển góc gục/ngóc đầu của súng

    private float cooldownBanTimer = 0f;
   

    private void Update()
    {
        // VÁ LỖ HỔNG: Nếu chưa kích hoạt, đóng băng toàn bộ code Update để nhẹ CPU
        if (!dangHoatDong) return;

        cooldownBanTimer += Time.deltaTime;

        // 1. Tìm con quái gần nhất mang tag "Enemy"
        Transform mucTieuGanNhat = TimKeThuGanNhat();

        if (mucTieuGanNhat != null)
        {
            // 2. Xoay nòng súng hướng về phía mục tiêu mượt mà (2D)
            Vector3 huongQuai = mucTieuGanNhat.position - nongSungXoay.position;
            float gocQuayZ = Mathf.Atan2(huongQuai.y, huongQuai.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, gocQuayZ);
            nongSungXoay.rotation = Quaternion.Lerp(nongSungXoay.rotation, targetRotation, tocDoXoayNong * Time.deltaTime);

            // 3. Xả đạn liên tục theo nhịp tốc độ bắn
            if (cooldownBanTimer >= tocDoBan)
            {
                BanDanXuyenThau();
                cooldownBanTimer = 0f;
            }
        }
    }

    public void ThucDayVaKichHoatSentry()
    {
        if (dangHoatDong) return; // Tránh kích hoạt trùng lặp khi đang bắn
        StopAllCoroutines();
        StartCoroutine(ChuoiHoatDongSentryRoutine());
    }

    IEnumerator ChuoiHoatDongSentryRoutine()
    {
        dangHoatDong = true;

        // Bật Animation dựng nòng súng lên
        if (sentryAnimator != null)
        {
            sentryAnimator.SetBool("IsAwake", true);
        }

        // Hoạt động càn quét trong thời gian quy định
        yield return new WaitForSeconds(thoiGianThucDay);

        dangHoatDong = false;

        // Cho súng gục xuống lại khi hết thời gian skill
        if (sentryAnimator != null)
        {
            sentryAnimator.SetBool("IsAwake", false);
        }
    }

    Transform TimKeThuGanNhat()
    {
        GameObject[] tatCaQuai = GameObject.FindGameObjectsWithTag("Enemy"); // Quái phải được đặt Tag là Enemy
        Transform keThuGanNhat = null;
        float khoangCachNhoNhat = Mathf.Infinity;

        foreach (GameObject quai in tatCaQuai)
        {
            float khoangCach = Vector3.Distance(transform.position, quai.transform.position);
            if (khoangCach < khoangCachNhoNhat && khoangCach <= tamQuetQuai)
            {
                khoangCachNhoNhat = khoangCach;
                keThuGanNhat = quai.transform;
            }
        }
        return keThuGanNhat;
    }

    void BanDanXuyenThau()
    {
        if (prefabDanXuyenThau != null && viTriBanDan != null)
        {
            Instantiate(prefabDanXuyenThau, viTriBanDan.position, nongSungXoay.rotation);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn tầm bắn màu đỏ trong Scene để bạn dễ căn chỉnh ngoài Editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tamQuetQuai);
    }
}