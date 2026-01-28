using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public GameObject gameOverPanel; 
    public TextMeshProUGUI gameOverHighScoreText;
    public GameObject pausePanel;
    
    [Header("Effects")]
    public ParticleSystem mergeParticle; 

    [Header("Score System")]
    public TextMeshProUGUI scoreText;     

    private int currentScore = 0;
    private int highScore = 0;

    private bool isGameActive = true;






    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        isGameActive = true; 
        Time.timeScale = 1f;

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        currentScore = 0;
        
        UpdateScoreUI();

        nextCoffeeIndex = GetRandomCoffeeIndex();

        SpawnCoffee();
    }

    void Update()
    {
        if (currentCoffee == null) return;
        if (!isGameActive) return;

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

    public void SpawnNextLevelCoffee(int currentLevelIndex, Vector3 spawnPos, Vector2 initialVelocity, float initialAngularVelocity)
    {
        if (currentLevelIndex >= allCoffeePrefabs.Length - 1) return;

        int nextLevelIndex = currentLevelIndex + 1;
        GameObject nextPrefab = allCoffeePrefabs[nextLevelIndex];

        Quaternion naturalRotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));

        GameObject newCoffee = Instantiate(nextPrefab, spawnPos, naturalRotation);

        Rigidbody2D newRb = newCoffee.GetComponent<Rigidbody2D>();
        
        newRb.linearVelocity = initialVelocity; 
        
        newRb.angularVelocity = initialAngularVelocity;
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
        isGameActive = false;

        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); 

            if (gameOverHighScoreText != null)
            {
                gameOverHighScoreText.text = "Highest Score:\n" + highScore.ToString();
            }
            
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1f; 
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void AddScore(int points)
    {
        currentScore += points;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
    }

    public void PauseGame()
    {
        isGameActive = false; 
        Time.timeScale = 0f;  
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    
    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f; 
        isGameActive = true; 
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void PlayMergeEffect(Vector3 position)
    {
        if (mergeParticle != null)
        {
            mergeParticle.transform.position = position;
            mergeParticle.Play();
        }
    }
}
