using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MapEnabler : NetworkBehaviour
{
    public GameObject[] maps; //Lista de los cuatro mapas
    private int mapSelected = 0; 
    [SerializeField] private GameObject mapMenu; //Menú de selección de mapa del host
    private bool selected = false;

    public void StartMapEnabler() //Llamada cuando el coche del host es instanciado, activando el menú sólo para él
    {
        mapMenu.SetActive(true);
    }

    public void MapPressed(int m) //Cada botón del menú de selección cambia el índice mapSelected para la lista maps[]
    {
        mapSelected = m;
    }

    public void OnSelectionDone() 
        //Una vez el host ha elegido mapa se desactiva el menú de selección y se actualiza el mapa para todos los jugadores
    {
        mapMenu.SetActive(false);
        selected = true;
        UpdateMapClientRpc(mapSelected, selected);
    }

    [ServerRpc]
    public void RequestMapServerRpc() 
        //Llamada por cada cliente al entrar en la partida para comprobar si el host ya había elegido mapa antes de que entrara
    {
        UpdateMapClientRpc(mapSelected, selected);
    }

    [ClientRpc]
    private void UpdateMapClientRpc(int m, bool selected) //Actualiza el mapa activo en todos los clientes
        //Recibe un booleano que indica si el mapa es el elegido finalmente por el host o el que está por defecto
    {
        maps[0].SetActive(false); //Por defecto está el 0, así que se desactiva
        maps[m].SetActive(true);
        //Si es el elegido por el host (y, por consiguiente, ya no se puede cambiar) se activan el CircuitController
        //y el RaceController (desactivados para que no se hiciera su Start/Awake antes de tener el mapa final 
        if (selected) 
        {
            GetComponent<CircuitController>().enabled = true;
            GetComponent<RaceController>().enabled = true;
        }
        //A cada coche se le asigna una posición de carrera, indicando si es el mapa final o no
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        foreach(GameObject car in cars)
        {
            car.GetComponent<CarController>().PositionOnMap(selected);
        }
    }
}
