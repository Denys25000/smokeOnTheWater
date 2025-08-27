using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    public Transform planet;
    public float gravity = 9.8f;
    public bool fixedRotation;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        Vector3 direction = (planet.position - transform.position).normalized;

        rb.AddForce(direction * gravity, ForceMode.Acceleration);
        
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -direction) * transform.rotation;
        transform.rotation = fixedRotation ? Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime) : targetRotation;
    }
}
