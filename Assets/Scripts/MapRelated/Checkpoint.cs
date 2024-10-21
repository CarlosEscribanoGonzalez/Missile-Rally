using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Checkpoint nextCheckpoint; //Cada chekcpoint almacena cuál es el siguiente en la carrera
    private bool isFinishLine = false; //Indica si es la meta
    [SerializeField] private Light light;
    [SerializeField] private GameObject arrow;

    public void SetNextCheckpoint(Checkpoint next)
    {
        nextCheckpoint = next;
        transform.forward = Vector3.Normalize(next.transform.position - this.transform.position); 
        //Al tener una flecha siempre debe aparecer apuntando hacia el siguiente checkpoint, indicando al jugador la dirección
    }

    public Checkpoint GetNext()
    {
        return nextCheckpoint;
    }

    public void SetFinishLine()
    {
        isFinishLine = true;
    }

    public bool IsFinishLine()
    {
        return isFinishLine;
    }

    //El checkpoint siempre está activo, pero cada cliente se encarga de "encender" el que le toque y "apagarlo" al pasar por él
    public void ToggleLight() 
    {
        light.enabled = !light.enabled;
        arrow.SetActive(!arrow.activeSelf);
    }
}
