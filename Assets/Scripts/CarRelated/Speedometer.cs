using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    public float maxSpeed = 0.0f; // The maximum speed of the target ** IN KM/H **
    public float minSpeedArrowAngle;
    public float maxSpeedArrowAngle;
    private float speed;
    [Header("UI")]
    public RectTransform arrow; // The arrow in the speedometer

    private void Update()
    {
        if (arrow != null)
            arrow.localEulerAngles =
                new Vector3(0, 0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, speed / maxSpeed));
    }

    public void SetSpeed(float s)
    {
        speed = s;
    }
}