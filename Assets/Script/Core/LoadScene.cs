using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] public float thoiGianChoAnMenu = 1.5f;   // Tự gõ số giây chờ ẩn Menu ngoài Inspector
    [SerializeField] public float thoiGianChoAnBangMap = 1.5f; // Tự gõ số gsây chờ ẩn Bảng chọn Map ngoài Inspector

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
    void Start()
    {
       if(pauGames != null) pauGames.SetActive(false);
       BanThuaGame.SetActive(false);
       Wingame.SetActive(false);

       if(mainMenuUI != null) mainMenuUI.SetActive(true);
       if (bangChonMapUI != null) bangChonMapUI.SetActive(false);
    }

    // --- 1. LUỒNG MỞ BẢNG CHỌN MAP (ẨN MENU -> HIỆN MAP) ---
    public void MoBangChonMap()
    {
        if (mainMenuUI != null && bangChonMapUI != null && mainMenuAnimator != null && bangMapAnimator != null)
        {
            StartCoroutine(LuongMoBangMap());
        }
    }

    private System.Collections.IEnumerator LuongMoBangMap()
    {
        mainMenuAnimator.Play(animAnMenu);

        // Chờ đúng số giây bạn đã cài đặt ở biến thoiGianChoAnMenu
        yield return new WaitForSeconds(thoiGianChoAnMenu);

        mainMenuUI.SetActive(false);
        bangChonMapUI.SetActive(true);
        bangMapAnimator.Play(animHienBangMap);
    }

    // --- 2. LUỒNG ĐÓNG BẢNG CHỌN MAP (ẨN MAP -> HIỆN MENU) ---
    public void DongBangChonMap()
    {
        if (mainMenuUI != null && bangChonMapUI != null && mainMenuAnimator != null && bangMapAnimator != null)
        {
            StartCoroutine(LuongDongBangMap());
        }
    }

    private System.Collections.IEnumerator LuongDongBangMap()
    {
        bangMapAnimator.Play(animAnBangMap);

        // Chờ đúng số giây bạn đã cài đặt ở biến thoiGianChoAnBangMap
        yield return new WaitForSeconds(thoiGianChoAnBangMap);

        bangChonMapUI.SetActive(false);
        mainMenuUI.SetActive(true);
        mainMenuAnimator.Play(animHienMenu);
    }

    // --- LOGIC GỐC CỦA BẠN ---
    public void VaoGame() { SceneManager.LoadScene(scene1.name); }
    public void TryAgain() { SceneManager.LoadScene(scene1.name); Time.timeScale = 1; }
    public void VeMenu() { SceneManager.LoadScene(Menu.name); Time.timeScale = 1; }
    private void FixedUpdate() { ThuaGame(); WinGame(); }
    private void ThuaGame() {
        if (Tayperer.skibidi != null && Tayperer.skibidi.GameOver) 
            BanThuaGame.SetActive(true); 
    }
    public void WinGame() { 
        if (ChaosDirector.instance != null && ChaosDirector.instance.WinGame) 
            Wingame.SetActive(true); 
    }
    public void Map2() { 
        SceneManager.LoadScene(scene2.name); 
        Time.timeScale = 1;
    }
    public void Map3()
    {
        SceneManager.LoadScene(scene3.name);
        Time.timeScale = 1;
    }

    public void Updates() { 
        SceneManager.LoadScene(UpdateBase.name);
        Time.timeScale = 1; 
    }
    public void Resume()
    {
        pauGames.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }
    
    public void pausegame()
    {
        if (!pauGames.activeInHierarchy)
        {
            pauGames.SetActive(true);
        }
        else pauGames.SetActive(false);
    }

    public void TogglePauseGame()
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0f : 1f;

    }

}