using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    [Header("Bảng trạng thái trận đấu")]
    [SerializeField] public GameObject BanThuaGame;
    [SerializeField] public GameObject Wingame;

    [Header("Giao diện UI & Animator riêng biệt")]
    [SerializeField] public GameObject mainMenuUI;
    [SerializeField] public Animator mainMenuAnimator;

    [Header("--- HỆ THỐNG SETTING UI (MỚI NÂNG CẤP) ---")]
    [SerializeField] public GameObject settingUI;
    [SerializeField] public Animator settingAnimator;
    [SerializeField] public float thoiGianChoAnSetting = 1.5f;
    [SerializeField] public string animHienSetting = "HienSetting";
    [SerializeField] public string animAnSetting = "AnSetting";

    [Header("--- HỆ THỐNG QUẢN LÝ CONTINUE & START GAME ---")]
    [SerializeField] private Button nutContinue;
    [SerializeField] private CanvasGroup canvasGroupContinue;

    [Tooltip("Bảng chọn Map dành riêng cho nút START GAME (Luôn khóa Map 2, 3)")]
    [SerializeField] public GameObject bangMapStartGameUI;
    [SerializeField] public Animator bangMapStartAnimator;

    [Header("Các nút bấm chọn Map của bảng Start Game để khóa")]
    [SerializeField] private Button nutChonMap2_StartGame;
    [SerializeField] private Button nutChonMap3_StartGame;

    [Tooltip("Bảng chọn Map dành riêng cho nút CONTINUE (Mở khóa dựa theo tiến trình file Save)")]
    [SerializeField] public GameObject bangMapContinueUI;
    [SerializeField] public Animator bangMapContinueAnimator;

    [Header("Các nút bấm chọn Map của bảng Continue để ẩn/hiện")]
    [SerializeField] private Button nutChonMap2_Continue;
    [SerializeField] private Button nutChonMap3_Continue;

    [Header("--- HỆ THỐNG CHỌN TƯỚNG ---")]
    [Tooltip("Bảng chứa các nút bấm hoặc khu vực Collider để người chơi chọn Tướng")]
    [SerializeField] private GameObject bangChonTuongUI;

    // 🌟 ĐÃ ĐỔI TÊN BIẾN CHO ĐÚNG Ý NGHĨA MỚI:
    [Tooltip("Thời gian chờ (giây) sau khi bảng Map hiện lên rồi mới kích hoạt bảng chọn Tướng.")]
    [SerializeField] private float thoiGianChoDeHienBangTuong = 1.0f;

    [Header("Tự quy định thời gian chờ (giây)")]
    [SerializeField] public float thoiGianChoAnMenu = 1.5f;
    [SerializeField] public float thoiGianChoAnBangMap = 1.5f;

    [Header("Tên các Animation (Điền đúng tên trong Animator)")]
    [SerializeField] public string animHienMenu = "HienMenu";
    [SerializeField] public string animAnMenu = "AnMenu";
    [SerializeField] public string animHienBangMap = "HienMap";
    [SerializeField] public string animAnBangMap = "AnMap";

    [Header("Cấu hình Scenes")]
    [SerializeField] public Object scene1;
    [SerializeField] public Object scene2;
    [SerializeField] public Object scene3;
    [SerializeField] public Object Menu;
    [SerializeField] public Object UpdateBase;
    [SerializeField] public GameObject pauGames;
    private bool isPaused = false;

    [Header("Kết nối Hệ thống Save để lưu độ khó")]
    [SerializeField] public SaveSystem boQuanLySave;

    [Header("--- VIỀN ĐỘ KHÓ CỦA BẢNG START GAME ---")]
    [SerializeField] private GameObject vienDoKhoDe_StartGame;
    [SerializeField] private GameObject vienDoKhoBinhThuong_StartGame;
    [SerializeField] private GameObject vienDoKhoKho_StartGame;

    [Header("--- VIỀN ĐỘ KHÓ CỦA BẢNG CONTINUE ---")]
    [SerializeField] private GameObject vienDoKhoDe_Continue;
    [SerializeField] private GameObject vienDoKhoBinhThuong_Continue;
    [SerializeField] private GameObject vienDoKhoKho_Continue;

    private AudioSource Amthanh;
    [SerializeField] private AudioClip Click;

    void Start()
    {
        Amthanh = GetComponent<AudioSource>();
        if (pauGames != null) pauGames.SetActive(false);

        BanThuaGame.SetActive(false);
        Wingame.SetActive(false);

        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (bangMapStartGameUI != null) bangMapStartGameUI.SetActive(false);
        if (bangMapContinueUI != null) bangMapContinueUI.SetActive(false);
        if (settingUI != null) settingUI.SetActive(false);

        // Ẩn bảng tướng lúc vừa vào Menu đầu game
        if (bangChonTuongUI != null) bangChonTuongUI.SetActive(false);

        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        CapNhatHienThiVienTheoSave();
        KiemTraTrangThaiNutContinue();
    }

    private void KiemTraTrangThaiNutContinue()
    {
        if (boQuanLySave != null)
        {
            bool coFileSave = boQuanLySave.KiemTraCoFileSave();
            if (nutContinue != null) nutContinue.interactable = coFileSave;
            if (canvasGroupContinue != null) canvasGroupContinue.alpha = coFileSave ? 1.0f : 0.4f;
        }
    }

    // ================= LOGIC ĐIỀU KHIỂN BẢNG SETTING =================
    public void MoBangSetting()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);

        if (mainMenuUI != null && settingUI != null && mainMenuAnimator != null && settingAnimator != null)
        {
            StartCoroutine(LuongMoBangSetting());
        }
    }

    private System.Collections.IEnumerator LuongMoBangSetting()
    {
        mainMenuAnimator.Play(animAnMenu);
        yield return new WaitForSeconds(thoiGianChoAnMenu);
        mainMenuUI.SetActive(false);

        settingUI.SetActive(true);
        settingAnimator.Play(animHienSetting);
    }

    public void DongBangSetting()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);

        if (mainMenuUI != null && settingUI != null && mainMenuAnimator != null && settingAnimator != null)
        {
            StartCoroutine(LuongDongBangSetting());
        }
    }

    private System.Collections.IEnumerator LuongDongBangSetting()
    {
        settingAnimator.Play(animAnSetting);
        yield return new WaitForSeconds(thoiGianChoAnSetting);
        settingUI.SetActive(false);

        mainMenuUI.SetActive(true);
        mainMenuAnimator.Play(animHienMenu);
    }

    // ================= CLICK CHỌN START GAME (BẢNG 1) =================
    public void MoBangMapStartGame()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        if (mainMenuUI != null && bangMapStartGameUI != null && mainMenuAnimator != null && bangMapStartAnimator != null)
        {
            if (nutChonMap2_StartGame != null) nutChonMap2_StartGame.interactable = false;
            if (nutChonMap3_StartGame != null) nutChonMap3_StartGame.interactable = false;

            StartCoroutine(LuongMoBangMapStart());
        }
    }

    // 🌟 ĐÃ SỬA: Quy trình luồng thời gian Start Game mới
    private System.Collections.IEnumerator LuongMoBangMapStart()
    {
        // 1. Tắt Menu chính
        mainMenuAnimator.Play(animAnMenu);
        yield return new WaitForSeconds(thoiGianChoAnMenu);
        mainMenuUI.SetActive(false);

        // 2. Hiện bảng chọn Map Start Game lên trước
        bangMapStartGameUI.SetActive(true);
        bangMapStartAnimator.Play(animHienBangMap);

        // 3. Đóng băng một lát (Thời gian trì hoãn do bạn chỉnh) rồi mới kích hoạt bảng tướng đè lên
        yield return new WaitForSeconds(thoiGianChoDeHienBangTuong);
        if (bangChonTuongUI != null)
        {
            bangChonTuongUI.SetActive(true);
        }
    }

    public void DongBangMapStartGame()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);

        // Chống lỗi: Nếu người chơi ấn thoát ra Menu, tắt luôn bảng tướng đi kèm
        if (bangChonTuongUI != null) bangChonTuongUI.SetActive(false);

        StartCoroutine(LuongDongBangMapStart());
    }

    private System.Collections.IEnumerator LuongDongBangMapStart()
    {
        bangMapStartAnimator.Play(animAnBangMap);
        yield return new WaitForSeconds(thoiGianChoAnBangMap);
        bangMapStartGameUI.SetActive(false);
        mainMenuUI.SetActive(true);
        mainMenuAnimator.Play(animHienMenu);
    }

    // ================= CLICK CHỌN CONTINUE (BẢNG 2) =================
    public void MoBangMapContinue()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        if (boQuanLySave != null && boQuanLySave.KiemTraCoFileSave())
        {
            if (mainMenuUI != null && bangMapContinueUI != null && mainMenuAnimator != null && bangMapContinueAnimator != null)
            {
                int maxMapMoKhoa = boQuanLySave.DocTienTrinhMap();

                if (nutChonMap2_Continue != null) nutChonMap2_Continue.interactable = (maxMapMoKhoa >= 2);
                if (nutChonMap3_Continue != null) nutChonMap3_Continue.interactable = (maxMapMoKhoa >= 3);

                StartCoroutine(LuongMoBangMapContinue());
            }
        }
    }

    // 🌟 ĐÃ SỬA: Quy trình luồng thời gian Continue mới
    private System.Collections.IEnumerator LuongMoBangMapContinue()
    {
        // 1. Tắt Menu chính
        mainMenuAnimator.Play(animAnMenu);
        yield return new WaitForSeconds(thoiGianChoAnMenu);
        mainMenuUI.SetActive(false);

        // 2. Hiện bảng chọn Map Continue lên trước
        bangMapContinueUI.SetActive(true);
        bangMapContinueAnimator.Play(animHienBangMap);

        // 3. Đóng băng một lát rồi mới kích hoạt bảng tướng đè lên
        yield return new WaitForSeconds(thoiGianChoDeHienBangTuong);
        if (bangChonTuongUI != null)
        {
            bangChonTuongUI.SetActive(true);
        }
    }

    public void HanhDongChonTuongButton(int idTuong)
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        if (boQuanLySave != null)
        {
            boQuanLySave.LuuTuongDaChon(idTuong);
        }
    }

    public void DongBangMapContinue()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);

        // Chống lỗi: Tắt kèm bảng tướng nếu thoát ra Menu
        if (bangChonTuongUI != null) bangChonTuongUI.SetActive(false);

        StartCoroutine(LuongDongBangMapContinue());
    }

    private System.Collections.IEnumerator LuongDongBangMapContinue()
    {
        bangMapContinueAnimator.Play(animAnBangMap);
        yield return new WaitForSeconds(thoiGianChoAnBangMap);
        bangMapContinueUI.SetActive(false);
        mainMenuUI.SetActive(true);
        mainMenuAnimator.Play(animHienMenu);
    }

    private void CapNhatHienThiVienTheoSave()
    {
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        int doKhoHienTai = 1;
        if (boQuanLySave != null) doKhoHienTai = boQuanLySave.DocDoKhoGame();

        KichHoatVienDuyNhat(doKhoHienTai);
    }

    private void KichHoatVienDuyNhat(int indexDoKho)
    {
        if (vienDoKhoDe_StartGame != null) vienDoKhoDe_StartGame.SetActive(false);
        if (vienDoKhoBinhThuong_StartGame != null) vienDoKhoBinhThuong_StartGame.SetActive(false);
        if (vienDoKhoKho_StartGame != null) vienDoKhoKho_StartGame.SetActive(false);

        if (vienDoKhoDe_Continue != null) vienDoKhoDe_Continue.SetActive(false);
        if (vienDoKhoBinhThuong_Continue != null) vienDoKhoBinhThuong_Continue.SetActive(false);
        if (vienDoKhoKho_Continue != null) vienDoKhoKho_Continue.SetActive(false);

        if (indexDoKho == 0)
        {
            if (vienDoKhoDe_StartGame != null) vienDoKhoDe_StartGame.SetActive(true);
            if (vienDoKhoDe_Continue != null) vienDoKhoDe_Continue.SetActive(true);
        }
        else if (indexDoKho == 1)
        {
            if (vienDoKhoBinhThuong_StartGame != null) vienDoKhoBinhThuong_StartGame.SetActive(true);
            if (vienDoKhoBinhThuong_Continue != null) vienDoKhoBinhThuong_Continue.SetActive(true);
        }
        else if (indexDoKho == 2)
        {
            if (vienDoKhoKho_StartGame != null) vienDoKhoKho_StartGame.SetActive(true);
            if (vienDoKhoKho_Continue != null) vienDoKhoKho_Continue.SetActive(true);
        }
    }

    public void ChonDoKhoDe()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        if (boQuanLySave != null) boQuanLySave.LuuDoKhoGame(0);
        KichHoatVienDuyNhat(0);
    }

    public void ChonDoKhoBinhThuong()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        if (boQuanLySave != null) boQuanLySave.LuuDoKhoGame(1);
        KichHoatVienDuyNhat(1);
    }

    public void ChonDoKhoKho()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        if (boQuanLySave != null) boQuanLySave.LuuDoKhoGame(2);
        KichHoatVienDuyNhat(2);
    }

    public void VaoGame()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(scene1.name);
    }

    public void TryAgain()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        Time.timeScale = 1;
    }

    public void VeMenu()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(Menu.name);
        Time.timeScale = 1;
    }

    private void FixedUpdate()
    {
        ThuaGame();
        KiemTraWinGame();
    }

    private void ThuaGame()
    {
        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver)
        {
            BanThuaGame.SetActive(true);
            Time.timeScale = 1;
        }
    }

    public void KiemTraWinGame()
    {
        if (ChaosDirector.instance != null && ChaosDirector.instance.WinGame)
        {
            if (!Wingame.activeSelf)
            {
                Wingame.SetActive(true);
                Time.timeScale = 1;

                string tenSceneHienTai = SceneManager.GetActiveScene().name;
                if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

                if (boQuanLySave != null)
                {
                    if (tenSceneHienTai == scene1.name) boQuanLySave.LuuTienTrinhMap(2);
                    else if (tenSceneHienTai == scene2.name) boQuanLySave.LuuTienTrinhMap(3);
                }
            }
        }
    }

    public void Map2()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(scene2.name);
        Time.timeScale = 1;
    }

    public void Map3()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(scene3.name);
        Time.timeScale = 1;
    }

    public void Updates()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(UpdateBase.name);
        Time.timeScale = 1;
    }

    public void Resume()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        pauGames.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    public void pausegame()
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        if (pauGames != null) pauGames.SetActive(!pauGames.activeInHierarchy);
    }

    public void TogglePauseGame()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void HanhDongChonTuongTrucCiep(int idTuong, GameObject goTuong)
    {
        if (Amthanh != null && Click != null) Amthanh.PlayOneShot(Click);
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        if (boQuanLySave != null)
        {
            boQuanLySave.LuuTuongDaChon(idTuong);
        }

        Animator animTuongMoiClick = goTuong.GetComponentInChildren<Animator>();
        if (animTuongMoiClick != null)
        {
            // Điền lệnh kích hoạt nếu cần
        }
    }
}