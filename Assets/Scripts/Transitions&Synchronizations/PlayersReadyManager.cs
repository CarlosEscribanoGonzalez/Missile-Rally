using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Runtime.ConstrainedExecution;

public class PlayersReadyManager : NetworkBehaviour
{
    private RaceController raceController;
    private static int numPlayersReady; //N�mero de jugadores que le han dado al bot�n de listo
    [SerializeField] private GameObject readyButton; //Bot�n de listo
    private bool pressed = false; //Indica si el bot�n ha sido pulsado
    public Action OnPlayersReady; //Desencadena el comienzo de la partida cuando los jugadores est�n listos

    void Awake()
    {
        raceController = GameObject.FindObjectOfType<RaceController>();
        readyButton.SetActive(false); //El bot�n comienza inactivo
    }

    public void ActiveButton()
    {
        if(!pressed) readyButton.SetActive(true); //Una vez se ha seleccionado el mapa final se activa el bot�n
    }

    public void OnClick() 
    {
        //Cuando se pulsa el bot�n este mismo se desactiva, controlando que un solo jugador no pueda pulsarlo varias veces
        readyButton.SetActive(false);
        if (!pressed)
        {
            pressed = true;
            CheckPlayersReadyServerRpc(); //Manda al servidor la informaci�n de que se ha pulsado el bot�n
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckPlayersReadyServerRpc() 
    {
        //Cuando al servidor le llega la informaci�n de que un jugador est�
        //listo comprueba si se puede empezar la carrera
        numPlayersReady++;
        float numPlayers = raceController.GetNumPlayers();
        if (numPlayers > 1 && numPlayersReady > numPlayers / 2) //Si m�s de la mitad est�n listos comienza la carrera
        {
            StartRaceClientRpc();
        }
    }

    [ClientRpc]
    private void StartRaceClientRpc()
    {
        readyButton.SetActive(false); //Se hubiera pulsado o no el bot�n se desactiva
        OnPlayersReady?.Invoke(); //Inicializa la HUD de los jugadores y comienza la cuenta atr�s del sem�foro
    }
}
