using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 100;
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private LayerMask groundLayers;

    private Camera cam;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(x, z).normalized;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

    }
}
