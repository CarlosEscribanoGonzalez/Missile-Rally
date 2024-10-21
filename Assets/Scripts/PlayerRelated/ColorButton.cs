using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorButton : MonoBehaviour
{
    public bool selected; //Indica que el bot�n ha sido pulsado
    [SerializeField] private int materialIndex; //Almacena el �ndice del material asociado al bot�n
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

    public void SelectColor() //Funci�n que se llama al pulsar el bot�n para elegir el material del coche
    {
        ColorButton[] colors = GameObject.FindObjectsOfType<ColorButton>();
        foreach(ColorButton c in colors) //Se desactiva la selecci�n anterior
        {
            if (c.selected) 
            { 
                c.selected = false;
                c.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        selected = true;
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); //Se hace m�s grande el bot�n para que se vea mejor el elegido
        pInfo.SetMaterial(materialIndex); //Se almacena en PlayerInfo el color elegido
    }
}
