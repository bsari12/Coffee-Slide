using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [Header("Settings")]
    public float timeToDie = 3f; 
    
    private float timer = 0f;
    private bool isDanger = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        
        if (isDanger)
        {
            timer += Time.deltaTime;
            
            
            float flicker = Mathf.PingPong(Time.time * 5, 1f); 
            Color c = spriteRenderer.color;
            c.a = 0.5f + (flicker * 0.5f); 
            spriteRenderer.color = c;

            
            if (timer >= timeToDie)
            {
                CoffeeServer.Instance.GameOver(); 
                isDanger = false; 
            }
        }
        else
        {
            timer = 0f;
            Color c = spriteRenderer.color;
            c.a = 0.2f; 
            spriteRenderer.color = c;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Coffee") || other.GetComponent<Coffee>() != null)
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                if (rb.linearVelocity.magnitude < 0.5f)
                {
                    isDanger = true;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Coffee>() != null)
        {
             isDanger = false;
        }
    }
}