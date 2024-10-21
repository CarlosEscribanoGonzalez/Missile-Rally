using System;
using System.Collections.Generic;
using UnityEngine;

public class RaceController : MonoBehaviour
{
    public int numPlayers;

    private readonly List<Player> _players = new(4);
    private CircuitController _circuitController;
    //private GameObject[] _debuggingSpheres;

    private float[] arcLs = new float[4]; //Longitud de cada jugador según el LineRenderer
    private int[] positions = new int[4]; //Array de posiciones de la carrera
    private int[] numCheckpoints = new int[4]; //Número de checkpoints cogidos por cada jugador

    private void Start()
    {
        if (_circuitController == null) _circuitController = GetComponent<CircuitController>();

        //_debuggingSpheres = new GameObject[GameManager.Instance.numPlayers];
        //for (int i = 0; i < GameManager.Instance.numPlayers; ++i)
        //{
        //    _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        //}
    }

    private void Update()
    {
        if (_players.Count == 0)
            return;

        for (int i = 0; i < _players.Count; ++i)
        {
            ComputeCarArcLength(i); //Se calculan los valores de arcLs
        }
        UpdatePositions(); //Se actualizan las posiciones de la carrera
    }

    private void UpdatePositions()
    {
        SortedList<float, int> posList = new SortedList<float, int>();
        for (int i = 0; i < _players.Count; i++)
        {
            //La detección de los puestos se hace en función del número de checkpoints.
            //El desempate es según la distancia almacenada en arcLs
            float totalDistance = numCheckpoints[i]*500 + arcLs[i]; 
            while (posList.ContainsKey(totalDistance)) totalDistance += 0.0001f; //Evitamos que dé un error al intentar almacenar dos llaves iguales
            posList.Add(totalDistance, i); //Guarda en la lista ordenada los ids en función de su distancia
        }
        //Se rellena el array de posiciones a partir del orden de almacenamiento de los ids en la SortedList
        int count = _players.Count;
        foreach(int id in posList.Values) 
        {
            positions[id] = count--; //La lista los almacena de menor a mayor, por lo que hay que asignar las posiciones "al revés"
        }
    }

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }

    float ComputeCarArcLength(int id)
    {
        // Compute the projection of the car position to the closest circuit 
        // path segment and accumulate the arc-length along of the car along
        // the circuit.
        this._players[id].car.GetComponent<CarController>().racerId = id;
        Vector3 carPos = this._players[id].car.transform.position;


        float minArcL =
            this._circuitController.ComputeClosestPointArcLength(carPos, out _, out var carProj, out _);

        //this._debuggingSpheres[id].transform.position = carProj;

        if (this._players[id].CurrentLap == 0)
        {
            minArcL -= _circuitController.CircuitLength;
        }
        else
        {
            minArcL += _circuitController.CircuitLength *
                       (_players[id].CurrentLap - 1);
        }
        arcLs[id] = minArcL; //Se añade la longitud sobre la línea del coche al array
        return minArcL;
    }

    public float GetArcL(int id)
    {
        return arcLs[id];
    }

    public int GetPos(int id)
    {
        return positions[id];
    }

    public int GetNumPlayers()
    {
        return _players.Count;
    }

    public void SetNumCheckpoints(int id, int c) //Actualiza el array de checkpoints recogidos, llamado cada vez que un coche pasa por uno
    {
        numCheckpoints[id] = c;
    }
}