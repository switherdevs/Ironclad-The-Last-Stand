using UnityEngine;
using System.Collections;

public class MapAirStrikeController : MonoBehaviour
{
    [Header("--- CẤU HÌNH VÙNG THẢ BOM HÌNH VUÔNG ---")]
    [Tooltip("Kéo BoxCollider2D quy định vùng thả bom vào đây.")]
    public BoxCollider2D vungThaBomBoxCollider;

    [Header("--- CẤU HÌNH MÁY BAY ---")]
    [Tooltip("Kéo Object chiếc máy bay ngoài Hierarchy vào đây.")]
    public GameObject ojectMayBay;
    [Tooltip("Thời gian CHỜ trước khi máy bay xuất hiện kể từ lúc bấm Skill (giây). Chỉnh số này LỚN = máy bay ra Trễ, NHỎ = máy bay ra Sớm.")]
    public float thoiGianMayBayXuatHienSomTre = 0.5f;
    [Tooltip("Thời gian máy bay bay trên bầu trời trước khi tự ẩn đi (giây).")]
    public float thoiGianMayBayBienMat = 3.0f;

    [Header("--- CẤU HÌNH LƯỢNG BOM VÀ THỜI GIAN ---")]
    [Tooltip("Số lượng quả bom xả xuống trong một lượt.")]
    public int soLuongBomMuonTha = 10;
    [Tooltip("Thời gian cách nhau giữa mỗi quả bom (giây). Tạo hiệu ứng rải từ trái qua phải.")]
    public float delayGiuaCacQuaBom = 0.15f;

    [Header("--- CẤU HÌNH BOM RƠI THẬT ---")]
    [Tooltip("Kéo Prefab quả bom vào đây.")]
    public GameObject prefabQuaBomThat;
    [Tooltip("Độ cao xuất phát của quả bom.")]
    public float doCaoBomRoi = 10f;
    [Tooltip("Tốc độ lao xuống của quả bom.")]
    public float tocDoBomRoi = 20f;

    [Header("--- VÙNG GÂY SÁT THƯƠNG KHI CHẠM ĐẤT ---")]
    [Tooltip("Kéo Prefab Vùng nổ (chứa Collider gây sát thương quái) vào đây.")]
    public GameObject prefabVungNoSattHuong;
    [Tooltip("Thời gian vùng sát thương tồn tại (giây).")]
    public float thoiGianTonTaiCuaO = 0.5f;

    private void Start()
    {
        // 🌟 THUẬT TOÁN KHỞI ĐẦU: Đảm bảo vừa vào game là máy bay phải bị ẩn hoàn toàn (Set Active = false)
        if (ojectMayBay != null)
        {
            ojectMayBay.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[MapAirStrike] Bạn chưa kéo Object máy bay vào ô ojectMayBay!");
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

    // 🌟 THUẬT TOÁN COROUTINE CẢI TIẾN: Điều khiển nhịp độ xuất hiện của máy bay và mưa bom
    IEnumerator ChuoiRaiThamBomRoutine()
    {
        // 1. 🌟 NÂNG CẤP MỚI: Chờ một khoảng thời gian chỉnh trước khi cho Máy bay xuất hiện
        yield return new WaitForSeconds(thoiGianMayBayXuatHienSomTre);

        if (ojectMayBay != null)
        {
            ojectMayBay.SetActive(true); // Chỉ bật lên khi đã hết thời gian chờ
        }

        // Tự động kích hoạt luồng tắt máy bay sau đó mà không làm nghẽn tiến trình thả bom dưới đất
        StartCoroutine(TuDongAnMayBaySauKhiBayXong());

        // 2. TÍNH TOÁN RANH GIỚI VÙNG HÌNH VUÔNG
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

            // Tính toán vị trí X tịnh tiến dần từ trái qua phải
            float toaDoX = canhTrai + (i * buocNhayX) + Random.Range(-0.2f, 0.2f);
            toaDoX = Mathf.Clamp(toaDoX, canhTrai, canhPhai);

            // Chọn ngẫu nhiên một cao độ Y nằm trong vùng hình vuông
            float toaDoY_DichDen = Random.Range(canhDuoi, canhTren);

            Vector3 toaDoDichXuongDat = new Vector3(toaDoX, toaDoY_DichDen, 0f);
            Vector3 viTriBomTrenTroi = new Vector3(toaDoX, toaDoY_DichDen + doCaoBomRoi, 0f);

            // Tạo góc xoay ngẫu nhiên sinh động cho quả bom
            Quaternion gocXoayNgauNhien = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            // Sinh quả bom
            GameObject bomInstance = Instantiate(prefabQuaBomThat, viTriBomTrenTroi, gocXoayNgauNhien);

            // Đồng bộ lớp hiển thị đè lên Map
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
    }

    // 🌟 HÀM PHỤ ĐỘC LẬP: Giúp máy bay tự tắt sau khi hoàn thành nhiệm vụ mà không ảnh hưởng vòng lặp bom
    IEnumerator TuDongAnMayBaySauKhiBayXong()
    {
        yield return new WaitForSeconds(thoiGianMayBayBienMat);
        if (ojectMayBay != null)
        {
            ojectMayBay.SetActive(false);
        }
    }
}