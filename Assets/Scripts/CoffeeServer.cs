using UnityEngine;
using UnityEngine.UI;

public class CoffeeServer : MonoBehaviour
{
    public static CoffeeServer Instance;

    [Header("Coffee List")]
    public GameObject[] allCoffeePrefabs; 
    
    [Header("Settings")]
    [SerializeField] private Transform spawnPoint;
    
    [Header("Movement & Physics")]
    [SerializeField] private float shootForce = 30f;
    [SerializeField] private float xLimit = 4f;

    private GameObject currentCoffee;
    private bool isDragging = false;
    private Rigidbody2D currentRb;

    private Vector2 initialVelocity;

    [Header("Next Coffee Logic")]
    public float[] spawnWeights; 

    private int nextCoffeeIndex;
    private int currentSpawnIndex; 

    [Header("UI Elements")]
    public Image nextCoffeeDisplay; 
    public Sprite[] coffeeSprites;  

    [Header("UI Elements")]
    public GameObject gameOverPanel; 
    private void Awake()
    {
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
        else if (Input.GetMouseButtonUp(0) && isDragging) 
        {
            isDragging = false;
            ShootCoffee();
        }
    }

    private void SpawnCoffee()
    {
        currentSpawnIndex = nextCoffeeIndex;

        currentCoffee = Instantiate(allCoffeePrefabs[currentSpawnIndex], spawnPoint.position, Quaternion.identity);

        currentRb = currentCoffee.GetComponent<Rigidbody2D>();
        currentRb.bodyType = RigidbodyType2D.Kinematic; 
        currentRb.linearVelocity = Vector2.zero;

        nextCoffeeIndex = GetRandomCoffeeIndex();

        Debug.Log("Next Coffee: C" + (nextCoffeeIndex + 1));

        nextCoffeeIndex = GetRandomCoffeeIndex();

        if (coffeeSprites.Length > nextCoffeeIndex)
        {
            nextCoffeeDisplay.sprite = coffeeSprites[nextCoffeeIndex];
            nextCoffeeDisplay.SetNativeSize(); 
        }
    }

    private void DragCoffee()
    {
        Vector3 mousePosPixels = Input.mousePosition;

        mousePosPixels.z = Camera.main.nearClipPlane; 

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePosPixels);

        float clampedX = Mathf.Clamp(worldPos.x, -xLimit, xLimit);

        Vector3 newPosition = new Vector3(clampedX, spawnPoint.position.y, 0);
        
        currentCoffee.transform.position = newPosition;
    }

    private void ShootCoffee()
    {
        currentRb.bodyType = RigidbodyType2D.Dynamic; 
        currentRb.AddForce(Vector2.up * shootForce, ForceMode2D.Impulse);

        currentRb.AddForce(Vector2.up * shootForce, ForceMode2D.Impulse);

        AudioManager.Instance.PlayThrow();

        currentCoffee = null; 
        currentRb = null;

        Invoke("SpawnCoffee", 0.7f);
    }

    public void SpawnNextLevelCoffee(int currentLevelIndex, Vector3 spawnPos, Vector2 initialVelocity)
    {
        if (currentLevelIndex >= allCoffeePrefabs.Length - 1) return;

        int nextLevelIndex = currentLevelIndex + 1;
        GameObject nextPrefab = allCoffeePrefabs[nextLevelIndex];

        GameObject newCoffee = Instantiate(nextPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D newRb = newCoffee.GetComponent<Rigidbody2D>();
        
        newRb.linearVelocity = initialVelocity;
    }

    private int GetRandomCoffeeIndex()
    {
        float totalWeight = 0f;
        foreach (float weight in spawnWeights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.Range(0, totalWeight);

        float cumulativeWeight = 0f;
        for (int i = 0; i < spawnWeights.Length; i++)
        {
            cumulativeWeight += spawnWeights[i];
            
            if (randomValue <= cumulativeWeight)
            {
                return i;
            }
        }

        return 0;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        if(gameOverPanel != null) 
        gameOverPanel.SetActive(true);
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
