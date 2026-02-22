using UnityEngine;
using UnityEngine.UI;

public class contratacaoScript : MonoBehaviour
{
    /*
        script para controlar os botões de contratação
        -----------------------------------------------
        verifica qual o personagem é o primeiro na fila
        método que contrata o personagem
        método que dispensa o personagem

    */
    public finalDaFilaScript finalDaFilaScript;
    public filaManager filaManager;
    public PersonagemFila fila;


    public GameObject ButtonCaramelo;
    public GameObject ButtonCapivara;
    public GameObject ButtonETBilu;
    public GameObject ButtonPombo;
    public GameObject ButtonFiatUno;

    public void contratado()
    {
        Debug.Log("Opa vem pra ca");
        if(finalDaFilaScript.personagemName != null)
        {
            switch (finalDaFilaScript.personagemName)
            {
                case "Capivara":
                    ButtonCapivara.SetActive(true);
                    break;
                case "Caramelo":
                    ButtonCaramelo.SetActive(true);
                    break;
                case "ET Bilu":
                    ButtonETBilu.SetActive(true);
                    break;
                case "Pombo":
                    ButtonPombo.SetActive(true);
                    break;
                case "Fiat Uno":
                    ButtonFiatUno.SetActive(true);
                    break;
            }
                
        }
        filaManager.SairPrimeiro();
    }

    public void dispensado()
    {
        Debug.Log("Sai pra lá");
        filaManager.SairPrimeiro();
    }


}
