using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections; // Necessário para usar Coroutines
using UnityEngine.UI; // Necessário para acessar elementos de UI como o Slider

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<string> personagensContratados = new List<string>();

    [Header("Configurações de Cena")]
    public string nomeDaProximaCena = "NomeDaProximaCena";

    [Header("Configurações de UI de Loading")]
    public GameObject painelDeLoading; // Arraste o Panel "TelaDeLoading" aqui
    public Slider barraDeProgresso;    // Arraste o Slider aqui

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

            if (personagensContratados.Count >= 7)
            {
                Debug.Log("Limite de 7 personagens atingido. Iniciando carregamento assíncrono...");
                StartCoroutine(CarregarCenaAsync(nomeDaProximaCena));
            }
        }
    }

    public bool JaFoiContratado(string id)
    {
        return personagensContratados.Contains(id);
    }

    // Coroutine que faz o carregamento em segundo plano
    private IEnumerator CarregarCenaAsync(string nomeCena)
    {
        // Ativa a tela de loading
        if (painelDeLoading != null) painelDeLoading.SetActive(true);

        // Inicia o carregamento da cena em background
        AsyncOperation operacao = SceneManager.LoadSceneAsync(nomeCena);

        // Impede que a cena mude antes do loading terminar completamente (opcional, mas recomendado)
        // operacao.allowSceneActivation = false; 

        // Enquanto o carregamento não terminar...
        while (!operacao.isDone)
        {
            // O progresso do Unity vai de 0 a 0.9. Dividimos por 0.9 para normalizar de 0 a 1.
            float progresso = Mathf.Clamp01(operacao.progress / 0.9f);

            // Atualiza a barra
            if (barraDeProgresso != null)
            {
                barraDeProgresso.value = progresso;
            }

            // Espera até o próximo frame antes de continuar o loop
            yield return null;
        }
    }
}