using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [Header("Settings")]
    public float limitTime = 3f;
    private float timer = 0f;
    private int coffeeCount = 0; 

    private SpriteRenderer sr;
    private Color originalColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    void Update()
    {
        if (coffeeCount > 0)
        {
            timer += Time.deltaTime;
            if (sr != null)
            {
                float lerp = Mathf.PingPong(Time.time * 6f, 1f);
                sr.color = Color.Lerp(originalColor, Color.red, lerp);
            }

            if (timer >= limitTime)
            {
                CoffeeServer.Instance.GameOver();
            }
        }
        else
        {
            timer = 0f;
            if (sr != null && sr.color != originalColor) sr.color = originalColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coffee") && other.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic)
        {
            coffeeCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Coffee"))
        {
            coffeeCount--;
            if (coffeeCount < 0) coffeeCount = 0;
        }
    }
}