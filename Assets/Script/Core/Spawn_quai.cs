using System.Collections;
using System.Collections.Generic;
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
    public float doLechSpawnX = 1.5f;

    [Header("--- KẾT NỐI UI MÀN HÌNH ---")]
    public TextMeshProUGUI textDongHo;
    public Slider thanhTienTrinhGame;

    [Header("--- KẾT NỐI HỆ THỐNG SAVE ---")]
    public SaveSystem boQuanLySave;

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
    public GameObject prefabMiniBoss;

    [Header("--- Thời gian và số lượng quái ---")]
    public int ThoiGianSpawn_gd1 = 6;
    public int ThoiGianSpawn_gd2 = 4;
    public int ThoiGianSpawn_gd3 = 3;

    private float dongHoDem = -20f;
    private float tongThoiGian = 600f; // 10 phút
    public bool WinGame = false;

    private bool daSpawnMiniBoss = false;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        if (boQuanLySave != null)
        {
            int indexLuu = boQuanLySave.DocDoKhoGame();
            doKhoHienTai = (DoKho)indexLuu;
            Debug.Log($"[ChaosDirector] Đã áp dụng độ khó từ file save: {doKhoHienTai}");
        }

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

        // ĐIỀU KIỆN WIN GAME: Đủ 10 phút (tongThoiGian)
        if (dongHoDem >= tongThoiGian)
        {
            // Và giết sạch sẽ quái vật trên bản đồ
            if (KiemTraKhongConQuaiTrenBanDo())
            {
                WinGame = true;
                Debug.Log("[CHIẾN THẮNG] Toàn bộ quái vật đã bị tiêu diệt sạch sẽ sau khi hết giờ!");

                // KÍCH HOẠT TỰ ĐỘNG SAVE TIỀN KIẾM ĐƯỢC TỪ HỆ THỐNG KINH TẾ
                TuDongSaveTienKhiWinGame();
                return;
            }
        }
        else
        {
            dongHoDem += Time.deltaTime;
            ChayGiaoDienUI();
        }
    }

    // ─── [HÀM TỰ ĐỘNG SAVE] KẾT NỐI HETHONGKINHTE ĐỂ LẤY TIỀN THỰC TẾ ───
    private void TuDongSaveTienKhiWinGame()
    {
        // Tìm script HeThongKinhTe đang có trong trận đấu
        HeThongKinhTe kinhTe = FindFirstObjectByType<HeThongKinhTe>();

        if (kinhTe != null && boQuanLySave != null)
        {
            // 1. Lấy số tiền người chơi ĐÃ KIẾM ĐƯỢC TỪ GIẾT QUÁI nãy giờ trong trận
            // LƯU Ý: Bạn hãy đổi chữ "TienTrongTran" thành tên biến chứa tiền thực tế trong script HeThongKinhTe của bạn nhé!
            int tienKiemDuoc = kinhTe.tienNangCapLinh;

            // 2. Đọc số tiền tích lũy cũ đã có trong file savegame.txt từ trước
            int tienSaveCu = boQuanLySave.DocThongTinGame();

            // 3. Cộng dồn lại thành tổng tài sản mới
            int tongTienMoi = tienSaveCu + tienKiemDuoc;

            // 4. Ghi đè vào file savegame.txt
            boQuanLySave.LuuThongTinGame(tongTienMoi);

            Debug.Log($"[AUTO SAVE SUCCESS] Thắng trận! Tiền cũ: {tienSaveCu} | Tiền cày được từ giết quái: {tienKiemDuoc} | Tổng tiền mới đã lưu: {tongTienMoi}");
        }
        else
        {
            Debug.LogError("[SAVE ERROR] Thiếu HeThongKinhTe hoặc SaveSystem trong Scene, không thể auto save tiền!");
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

    private bool KiemTraKhongConQuaiTrenBanDo()
    {
        BaseEnemy[] danhSachQuaiThongThuong = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        EnemyCharger[] danhSachQuaiCharger = FindObjectsByType<EnemyCharger>(FindObjectsSortMode.None);
        return (danhSachQuaiThongThuong.Length == 0 && danhSachQuaiCharger.Length == 0);
    }
}