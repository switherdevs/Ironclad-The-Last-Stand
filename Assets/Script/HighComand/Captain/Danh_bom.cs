using UnityEngine;
using System.Collections;

public class MapAirStrikeController : MonoBehaviour
{
    [Header("--- CẤU HÌNH MÁY BAY ---")]
    [Tooltip("Kéo GameObject máy bay trên Map vào đây")]
    public GameObject ojectMayBay;
    public float thoiGianMayBayXuatHien = 0.5f;
    public float thoiGianMayBayBienMat = 3.0f;

    [Header("--- MẢNG VÙNG THẢ BOM THEO THỨ TỰ (COLLIDER) ---")]
    [Tooltip("Kéo các Vùng gây sát thương (Damage Zones) vào đây theo thứ tự bạn muốn nó nổ từ trước ra sau")]
    public GameObject[] danhSachVungNoCollider;

    [Header("--- THỜI GIAN DELAY GIỮA CÁC Ô ---")]
    [Tooltip("Thời gian cách nhau giữa mỗi lần ô tiếp theo xuất hiện (giây)")]
    public float thoiGianDelayGiuaCacO = 0.3f;
    [Tooltip("Thời gian mỗi ô Collider tồn tại trước khi tự ẩn đi (giây)")]
    public float thoiGianTonTaiCuaO = 0.5f;

    private void Start()
    {
        // Ban đầu ẩn hết tất cả máy bay và các vùng collider đi
        if (ojectMayBay != null) ojectMayBay.SetActive(false);

        ẨnHếtVùngCollider();
    }

    public void KichHoatKhongKich()
    {
        StopAllCoroutines();
        StartCoroutine(ChuoiKhongKichRoutine());
    }

    // 🌟 THUẬT TOÁN COROUTINE: Kích hoạt tuần tự bằng Yield Return
    IEnumerator ChuoiKhongKichRoutine()
    {
        // 1. Máy bay xuất hiện
        if (ojectMayBay != null)
        {
            yield return new WaitForSeconds(thoiGianMayBayXuatHien);
            ojectMayBay.SetActive(true);
        }

        // 2. Kích hoạt các ô Collider nổ từ từ theo thứ tự mảng
        if (danhSachVungNoCollider != null && danhSachVungNoCollider.Length > 0)
        {
            foreach (GameObject vungNo in danhSachVungNoCollider)
            {
                if (vungNo != null)
                {
                    // Bật ô hiện tại lên để quét sát thương quái
                    vungNo.SetActive(true);

                    // Chờ một chút rồi tắt ô đó đi (bằng cách gọi một hàm phụ tránh làm nghẽn vòng lặp)
                    StartCoroutine(TuDongTatVungNo(vungNo, thoiGianTonTaiCuaO));

                    // Hoãn lại một khoảng thời gian quy định trước khi kích hoạt ô tiếp theo trong mảng
                    yield return new WaitForSeconds(thoiGianDelayGiuaCacO);
                }
            }
        }

        // 3. Máy bay biến mất sau khoảng thời gian định sẵn
        yield return new WaitForSeconds(thoiGianMayBayBienMat);
        if (ojectMayBay != null) ojectMayBay.SetActive(false);
    }

    IEnumerator TuDongTatVungNo(GameObject targetVungNo, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (targetVungNo != null) targetVungNo.SetActive(false);
    }

    void ẨnHếtVùngCollider()
    {
        if (danhSachVungNoCollider == null) return;
        foreach (GameObject vungNo in danhSachVungNoCollider)
        {
            if (vungNo != null) vungNo.SetActive(false);
        }
    }
}