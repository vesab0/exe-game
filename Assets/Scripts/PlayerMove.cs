using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    private Camera cam;
    public GameObject bulletPrefab;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    public float speed = 15f;
    public float bulletSpeed = 20f;
    public float dashDistance = 30f;
    public float dashDuration = 0.25f;

    private bool canShoot = true;
    private bool canDash = true;
    private bool canMove = true;

    void Start()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Direction();
        Shoot();
        Dash();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (!canMove) return;

        Vector2 dir = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) dir += Vector2.up;
        if (Keyboard.current.sKey.isPressed) dir += Vector2.down;
        if (Keyboard.current.aKey.isPressed) dir += Vector2.left;
        if (Keyboard.current.dKey.isPressed) dir += Vector2.right;

        rb.MovePosition(rb.position + dir.normalized * speed * Time.fixedDeltaTime);
    }

    void Direction()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90);
    }

    void Shoot()
    {
        if (!canShoot) return;
        if (Mouse.current.leftButton.isPressed)
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePos.z = 0f;
            Vector3 direction = (mousePos - transform.position).normalized;
            canShoot = false;
            StartCoroutine(ResetShoot());
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            bulletRb.linearVelocity = direction * bulletSpeed;
        }
    }

    void Dash()
    {
        if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;
        if (!canDash) return;

        Vector3 dashDirection = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) dashDirection += Vector3.up;
        if (Keyboard.current.sKey.isPressed) dashDirection += Vector3.down;
        if (Keyboard.current.aKey.isPressed) dashDirection += Vector3.left;
        if (Keyboard.current.dKey.isPressed) dashDirection += Vector3.right;

        if (dashDirection != Vector3.zero)
        {
            canDash = false;
            canMove = false;
            StartCoroutine(ResetDash());
            StartCoroutine(DashCoroutine(dashDirection.normalized));
        }
    }

    IEnumerator DashCoroutine(Vector3 dir)
    {
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.MovePosition(rb.position + (Vector2)dir * (dashDistance / dashDuration) * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        spriteRenderer.color = new Color(1, 1, 1, 1f);
        canMove = true;
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
}