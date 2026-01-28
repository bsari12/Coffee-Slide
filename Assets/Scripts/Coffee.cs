using UnityEngine;

public class Coffee : MonoBehaviour
{
    [Header("Identity")]
    public int coffeeLevelIndex; // 0=C1, 1=C2, 2=C3...
    
    private bool hasMerged = false; // Aynı anda iki kez merge olmasın diye kilit

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float impactVelocity = collision.relativeVelocity.magnitude;
        AudioManager.Instance.PlayCollision(impactVelocity);
        
        // Eğer zaten birleşme sürecindeysem işlem yapma
        if (hasMerged) return;

        // Çarptığım şeyin üzerinde "Coffee" scripti var mı?
        Coffee otherCoffee = collision.gameObject.GetComponent<Coffee>();

        
        if (otherCoffee != null)
        {
            // Seviyelerimiz aynı mı? (C1 vs C1 mi?)
            if (otherCoffee.coffeeLevelIndex == this.coffeeLevelIndex)
            {
                // BURASI ÖNEMLİ: İki kahve çarpışınca kod iki kez çalışır (biri ben, biri o).
                // Sadece biri işlemi yapsın diye InstanceID kontrolü yapıyoruz.
                // Kimliği büyük olan işlemi üstlensin.
                if (this.gameObject.GetInstanceID() > otherCoffee.gameObject.GetInstanceID())
                {
                    MergeWith(otherCoffee);
                }
            }
        }
    }

    private void MergeWith(Coffee other)
    {
        this.hasMerged = true;
        other.hasMerged = true;

        Vector3 middlePos = (this.transform.position + other.transform.position) / 2f;

        // --- YENİ KISIM: HIZ HESAPLAMA ---
        // Benim hızım ve çarptığımın hızını al
        Rigidbody2D myRb = this.GetComponent<Rigidbody2D>();
        Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();

        // İki vektörü toplayıp 2'ye bölerek ortalama hızı buluyoruz.
        // Böylece çarpışma yönüne doğru akmaya devam ederler.
        Vector2 averageVelocity = (myRb.linearVelocity + otherRb.linearVelocity) / 2f;
        
        AudioManager.Instance.PlayMerge();

        // Hesapladığımız hızı Manager'a gönderiyoruz
        CoffeeServer.Instance.SpawnNextLevelCoffee(this.coffeeLevelIndex, middlePos, averageVelocity);

        Destroy(other.gameObject);
        Destroy(this.gameObject);
    }
}