using UnityEngine;

public class DistanceTrigger : MonoBehaviour
{
    [SerializeField] private float loadDistance = 200f;   // радіус прогрузки чанків
    [SerializeField] private float checkThreshold = 20f; // як далеко треба пройти, щоб перевірити знову

    private Vector3 lastPlayerPos;

    private void Start()
    {
        if (PlayerMovement.instance != null)
            lastPlayerPos = PlayerMovement.instance.transform.position;
    }

    private void Update()
    {
        if (PlayerMovement.instance == null) return;

        Vector3 playerPos = PlayerMovement.instance.transform.position;

        // перевіряємо лише якщо гравець пройшов достатньо
        if ((playerPos - lastPlayerPos).sqrMagnitude >= checkThreshold * checkThreshold)
        {
            lastPlayerPos = playerPos;

            foreach (Transform child in transform)
            {
                bool shouldBeActive = (playerPos - child.position).sqrMagnitude < loadDistance * loadDistance;
                if (child.gameObject.activeSelf != shouldBeActive)
                    child.gameObject.SetActive(shouldBeActive);
            }
        }
    }
}
