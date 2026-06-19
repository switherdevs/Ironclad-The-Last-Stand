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

    [Header("--- SỐ LƯỢNG QUÁI GỐC ---")]
    public CauHinhQuai soLuongQuaiTheoDoKho;

    [Header("--- SỐ LƯỢNG QUÁI BỔ SUNG ---")]
    [Tooltip("Số lượng quái cộng thêm/bổ sung riêng biệt cho từng độ khó")]
    public CauHinhQuai soLuongQuaiBoSungTheoDoKho;

    [Header("--- QUẢN LÝ SỐ LƯỢNG QUÁI HIỆN TẠI ---")]
    [SerializeField] private int soLuongQuaiHienTai = 0;
    public int SoLuongQuaiHienTai => soLuongQuaiHienTai;

    [System.Serializable]
    public struct CauHinhThongBaoWave
    {
        [Tooltip("Phần trăm tiến trình trận đấu để kích hoạt (Ví dụ: 20, 50, 70...)")]
        public float phanTramKichHoat;
        [Tooltip("Bảng GameObject thông báo chữ Wave hiện lên màn hình")]
        public GameObject bangThongBaoUI;
        [Tooltip("Âm thanh thông báo riêng cho Wave này")]
        public AudioClip amThanhWave;
        [HideInInspector] public bool daKichHoat;
    }

    [Header("--- CẤU HÌNH THÔNG BÁO WAVE THEO MỐC ---")]
    [SerializeField] private List<CauHinhThongBaoWave> danhSachThongBaoWave;
    [SerializeField] private float thoiGianChoAnThongBao = 3.0f;

    [Header("--- CẤU HÌNH CHỐNG TRÙNG VỊ TRÍ ---")]
    public float doLechSpawnX = 1.5f;

    [Header("--- KẾT NỐI UI MÀN HÌNH ---")]
    public TextMeshProUGUI textDongHo;
    public Slider thanhTienTrinhGame;

    [Header("--- KẾT NỐI HỆ THỐNG SAVE ---")]
    public SaveSystem boQuanLySave;

    [Header("--- DANH SÁCH VỊ TRÍ SPAWN QUÁI LÍNH ---")]
    public Transform[] danhSachDiemSpawn;

    [Header("--- VỊ TRÍ SPAWN CỐ ĐỊNH CHO BOSS ---")]
    [Tooltip("Kéo Object vị trí chỉ định dành riêng cho Boss vào đây")]
    public Transform diemSpawnBossCoDinh;

    [Header("--- KHUÔN ĐÚC CÁC LOẠI QUÁI ---")]
    public GameObject prefabZealot;
    public GameObject prefabMarine;
    public GameObject prefabMarine_Sword;
    public GameObject prefabTerminator;
    public GameObject prefabHellBrute;
    public GameObject prefabDemonPrince;

    [Header("--- KHUÔN ĐÚC MINI-BOSS ---")]
    public GameObject prefabMiniBoss;

    [Header("--- ÂM THANH XUẤT QUÂN ---")]
    [SerializeField] private AudioSource nguonAmThanh;
    [SerializeField] private AudioClip amThanhBatDauQuaiRa;

    [Header("--- Thời gian và số lượng quái ---")]
    public int ThoiGianSpawn_gd1 = 6;
    public int ThoiGianSpawn_gd2 = 4;
    public int ThoiGianSpawn_gd3 = 3;

    private float dongHoDem = -20f;
    private float tongThoiGian = 600f;
    public bool WinGame = false;

    private bool daSpawnMiniBoss = false;
    private bool daSpawnBossCuoi = false;
    private bool daPhatSoundBatDau = false;

    [Header("--- CẤU HÌNH TIẾN TRÌNH MÀN HIỆN TẠI ---")]
    public int indexMapHienTai = 1;

    private void Awake()
    {
        instance = this;

        soLuongQuaiTheoDoKho.ez = 3;
        soLuongQuaiTheoDoKho.nm = 5;
        soLuongQuaiTheoDoKho.hr = 7;

        soLuongQuaiBoSungTheoDoKho.ez = 1;
        soLuongQuaiBoSungTheoDoKho.nm = 2;
        soLuongQuaiBoSungTheoDoKho.hr = 4;
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

        if (nguonAmThanh == null) nguonAmThanh = GetComponent<AudioSource>();

        if (danhSachThongBaoWave != null)
        {
            foreach (var wave in danhSachThongBaoWave)
            {
                if (wave.bangThongBaoUI != null) wave.bangThongBaoUI.SetActive(false);
            }
        }

        StartCoroutine(HeThongQuanLySpawnQuaiToiUu());
    }

    void Update()
    {
        if (WinGame) return;
        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver) return;

        if (dongHoDem >= 0f && !daPhatSoundBatDau)
        {
            daPhatSoundBatDau = true;

            if (nguonAmThanh != null && amThanhBatDauQuaiRa != null)
            {
                nguonAmThanh.PlayOneShot(amThanhBatDauQuaiRa);
            }

            // ⭐ SỬA LỖI: Nếu map này KHÔNG CÓ BOSS (prefab == null), tự đánh dấu là đã xử lý boss xong để tránh kẹt logic giảm quái.
            if (prefabDemonPrince != null)
            {
                if (!daSpawnBossCuoi)
                {
                    daSpawnBossCuoi = true;
                    Vector3 viTriSpawnBoss = (diemSpawnBossCoDinh != null) ? diemSpawnBossCoDinh.position : transform.position;
                    SimpleObjectPool.Instance.LayQuaiRa(prefabDemonPrince, viTriSpawnBoss);
                    TangQuai(1);
                    Debug.Log("[CẢNH BÁO BOSS TRÙM] Demon Prince đã xuất hiện tại vị trí cố định!");
                }
            }
            else
            {
                // Map 1 và Map 2 lọt vào đây, coi như không cần tính boss cuối
                daSpawnBossCuoi = true;
            }
        }

        float tienTrinhHienTai = (dongHoDem / tongThoiGian) * 100f;
        KiemTraKichHoatThongBaoWave(tienTrinhHienTai);

        if (dongHoDem >= tongThoiGian)
        {
            // Khi hết 600 giây, quét sạch quái trên map là thắng luôn!
            if (KiemTraKhongConQuaiTrenBanDo())
            {
                WinGame = true;
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

    public void TangQuai(int soLuong = 1) => soLuongQuaiHienTai += soLuong;

    public void GiamQuai(int soLuong = 1)
    {
        soLuongQuaiHienTai -= soLuong;
        if (soLuongQuaiHienTai < 0) soLuongQuaiHienTai = 0;
    }

    private void KiemTraKichHoatThongBaoWave(float phanTramHienTai)
    {
        if (danhSachThongBaoWave == null) return;

        for (int i = 0; i < danhSachThongBaoWave.Count; i++)
        {
            var wave = danhSachThongBaoWave[i];
            if (phanTramHienTai >= wave.phanTramKichHoat && !wave.daKichHoat)
            {
                wave.daKichHoat = true;
                danhSachThongBaoWave[i] = wave;

                if (wave.bangThongBaoUI != null)
                {
                    StartCoroutine(LuongHienThiThongBao(wave.bangThongBaoUI, wave.amThanhWave));
                }
            }
        }
    }

    private IEnumerator LuongHienThiThongBao(GameObject uiGo, AudioClip clip)
    {
        uiGo.SetActive(true);
        if (nguonAmThanh != null && clip != null)
        {
            nguonAmThanh.PlayOneShot(clip);
        }
        yield return new WaitForSeconds(thoiGianChoAnThongBao);
        uiGo.SetActive(false);
    }

    private void TuDongSaveTienKhiWinGame()
    {
        HeThongKinhTe kinhTe = FindFirstObjectByType<HeThongKinhTe>();
        if (kinhTe != null && boQuanLySave != null)
        {
            int tienKiemDuoc = kinhTe.tienNangCapLinh;
            int tienSaveCu = boQuanLySave.DocThongTinGame();
            int tongTienMoi = tienSaveCu + tienKiemDuoc;
            boQuanLySave.LuuThongTinGame(tongTienMoi);

            int mapDuocMoKhoaTiepTheo = indexMapHienTai + 1;
            boQuanLySave.LuuTienTrinhMap(mapDuocMoKhoaTiepTheo);
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
                    TangQuai(1);
                    Debug.Log("[CẢNH BÁO] Mini-Boss đã xuất hiện!");
                }

                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabZealot));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine_Sword));
                yield return StartCoroutine(AloloGoiKhoRaQuaiToiUu(prefabMarine));

                SimpleObjectPool.Instance.LayQuaiRa(prefabTerminator, LayViTriSpawnTandRa());
                TangQuai(1);
                yield return new WaitForSeconds(ThoiGianSpawn_gd3);
            }

            yield return null;
        }

        if (!daSpawnMiniBoss && prefabMiniBoss != null)
        {
            daSpawnMiniBoss = true;
            SimpleObjectPool.Instance.LayQuaiRa(prefabMiniBoss, LayViTriSpawnTandRa());
            TangQuai(1);
        }
    }

    IEnumerator AloloGoiKhoRaQuaiToiUu(GameObject khuonMuonLay)
    {
        if (khuonMuonLay == null || SimpleObjectPool.Instance == null) yield break;

        // ⭐ SỬA LỖI: Khi thời gian đã hết (>= 600s), chặn đứng hoàn toàn việc sinh thêm lính mới
        if (dongHoDem >= tongThoiGian) yield break;

        int soLuongQuaiGocMax = LaySoLuongQuaiToiDaTheoDoKho();
        int soLuongBoSung = LaySoLuongQuaiBoSungTheoDoKho();
        int tongMax = soLuongQuaiGocMax + soLuongBoSung;

        int soLuongQuaiDotNay = Random.Range(1, tongMax + 1);

        if (daSpawnBossCuoi)
        {
            soLuongQuaiDotNay = Mathf.Max(1, soLuongQuaiDotNay / 2);
        }

        for (int i = 0; i < soLuongQuaiDotNay; i++)
        {
            Vector3 viTriTandRa = LayViTriSpawnTandRa();
            SimpleObjectPool.Instance.LayQuaiRa(khuonMuonLay, viTriTandRa);
            TangQuai(1);
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

    int LaySoLuongQuaiBoSungTheoDoKho()
    {
        switch (doKhoHienTai)
        {
            case DoKho.Easy: return soLuongQuaiBoSungTheoDoKho.ez;
            case DoKho.Normal: return soLuongQuaiBoSungTheoDoKho.nm;
            case DoKho.Hard: return soLuongQuaiBoSungTheoDoKho.hr;
            default: return soLuongQuaiBoSungTheoDoKho.nm;
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
            textDongHo.text = string.Format("{0:00}:{1:00} (Quái: {2})", phut, giay, soLuongQuaiHienTai);
            if (thanhTienTrinhGame != null) thanhTienTrinhGame.value = dongHoDem;
        }
    }

    private bool KiemTraKhongConQuaiTrenBanDo()
    {
        if (soLuongQuaiHienTai > 0) return false;

        BaseEnemy[] danhSachQuaiThongThuong = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        EnemyCharger[] danhSachQuaiCharger = FindObjectsByType<EnemyCharger>(FindObjectsSortMode.None);
        return (danhSachQuaiThongThuong.Length == 0 && danhSachQuaiCharger.Length == 0);
    }
}