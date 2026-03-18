using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    private Camera cam;
    public GameObject bulletPrefab;

    public float speed = 15f;
    public float bulletSpeed = 20f;
    private bool canShoot = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Direction();   

        if (Mouse.current.leftButton.isPressed)
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3 direction = (mousePos - transform.position).normalized;
            Shoot(direction);
        }
    }   

    void Move(){
        if ((Keyboard.current.wKey.isPressed))
        {
            transform.position += Vector3.up * Time.deltaTime * speed;
        }
         if (Keyboard.current.sKey.isPressed)
        {
            transform.position += Vector3.down * Time.deltaTime * speed;
        }
         if (Keyboard.current.aKey.isPressed)
        {
            transform.position += Vector3.left * Time.deltaTime * speed;
        }
         if (Keyboard.current.dKey.isPressed)
        {
            transform.position += Vector3.right * Time.deltaTime * speed;
        }
    }

    void Direction(){
        Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90);
    }
    
    void Shoot(Vector2 direction)
    {
        if (!canShoot) return;

        StartCoroutine(ResetShoot());
        canShoot = false;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * bulletSpeed;
    }

    IEnumerator ResetShoot()
    {
        yield return new WaitForSeconds(0.25f);
        canShoot = true;
    }
}