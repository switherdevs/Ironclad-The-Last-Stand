using UnityEngine;

public class chuot : MonoBehaviour
{
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; 
        transform.position = mousePos;
    }
}