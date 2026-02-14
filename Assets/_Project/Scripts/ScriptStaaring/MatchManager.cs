using UnityEngine;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour {

    [Header("Arena")]
    public Transform bola;
    public Transform golEsquerda; // Onde o Time B defende
    public Transform golDireita;  // Onde o Time A defende
    public GameObject prefabJogador;

    [Header("Times")]
    public List<PlayerData> timeCasa; // Jogam da Esquerda -> Direita
    public List<PlayerData> timeVisitante; // Jogam da Direita -> Esquerda

    [Header("Spawns")]
    public Transform[] spawnsCasa;     // Posições no lado esquerdo
    public Transform[] spawnsVisitante; // Posições no lado direito

    // Listas internas
    private List<PlayerController> todosJogadores = new List<PlayerController>();
    private Vector3 bolaPosicaoInicial;
    
    // Placar
    private int placarCasa = 0;
    private int placarVisitante = 0;

    void Start() {
        bolaPosicaoInicial = bola.position;
        IniciarPartida();
    }

    void IniciarPartida() {
        // Spawnar Time da Casa (Ataca Direita, Defende Esquerda)
        SpawnarTime(timeCasa, spawnsCasa, golDireita, golEsquerda);

        // Spawnar Time Visitante (Ataca Esquerda, Defende Direita)
        SpawnarTime(timeVisitante, spawnsVisitante, golEsquerda, golDireita);
    }

    void SpawnarTime(List<PlayerData> elenco, Transform[] posicoes, Transform ataque, Transform defesa) {
        for (int i = 0; i < elenco.Count; i++) {
            if (i >= posicoes.Length) break;

            GameObject p = Instantiate(prefabJogador, posicoes[i].position, Quaternion.identity);
            PlayerController ctrl = p.GetComponent<PlayerController>();
            
            // Injeta a inteligência: "Aquele é seu alvo, essa é sua casa"
            ctrl.Initialize(elenco[i], bola, ataque, defesa);
            
            todosJogadores.Add(ctrl);
        }
    }

    public void RegistrarGol(string quemMarcou) {
        Debug.Log("GOOOOL DO " + quemMarcou.ToUpper() + "!");
        
        if (quemMarcou == "Casa") placarCasa++;
        else placarVisitante++;

        Debug.Log($"PLACAR: Casa {placarCasa} x {placarVisitante} Visitante");

        // Reseta a rodada
        Invoke("ResetarRodada", 2.0f); // Espera 2 segundos e reseta
    }

    void ResetarRodada() {
        // 1. Bola volta pro meio e para
        bola.position = bolaPosicaoInicial;
        bola.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        bola.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // 2. Jogadores voltam pros seus lugares
        foreach (var jogador in todosJogadores) {
            jogador.ResetPosition();
        }
    }
}