using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ResultField : MonoBehaviour
{
    //Encargado de mostrar por cada puesto los datos del jugador 
    [SerializeField] private int num; //Número de puesto que tiene que mostrar
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI avgTimeText;
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [SerializeField] private TextMeshProUGUI disqualifiedText;
    [SerializeField] private MeshRenderer carRenderer;
    private ResultsInformation results;

    void Awake()
    {
        results = GameObject.FindObjectOfType<ResultsInformation>();
        //Si el número de jugadores es menor al puesto a mostrar se destruye, pues no tiene informacion que mostrar
        if (num >= results.names.Count) Destroy(this.gameObject);
    }

    private void Start() //Enseña los datos del índice "num" almacenados en las listas de ResultsInformation
    {
        nameText.text = results.names[num];
        totalTimeText.text = $"Total time: {results.totalTimes[num]:F2}";
        avgTimeText.text = $"Avg time: {results.avgTimes[num]:F2}";
        bestTimeText.text = $"Best time: {results.bestTimes[num]:F2}";
        if (!results.disqualified[num]) disqualifiedText.enabled = false;
        Material[] carMaterials = carRenderer.materials;
        carMaterials[1] = results.carMaterials[num];
        carRenderer.materials = carMaterials;
    }
}
