using System.Collections; // BẮT BUỘC PHẢI CÓ: Để sử dụng được kiểu cấu trúc IEnumerator
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChaosDirector : MonoBehaviour
{
    public static ChaosDirector instance { get; private set; }

    [Header("--- KẾT NỐI UI MÀN HÌNH ---")]
    public TextMeshProUGUI textDongHo;
    public Slider thanhTienTrinhGame;

    [Header("--- DANH SÁCH 3 VỊ TRÍ SPAWN QUÁI ---")]
    public Transform[] danhSachDiemSpawn;

    [Header("--- KHUÔN ĐÚC CÁC LOẠI QUÁI ---")]
    public GameObject prefabZealot;
    public GameObject prefabMarine;
    public GameObject prefabMarine_Sword;
    public GameObject prefabTerminator;
    public GameObject prefabHellBrute;
    public GameObject prefabDemonPrince;

    private float dongHoDem = -20f;
    private float tongThoiGian = 600f;
    //private bool daRaBossCuoi = false;
    public bool WinGame = false;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (thanhTienTrinhGame != null)
        {
            thanhTienTrinhGame.minValue = 0f;
            thanhTienTrinhGame.maxValue = tongThoiGian;
            thanhTienTrinhGame.value = 0f;
        }

        // KÍCH HOẠT ĐƯỜNG DÂY TỐI ƯU: Chạy lõi quản lý sinh quái độc lập với hàm Update
        StartCoroutine(HeThongQuanLySpawnQuaiToiUu());
    }

    void Update()
    {
        // ĐÃ SỬA: Kiểm tra nếu thời gian chạy chạm hoặc vượt mốc tổng thời gian thì Thắng trận
        if (dongHoDem >= tongThoiGian)
        {
            WinGame = true;
        }
        if (WinGame) return;

        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver) return;

        // Hàm Update bây giờ SIÊU NHẸ, chỉ duy nhất làm nhiệm vụ chạy thời gian và vẽ giao diện UI
        if (dongHoDem < tongThoiGian)
        {
            dongHoDem += Time.deltaTime;
            ChayGiaoDienUI();
        }
    }

    // LÕI TỐI ƯU CƠ CHẾ SINH QUÁI (COROUTINE)
    IEnumerator HeThongQuanLySpawnQuaiToiUu()
    {
        // VÒNG LẶP VÔ HẠN: Chạy song song với game cho đến khi hết trận
        while (dongHoDem < tongThoiGian)
        {
            // Nếu game đang trong thời gian 10 giây chuẩn bị (đồng hồ âm), máy tính sẽ treo lệnh chờ 0.1 giây rồi kiểm tra lại
            if (dongHoDem < 0f)
            {
                yield return new WaitForSeconds(0.1f);
                continue; // Quay lại đầu vòng lặp while, bỏ qua đoạn sinh quái phía dưới
            }

            float tienTrinh = (dongHoDem / tongThoiGian) * 100f;

            // GIAI ĐOẠN 1: Dưới 20% thời gian (2 phút đầu)
            if (tienTrinh < 20f)
            {
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot)); // Gọi quái
                yield return new WaitForSeconds(7f); // Đợi 6 giây sau mới thực hiện đợt quét tiếp theo
            }
            // GIAI ĐOẠN 2: Từ 20% đến 60% thời gian (Từ phút thứ 2 đến phút thứ 6)
            else if (tienTrinh >= 20f && tienTrinh < 60f)
            {
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine_Sword)); // Đẻ con này sau con trước vài mili-giây
                yield return new WaitForSeconds(6f); // Đợi 5 giây
            }
            // GIAI ĐOẠN 3: SỬA ĐỒNG BỘ - Để kết nối mượt mà với Giai đoạn 4, loại bỏ khoảng treo máy
            else if (tienTrinh >= 60f && tienTrinh < 80f)
            {
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine_Sword));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine));
                yield return new WaitForSeconds(5f); // Đợi 4 giây
            }
            // GIAI ĐOẠN 4: Trên 90% thời gian (Phút thứ 8 trở đi)
            else if (tienTrinh >= 80f)
            {
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine_Sword));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine));
                SimpleObjectPool.Instance.LayQuaiRa(prefabTerminator, LayViTriSpawnNgauNhien());

                //if (!daRaBossCuoi)
                //{
                //    Vector3 viTriGiaoBoss = LayViTriSpawnNgauNhien();
                //    SimpleObjectPool.Instance.LayQuaiRa(prefabDemonPrince, viTriGiaoBoss);
                //    daRaBossCuoi = true;
                //}
                yield return new WaitForSeconds(6f); // Đợi 2 giây dồn dập
            }

            // ĐÃ THÊM: Lệnh bảo hiểm tối ưu, ngăn treo Coroutine tuyệt đối nếu có mili-giây lệch mốc
            yield return null;
        }
    }

    // HÀM GIAO HÀNG TỐI ƯU: Đẻ từng con một cách nhau 1 khung hình, triệt tiêu lag dồn dập
    IEnumerator AloloGoiKhoRaQuaiToiUu(GameObject khuonMuonLay)
    {
        if (khuonMuonLay == null || SimpleObjectPool.Instance == null) yield break;

        int soLuongQuaiDotNay = Random.Range(1, 3);

        for (int i = 0; i < soLuongQuaiDotNay; i++)
        {
            Vector3 viTriNgauNhien = LayViTriSpawnNgauNhien();
            SimpleObjectPool.Instance.LayQuaiRa(khuonMuonLay, viTriNgauNhien);

            // DÒNG CODE CỨU CÁNH TOÀN DIỆN: Ép máy tính đẻ xong 1 con thì dừng lại, đợi đúng 1 khung hình sau mới đẻ tiếp con thứ 2
            yield return null;
        }
    }

    void ChayGiaoDienUI()
    {
        if (textDongHo == null) return;

        if (dongHoDem < 0f)
        {
            int giayChuanBi = Mathf.CeilToInt(Mathf.Abs(dongHoDem));
            textDongHo.text = "CHUẨN BỊ: " + giayChuanBi + "s";
            if (thanhTienTrinhGame != null) thanhTienTrinhGame.value = 0f;
        }
        else
        {
            int phut = Mathf.FloorToInt(dongHoDem / 60f);
            int giay = Mathf.FloorToInt(dongHoDem % 60f);
            textDongHo.text = string.Format("{0:00}:{1:00}", phut, giay);
            if (thanhTienTrinhGame != null) thanhTienTrinhGame.value = dongHoDem;
        }
    }

    Vector3 LayViTriSpawnNgauNhien()
    {
        if (danhSachDiemSpawn == null || danhSachDiemSpawn.Length == 0) return transform.position;
        int indexNgauNhien = Random.Range(0, danhSachDiemSpawn.Length);
        return danhSachDiemSpawn[indexNgauNhien].position;
    }
}