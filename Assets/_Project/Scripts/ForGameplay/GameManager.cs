using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<string> personagensContratados = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AdicionarContratado(PersonagemFila personagem)
    {
        if (personagem == null) return;

        if (!personagensContratados.Contains(personagem.ID))
        {
            personagensContratados.Add(personagem.ID);
            Debug.Log("Salvo ID: " + personagem.ID);
        }
    }

    public bool JaFoiContratado(string id)
    {
        return personagensContratados.Contains(id);
    }
}