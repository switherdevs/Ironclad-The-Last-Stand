using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChaosDirector : MonoBehaviour
{
    public static ChaosDirector instance { get; private set; }

    public enum DoKho { Easy, Normal, Hard }

    [Header("--- THIẾT LẬP ĐỘ KHÓ GAME ---")]
    public DoKho doKhoHienTai = DoKho.Normal;

    [System.Serializable]
    public struct CauHinhQuai
    {
        public int ez;
        public int nm;
        public int hr;
    }
    public CauHinhQuai soLuongQuaiTheoDoKho = new CauHinhQuai { ez = 3, nm = 5, hr = 7 };

    [Header("--- CẤU HÌNH CHỐNG TRÙNG VỊ TRÍ ---")]
    [Tooltip("Khoảng cách lệch ngẫu nhiên sang trái/phải so với điểm gốc để quái không đè lên nhau")]
    public float doLechSpawnX = 1.5f;

    [Header("--- KẾT NỐI UI MÀN HÌNH ---")]
    public TextMeshProUGUI textDongHo;
    public Slider thanhTienTrinhGame;

    [Header("--- DANH SÁCH VỊ TRÍ SPAWN QUÁI ---")]
    public Transform[] danhSachDiemSpawn;

    [Header("--- KHUÔN ĐÚC CÁC LOẠI QUÁI ---")]
    public GameObject prefabZealot;
    public GameObject prefabMarine;
    public GameObject prefabMarine_Sword;
    public GameObject prefabTerminator;
    public GameObject prefabHellBrute;
    public GameObject prefabDemonPrince;

    [Header("--- KHUÔN ĐÚC MINI-BOSS ---")]
    [Tooltip("Kéo thả chủng quái muốn làm Mini-Boss vào đây (Ví dụ: HellBrute hoặc DemonPrince)")]
    public GameObject prefabMiniBoss;

    [Header("--- Thời gian và số lượng quái ---")]
    public int ThoiGianSpawn_gd1 = 6;
    public int ThoiGianSpawn_gd2 = 4;
    public int ThoiGianSpawn_gd3 = 3;

    private float dongHoDem = -20f;
    private float tongThoiGian = 600f;
    public bool WinGame = false;

    // Cờ hiệu kiểm soát chỉ sinh Mini-Boss đúng 1 lần duy nhất
    private bool daSpawnMiniBoss = false;

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

        StartCoroutine(HeThongQuanLySpawnQuaiToiUu());
    }

    void Update()
    {
        if (WinGame) return;

        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver) return;

        // CẬP NHẬT LOGIC WIN GAME: Hết thời gian VÀ đồng thời trên bản đồ không còn quái
        if (dongHoDem >= tongThoiGian)
        {
            if (KiemTraKhongConQuaiTrenBanDo())
            {
                WinGame = true;
                Debug.Log("[CHIẾN THẮNG] Toàn bộ quái vật đã bị tiêu diệt sạch sẽ sau khi hết giờ!");
                return;
            }
        }
        else
        {
            // Chỉ chạy đồng hồ đếm ngược khi chưa hết thời gian quy định
            dongHoDem += Time.deltaTime;
            ChayGiaoDienUI();
        }
    }

    IEnumerator HeThongQuanLySpawnQuaiToiUu()
    {
        while (dongHoDem < tongThoiGian)
        {
            if (dongHoDem < 0f)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            float tienTrinh = (dongHoDem / tongThoiGian) * 100f;

            if (tienTrinh < 20f)
            {
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return new WaitForSeconds(ThoiGianSpawn_gd1);
            }
            else if (tienTrinh >= 20f && tienTrinh < 50f)
            {
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine_Sword));
                yield return new WaitForSeconds(6f);
            }
            else if (tienTrinh >= 50f && tienTrinh < 70f)
            {
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine_Sword));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine));
                yield return new WaitForSeconds(ThoiGianSpawn_gd2);
            }
            else if (tienTrinh >= 70f)
            {
                // KIỂM TRA MINI-BOSS: Spawn chuẩn xác 1 lần độc nhất khi vừa bước vào giai đoạn cuối
                if (!daSpawnMiniBoss && prefabMiniBoss != null)
                {
                    daSpawnMiniBoss = true;
                    SimpleObjectPool.Instance.LayQuaiRa(prefabMiniBoss, LayViTriSpawnTandRa());
                    Debug.Log("[CẢNH BÁO] Mini-Boss đã xuất hiện trên chiến trường!");
                }

                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine_Sword));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine));

                SimpleObjectPool.Instance.LayQuaiRa(prefabTerminator, LayViTriSpawnTandRa());
                yield return new WaitForSeconds(ThoiGianSpawn_gd3);
            }

            yield return null;
        }

        // ĐÈ LOGIC PHÒNG BỊ: Nếu hết thời gian mà vì lý do gì đó Mini-Boss chưa kịp ra, ép xuất hiện luôn ở giây cuối cùng
        if (!daSpawnMiniBoss && prefabMiniBoss != null)
        {
            daSpawnMiniBoss = true;
            SimpleObjectPool.Instance.LayQuaiRa(prefabMiniBoss, LayViTriSpawnTandRa());
        }
    }

    IEnumerator AloloGoiKhoRaQuaiToiUu(GameObject khuonMuonLay)
    {
        if (khuonMuonLay == null || SimpleObjectPool.Instance == null) yield break;

        int soLuongQuaiMax = LaySoLuongQuaiToiDaTheoDoKho();
        int soLuongQuaiDotNay = Random.Range(1, soLuongQuaiMax + 1);

        for (int i = 0; i < soLuongQuaiDotNay; i++)
        {
            Vector3 viTriTandRa = LayViTriSpawnTandRa();
            SimpleObjectPool.Instance.LayQuaiRa(khuonMuonLay, viTriTandRa);
            yield return null;
        }
    }

    int LaySoLuongQuaiToiDaTheoDoKho()
    {
        switch (doKhoHienTai)
        {
            case DoKho.Easy: return soLuongQuaiTheoDoKho.ez;
            case DoKho.Normal: return soLuongQuaiTheoDoKho.nm;
            case DoKho.Hard: return soLuongQuaiTheoDoKho.hr;
            default: return soLuongQuaiTheoDoKho.nm;
        }
    }

    Vector3 LayViTriSpawnTandRa()
    {
        if (danhSachDiemSpawn == null || danhSachDiemSpawn.Length == 0) return transform.position;

        int indexNgauNhien = Random.Range(0, danhSachDiemSpawn.Length);
        Vector3 viTriGoc = danhSachDiemSpawn[indexNgauNhien].position;

        float lechX = Random.Range(-doLechSpawnX, doLechSpawnX);
        return new Vector3(viTriGoc.x + lechX, viTriGoc.y, viTriGoc.z);
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

    /// <summary>
    /// Hàm bổ sung quét hệ thống: Kiểm tra xem còn thực thể quái nào còn hoạt động hay không.
    /// </summary>
    private bool KiemTraKhongConQuaiTrenBanDo()
    {
        // Quét tìm tất cả các Object thừa kế từ lớp quái gốc BaseEnemy
        BaseEnemy[] danhSachQuaiThongThuong = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);

        // Quét bổ sung lớp quái đặc biệt Charger (Hell Iron) nếu nó không kế thừa từ BaseEnemy
        EnemyCharger[] danhSachQuaiCharger = FindObjectsByType<EnemyCharger>(FindObjectsSortMode.None);

        // Nếu cả 2 danh sách quét đều trống rỗng (bằng 0) -> Trả về true (Bản đồ sạch bóng quân thù)
        return (danhSachQuaiThongThuong.Length == 0 && danhSachQuaiCharger.Length == 0);
    }
}