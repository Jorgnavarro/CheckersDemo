using UnityEngine;

public class DynamicScalerCamera : MonoBehaviour
{
    public float baseResolutionHeight = 1080f;  // Base resolution for calibrating size
    public float baseOrthographicSize = 5f;    

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCameraSize();
    }

    void AdjustCameraSize()
    {
        float currentScreenHeight = Screen.height;
        float scaleFactor = currentScreenHeight / baseResolutionHeight;
        cam.orthographicSize = baseOrthographicSize * scaleFactor;
    }
}
