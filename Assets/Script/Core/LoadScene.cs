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

    [Header("--- HỆ THỐNG CHỌN TƯỚNG (MỚI BỔ SUNG) ---")]
    [Tooltip("Bảng chứa các nút bấm để người chơi click chọn Tướng")]
    [SerializeField] private GameObject bangChonTuongUI;
    [Tooltip("Thời gian (giây) bảng tướng hiển thị hoạt ảnh trước khi tự tắt để hiện bảng Map")]
    [SerializeField] private float thoiGianActiveBangTuong = 3.0f;

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

    // ================= CLICK CHỌN START GAME (BẢNG 1) =================
    public void MoBangMapStartGame()
    {
        Amthanh.PlayOneShot(Click);
        if (mainMenuUI != null && bangMapStartGameUI != null && mainMenuAnimator != null && bangMapStartAnimator != null)
        {
            if (nutChonMap2_StartGame != null) nutChonMap2_StartGame.interactable = false;
            if (nutChonMap3_StartGame != null) nutChonMap3_StartGame.interactable = false;

            StartCoroutine(LuongMoBangMapStart());
        }
    }

    private System.Collections.IEnumerator LuongMoBangMapStart()
    {
        mainMenuAnimator.Play(animAnMenu);
        yield return new WaitForSeconds(thoiGianChoAnMenu);
        mainMenuUI.SetActive(false);

        // ⭐ NÂNG CẤP: Hiện bảng chọn tướng trước
        if (bangChonTuongUI != null)
        {
            bangChonTuongUI.SetActive(true);
            yield return new WaitForSeconds(thoiGianActiveBangTuong); // Chờ người chơi thao tác bấm chọn tướng
            bangChonTuongUI.SetActive(false);
        }

        bangMapStartGameUI.SetActive(true);
        bangMapStartAnimator.Play(animHienBangMap);
    }

    public void DongBangMapStartGame()
    {
        Amthanh.PlayOneShot(Click);
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
        Amthanh.PlayOneShot(Click);
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

    private System.Collections.IEnumerator LuongMoBangMapContinue()
    {
        mainMenuAnimator.Play(animAnMenu);
        yield return new WaitForSeconds(thoiGianChoAnMenu);
        mainMenuUI.SetActive(false);

        // ⭐ NÂNG CẤP: Hiện bảng chọn tướng trước khi vào bảng Continue Map
        if (bangChonTuongUI != null)
        {
            bangChonTuongUI.SetActive(true);
            yield return new WaitForSeconds(thoiGianActiveBangTuong);
            bangChonTuongUI.SetActive(false);
        }

        bangMapContinueUI.SetActive(true);
        bangMapContinueAnimator.Play(animHienBangMap);
    }

    // Hàm public này dùng để gán trực tiếp vào sự kiện OnClick của từng nút bấm Avatar Tướng ngoài Menu
    public void HanhDongChonTuongButton(int idTuong)
    {
        Amthanh.PlayOneShot(Click);
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        if (boQuanLySave != null)
        {
            boQuanLySave.LuuTuongDaChon(idTuong);
        }
    }

    public void DongBangMapContinue()
    {
        Amthanh.PlayOneShot(Click);
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
        Amthanh.PlayOneShot(Click);
        if (boQuanLySave != null) boQuanLySave.LuuDoKhoGame(0);
        KichHoatVienDuyNhat(0);
    }

    public void ChonDoKhoBinhThuong()
    {
        Amthanh.PlayOneShot(Click);
        if (boQuanLySave != null) boQuanLySave.LuuDoKhoGame(1);
        KichHoatVienDuyNhat(1);
    }

    public void ChonDoKhoKho()
    {
        Amthanh.PlayOneShot(Click);
        if (boQuanLySave != null) boQuanLySave.LuuDoKhoGame(2);
        KichHoatVienDuyNhat(2);
    }

    public void VaoGame()
    {
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(scene1.name);
    }

    public void TryAgain()
    {
        Amthanh.PlayOneShot(Click);
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        Time.timeScale = 1;
    }

    public void VeMenu()
    {
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(Menu.name);
        Time.timeScale = 1;
    }

    private void FixedUpdate()
    {
        ThuaGame();
        WinGame();
    }

    private void ThuaGame()
    {
        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver)
        {
            BanThuaGame.SetActive(true);
            Time.timeScale = 1;
        }
    }

    public void WinGame()
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
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(scene2.name);
        Time.timeScale = 1;
    }

    public void Map3()
    {
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(scene3.name);
        Time.timeScale = 1;
    }

    public void Updates()
    {
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(UpdateBase.name);
        Time.timeScale = 1;
    }

    public void Resume()
    {
        Amthanh.PlayOneShot(Click);
        pauGames.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    public void pausegame()
    {
        Amthanh.PlayOneShot(Click);
        if (pauGames != null) pauGames.SetActive(!pauGames.activeInHierarchy);
    }

    public void TogglePauseGame()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }
    // ================= HỆ THỐNG CLICK CHỌN TƯỚNG TRỰC TIẾP (MỚI) =================

    /// <summary>
    /// Hàm xử lý khi người chơi click trực tiếp vào Collider của tướng trên màn hình
    /// </summary>
    /// <param name="idTuong">ID của tướng được click</param>
    /// <param name="goTuong">GameObject của con tướng đó để làm hiệu ứng nếu muốn</param>
    public void HanhDongChonTuongTrucCiep(int idTuong, GameObject goTuong)
    {
        // 1. Phát âm thanh click chuột
        if (Amthanh != null && Click != null)
        {
            Amthanh.PlayOneShot(Click);
        }

        // 2. Tìm hệ thống save nếu chưa có
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        // 3. TỰ ĐỘNG LƯU VÀO FILE SAVE NGAY LẬP TỨC
        if (boQuanLySave != null)
        {
            boQuanLySave.LuuTuongDaChon(idTuong);
        }

        // --- GỢI Ý THÊM HIỆU ỨNG (Tùy chọn) ---
        // Bạn có thể viết thêm code làm con tướng đó nhún nhảy (Play Animation) 
        // hoặc sáng lên để người chơi biết là đã bấm trúng thành công ở đây.
        Animator animTuongMoiClick = goTuong.GetComponentInChildren<Animator>();
        if (animTuongMoiClick != null)
        {
            // Ví dụ phát một animation chào hỏi khi được chọn
            // animTuongMoiClick.SetTrigger("OnSelected"); 
        }
    }
}