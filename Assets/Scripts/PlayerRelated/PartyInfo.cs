using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PartyInfo : NetworkBehaviour
{
    //Diccionarios que almacenan los nombres y materiales del coche de todos los jugadores de la partida:
    public Dictionary<int, string> names = new Dictionary<int, string>();
    public Dictionary<int, int> materials = new Dictionary<int, int>();

    public void AddPlayer(int id, string name, int material) //Se utiliza como id el NetworkObjectId
    {
        names.Add(id, name);
        materials.Add(id, material);
    }
}
