using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;

public class CarController : NetworkBehaviour
{
    #region Variables
    [Header("Movement")] public List<AxleInfo> axleInfos;
    [SerializeField] private float forwardMotorTorque = 100000;
    [SerializeField] private float backwardMotorTorque = 50000;
    [SerializeField] private float maxSteeringAngle = 15;
    [SerializeField] private float engineBrake = 1e+12f;
    [SerializeField] private float footBrake = 1e+24f;
    [SerializeField] private float topSpeed = 200f;
    [SerializeField] private float downForce = 100f;
    [SerializeField] private float slipLimit = 0.2f;

    private float CurrentRotation { get; set; }
    public float InputAcceleration { get; set; }
    public float InputSteering { get; set; }
    public float InputBrake { get; set; }

    //private PlayerInfo m_PlayerInfo;

    private Rigidbody _rigidbody;
    private float _steerHelper = 0.8f;

    private float _currentSpeed = 0;

    //VARIABLES PROPIAS
    public int racerId = 0; //Id de corredor almacenado en las listas de RaceController
    RaceController raceController;
    public string name; //Nombre del jugador que controla al coche
    public Material material; //Material del coche
    private bool addedToResults = false; //Booleano que controla que la información del coche se ha añadido a los resultados una única vez
    static Queue<Vector3> startPositions = new Queue<Vector3>(); //Posiciones de inicio de carrera
    //Restricción o correcciones de movimiento:
    private bool correctionCooldown = false; //Cooldown de tiempo para la corrección de la posición en caso de vuelco/choque
    private float directionTimer = 0; //Timer que controla el tiempo durante el cual el jugador hace el recorrido mal
    private bool canMove = true; //Indica si el jugador tiene control del coche o no
    public bool raceStarted = false; //Booleano que indica si la carrera ha comenzado
    //Información de progreso de la carrera:
    private float currentArcLength = 0; //Longitud actual sobre el LineRenderer
    private float totalTime = 0; //Tiempo total 
    private float bestTime = float.PositiveInfinity; //Mejor tiempo para dar una vuelta
    private float lapTimer = 0; //Temporizador que se resetea cada vez que se da una vuelta
    private List<float> lapTimes = new List<float>(); //Lista de tiempos tardados por vuelta
    private int laps = 0; //Cantidad de vueltas realizadas
    private bool updateLaps = false; //Booleano que indica que el jugador ha efectuado una vuelta
    //Checkpoints:
    public Checkpoint nextPoint; //Próximo checkpoint a alcanzar
    public Checkpoint prevPoint; //Último checkpoint cogido
    private int numCheckpoints = 0; //Número de checkpoints cogido
    //Textos del HUD (comienzan desactivados):
    private TextMeshProUGUI speedText;
    private TextMeshProUGUI totalTimeText;
    private TextMeshProUGUI lapsText;
    private TextMeshProUGUI avgTimeText;
    private TextMeshProUGUI bestTimeText;
    private TextMeshProUGUI positionText;
    private Speedometer speedometer;

    private float Speed
    {
        get => _currentSpeed;
        set
        {
            if (Math.Abs(_currentSpeed - value) < float.Epsilon) return;
            _currentSpeed = value;
            if (OnSpeedChangeEvent != null)
                OnSpeedChangeEvent(_currentSpeed);
        }
    }

    public delegate void OnSpeedChangeDelegate(float newVal);

    public event OnSpeedChangeDelegate OnSpeedChangeEvent;

    #endregion Variables

    #region Unity Callbacks

    public void Awake()
    {
        raceController = GameObject.FindObjectOfType<RaceController>();
        speedText = transform.Find("Canvas").Find("SpeedText").GetComponent<TextMeshProUGUI>();
        totalTimeText = transform.Find("Canvas").Find("TotalTimeText").GetComponent<TextMeshProUGUI>();
        lapsText = transform.Find("Canvas").Find("LapsText").GetComponent<TextMeshProUGUI>();
        avgTimeText = transform.Find("Canvas").Find("AvgTimeText").GetComponent<TextMeshProUGUI>();
        bestTimeText = transform.Find("Canvas").Find("BestTimeText").GetComponent<TextMeshProUGUI>();
        positionText = transform.Find("Canvas").Find("PositionText").GetComponent<TextMeshProUGUI>();
        speedometer = GameObject.FindObjectOfType<Speedometer>();
        speedometer.gameObject.SetActive(false);
    }

