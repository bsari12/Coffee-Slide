using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 
using UnityEngine.SceneManagement;

public class CoffeeServer : MonoBehaviour
{
    public static CoffeeServer Instance;

    [Header("Coffee List")]
    public GameObject[] allCoffeePrefabs; 

    [Header("Audio")]
    public AudioSource bgmSource;

    [Header("Settings")]
    [SerializeField] private Transform spawnPoint;
    
    [Header("Movement & Physics")]
    [SerializeField] private float shootForce = 30f;
    [SerializeField] private float xLimit = 4f;

    private GameObject currentCoffee;
    private bool isDragging = false;
    private Rigidbody2D currentRb;

    private Vector2 initialVelocity;
    private bool isGameOver = false;

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
    public GameObject inGameUI;
    public GameObject mainScreenPanel;

    [Header("Effects")]
    public ParticleSystem mergeParticle; 

    [Header("Aiming System")]
    public SpriteRenderer aimSprite; 
    public float aimLineLength = 10f; 

    [Header("Score System")]
    public TextMeshProUGUI scoreText;     

    private int currentScore = 0;
    private int highScore = 0;

    private bool isGameActive = true;
    public Image musicButtonImage; 
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        Time.timeScale = 0f; 
        mainScreenPanel.SetActive(true);

        Application.targetFrameRate = 60;
        isGameActive = true; 
        Time.timeScale = 1f;
        if (aimSprite != null) aimSprite.enabled = false;

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

#if UNITY_EDITOR || UNITY_STANDALONE
        if (EventSystem.current.IsPointerOverGameObject()) return;
#else
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
#endif

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

        if (aimSprite != null)
        {
            aimSprite.enabled = true;
            aimSprite.transform.position = currentCoffee.transform.position;
            Vector2 newSize = aimSprite.size;
            newSize.y = aimLineLength; 
            aimSprite.size = newSize;
        }
    }

    private void ShootCoffee()
    {
        if (aimSprite != null) aimSprite.enabled = false;

        currentRb.bodyType = RigidbodyType2D.Dynamic; 
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

        GameObject newCoffee = Instantiate(nextPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D newRb = newCoffee.GetComponent<Rigidbody2D>();
        
        newRb.linearVelocity = initialVelocity * 0.8f; 
        newRb.angularVelocity = 0f; 
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
        if (isGameOver) return; 
    
        isGameOver = true;
        isGameActive = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverHighScoreText != null)
                gameOverHighScoreText.text = "Highest Score:\n" + PlayerPrefs.GetInt("HighScore", 0).ToString();
        }

        Time.timeScale = 0f;
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
        if (isGameOver) return; 

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f; 
        isGameActive = true;
    }

    public void QuitGame()
    {
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

    public void StartGame()
    {
        mainScreenPanel.SetActive(false);    
        Time.timeScale = 1f;            
        isGameActive = true;            

        if(currentCoffee == null) SpawnCoffee(); 
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToggleMusic()
    {
        if (bgmSource != null)
        {
            bgmSource.mute = !bgmSource.mute;
            musicButtonImage.sprite = bgmSource.mute ? musicOffSprite : musicOnSprite;
        }
    }

    public void RestartGameFast()
    {
        GameObject[] coffees = GameObject.FindGameObjectsWithTag("Coffee");
        foreach (GameObject c in coffees)
        {
            Destroy(c);
        }

        currentScore = 0;
        UpdateScoreUI();
        isGameOver = false;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (mainScreenPanel != null) mainScreenPanel.SetActive(false); 
        if (inGameUI != null) inGameUI.SetActive(true);               

        Time.timeScale = 1f;
        isGameActive = true;
        
        SpawnCoffee();
    }
}
