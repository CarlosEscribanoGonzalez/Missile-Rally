using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultsInformation : MonoBehaviour
{
    //Perdura entre escenas y almacena en listas los datos a ser mostrados en la pantalla de resultados
    public List<string> names = new List<string>();
    public List<float> totalTimes = new List<float>();
    public List<float> avgTimes = new List<float>();
    public List<float> bestTimes = new List<float>();
    public List<Material> carMaterials = new List<Material>();
    public List<bool> disqualified = new List<bool>();

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    //Cada vez que un jugador finaliza la carrera o es descalificado (el resto terminó pero él no) se añaden sus datos
    public void AddResultInfo(string n, float tt, float av, float bt, Material m, bool d)
    {
        names.Add(n);
        totalTimes.Add(tt);
        avgTimes.Add(av);
        bestTimes.Add(bt);
        carMaterials.Add(m);
        disqualified.Add(d);   
    }
}
