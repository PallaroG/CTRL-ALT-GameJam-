using UnityEngine;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour {

    public static MatchManager Instance; // Singleton para facilitar acesso global

    [Header("Arena")]
    public Transform bola;
    public Transform golEsquerda; 
    public Transform golDireita;  
    public GameObject prefabJogador; // O "BasePlayer" (Esfera com Sprite)

    [Header("Times")]
    public List<PlayerData> timeCasa; 
    public List<PlayerData> timeVisitante; 

    [Header("Spawns")]
    public Transform[] spawnsCasa;     
    public Transform[] spawnsVisitante; 

    // Placar Interno
    private int golsCasa = 0;
    private int golsVisitante = 0;
    private Vector3 posicaoInicialBola;

    void Awake() {
        Instance = this; // Garante que outros scripts achem esse aqui
    }

    void Start() {
        if (bola != null) posicaoInicialBola = bola.position;
        IniciarPartida();
    }

    void IniciarPartida() {
        // Time da Casa ataca para a Direita (Gol Direita é o alvo)
        SpawnarTime(timeCasa, spawnsCasa, golDireita, golEsquerda); 

        // Time Visitante ataca para a Esquerda (Gol Esquerda é o alvo)
        SpawnarTime(timeVisitante, spawnsVisitante, golEsquerda, golDireita); 
        
        Debug.Log("Partida Iniciada!");
    }

    void SpawnarTime(List<PlayerData> elenco, Transform[] posicoes, Transform ataque, Transform defesa) {
        for (int i = 0; i < elenco.Count; i++) {
            if (i >= posicoes.Length) break;

            // Cria o objeto
            GameObject p = Instantiate(prefabJogador, posicoes[i].position, Quaternion.identity);
            p.name = elenco[i].nomePersonagem;

            // Configura o Cérebro
            FootballBrain brain = p.GetComponent<FootballBrain>();
            if (brain) {
                brain.Initialize(elenco[i], bola, ataque, defesa);
            }
        }
    }

    // --- A FUNÇÃO QUE FALTAVA ---
    public void RegistrarGol(string timeQueMarcou) {
        if (timeQueMarcou == "Casa") {
            golsCasa++;
        } else {
            golsVisitante++;
        }

        Debug.Log($"<color=green>GOL DO {timeQueMarcou.ToUpper()}!</color> Placar: {golsCasa} x {golsVisitante}");

        // Reinicia a bola após 2 segundos
        Invoke("ResetarBola", 2.0f);
    }

    void ResetarBola() {
        if (bola == null) return;

        // Reseta posição
        bola.position = posicaoInicialBola;

        // Zera a física (para a bola não continuar rolando sozinha)
        Rigidbody rb = bola.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = Vector3.zero; // Unity 6
            rb.angularVelocity = Vector3.zero;
        }
    }
}