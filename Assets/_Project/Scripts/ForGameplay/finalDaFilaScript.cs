using System;
using UnityEngine;

public class finalDaFilaScript : MonoBehaviour
{
    private PersonagemFila personagemFila;
    [NonSerialized]public string personagemName;
    void OnTriggerEnter(Collider other)
    {
        personagemFila = other.GetComponent<PersonagemFila>();

        if (personagemFila != null)
        {
            Debug.Log(personagemFila.nomeDoPersonagem);
            personagemName = personagemFila.nomeDoPersonagem;
        }
    }
}
