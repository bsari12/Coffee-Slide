using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip throwSound;     
    public AudioClip collisionSound; 
    public AudioClip mergeSound;     

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayThrow()
    {

        audioSource.PlayOneShot(throwSound, 0.3f); 
    }

    public void PlayMerge()
    {
        audioSource.PlayOneShot(mergeSound, 0.3f);
    }

    public void PlayCollision(float impactForce)
    {
        if (impactForce < 2f) return;

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        
        float volume = Mathf.Min(impactForce / 20f, 1f) * 0.3f;
        audioSource.PlayOneShot(collisionSound, volume);
    }
}