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
            if (collision.gameObject.CompareTag("enemy"))
        {
        Enemyscript enemy = collision.gameObject.GetComponent<Enemyscript>();
        if (enemy != null)
        {
            enemy.setHealth(1.3f);
        }
        Destroy(this.gameObject);
        }
    }
}
