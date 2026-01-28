using UnityEngine;

[ExecuteAlways] 
public class CameraFitter : MonoBehaviour
{
    public float targetWidth = 6f; 
    public float targetHeight = 11f; 

    void Update()
    {
        AdjustCamera();
    }

    void AdjustCamera()
    {
        if (Screen.height == 0) return;

        float screenRatio = (float)Screen.width / (float)Screen.height;

        float sizeBasedOnWidth = (targetWidth / screenRatio) / 2f;

        float sizeBasedOnHeight = targetHeight / 2f;

        float finalSize = Mathf.Max(sizeBasedOnWidth, sizeBasedOnHeight);

        Camera.main.orthographicSize = finalSize;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(targetWidth, targetHeight, 1));
    }
}