
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Recoil")]
    public float recoilDistance = 0.15f;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float facingAngle;

    [Header("Mobile Controls")]
    public bool enableMobileControls = true;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Camera cam;

    // Tracks each arrow button independently.
    private bool moveUp;
    private bool moveDown;
    private bool moveLeft;
    private bool moveRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    private void Update()
    {
        // Keyboard controls for PC.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Combine keyboard and mobile button inputs.
        if (enableMobileControls)
        {
            if (moveLeft) horizontal -= 1f;
            if (moveRight) horizontal += 1f;
            if (moveDown) vertical -= 1f;
            if (moveUp) vertical += 1f;
        }

        movement = new Vector2(horizontal, vertical).normalized;

        // Mouse aiming for desktop.
        // Avoid rotating towards the finger pressing mobile buttons.
        if (!Application.isMobilePlatform)
        {
            RotateToMouse();
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(
                rb.position +
                movement * moveSpeed * Time.fixedDeltaTime
            );
        }
    }

    public void SetDirection(string direction, bool pressed)
    {
        switch (direction)
        {
            case "Up":
                moveUp = pressed;
                break;

            case "Down":
                moveDown = pressed;
                break;

            case "Left":
                moveLeft = pressed;
                break;

            case "Right":
                moveRight = pressed;
                break;
        }
    }

    private void RotateToMouse()
    {
        if (cam == null)
            return;

        Vector3 mousePos = cam.ScreenToWorldPoint(
            Input.mousePosition
        );

        Vector2 direction = mousePos - transform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        rb.rotation = angle - facingAngle;
    }

    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        // Recoil pushback.
        Vector2 recoilDirection = -firePoint.right;

        rb.MovePosition(
            rb.position +
            recoilDirection * recoilDistance
        );
    }
}


/*using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Recoil")]
    public float recoilDistance = 0.15f;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float facingAngle;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Camera cam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    private void Update()
    {
        // Movement Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;

        // Rotate player to mouse
        RotateToMouse();

        // Shoot bullet
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void RotateToMouse()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = mousePos - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //rb.rotation = angle;
        rb.rotation = angle - facingAngle;
    }

    void Shoot()
    {
        Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        // Recoil pushback
        Vector2 recoilDirection = -firePoint.right;

        rb.MovePosition(
            rb.position + recoilDirection * recoilDistance
        );
    }
}*/
