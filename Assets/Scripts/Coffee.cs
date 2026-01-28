using UnityEngine;

public class Coffee : MonoBehaviour
{
    [Header("Identity")]
    public int coffeeLevelIndex; 
    
    private bool hasMerged = false; 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        float impactVelocity = collision.relativeVelocity.magnitude;
        AudioManager.Instance.PlayCollision(impactVelocity);

        if (hasMerged) return;

        Coffee otherCoffee = collision.gameObject.GetComponent<Coffee>();

        
        if (otherCoffee != null)
        {
            if (otherCoffee.coffeeLevelIndex == this.coffeeLevelIndex)
            {
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

        Rigidbody2D myRb = this.GetComponent<Rigidbody2D>();
        Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();

        Vector2 averageVelocity = (myRb.linearVelocity + otherRb.linearVelocity) / 2f;
        
        AudioManager.Instance.PlayMerge();

        CoffeeServer.Instance.SpawnNextLevelCoffee(this.coffeeLevelIndex, middlePos, averageVelocity);

        Destroy(other.gameObject);
        Destroy(this.gameObject);
    }
}