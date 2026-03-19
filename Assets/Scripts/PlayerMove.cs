using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    private Camera cam;
    public GameObject bulletPrefab;
    private SpriteRenderer spriteRenderer;

    public float speed = 15f;
    public float bulletSpeed = 20f;
    public float dashDistance = 30f; 
    public float dashDuration = 0.25f; 


    private bool canShoot = true;
    private bool canDash = true;
    private bool canMove = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Direction();   

        Shoot();
        Dash();
    }   

    void Move(){

        if (!canMove) return;

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
    
    void Shoot()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3 direction = (mousePos - transform.position).normalized;

            if (!canShoot) return;
            StartCoroutine(ResetShoot());
            canShoot = false;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = direction * bulletSpeed;
        }
        
    }

    void Dash(){

        if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;

        Vector3 dashDirection = Vector3.zero;

        if (!canDash) return;
        StartCoroutine(ResetDash());
        canDash = false;

        if (Keyboard.current.wKey.isPressed) dashDirection += Vector3.up;
        if (Keyboard.current.sKey.isPressed) dashDirection += Vector3.down;
        if (Keyboard.current.aKey.isPressed) dashDirection += Vector3.left;
        if (Keyboard.current.dKey.isPressed) dashDirection += Vector3.right;

        if (dashDirection != Vector3.zero){
            canMove = false;
            StartCoroutine(DashCoroutine(dashDirection.normalized));
        }
    }

    IEnumerator ResetShoot()
    {
        yield return new WaitForSeconds(0.25f);
        canShoot = true;
    }
    IEnumerator ResetDash()
    {
        yield return new WaitForSeconds(0.4f);
        canDash = true;
    }

    IEnumerator DashCoroutine(Vector3 dir){
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        float elapsed = 0f;
        while (elapsed < dashDuration){
            transform.position += dir * (dashDistance / dashDuration) * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = new Color(1, 1, 1, 1f);
        canMove = true;

    }
}