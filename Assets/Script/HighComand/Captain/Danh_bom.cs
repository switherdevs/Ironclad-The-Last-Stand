using UnityEngine;
using System.Collections;

public class MapAirStrikeController : MonoBehaviour
{
    [Header("--- CẤU HÌNH VÙNG THẢ BOM HÌNH VUÔNG ---")]
    [Tooltip("Kéo BoxCollider2D quy định vùng thả bom vào đây (Nên nằm chung trong một Prefab).")]
    public BoxCollider2D vungThaBomBoxCollider;

    [Header("--- CẤU HÌNH MÁY BAY NẰM TRONG PREFAB ---")]
    [Tooltip("🌟 QUAN TRỌNG: Kéo Object TRỐNG TRUNG GIAN (Cha của máy bay chứa Animation) vào đây.")]
    public GameObject ojectMayBay;

    [Tooltip("Kéo một Transform (Ví dụ: một GameObject trống đặt tên là Diem_Neo) nằm trong Prefab vào đây để làm mốc xuất phát cố định.")]
    public Transform viTriXuatPhatCuaShip;

    [Header("--- CẤU HÌNH LƯỢNG BOM VÀ THỜI GIAN ---")]
    [Tooltip("Số lượng quả bom xả xuống trong một lượt.")]
    public int soLuongBomMuonTha = 10;
    [Tooltip("Thời gian cách nhau giữa mỗi quả bom (giây). Tạo hiệu ứng rải từ trái qua phải.")]
    public float delayGiuaCacQuaBom = 0.15f;

    [Header("--- CẤU HÌNH BOM RƠI THẬT & RANDOM ---")]
    [Tooltip("Kéo Prefab quả bom vào đây.")]
    public GameObject prefabQuaBomThat;
    [Tooltip("Độ cao xuất phát của quả bom tính từ điểm đích.")]
    public float doCaoBomRoi = 10f;
    [Tooltip("Tốc độ lao xuống của quả bom.")]
    public float tocDoBomRoi = 20f;
    [Tooltip("Độ lệch ngẫu nhiên tối đa trục X của ĐIỂM XUẤT PHÁT bom trên trời.")]
    public float offsetNgauNhienX_DiemBatDau = 1.0f;

    [Header("--- VÙNG GÂY SÁT THƯƠNG KHI CHẠM ĐẤT ---")]
    [Tooltip("Kéo Prefab Vùng nổ (chứa Collider gây sát thương quái) vào đây.")]
    public GameObject prefabVungNoSattHuong;
    [Tooltip("Thời gian vùng sát thương tồn tại (giây).")]
    public float thoiGianTonTaiCuaO = 0.5f;

    private void Start()
    {
        // 🌟 Đảm bảo vừa vào game là cụm máy bay phải bị ẩn hoàn toàn
        if (ojectMayBay != null)
        {
            ojectMayBay.SetActive(false);
        }
    }

    public void KichHoatKhongKich()
    {
        if (vungThaBomBoxCollider == null)
        {
            Debug.LogError("[MapAirStrike] Thiếu BoxCollider2D vùng thả bom ngoài Inspector!");
            return;
        }
        StopAllCoroutines();
        StartCoroutine(ChuoiRaiThamBomRoutine());
    }

    // 🌟 THUẬT TOÁN COROUTINE: Reset vị trí vật lý trước, rồi ép Animator cập nhật lại
    IEnumerator ChuoiRaiThamBomRoutine()
    {
        // 1. 🎯 RESET VỊ TRÍ CỤM CHA TRUNG GIAN BẰNG BIẾN TRANSFORM
        if (ojectMayBay != null && viTriXuatPhatCuaShip != null)
        {
            ojectMayBay.transform.position = viTriXuatPhatCuaShip.position;
            ojectMayBay.transform.rotation = viTriXuatPhatCuaShip.rotation;

            // Bật cụm máy bay lên
            ojectMayBay.SetActive(true);

            // 🎯 LỆNH THÔNG MINH BỔ SUNG: Ép tất cả Animator con (nếu có) phải reset về trạng thái đầu tiên
            Animator animatorCuaMayBay = ojectMayBay.GetComponentInChildren<Animator>();
            if (animatorCuaMayBay != null)
            {
                animatorCuaMayBay.Rebind(); // Reset hoàn toàn dòng thời gian Animation về giây thứ 0
                animatorCuaMayBay.Update(0f); // Ép cập nhật ngay lập tức để tránh bị giật hình
            }
        }

        // 2. TÍNH TOÁN RANH GIỚI VÙNG HÌNH VUÔNG CỦA BOM
        Bounds ranhGioiVung = vungThaBomBoxCollider.bounds;
        float canhTrai = ranhGioiVung.min.x;
        float canhPhai = ranhGioiVung.max.x;
        float canhDuoi = ranhGioiVung.min.y;
        float canhTren = ranhGioiVung.max.y;

        float chieuRongVung = canhPhai - canhTrai;
        float buocNhayX = chieuRongVung / soLuongBomMuonTha;

        // 3. VÒNG LẶP THẢ BOM TỪ TRÁI QUA PHẢI
        for (int i = 0; i < soLuongBomMuonTha; i++)
        {
            if (prefabQuaBomThat == null) break;

            float toaDoX_DichDen = canhTrai + (i * buocNhayX) + Random.Range(-0.2f, 0.2f);
            toaDoX_DichDen = Mathf.Clamp(toaDoX_DichDen, canhTrai, canhPhai);

            float toaDoY_DichDen = Random.Range(canhDuoi, canhTren);

            Vector3 toaDoDichXuongDat = new Vector3(toaDoX_DichDen, toaDoY_DichDen, 0f);

            float toaDoX_XuatPhatTrenTroi = toaDoX_DichDen + Random.Range(-offsetNgauNhienX_DiemBatDau, offsetNgauNhienX_DiemBatDau);
            Vector3 viTriBomTrenTroi = new Vector3(toaDoX_XuatPhatTrenTroi, toaDoY_DichDen + doCaoBomRoi, 0f);

            Quaternion gocXoayNgauNhien = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            GameObject bomInstance = Instantiate(prefabQuaBomThat, viTriBomTrenTroi, gocXoayNgauNhien);

            SpriteRenderer sRendererBom = bomInstance.GetComponent<SpriteRenderer>();
            if (sRendererBom != null)
            {
                sRendererBom.sortingLayerName = "UI";
                sRendererBom.sortingOrder = 100;
            }

            AirStrikeBomb scriptBom = bomInstance.GetComponent<AirStrikeBomb>();
            if (scriptBom != null)
            {
                scriptBom.KhoiHanh(toaDoDichXuongDat, doCaoBomRoi, tocDoBomRoi, () => {

                    if (prefabVungNoSattHuong != null)
                    {
                        GameObject vungNoInstance = Instantiate(prefabVungNoSattHuong, toaDoDichXuongDat, Quaternion.identity);
                        Destroy(vungNoInstance, thoiGianTonTaiCuaO);
                    }
                });
            }

            yield return new WaitForSeconds(delayGiuaCacQuaBom);
        }

        // 4. TẮT CỤM MÁY BAY SAU KHI THẢ HẾT LOẠT BOM
        if (ojectMayBay != null)
        {
            ojectMayBay.SetActive(false);
        }
    }
}