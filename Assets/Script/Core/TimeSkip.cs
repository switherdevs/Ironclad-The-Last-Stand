using UnityEngine;

public class TimeSkip : MonoBehaviour
{
    public bool skip = false;
    private AudioSource Amthanh;
    [SerializeField]
    private AudioClip clip;
    private void Start()
    {
        Amthanh = GetComponent<AudioSource>();
    }
    public void Timeskip()
    {
        Amthanh.PlayOneShot(clip);
        skip = !skip;
        Time.timeScale = skip ? 6 : 1;
    }
}
