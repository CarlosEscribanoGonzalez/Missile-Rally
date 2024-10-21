using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class SemaphoreLight : MonoBehaviour
{
    [SerializeField] private float timeToChange; //Tiempo que tarda en volverse verde una vez los jugadores están listos
    private bool changed = false; //Indica si el semáforo ya ha cambiado
    private bool playersReady = false; //Indica si los jugadores están listos
    private static int changeCount = 0; //Número de semáforos que se han puesto en verde
    //Color:
    private MeshRenderer renderer;
    private Light light;
    [SerializeField] private Material greenMaterial;
    //Audio:
    private AudioSource source;
    [SerializeField] private AudioClip boop;

    void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        light = transform.Find("Point Light").GetComponent<Light>();
        source = GetComponent<AudioSource>();
        GameObject.Find("ReadyButton").GetComponent<PlayersReadyManager>().OnPlayersReady += OnPlayersReady;
    }

    void Update()
    {
        if (!playersReady || changed) return; //Si el semáforo ya cambió o no están listos los jugadores no hace nada

        timeToChange -= Time.deltaTime;
        if(timeToChange < 0) //Cuando el temporizador acaba el color se cambia y suena un SFX
        {
            changed = true;
            source.Play(); //El AudioSource ya tiene un audio asociado por defecto
            renderer.material = greenMaterial;
            light.color = Color.green;
            changeCount++;
            if (changeCount < 3) this.enabled = false; //Se desactiva para no ejecutar su Update, pues ya no tiene nada más que hacer 
            else StartCoroutine(StartRace());
        }
    }

    private void OnPlayersReady() //Llamada cuando más de la mitad de los jugadores están listos
    {
        playersReady = true;
    }

    IEnumerator StartRace()//Cuando los tres semáforos están en verde comienza la corrutina que da paso al inicio de la carrera
    {
        yield return new WaitForSeconds(1);
        source.clip = boop; //Se cambia el efecto de sonido para indicar el comienzo de la carrera
        source.Play();
        //Se inicializan los checkpoints y se permite el movimiento a los coches
        GameObject.FindObjectOfType<CheckpointManager>().InitializeCheckpoints();
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        foreach(GameObject car in cars)
        {
            CarController carController = car.GetComponent<CarController>();
            carController.raceStarted = true;
            //A cada coche se le asigna su siguiente checkpoint a alcanzar
            carController.nextPoint = GameObject.FindObjectOfType<CheckpointManager>().GetFirstCheckpoint();
            //El checkpoint anterior es de momento el mismo que el siguiente, pues el coche todavía no ha pasado por ninguno
            carController.prevPoint = carController.nextPoint;
            //Para cada jugador sólo se activan los efectos visuales de su propio coche
            if (carController.IsOwner) carController.nextPoint.ToggleLight(); 
        }
        this.enabled = false;
    }
}
