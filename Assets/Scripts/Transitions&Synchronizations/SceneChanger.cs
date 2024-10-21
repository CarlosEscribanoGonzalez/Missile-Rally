using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private float fadeinSpeed; //Velocidad del fadein
    [SerializeField] private float fadeoutSpeed; //Velocidad base del fadeout
    private bool forced = false; //Indica si la escena a la que se quiere cambiar es forzada o sigue el build index
    private string forcedScene; //Escena a la que se quiere forzar el cambio

    private void Awake()
    {
        anim = GetComponent<Animator>();
        anim.speed = fadeinSpeed; //La animación de fadein se realiza nada más se carga la escena
    }

    public void ChangeScene() //Indica el cambio a la siguiente escena
    {
        anim.speed = fadeoutSpeed;
        anim.SetTrigger("ChangeScene");
    }

    public void ForceScene(string name, float speed) //Fuerza el cambio a una escena determinada con una velocidad "speed"
    {
        anim.speed = speed;
        forcedScene = name;
        forced = true;
        anim.SetTrigger("ChangeScene");
    }

    private void OnAnimationEnded() //Llamada desde la animación de fadeout, cambia la escena
    {
        if (!forced) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        else SceneManager.LoadScene(forcedScene);
    }
}
