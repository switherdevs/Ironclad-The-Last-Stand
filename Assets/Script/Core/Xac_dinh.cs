using UnityEngine;

public class KhoaMucTieu : MonoBehaviour
{
    [HideInInspector] public bool daBiKhoaMucTieu = false;

    // Khi con quái bị chết hoặc bị ẩn (SetActive(false)), tự động giải phóng khóa
    private void OnDisable()
    {
        daBiKhoaMucTieu = false;
    }
}