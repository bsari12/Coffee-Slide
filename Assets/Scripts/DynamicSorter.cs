using UnityEngine;

public class DynamicSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb; 
    private const int SORTING_PRECISION = 100; 
    public int sortingOffset = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); 
    }

    void LateUpdate()
    {
        if (rb != null && rb.IsSleeping()) return;

        if (transform.hasChanged)
        {
            spriteRenderer.sortingOrder = (int)(transform.position.y * -SORTING_PRECISION) + sortingOffset;
            transform.hasChanged = false; 
        }
    }
}