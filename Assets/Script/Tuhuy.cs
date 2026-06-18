using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float lifeTime = 0.5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}