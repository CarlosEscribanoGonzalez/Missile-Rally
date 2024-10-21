using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Runtime.ConstrainedExecution;

public class PlayersReadyManager : NetworkBehaviour
{
    private RaceController raceController;
    private static int numPlayersReady; //Número de jugadores que le han dado al botón de listo
    [SerializeField] private GameObject readyButton; //Botón de listo
    private bool pressed = false; //Indica si el botón ha sido pulsado
    public Action OnPlayersReady; //Desencadena el comienzo de la partida cuando los jugadores están listos

    void Awake()
    {
        raceController = GameObject.FindObjectOfType<RaceController>();
        readyButton.SetActive(false); //El botón comienza inactivo
    }

    public void ActiveButton()
    {
        if(!pressed) readyButton.SetActive(true); //Una vez se ha seleccionado el mapa final se activa el botón
    }

    public void OnClick() 
    {
        //Cuando se pulsa el botón este mismo se desactiva, controlando que un solo jugador no pueda pulsarlo varias veces
        readyButton.SetActive(false);
        if (!pressed)
        {
            pressed = true;
            CheckPlayersReadyServerRpc(); //Manda al servidor la información de que se ha pulsado el botón
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckPlayersReadyServerRpc() 
    {
        //Cuando al servidor le llega la información de que un jugador está
        //listo comprueba si se puede empezar la carrera
        numPlayersReady++;
        float numPlayers = raceController.GetNumPlayers();
        if (numPlayers > 1 && numPlayersReady > numPlayers / 2) //Si más de la mitad están listos comienza la carrera
        {
            StartRaceClientRpc();
        }
    }

    [ClientRpc]
    private void StartRaceClientRpc()
    {
        readyButton.SetActive(false); //Se hubiera pulsado o no el botón se desactiva
        OnPlayersReady?.Invoke(); //Inicializa la HUD de los jugadores y comienza la cuenta atrás del semáforo
    }
}
