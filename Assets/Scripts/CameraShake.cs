using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPos;
    private float shakeTimer = 0f;
    private float shakeAmount = 0.1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            Vector3 randomPoint = originalPos + Random.insideUnitSphere * shakeAmount;
            randomPoint.z = originalPos.z; 
            transform.localPosition = randomPoint;

            shakeTimer -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }

    public void Shake(float duration, float amount)
    {
        shakeTimer = duration;
        shakeAmount = amount;
    }
}