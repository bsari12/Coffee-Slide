using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip throwSound;     // Fırlatma
    public AudioClip collisionSound; // Çarpışma
    public AudioClip mergeSound;     // Birleşme (Pop)

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        // Objenin üzerine kodla bir AudioSource ekliyoruz
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // 1. Fırlatma Sesi
    public void PlayThrow()
    {
        // PlayOneShot: Seslerin üst üste binmesine izin verir
        audioSource.PlayOneShot(throwSound, 0.3f); // 0.8f ses şiddeti (Volume)
    }

    // 2. Birleşme Sesi
    public void PlayMerge()
    {
        audioSource.PlayOneShot(mergeSound, 0.3f);
    }

    // 3. Çarpışma Sesi (Bunu biraz akıllı yapacağız)
    public void PlayCollision(float impactForce)
    {
        // Çok hafif dokunuşlarda ses çalma (kafa şişirmesin)
        if (impactForce < 2f) return;

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        
        float volume = Mathf.Min(impactForce / 20f, 1f) * 0.3f;
        audioSource.PlayOneShot(collisionSound, volume);
    }
}