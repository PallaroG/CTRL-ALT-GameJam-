using UnityEngine;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour {

    [Header("Arena")]
    public Transform bola;
    public Transform golEsquerda; // Onde o Time Visitante ataca / Casa defende
    public Transform golDireita;  // Onde o Time Casa ataca / Visitante defende
    public GameObject prefabJogador;

    [Header("Times")]
    public List<PlayerData> timeCasa; 
    public List<PlayerData> timeVisitante; 

    [Header("Spawns")]
    public Transform[] spawnsCasa;     
    public Transform[] spawnsVisitante; 

    // Listas internas para gestão da partida
    private List<PlayerController> todosJogadores = new List<PlayerController>();
    private Vector3 bolaPosicaoInicial;
    
    // Placar
    private int placarCasa = 0;
    private int placarVisitante = 0;

    void Start() {
        if (bola != null) bolaPosicaoInicial = bola.position;
        IniciarPartida();
    }

    void IniciarPartida() {
        // Spawnar Time da Casa
        SpawnarTime(timeCasa, spawnsCasa, golDireita, golEsquerda, "Casa");

        // Spawnar Time Visitante
        SpawnarTime(timeVisitante, spawnsVisitante, golEsquerda, golDireita, "Visitante");
        
        Debug.Log("<color=green>PARTIDA INICIADA:</color> Todos os times foram devidamente apresentados.");
    }

    void SpawnarTime(List<PlayerData> elenco, Transform[] posicoes, Transform ataque, Transform defesa, string nomeTime) {
        
        List<PlayerController> timeAtual = new List<PlayerController>();

        // FASE 1: INSTANCIAÇÃO (Cria os corpos físicos primeiro)
        for (int i = 0; i < elenco.Count; i++) {
            if (i >= posicoes.Length) break;

            GameObject p = Instantiate(prefabJogador, posicoes[i].position, Quaternion.identity);
            PlayerController ctrl = p.GetComponent<PlayerController>();
            
            timeAtual.Add(ctrl);          
            todosJogadores.Add(ctrl);      
        }

        // FASE 2: CONSCIENTIZAÇÃO (Apresenta o time completo para cada cérebro de IA)
        // Isso garante que todos os aliados já existem no mundo antes da IA começar a pensar
        for (int i = 0; i < timeAtual.Count; i++) {
            timeAtual[i].Initialize(elenco[i], bola, ataque, defesa, timeAtual);
        }
        
        Debug.Log($"Time {nomeTime} spawnado com {timeAtual.Count} jogadores.");
    }

    public void RegistrarGol(string quemMarcou) {
        if (quemMarcou == "Casa") placarCasa++;
        else placarVisitante++;

        Debug.Log($"<color=cyan>GOOOOL DO {quemMarcou.ToUpper()}!</color>");
        Debug.Log($"<b>PLACAR:</b> Casa {placarCasa} x {placarVisitante} Visitante");

        Invoke("ResetarRodada", 2.0f); 
    }

    void ResetarRodada() {
        if (bola != null) {
            bola.position = bolaPosicaoInicial;
            Rigidbody ballRb = bola.GetComponent<Rigidbody>();
            if (ballRb != null) {
                ballRb.linearVelocity = Vector3.zero; // Unity 6
                ballRb.angularVelocity = Vector3.zero;
            }
        }

        foreach (var jogador in todosJogadores) {
            jogador.ResetPosition();
        }
    }
}