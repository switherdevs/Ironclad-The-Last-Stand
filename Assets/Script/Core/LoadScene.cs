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

    [SerializeField] public GameObject bangChonMapUI;
    [SerializeField] public Animator bangMapAnimator;

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

    [Header("--- HỆ THỐNG VIỀN HIỂN THỊ ĐỘ KHÓ (MỚI) ---")]
    [SerializeField] private GameObject vienDoKhoDe;
    [SerializeField] private GameObject vienDoKhoBinhThuong;
    [SerializeField] private GameObject vienDoKhoKho;

    private AudioSource Amthanh;
    [SerializeField] private AudioClip Click;

    void Start()
    {
        Amthanh = GetComponent<AudioSource>();
        if (pauGames != null) pauGames.SetActive(false);
        BanThuaGame.SetActive(false);
        Wingame.SetActive(false);

        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (bangChonMapUI != null) bangChonMapUI.SetActive(false);

        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        CapNhatHienThiVienTheoSave();
    }

    private void CapNhatHienThiVienTheoSave()
    {
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();

        int doKhoHienTai = 1;
        if (boQuanLySave != null)
        {
            doKhoHienTai = boQuanLySave.DocDoKhoGame();
        }

        KichHoatVienDuyNhat(doKhoHienTai);
    }

    private void KichHoatVienDuyNhat(int indexDoKho)
    {
        if (vienDoKhoDe != null) vienDoKhoDe.SetActive(false);
        if (vienDoKhoBinhThuong != null) vienDoKhoBinhThuong.SetActive(false);
        if (vienDoKhoKho != null) vienDoKhoKho.SetActive(false);

        if (indexDoKho == 0 && vienDoKhoDe != null) vienDoKhoDe.SetActive(true);
        else if (indexDoKho == 1 && vienDoKhoBinhThuong != null) vienDoKhoBinhThuong.SetActive(true);
        else if (indexDoKho == 2 && vienDoKhoKho != null) vienDoKhoKho.SetActive(true);
    }

    public void ChonDoKhoDe()
    {
        Amthanh.PlayOneShot(Click);
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();
        if (boQuanLySave != null) { boQuanLySave.LuuDoKhoGame(0); }
        KichHoatVienDuyNhat(0);
    }

    public void ChonDoKhoBinhThuong()
    {
        Amthanh.PlayOneShot(Click);
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();
        if (boQuanLySave != null) { boQuanLySave.LuuDoKhoGame(1); }
        KichHoatVienDuyNhat(1);
    }

    public void ChonDoKhoKho()
    {
        Amthanh.PlayOneShot(Click);
        if (boQuanLySave == null) boQuanLySave = FindFirstObjectByType<SaveSystem>();
        if (boQuanLySave != null) { boQuanLySave.LuuDoKhoGame(2); }
        KichHoatVienDuyNhat(2);
    }

    public void MoBangChonMap()
    {
        Amthanh.PlayOneShot(Click);
        if (mainMenuUI != null && bangChonMapUI != null && mainMenuAnimator != null && bangMapAnimator != null)
        {
            StartCoroutine(LuongMoBangMap());
        }
    }

    private System.Collections.IEnumerator LuongMoBangMap()
    {
        Amthanh.PlayOneShot(Click);
        mainMenuAnimator.Play(animAnMenu);
        yield return new WaitForSeconds(thoiGianChoAnMenu);
        mainMenuUI.SetActive(false);
        bangChonMapUI.SetActive(true);
        bangMapAnimator.Play(animHienBangMap);
    }

    public void DongBangChonMap()
    {
        Amthanh.PlayOneShot(Click);
        if (mainMenuUI != null && bangChonMapUI != null && mainMenuAnimator != null && bangMapAnimator != null)
        {
            StartCoroutine(LuongDongBangMap());
        }
    }

    private System.Collections.IEnumerator LuongDongBangMap()
    {
        bangMapAnimator.Play(animAnBangMap);
        yield return new WaitForSeconds(thoiGianChoAnBangMap);
        bangChonMapUI.SetActive(false);
        mainMenuUI.SetActive(true);
        mainMenuAnimator.Play(animHienMenu);
    }

    public void VaoGame()
    {
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(scene1.name);
    }

    public void TryAgain()
    {
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(scene1.name); Time.timeScale = 1;
    }

    public void VeMenu()
    {
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(Menu.name); Time.timeScale = 1;
    }

    private void FixedUpdate()
    {
        ThuaGame(); WinGame();
    }

    private void ThuaGame()
    {
        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver)
            BanThuaGame.SetActive(true);
    }

    public void WinGame()
    {
        if (ChaosDirector.instance != null && ChaosDirector.instance.WinGame)
            Wingame.SetActive(true);
    }

    public void Map2()
    {
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(scene2.name); Time.timeScale = 1;
    }

    public void Map3()
    {
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(scene3.name); Time.timeScale = 1;
    }

    public void Updates()
    {
        Amthanh.PlayOneShot(Click);
        SceneManager.LoadScene(UpdateBase.name); Time.timeScale = 1;
    }

    public void Resume()
    {
        Amthanh.PlayOneShot(Click);
        pauGames.SetActive(false); Time.timeScale = 1; isPaused = false;
    }

    public void pausegame()
    {
        Amthanh.PlayOneShot(Click);
        if (!pauGames.activeInHierarchy) pauGames.SetActive(true);
        else pauGames.SetActive(false);
    }

    public void TogglePauseGame()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }
}