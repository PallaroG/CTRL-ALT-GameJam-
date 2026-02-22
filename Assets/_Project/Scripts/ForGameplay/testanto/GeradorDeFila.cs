using UnityEngine;

public class GeradorDeFila : MonoBehaviour
{
    [Header("Configurações de Geração")]
    public PersonagemFila[] prefabsPersonagens; // Lista com o Caramelo, Capivara, Fiat Uno, etc.
    public Transform pontoDeNascimento;         // Onde o personagem surge na cena
    public float tempoEntreChegadas = 3f;       // Tempo em segundos entre cada spawn

    [Header("Conexão com a Fila")]
    public filaManager managerDaFila;

    private float tempoDecorrido;

    // Adicione isso dentro do seu GeradorDeFila.cs
    void Start()
    {
        // Preenche a fila automaticamente assim que o jogo (e o menu) abre
        while (!managerDaFila.FilaCheia())
        {
            GerarNovoPersonagem();
        }
    }

    void Update()
    {
        // Só tenta gerar alguém se a fila tiver espaço
        if (!managerDaFila.FilaCheia())
        {
            tempoDecorrido += Time.deltaTime; // Conta o tempo

            // Quando o tempo bater o limite estabelecido
            if (tempoDecorrido >= tempoEntreChegadas)
            {
                GerarNovoPersonagem();
                tempoDecorrido = 0f; // Zera o cronômetro
            }
        }
    }

    void GerarNovoPersonagem()
    {
        // Sorteia um número de 0 até o total de personagens na sua lista
        int indexSorteado = Random.Range(0, prefabsPersonagens.Length);
        PersonagemFila prefabEscolhido = prefabsPersonagens[indexSorteado];

        // Cria o personagem fisicamente no ponto de nascimento
        PersonagemFila novoPersonagem = Instantiate(prefabEscolhido, pontoDeNascimento.position, pontoDeNascimento.rotation);

        // Manda o personagem entrar na fila do manager
        managerDaFila.EntrarNaFila(novoPersonagem);
    }
}