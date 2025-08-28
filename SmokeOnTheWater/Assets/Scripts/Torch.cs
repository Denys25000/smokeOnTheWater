using UnityEngine;

[RequireComponent(typeof(Light))]
public class Torch : MonoBehaviour
{
    public Camera playerCamera;       // камера гравця
    public float maxDistance = 20f;   // на якій дистанції світло мінімальне
    public float minDistance = 2f;    // на якій дистанції світло максимальне
    public float maxIntensity = 5f;
    public float minIntensity = 0.1f;
    public LayerMask GroundMask;

    private Light lightComponent;

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera not assigned!");
            enabled = false;
            return;
        }

        lightComponent = GetComponent<Light>();
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        float targetDistance = maxDistance;

        if (Physics.Raycast(ray, out hit, maxDistance, GroundMask))
        {
            targetDistance = hit.distance;
        }

        // нормалізуємо відстань і інтерполюємо Intensity
        float t = Mathf.InverseLerp(minDistance, maxDistance, targetDistance);
        lightComponent.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}
