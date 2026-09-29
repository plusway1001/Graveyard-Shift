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
        /*if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }*/
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
}
