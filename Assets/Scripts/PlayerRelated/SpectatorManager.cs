using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class SpectatorManager : MonoBehaviour
{
    private CinemachineVirtualCamera cam;
    [SerializeField] private TextMeshProUGUI spectatingText; //Texto que indica que el jugador est� de observador

    void Awake()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
        StartCoroutine(CheckTarget());
    }

    IEnumerator CheckTarget() //Cada 3 segundos la c�mara comprueba que el objeto al que sigue est� activo
    {
        yield return new WaitForSeconds(3);
        if (cam.Follow.parent)
        {
            if (!cam.Follow.parent.gameObject.activeSelf) //Si no est� activo busca un coche cualquiera y lo observa
            {
                GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");             
                if(cars.Length > 0)
                {
                    spectatingText.enabled = true;
                    cam.Follow = cars[0].transform;
                    cam.LookAt = cars[0].transform;
                }
            }
        }
        StartCoroutine(CheckTarget());
    }
}
