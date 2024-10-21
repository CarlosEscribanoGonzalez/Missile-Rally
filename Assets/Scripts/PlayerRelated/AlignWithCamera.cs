using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignWithCamera : MonoBehaviour
{
    private GameObject cam;

    private void Awake()
    {
        cam = GameObject.Find("CustomCamera");
    }

    void Update()
    {
        this.transform.forward = Vector3.Normalize(this.transform.position - cam.transform.position);
    }
}
