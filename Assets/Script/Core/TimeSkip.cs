using UnityEngine;

public class TimeSkip : MonoBehaviour
{
    public bool skip = false;

    public void Timeskip()
    {
        skip = !skip;
        Time.timeScale = skip ? 6 : 1;
    }
}
