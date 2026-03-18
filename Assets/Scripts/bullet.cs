using UnityEngine;

public class bullet : MonoBehaviour
{
    
    void Start()
    {
        
    }


    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("walls"))
        {
            Destroy(this.gameObject);
        }
    }
}