    public void Start()
    {
        MapEnabler mapEnabler = GameObject.Find("CircuitManager").GetComponent<MapEnabler>();
        mapEnabler.RequestMapServerRpc(); //Se actualiza el mapa y las posiciones cada vez que entra un jugador
        if (IsServer && IsOwner) mapEnabler.StartMapEnabler(); //Se activa la pestaña de selección de mapa para el host
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Update()
    {
        if (!raceStarted) return;
        totalTime += Time.deltaTime;
        lapTimer += Time.deltaTime;
        if (updateLaps) //Si el jugador ha dado una vuelta se actualizan los valores
        {
            laps++;
            lapTimes.Add(lapTimer);
            foreach (float time in lapTimes) //Se obtiene el mejor tiempo
            {
                if (time < bestTime) bestTime = time;
            }
            lapTimer = 0;
        }

        if (IsOwner) //Se actualiza la HUD del jugador y se calcual si está haciendo bien el recorrido
        {
            CalculateDirection(); //Calcula si está haciendo bien el recorrido o no
            totalTimeText.text = $"Total time: {totalTime:F2}";
            if (updateLaps)
            {
                lapsText.text = "Laps: " + laps;
                bestTimeText.text = $"Best time: {bestTime:F2}";
                if (laps >= 1) avgTimeText.text = $"Avg. time per lap: {totalTime / laps:F2}";
            }
            positionText.text = raceController.GetPos(racerId).ToString();
        }
        updateLaps = false;

        if(laps == 3) //Cuando da tres vueltas finaliza la carrera, añadiendo sus resultados a ResultsInformation
        {
            if (!addedToResults)
            {
                addedToResults = true;
                float avgTime = float.PositiveInfinity;
                if (laps > 0) avgTime = totalTime / laps;
                GameObject.FindObjectOfType<ResultsInformation>().AddResultInfo(name, totalTime, avgTime, bestTime, material, false);
            }
            Invoke("DisableCar", 1f); //Desactiva el coche tras un rato
        }

        if (IsServer) //El servidor calcula la velocidad de todos los coches y se la comunica (no se actualiza bien de otra forma)
        {
            Speed = Vector3.Dot(_rigidbody.velocity, transform.forward);
            Speed *= 3.6f; //En km/h
            if (Math.Abs(Speed) >= 0 && Math.Abs(Speed) < 1) Speed = 0; //A veces no marca bien cuando está quieto
            UpdateSpeedClientRpc(racerId, Speed);
        }

        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        if (cars.Length <= 1) //Si sólo queda un coche se añaden sus resultados a ResultsInfo (como descalificado) y se cambia la escena
        {
            if (!addedToResults)
            {
                addedToResults = true;
                GameObject.FindObjectOfType<ResultsInformation>().AddResultInfo(name, totalTime, totalTime / laps, bestTime, material, true);
            }
            GameObject.FindObjectOfType<SceneChanger>().ChangeScene();
        }
    }

    public void FixedUpdate()
    {
        if (!raceStarted || !IsServer) return;

        CorrectTurnOver(); //Comprueba que el coche no esté dado la vuelta

        if (canMove)
        {
            InputSteering = Mathf.Clamp(InputSteering, -1, 1);
            InputAcceleration = Mathf.Clamp(InputAcceleration, -1, 1);
            InputBrake = Mathf.Clamp(InputBrake, 0, 1);

            float steering = maxSteeringAngle * InputSteering;

            foreach (AxleInfo axleInfo in axleInfos)
            {
                if (axleInfo.steering)
                {
                    axleInfo.leftWheel.steerAngle = steering;
                    axleInfo.rightWheel.steerAngle = steering;
                }

                if (axleInfo.motor)
                {
                    if (InputAcceleration > float.Epsilon)
                    {
                        axleInfo.leftWheel.motorTorque = forwardMotorTorque;
                        axleInfo.leftWheel.brakeTorque = 0;
                        axleInfo.rightWheel.motorTorque = forwardMotorTorque;
                        axleInfo.rightWheel.brakeTorque = 0;
                    }

                    if (InputAcceleration < -float.Epsilon)
                    {
                        axleInfo.leftWheel.motorTorque = -backwardMotorTorque;
                        axleInfo.leftWheel.brakeTorque = 0;
                        axleInfo.rightWheel.motorTorque = -backwardMotorTorque;
                        axleInfo.rightWheel.brakeTorque = 0;
                    }

                    if (Math.Abs(InputAcceleration) < float.Epsilon)
                    {
                        axleInfo.leftWheel.motorTorque = 0;
                        axleInfo.leftWheel.brakeTorque = engineBrake;
                        axleInfo.rightWheel.motorTorque = 0;
                        axleInfo.rightWheel.brakeTorque = engineBrake;
                    }

                    if (InputBrake > 0)
                    {
                        axleInfo.leftWheel.brakeTorque = footBrake;
                        axleInfo.rightWheel.brakeTorque = footBrake;
                    }
                }

                ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            }

            SteerHelper();
            SpeedLimiter();
            AddDownForce();
            TractionControl();
        }
    }

    #endregion

    #region Methods

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        foreach (var axleInfo in axleInfos)
        {
            WheelHit wheelHitLeft;
            WheelHit wheelHitRight;
            axleInfo.leftWheel.GetGroundHit(out wheelHitLeft);
            axleInfo.rightWheel.GetGroundHit(out wheelHitRight);

            if (wheelHitLeft.forwardSlip >= slipLimit)
            {
                var howMuchSlip = (wheelHitLeft.forwardSlip - slipLimit) / (1 - slipLimit);
                axleInfo.leftWheel.motorTorque -= axleInfo.leftWheel.motorTorque * howMuchSlip * slipLimit;
            }

            if (wheelHitRight.forwardSlip >= slipLimit)
            {
                var howMuchSlip = (wheelHitRight.forwardSlip - slipLimit) / (1 - slipLimit);
                axleInfo.rightWheel.motorTorque -= axleInfo.rightWheel.motorTorque * howMuchSlip * slipLimit;
            }
        }
    }

// this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        foreach (var axleInfo in axleInfos)
        {
            axleInfo.leftWheel.attachedRigidbody.AddForce(
                -transform.up * (downForce * axleInfo.leftWheel.attachedRigidbody.velocity.magnitude));
        }
    }

    private void SpeedLimiter()
    {
        float speed = _rigidbody.velocity.magnitude;
        if (speed > topSpeed)
            _rigidbody.velocity = topSpeed * _rigidbody.velocity.normalized;
    }

