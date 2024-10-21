using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    //Script encargado de almacenar los valores del nombre y material elegidos por el jugador en la escena de personalización
    private string playerName;
    private int carMaterial;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public string GetName() { return playerName; }

    public void SetName(string n)
    {
        playerName = n;
    }

    public int GetMaterial() { return carMaterial; }

    public void SetMaterial(int m)
    {
        carMaterial = m;
    }
}
