using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public struct ParallaxLayer
    {
        public Transform transform; // Ảnh nền
        public float speed;         // Tốc độ di chuyển theo camera
    }

    [Header("Danh sách các lớp Background")]
    public ParallaxLayer[] layers;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        // Tính toán khoảng cách camera đã di chuyển từ khung hình trước
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        for (int i = 0; i < layers.Length; i++)
        {
            // Di chuyển layer dựa trên speed của nó
            layers[i].transform.position += new Vector3(deltaMovement.x * layers[i].speed, deltaMovement.y * layers[i].speed, 0);
        }

        lastCameraPosition = cameraTransform.position;
    }
}