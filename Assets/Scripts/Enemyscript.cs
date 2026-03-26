using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Enemyscript : MonoBehaviour
{

    public float Health;
    public float MaxHealth;

    [SerializeField]
    private HealthBar healthBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar.setMaxHealth(MaxHealth);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
    public void setHealth(float healthChange){ 
       
    Health -= healthChange;
    Health = Mathf.Clamp(Health, 0, MaxHealth); // don't go below 0 or above max (this is the most ai shit ever btw po mu desht)
    healthBar.setHealth(Health);

    if (Health <= 0){
        Destroy(this.gameObject);
    }

    }

}
