using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadScene : MonoBehaviour
{
    [SerializeField]
    public GameObject BanThuaGame;
    [SerializeField]
    public GameObject Wingame;
    [SerializeField]
    public Object scene1;
    [SerializeField]
    public Object scene2;
    [SerializeField]
    public Object Menu;
    [SerializeField]
    public Object UpdateBase;
    void Start()
    {
        BanThuaGame.SetActive(false);
        Wingame.SetActive(false);
    }

    public void VaoGame()
    {
        SceneManager.LoadScene(scene1.name);
    }
    public void TryAgain()
    {
        SceneManager.LoadScene(scene1.name);
        Time.timeScale = 1;
    }
    public void VeMenu()
    {
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
        if(Tayperer.skibidi != null && Tayperer.skibidi.GameOver)
        {
            BanThuaGame.SetActive(true);
        }
    }
    public void WinGame()
    {
        if (ChaosDirector.instance != null && ChaosDirector.instance.WinGame)
        {
            Wingame.SetActive(true);
        }
    }
    public void Nextmap()
    {
        SceneManager.LoadScene(scene2.name);
        Time.timeScale = 1;
    }
    public void Updates() 
    {
        SceneManager.LoadScene(UpdateBase.name);
        Time.timeScale = 1;
    }
}
