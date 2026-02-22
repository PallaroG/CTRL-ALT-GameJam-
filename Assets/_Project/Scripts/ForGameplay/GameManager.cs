using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para trocar de cena
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<string> personagensContratados = new List<string>();

    [Header("Configurações de Cena")]
    public string nomeDaProximaCena = "NomeDaSuaCenaAqui"; // Defina o nome da cena no Inspector

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

            // Verifica se a lista atingiu 7 personagens
            if (personagensContratados.Count >= 7)
            {
                CarregarProximaCena();
            }
        }
    }

    public bool JaFoiContratado(string id)
    {
        return personagensContratados.Contains(id);
    }

    private void CarregarProximaCena()
    {
        Debug.Log("7 personagens contratados! Carregando a cena: " + nomeDaProximaCena);
        SceneManager.LoadScene(nomeDaProximaCena);
    }
}