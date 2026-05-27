using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadScene : MonoBehaviour
{
    public Object scene1;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void VaoGame()
    {
        SceneManager.LoadScene(scene1.name);
    }
}
