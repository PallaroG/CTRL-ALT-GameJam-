using UnityEngine;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour {

    public static MatchManager Instance; 

    [Header("Configuração da Partida")]
    public float tempoDePartida = 120f; // Mantive seus 120 segundos
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

        SpawnarTime(timeCasa, spawnsCasa, golDireita, golEsquerda); 
        SpawnarTime(timeVisitante, spawnsVisitante, golEsquerda, golDireita); 
        
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
        
        // Trava a bola
        if(bola) bola.GetComponent<Rigidbody>().isKinematic = true;
    }

    // --- SPAWN ATUALIZADO COM SISTEMA DE TIME ---
    void SpawnarTime(List<PlayerData> elenco, Transform[] posicoes, Transform ataque, Transform defesa) {
        List<FootballBrain> timeAtual = new List<FootballBrain>();

        // Fase 1: Cria todo mundo
        for (int i = 0; i < elenco.Count; i++) {
            if (i >= posicoes.Length) break;

            GameObject p = Instantiate(prefabJogador, posicoes[i].position, Quaternion.identity);
            p.name = elenco[i].nomePersonagem;
            
            FootballBrain brain = p.GetComponent<FootballBrain>();
            if (brain != null) {
                timeAtual.Add(brain);
            }
        }

        // Fase 2: Apresenta os amigos (Passa a lista 'timeAtual')
        for (int i = 0; i < timeAtual.Count; i++) {
            timeAtual[i].Initialize(elenco[i], bola, ataque, defesa, timeAtual);
        }
    }

    public void RegistrarGol(string timeQueMarcou) {
        if (jogoAcabou) return; 

        if (timeQueMarcou == "Casa") placarCasa++;
        else placarVisitante++;

        if (UIManager.Instance != null) {
            UIManager.Instance.AtualizarPlacar(placarCasa, placarVisitante);
        }
        Invoke("ResetarBola", 2.0f);
    }

    void ResetarBola() {
        if (bola == null || jogoAcabou) return;
        bola.position = posicaoInicialBola;
        Rigidbody rb = bola.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
        }
    }
}