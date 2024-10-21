using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using TMPro;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class InputController : NetworkBehaviour
{
    static Dictionary<int, CarController> cars = new Dictionary<int, CarController>(); //Diccionario que almacena todos los coches de la partida
    [SerializeField] public Material[] materials = new Material[6]; //Lista de materiales
    private CarController car;
    private PlayerInfo pInfo; //Informaci�n del cliente almacenada en la escena de personalizaci�n
    private int id; //NetworkId de car
    string name = "Name";

    private void Awake()
    {
        pInfo = GameObject.FindObjectOfType<PlayerInfo>();
        car = GetComponent<Player>().car.GetComponent<CarController>();
    }

    private void Start()
    {
        id = (int)GetComponent<NetworkObject>().NetworkObjectId;
        cars.Add(id, car); //Cada vez que un coche se instancia se a�ade al diccionario de coches
        if (IsOwner) 
        {
            //Si el coche es el manejado por el jugador manda su informaci�n de pInfo al servidor,
            //hace que la c�mara le siga y activa el PlayerInput
            AddPlayerToPartyServerRpc(id, pInfo.GetName(), pInfo.GetMaterial());
            GetComponent<PlayerInput>().enabled = true;
            GameObject.Find("CustomCamera").GetComponent<CinemachineVirtualCamera>().Follow = car.transform;
            GameObject.Find("CustomCamera").GetComponent<CinemachineVirtualCamera>().LookAt = car.transform;
        }
    }

    [ServerRpc]
    private void AddPlayerToPartyServerRpc(int id, string name, int materialIndex)
    {
        PartyInfo party = GameObject.FindWithTag("PartyInfo").GetComponent<PartyInfo>();
        party.AddPlayer(id, name, materialIndex); //A�ade la informaci�n del jugador conectado a los diccionarios
        //Actualiza los nombres y materiales de los coches para todos los jugadores
        foreach (int i in party.names.Keys)
        {
            UpdateNameClientRpc(i, party.names[i]);
            UpdateMaterialClientRpc(i, party.materials[i]);
        }
    }

    [ClientRpc]
    private void UpdateNameClientRpc(int index, string name)
    {
        if (cars.ContainsKey(index))
        {
            cars[index].transform.Find("Name").GetComponent<TextMeshPro>().text = name;
            //Tambi�n almacena la informaci�n en el script CarController para que sea propagada a la escena de resultados
            cars[index].GetComponent<CarController>().name = name;
        }
    }

    [ClientRpc]
    private void UpdateMaterialClientRpc(int index, int materialIndex) //El material se pasa como �ndice para ser serializable
    {
        if (cars.ContainsKey(index))
        {
            MeshRenderer carRenderer = cars[index].transform.Find("body").GetComponent<MeshRenderer>();
            Material[] carMaterials = carRenderer.materials;
            carMaterials[1] = materials[materialIndex];
            carRenderer.materials = carMaterials;
            cars[index].GetComponent<CarController>().material = materials[materialIndex];
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        OnMoveServerRpc(context.ReadValue<Vector2>());
    }

    [ServerRpc]
    private void OnMoveServerRpc(Vector2 input)
    {
        car.InputAcceleration = input.y;
        car.InputSteering = input.x;
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        OnBrakeServerRpc(context.ReadValue<float>());
    }

    [ServerRpc]
    private void OnBrakeServerRpc(float input)
    {
        car.InputBrake = input;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
    }
}