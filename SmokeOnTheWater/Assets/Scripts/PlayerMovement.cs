using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;

    [Header("Movement")]
    public float Speed = 5f;
    public float JumpForce = 5f;
    public float gravityStrength;

    [Header("Camera")]
    public float Sensitivity = 100f;
    public Transform СameraHolder; // Обгортка для камери (прикріпи до гравця)

    [Header("Planet")]
    public Transform Planet; // Посилання на центр планети
    public LayerMask GroundMask;

    private Rigidbody rb;
    private float camPitch = 0f;

    private void Awake() => instance = this;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // ми самі робимо гравітацію

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleJump();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;

        // Камера обертається по X (вгору-вниз)
        camPitch -= mouseY;
        camPitch = Mathf.Clamp(camPitch, -80f, 80f);
        СameraHolder.localRotation = Quaternion.Euler(camPitch, 0f, 0f);

        // Гравець обертається по Y (вліво-вправо)
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        // Локальні осі камери, але без "вгору-вниз"
        Vector3 camForward = Vector3.ProjectOnPlane(СameraHolder.forward, transform.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(СameraHolder.right, transform.up).normalized;

        Vector3 moveDir = (camForward * input.z + camRight * input.x).normalized;
        Vector3 velocity = moveDir * Speed;

        rb.MovePosition(rb.position + velocity * Time.deltaTime);

        // Обертання моделі гравця у напрямку руху
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, transform.up);
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && CheckGround())
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);
        }
    }

    private void ApplyGravity()
    {
        Vector3 gravityDir = (Planet.position - transform.position).normalized;
        Vector3 gravity = gravityDir * gravityStrength; // сила притягання

        rb.AddForce(gravity, ForceMode.Acceleration);

        transform.rotation = Quaternion.FromToRotation(transform.up, -gravityDir) * transform.rotation;
    }

    private bool CheckGround()
    {
        return Physics.Raycast(transform.position, -transform.up, 1.1f, GroundMask);
    }
}