// finds the corresponding visual wheel
// correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider col)
    {
        if (col.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = col.transform.GetChild(0);
        Vector3 position;
        Quaternion rotation;
        col.GetWorldPose(out position, out rotation);
        var myTransform = visualWheel.transform;
        myTransform.position = position;
        myTransform.rotation = rotation;
    }

    private void SteerHelper()
    {
        foreach (var axleInfo in axleInfos)
        {
            WheelHit[] wheelHit = new WheelHit[2];
            axleInfo.leftWheel.GetGroundHit(out wheelHit[0]);
            axleInfo.rightWheel.GetGroundHit(out wheelHit[1]);
            foreach (var wh in wheelHit)
            {
                if (wh.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }
        }

// this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(CurrentRotation - transform.eulerAngles.y) < 10f)
        {
            var turnAdjust = (transform.eulerAngles.y - CurrentRotation) * _steerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnAdjust, Vector3.up);
            _rigidbody.velocity = velRotation * _rigidbody.velocity;
        }

        CurrentRotation = transform.eulerAngles.y;
    }

    private void DisableCar()
    {
        this.transform.parent.gameObject.SetActive(false);
    }

    public void PositionOnMap(bool mapSelected) //Posiciona a los jugadores en las posiciones iniciales 
    {
        if (IsServer && IsOwner) //El script del coche del host obtiene la cola de posiciones iniciales del mapa
        {
            startPositions.Clear();
            GameObject[] positions = GameObject.FindGameObjectsWithTag("StartingPos");
            foreach (GameObject pos in positions)
            {
                startPositions.Enqueue(pos.transform.position);
            }
        }

        if (IsServer) //Una vez obtenida la cola posiciona a todos los coches en una posición inicial distinta
        {
            transform.position = startPositions.Dequeue();
        }

        if (mapSelected) //Si el mapa sobre el que se han posicionado es el elegido por el host se activa el botón de listo
        {
            PlayersReadyManager readyButton = GameObject.Find("ReadyButton").GetComponent<PlayersReadyManager>();
            readyButton.ActiveButton();
            readyButton.OnPlayersReady += OnPlayersReady;
        }
    }

    private void OnPlayersReady() //Activa el HUD del jugador una vez la partida va a comenzar
    {
        if (IsOwner)
        {
            speedText.enabled = true;
            totalTimeText.enabled = true;
            lapsText.enabled = true;
            avgTimeText.enabled = true;
            bestTimeText.enabled = true;
            positionText.enabled = true;
            speedometer.gameObject.SetActive(true);
        }
    }

    [ClientRpc]
    private void UpdateSpeedClientRpc(int id, float speed) //Actualiza los valores de velocidad del coche
    {
        if (id != racerId || !IsOwner) return;
        speedText.text = $"{speed:F2} km/h";
        speedometer.SetSpeed(Math.Abs(speed));
    }

    private void CorrectTurnOver()
    {
        //Comprueba si el coche está volcado verificando el componente Y del vector transform.up
        if (transform.up.y <= 0f && !correctionCooldown)
        {
            StartCoroutine(CorrectPosition()); //De estar volcado corrige su posición
        }
    }
    
    IEnumerator CorrectPosition()
    {
        correctionCooldown = true; //El cooldown se activa para evitar que se corrija la posición varias veces de seguido
        canMove = false; //El jugador pierde el control del coche
        yield return new WaitForSeconds(2.0f); //Dos segundos de penalización antes de corregir su posición
        //Reset de la rotacion del coche para ponerlo recto
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        //Mover el coche al último checkpoint apuntando hacia el siguiente
        transform.position = prevPoint.transform.position;
        transform.rotation = prevPoint.transform.rotation;
        //Moverlo un poco en vertical
        transform.position += Vector3.up * 1f;
        //Poner las velocidades a 0
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.velocity = Vector3.zero;
        //Reestablecer cooldown y movimiento
        correctionCooldown = false;
        canMove = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if(Math.Abs(Speed) > 120 && !correctionCooldown) 
            //Si se golpea a velocidad considerable pierde control del coche y se resetea la posición
            //Similar a cuando se vuelca el coche
        {
            StartCoroutine(CorrectPosition());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            if(other.GetComponent<Checkpoint>() == nextPoint) //Si el checkpoint en el que entra era el que le tocaba:
            {
                numCheckpoints++;
                raceController.SetNumCheckpoints(racerId, numCheckpoints); //Acutaliza su numCheckpoints para calcular las posiciones
                //Se actualizan los checkpoints del coche
                prevPoint = nextPoint;
                if (IsOwner) prevPoint.ToggleLight();
                nextPoint = nextPoint.GetNext();
                if (IsOwner) nextPoint.ToggleLight();
                //Si el checkpoint era la meta se actualizan las vueltas
                if (prevPoint.IsFinishLine())
                {
                    updateLaps = true;
                    if (laps >= 2 && IsOwner)
                    {
                        nextPoint.ToggleLight(); //Si ha finalizado el recorrido el siguiente punto se apaga
                        nextPoint = null; //Para evitar que lo coja
                    }
                }
            }
        }
    }

    private void CalculateDirection() //Calcula si está efectuando el recorrido al revés
    {
        float newArcLength = raceController.GetArcL(racerId);       
        if (newArcLength < currentArcLength)
        {
            directionTimer += Time.deltaTime; //Si ha retrocedido se aumenta el temporizador directionTimer
        }
        else if(newArcLength > currentArcLength) //En el momento que avanza se resetea el temporizador
        {
            directionTimer = 0;
            GameObject.Find("WarningText").GetComponent<TextMeshProUGUI>().enabled = false;
        }
        if(directionTimer > 1.5) //Pasado un rato de ir marcha atrás sale la advertencia
        {
            GameObject.Find("WarningText").GetComponent<TextMeshProUGUI>().enabled = true;
        }
        currentArcLength = newArcLength;
    }

    #endregion
    
}