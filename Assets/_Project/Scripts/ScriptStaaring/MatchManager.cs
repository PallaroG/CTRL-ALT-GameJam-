using UnityEngine;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour {

    public static MatchManager Instance; 

    [Header("Configuração da Partida")]
    public float tempoDePartida = 120f; 
    private float tempoAtual;
    private bool jogoAcabou = false;

    [Header("Arena")]
    public Transform bola;
    public Transform golEsquerda; 
    public Transform golDireita;  
    public GameObject prefabJogador; 

    [Header("Times")]
    public List<PlayerData> timeCasa; 
    public List<PlayerData> timeVisitante; 

    [Header("Spawns")]
    public Transform[] spawnsCasa;     
    public Transform[] spawnsVisitante; 

    private int placarCasa = 0;      
    private int placarVisitante = 0; 
    private Vector3 posicaoInicialBola;

    private List<FootballBrain> jogadoresCasa = new List<FootballBrain>();
    private List<FootballBrain> jogadoresVisitante = new List<FootballBrain>();

    void Awake() { Instance = this; }

    void Start() {
        if (bola != null) posicaoInicialBola = bola.position;
        IniciarPartida();
    }

    void IniciarPartida() {
        placarCasa = 0;
        placarVisitante = 0;
        jogoAcabou = false;
        tempoAtual = tempoDePartida;

        if(UIManager.Instance != null) {
            UIManager.Instance.AtualizarPlacar(0,0);
            if(UIManager.Instance.painelFimJogo) UIManager.Instance.painelFimJogo.SetActive(false);
        }

        jogadoresCasa.Clear();
        jogadoresVisitante.Clear();

        // --- MUDANÇA: Agora passamos a Tag correspondente a cada time ---
        // Casa: Nasce na esquerda, ataca a direita, defende a esquerda
        SpawnarTime(timeCasa, spawnsCasa, golDireita, golEsquerda, jogadoresCasa, "CASA"); 
        
        // Visitante: Nasce na direita, ataca a esquerda, defende a direita
        SpawnarTime(timeVisitante, spawnsVisitante, golEsquerda, golDireita, jogadoresVisitante, "VISITANTE"); 
        
        Debug.Log("APITA O ÁRBITRO! BOLA ROLANDO!");
    }

    void Update() {
        if (jogoAcabou) return;
        if (tempoAtual > 0) {
            tempoAtual -= Time.deltaTime;
            if(UIManager.Instance) UIManager.Instance.AtualizarTempo(tempoAtual);
        } else {
            ApitarFimDeJogo();
        }
    }

    void ApitarFimDeJogo() {
        jogoAcabou = true;
        tempoAtual = 0;
        string resultado = placarCasa > placarVisitante ? "CASA VENCEU!" : 
                           placarVisitante > placarCasa ? "VISITANTE VENCEU!" : "EMPATE!";
        if (UIManager.Instance) UIManager.Instance.MostrarFimDeJogo(resultado);
        
        if(bola) bola.GetComponent<Rigidbody>().isKinematic = true;
    }

    // --- MUDANÇA: A função agora aceita a tag do time como parâmetro ---
    void SpawnarTime(List<PlayerData> elenco, Transform[] posicoes, Transform ataque, Transform defesa, List<FootballBrain> listaInstancias, string tagDoTime) {
        
        for (int i = 0; i < elenco.Count; i++) {
            if (i >= posicoes.Length) break;

            GameObject p = Instantiate(prefabJogador, posicoes[i].position, posicoes[i].rotation);
            p.name = elenco[i].nomePersonagem;
            
            // --- AQUI ESTÁ A MÁGICA: Atribui a tag dinamicamente ---
            p.tag = tagDoTime; 
            
            FootballBrain brain = p.GetComponent<FootballBrain>();
            if (brain != null) {
                listaInstancias.Add(brain);
            }
        }

        for (int i = 0; i < listaInstancias.Count; i++) {
            listaInstancias[i].Initialize(elenco[i], bola, ataque, defesa, listaInstancias);
        }
    }

    public void RegistrarGol(string timeQueMarcou) {
        if (jogoAcabou) return; 

        if (timeQueMarcou == "Casa") placarCasa++;
        else placarVisitante++;

        if (UIManager.Instance != null) {
            UIManager.Instance.AtualizarPlacar(placarCasa, placarVisitante);
        }
        
        Rigidbody rb = bola.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        Invoke("ResetarCampo", 2.0f);
    }

    void ResetarCampo() {
        if (jogoAcabou) return;
        
        if (bola != null) {
            bola.position = posicaoInicialBola;
            Rigidbody rb = bola.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero; 
                rb.angularVelocity = Vector3.zero;
            }
        }

        ResetarPosicoesDoTime(jogadoresCasa, spawnsCasa);
        ResetarPosicoesDoTime(jogadoresVisitante, spawnsVisitante);
        
        Debug.Log("BOLA NO CENTRO, RECOMEÇA O JOGO!");
    }

    void ResetarPosicoesDoTime(List<FootballBrain> time, Transform[] spawns) {
        for (int i = 0; i < time.Count; i++) {
            if (i < spawns.Length && time[i] != null) {
                
                time[i].transform.position = spawns[i].position;
                time[i].transform.rotation = spawns[i].rotation;
                
                // O AIState agora será reconhecido globalmente
                time[i].estadoAtual = AIState.RETURN_POSITION;
                
                Rigidbody rb = time[i].GetComponent<Rigidbody>();
                if(rb) {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}