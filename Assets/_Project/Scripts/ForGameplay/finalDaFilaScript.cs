using System;
using UnityEngine;

public class finalDaFilaScript : MonoBehaviour
{
    [NonSerialized] public bool PodeContratar = false;
    [NonSerialized] public PersonagemFila personagem;

    private void OnTriggerEnter(Collider other)
    {
        PersonagemFila p = other.GetComponent<PersonagemFila>();
        if (p == null) return;

        personagem = p;
        PodeContratar = true;

        Transform[] filhos = other.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in filhos)
        {
            if (t.CompareTag("dialogueBox"))
            {
                t.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PersonagemFila p = other.GetComponent<PersonagemFila>();
        if (p == null) return;

        if (personagem == p)
        {
            PodeContratar = false;
            personagem = null;
        }

        Transform[] filhos = other.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in filhos)
        {
            if (t.CompareTag("dialogueBox"))
            {
                t.gameObject.SetActive(false);
            }
        }
    }

    // 🔹 ADICIONADO: força verificação manual de quem está dentro do trigger
    public void ForcarRevalidacao()
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position,
            transform.localScale / 2,
            transform.rotation
        );

        personagem = null;
        PodeContratar = false;

        foreach (Collider col in colliders)
        {
            PersonagemFila p = col.GetComponent<PersonagemFila>();
            if (p != null)
            {
                personagem = p;
                PodeContratar = true;

                Transform[] filhos = col.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in filhos)
                {
                    if (t.CompareTag("dialogueBox"))
                    {
                        t.gameObject.SetActive(true);
                    }
                }

                return;
            }
        }
    }
}