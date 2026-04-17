using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.up;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Arrow keys only for movement
        moveInput = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow)) moveInput.y += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveInput.y -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput.x += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveInput.x -= 1f;
        moveInput.Normalize();

        // Face movement direction
        if (moveInput.sqrMagnitude > 0.01f)
        {
            facingDirection = moveInput;
            float targetAngle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);
        }
    }

    void FixedUpdate()
    {
        rb.velocity = moveInput * moveSpeed;
    }
}
