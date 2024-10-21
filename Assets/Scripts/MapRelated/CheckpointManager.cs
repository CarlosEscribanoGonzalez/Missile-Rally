using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private Checkpoint[] checkpoints;
    [SerializeField] private GameObject checkpointPrefab;

    public void InitializeCheckpoints() //Llamada una vez la carrera comienza, inicializa la lista de checkpoints del mismo
    {
        LineRenderer lineRenderer = GameObject.FindWithTag("Map").GetComponent<LineRenderer>();
        Vector3[] points = new Vector3[lineRenderer.positionCount];
        checkpoints = new Checkpoint[lineRenderer.positionCount];
        lineRenderer.GetPositions(points);
        //Para cada posici�n del LineRenderer se crea un checkpoint
        for(int i = 0; i < points.Length; i++)
        {
            checkpoints[i] = Instantiate(checkpointPrefab, points[i], this.transform.rotation).GetComponent<Checkpoint>();
        }
        //A cada checkpoint se le indica cu�l es el que le sigue para determinar el orden de la carrera
        for (int i = 0; i < checkpoints.Length; i++)
        {
            int next = i + 1;
            if (next == checkpoints.Length) next = 1; //El primer y �ltimo checkpoint comparten posici�n, por lo que el siguiente del �ltimo es el segundo
            checkpoints[i].SetNextCheckpoint(checkpoints[next]);
        }
        checkpoints[checkpoints.Length - 1].SetFinishLine(); //El �ltimo se marca como meta
    }

    public Checkpoint GetFirstCheckpoint() //Le indica a los coches cu�l es el primer checkpoint para iniciar su recorrido
    {
        return checkpoints[0];
    }
}
