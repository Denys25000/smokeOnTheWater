using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{

    public float timeSpeed;
    public float time;
    public float day => time / 360;

    private void Update()
    {
        time += Time.deltaTime * timeSpeed;
        transform.rotation = Quaternion.Euler(time,-30 ,5);
    }
}
