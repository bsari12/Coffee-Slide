using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [Header("Settings")]
    public float timeToDie = 3f; // Kaç saniye durursa yansın?
    
    private float timer = 0f;
    private bool isDanger = false; // Şu an tehlike var mı?
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Eğer tehlike bölgesinde biri varsa sayacı çalıştır
        if (isDanger)
        {
            timer += Time.deltaTime;
            
            // Görsel Uyarı: Çizgi koyu kırmızıya dönsün, yanıp sönsün vs.
            // Şimdilik Alpha'yı artırarak uyarı verelim
            float flicker = Mathf.PingPong(Time.time * 5, 1f); // Yanıp sönme efekti
            Color c = spriteRenderer.color;
            c.a = 0.5f + (flicker * 0.5f); 
            spriteRenderer.color = c;

            // Süre doldu mu?
            if (timer >= timeToDie)
            {
                Debug.Log("GAME OVER!");
                CoffeeServer.Instance.GameOver(); // Manager'daki bitirme fonksiyonunu çağır
                isDanger = false; // Sayacı durdur
            }
        }
        else
        {
            // Tehlike yoksa sayacı ve rengi sıfırla
            timer = 0f;
            Color c = spriteRenderer.color;
            c.a = 0.2f; // Eski şeffaf haline dön
            spriteRenderer.color = c;
        }
    }

    // Bir obje Trigger alanına girdiğinde veya içinde kaldığında çalışır
    private void OnTriggerStay2D(Collider2D other)
    {
        // Giren şey bir Kahve mi?
        if (other.CompareTag("Coffee") || other.GetComponent<Coffee>() != null)
        {
            // KRİTİK KONTROL: Elimizdeki kahve de bu alanda doğuyor!
            // Elimizdeki kahve "Kinematic", masadakiler "Dynamic".
            // Sadece masadaki (Dynamic) kahveler oyunu bitirebilir.
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                // Ayrıca kahvenin hızı çok düşükse (durmuşsa) saymaya başla
                // Hareket halindeyse (sekiyorsa) hemen öldürme
                if (rb.linearVelocity.magnitude < 0.5f)
                {
                    isDanger = true;
                }
            }
        }
    }

    // Obje alandan çıkınca
    private void OnTriggerExit2D(Collider2D other)
    {
        // Çıkan şey kahve ise tehlikeyi bitir
        if (other.GetComponent<Coffee>() != null)
        {
             // Burası biraz hassas, birden fazla kahve varsa biri çıkınca diğeri kalabilir.
             // Basit çözüm: Çıkan kahve Dynamic ise sayacı sıfırla.
             // (Daha gelişmişi için içerideki kahve sayısını int olarak tutmak gerekir ama şimdilik bu yeter)
             isDanger = false;
        }
    }
}