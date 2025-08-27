using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float baseZoom, maxZoom, currentZoom;
    private Camera cam;

    private void Start() => cam = Camera.main;

    private void Update()
    {
        if (Input.GetButton("Fire2"))
            currentZoom -= Time.deltaTime * baseZoom;
        else 
            currentZoom += Time.deltaTime * baseZoom;
        currentZoom = Mathf.Clamp(currentZoom, maxZoom, baseZoom);
        cam.fieldOfView = currentZoom;
    }
}
