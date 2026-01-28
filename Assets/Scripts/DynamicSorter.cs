using UnityEngine;

public class DynamicSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    private const int SORTING_PRECISION = 100; 
    
    public int sortingOffset = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        
        spriteRenderer.sortingOrder = (int)(transform.position.y * -SORTING_PRECISION) + sortingOffset;
    }
}