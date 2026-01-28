using UnityEngine;
using UnityEngine.UI;

public class CoffeeServer : MonoBehaviour
{
    public static CoffeeServer Instance;

    [Header("Coffee List")]
    public GameObject[] allCoffeePrefabs; // C1, C2, C3... hepsini buraya koyacağız
    
    [Header("Settings")]
    [SerializeField] private Transform spawnPoint;
    
    [Header("Movement & Physics")]
    [SerializeField] private float shootForce = 30f;
    [SerializeField] private float xLimit = 4f;

    // Internal variables
    private GameObject currentCoffee;
    private bool isDragging = false;
    private Rigidbody2D currentRb;

    private Vector2 initialVelocity;

    [Header("Next Coffee Logic")]
    public float[] spawnWeights; 

    // UI'da göstermek için bir sonraki kahvenin ne olacağını tutmamız lazım
    private int nextCoffeeIndex;
    private int currentSpawnIndex; // Şu an elimizdeki kahve

    [Header("UI Elements")]
    public Image nextCoffeeDisplay; // UI'daki resmi buraya sürükleyeceğiz
    public Sprite[] coffeeSprites;  // Kahvelerin resimlerini (Sprite) buraya koyacağız ki UI değişsin

    [Header("UI Elements")]
    public GameObject gameOverPanel; // <-- YENİ

    private void Awake()
    {
        // Singleton Kurulumu:
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        nextCoffeeIndex = GetRandomCoffeeIndex();

        SpawnCoffee();
    }

    void Update()
    {
        if (currentCoffee == null) return;

        if (Input.GetMouseButton(0)) 
        {
            isDragging = true;
            DragCoffee();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging) // isDragging kontrolü önemli
        {
            isDragging = false;
            ShootCoffee();
        }
    }

    private void SpawnCoffee()
    {
        // 1. Sıradakini elimize alıyoruz
        currentSpawnIndex = nextCoffeeIndex;

        // 2. Elimizdeki kahveyi yaratıyoruz
        currentCoffee = Instantiate(allCoffeePrefabs[currentSpawnIndex], spawnPoint.position, Quaternion.identity);

        currentRb = currentCoffee.GetComponent<Rigidbody2D>();
        currentRb.bodyType = RigidbodyType2D.Kinematic; 
        currentRb.linearVelocity = Vector2.zero;

        // 3. BİR SONRAKİ atış için şimdiden zar atıp hazırlıyoruz
        nextCoffeeIndex = GetRandomCoffeeIndex();

        // (İlerde buraya UI güncelleme kodu gelecek: "Ekrana nextCoffeeIndex resmini bas" diyeceğiz)
        Debug.Log("Next Coffee: C" + (nextCoffeeIndex + 1));

        nextCoffeeIndex = GetRandomCoffeeIndex();

        // UI RESMİNİ GÜNCELLE
        // Eğer sprite listemiz doluysa değiştir
        if (coffeeSprites.Length > nextCoffeeIndex)
        {
            nextCoffeeDisplay.sprite = coffeeSprites[nextCoffeeIndex];
            // Resmin oranını düzelt (büzüşmesin)
            nextCoffeeDisplay.SetNativeSize(); 
            // Eğer çok büyükse scale ile küçültmen gerekebilir, şimdilik böyle kalsın.
        }
    }

    private void DragCoffee()
    {
        // 1. Get mouse position in pixels
        Vector3 mousePosPixels = Input.mousePosition;

        // 2. Set the distance from camera (Required for correct conversion)
        mousePosPixels.z = Camera.main.nearClipPlane; 

        // 3. Convert Pixels to World Units
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePosPixels);

        // 4. Limit the X position so it stays inside walls (Clamping)
        float clampedX = Mathf.Clamp(worldPos.x, -xLimit, xLimit);

        // 5. Apply the new position
        // We keep Y fixed at spawnPoint.y, only move X
        Vector3 newPosition = new Vector3(clampedX, spawnPoint.position.y, 0);
        
        // Move the coffee instantly to finger position
        currentCoffee.transform.position = newPosition;
    }

    private void ShootCoffee()
    {
        // 1. Re-enable Physics (Fiziği tekrar aç)
        // Unity 6: Dynamic is the default physics simulation mode
        currentRb.bodyType = RigidbodyType2D.Dynamic; 

        // 2. Add Force Upwards (Yukarı kuvvet uygula)
        // Vector2.up = (0, 1) yani yukarı yön.
        // ForceMode2D.Impulse = Anlık patlama/vuruş kuvveti (Roket gibi değil, bilardo topuna vurmak gibi)
        currentRb.AddForce(Vector2.up * shootForce, ForceMode2D.Impulse);

        currentRb.AddForce(Vector2.up * shootForce, ForceMode2D.Impulse);

        // --- YENİ: SES ÇAL ---
        AudioManager.Instance.PlayThrow();

        // 3. Clear the reference (Artık bu kahveyle bağımızı kopar)
        currentCoffee = null; 
        currentRb = null;

        // 4. Spawn a new coffee after a delay (Yeni kahveyi biraz bekleyip ver)
        Invoke("SpawnCoffee", 0.7f); // 0.7 saniye sonra SpawnCoffee'yi çalıştır
    }

    public void SpawnNextLevelCoffee(int currentLevelIndex, Vector3 spawnPos, Vector2 initialVelocity)
    {
        if (currentLevelIndex >= allCoffeePrefabs.Length - 1) return;

        int nextLevelIndex = currentLevelIndex + 1;
        GameObject nextPrefab = allCoffeePrefabs[nextLevelIndex];

        // Yeni kahveyi yarat
        GameObject newCoffee = Instantiate(nextPrefab, spawnPos, Quaternion.identity);

        // --- YENİ KISIM: HIZI AKTAR ---
        Rigidbody2D newRb = newCoffee.GetComponent<Rigidbody2D>();
        
        // Gelen hızı yeni objeye ver. 
        // Biraz yavaşlatmak istersen sonuna '* 0.8f' gibi bir çarpan koyabilirsin.
        newRb.linearVelocity = initialVelocity;
    }

    private int GetRandomCoffeeIndex()
    {
        // 1. Toplam ağırlığı hesapla (Örn: 60+30+10 = 100)
        float totalWeight = 0f;
        foreach (float weight in spawnWeights)
        {
            totalWeight += weight;
        }

        // 2. 0 ile Toplam Ağırlık arasında rastgele bir sayı tut
        float randomValue = Random.Range(0, totalWeight);

        // 3. Hangi dilime denk geldiğini bul
        float cumulativeWeight = 0f;
        for (int i = 0; i < spawnWeights.Length; i++)
        {
            cumulativeWeight += spawnWeights[i];
            
            // Eğer rastgele sayı bu dilimin içindeyse, bu indexi seç
            if (randomValue <= cumulativeWeight)
            {
                return i;
            }
        }

        // Matematiksel bir hata olursa (olmaz ama) en küçüğü (C1) döndür
        return 0;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        if(gameOverPanel != null) 
        gameOverPanel.SetActive(true); // Paneli aç
    }
    
    // Oyunu yeniden başlatmak için
    public void RestartGame()
    {
        Time.timeScale = 1f; // Zamanı tekrar akıt
        // Sahneyi baştan yükle
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
