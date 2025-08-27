using UnityEngine;

public class DistanceTrigger : MonoBehaviour
{
    [SerializeField] private float distance = 200;
    private float timer;

    private void Update()
    {
        if (timer + 5 < Time.time)
        {
            timer = Time.time;
            foreach(Transform child in transform) child.gameObject.SetActive(Vector3.Distance(PlayerMovement.instance.transform.position, child.position) < distance);
        }
    }
}
