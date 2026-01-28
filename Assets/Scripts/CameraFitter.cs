using UnityEngine;

public class CameraFitter : MonoBehaviour
{
    public float targetWidth = 6f; 

    void Start()
    {
        AdjustCamera();
    }

    void Update()
    {
#if UNITY_EDITOR
        AdjustCamera();
#endif
    }

    void AdjustCamera()
    {

        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetSize = (targetWidth / screenRatio) / 2f;
        Camera.main.orthographicSize = targetSize;
    }
}