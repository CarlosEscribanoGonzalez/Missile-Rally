using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorButton : MonoBehaviour
{
    public bool selected; //Indica que el botón ha sido pulsado
    [SerializeField] private int materialIndex; //Almacena el índice del material asociado al botón
    PlayerInfo pInfo;
    
    void Awake()
    {
        pInfo = GameObject.FindObjectOfType<PlayerInfo>();
        if (selected)
        {
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); 
            pInfo.SetMaterial(materialIndex);
        }
    }

    public void SelectColor() //Función que se llama al pulsar el botón para elegir el material del coche
    {
        ColorButton[] colors = GameObject.FindObjectsOfType<ColorButton>();
        foreach(ColorButton c in colors) //Se desactiva la selección anterior
        {
            if (c.selected) 
            { 
                c.selected = false;
                c.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        selected = true;
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); //Se hace más grande el botón para que se vea mejor el elegido
        pInfo.SetMaterial(materialIndex); //Se almacena en PlayerInfo el color elegido
    }
}
