using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class NameCustomization : MonoBehaviour
{
    private int maxNameLength = 15; //Máximo de caracteres permitidos por un nombre
    private PlayerInfo pInfo; //Información del nombre elegido y el material que perdura entre escenas
    [SerializeField] private TMP_InputField nameText; //TextField para el nombre del jugador

    private void Awake()
    {
        pInfo = GameObject.FindObjectOfType<PlayerInfo>();
        InitializePlayerName();
    }

    void InitializePlayerName() //Por defecto se pone un nombre aleatorio al jugador
    {
        nameText.text = "Player";
        for (int i = 0; i < 5; i++)
        {
            float number = Random.Range(0, 10);
            nameText.text += number.ToString();
        }
    }

    public void OnTextChanged(string newString)
    {
        nameText.text = newString;
        if (nameText.text.Length > maxNameLength) nameText.text = nameText.text.Substring(0, maxNameLength);
        pInfo.SetName(nameText.text); //Se almacena el nombre en PlayerInfo
    }
}

