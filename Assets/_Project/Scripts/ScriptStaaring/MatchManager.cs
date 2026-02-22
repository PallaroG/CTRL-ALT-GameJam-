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
    
    [Header("Prefabs de Jogadores")]
    public GameObject prefabJogador; 
    public GameObject prefabGoleiro; 

    [Header("Times")]
    public List<PlayerData> timeCasa; 
    public List<PlayerData> timeVisitante; 

    [Header("Spawns & Táticas (Casa)")]
    public Transform[] spawnsCasa;  
    public Vector2[] coordenadasCasa; 

    [Header("Spawns & Táticas (Visitante)")]
    public Transform[] spawnsVisitante; 
    public Vector2[] coordenadasVisitante;

    private int placarCasa = 0;      
    private int placarVisitante = 0; 
    private Vector3 posicaoInicialBola;

    private List<FootballBrain> jogadoresCasa = new List<FootballBrain>();
    private List<FootballBrain> jogadoresVisitante = new List<FootballBrain>();
    private GoleiroBrain goleiroCasaObj;
    private GoleiroBrain goleiroVisitanteObj;

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

        SpawnarTime(timeCasa, spawnsCasa, coordenadasCasa, golDireita, golEsquerda, jogadoresCasa, "CASA"); 
        SpawnarTime(timeVisitante, spawnsVisitante, coordenadasVisitante, golEsquerda, golDireita, jogadoresVisitante, "VISITANTE"); 
    }

    void Update() {
        if (jogoAcabou) return;
        if (tempoAtual > 0) {
            tempoAtual -= Time.deltaTime;
            if(UIManager.Instance) UIManager.Instance.AtualizarTempo(tempoAtual);
            
            DefinirPapeisTaticos();
        } else {
            ApitarFimDeJogo();
        }
    }

    void DefinirPapeisTaticos() {
        FootballBrain ativoCasa = ObterJogadorMaisProximo(jogadoresCasa);
        FootballBrain ativoVisitante = ObterJogadorMaisProximo(jogadoresVisitante);

        foreach(var j in jogadoresCasa) { if(j != null) j.souOAtivo = (j == ativoCasa); }
        foreach(var j in jogadoresVisitante) { if(j != null) j.souOAtivo = (j == ativoVisitante); }
    }

    FootballBrain ObterJogadorMaisProximo(List<FootballBrain> time) {
        FootballBrain maisPerto = null;
        float menorDist = Mathf.Infinity;
        foreach(var j in time) {
            if (j == null) continue;
            if (j.estadoAtual == AIState.WITH_BALL) return j;

            float dist = Vector3.Distance(j.transform.position, bola.position);
            if (dist < menorDist) { 
                menorDist = dist; 
                maisPerto = j; 
            }
        }
        return maisPerto;
    }

    void ApitarFimDeJogo() {
        jogoAcabou = true;
        tempoAtual = 0;
        string resultado = placarCasa > placarVisitante ? "CASA VENCEU!" : 
                           placarVisitante > placarCasa ? "VISITANTE VENCEU!" : "EMPATE!";
        if (UIManager.Instance) UIManager.Instance.MostrarFimDeJogo(resultado);
        
        if(bola) bola.GetComponent<Rigidbody>().isKinematic = true;
    }

    void SpawnarTime(List<PlayerData> elenco, Transform[] posicoes, Vector2[] coordenadasTaticas, Transform ataque, Transform defesa, List<FootballBrain> listaInstancias, string tagDoTime) {
        for (int i = 0; i < elenco.Count; i++) {
            if (i >= posicoes.Length) break;

            float tX = (i < coordenadasTaticas.Length) ? coordenadasTaticas[i].x : 0.5f;
            float tZ = (i < coordenadasTaticas.Length) ? coordenadasTaticas[i].y : 0.5f;

            if (elenco[i].funcaoTatica == PosicaoTatica.Goleiro) {
                GameObject p = Instantiate(prefabGoleiro, posicoes[i].position, posicoes[i].rotation);
                p.name = elenco[i].nomePersonagem;
                p.tag = tagDoTime; 
                
                GoleiroBrain gb = p.GetComponent<GoleiroBrain>();
                if (gb != null) {
                    gb.Initialize(elenco[i], bola, defesa, ataque); 
                    
                    if (tagDoTime == "CASA") goleiroCasaObj = gb;
                    else goleiroVisitanteObj = gb;
                }
            } 
            else {
                GameObject p = Instantiate(prefabJogador, posicoes[i].position, posicoes[i].rotation);
                p.name = elenco[i].nomePersonagem;
                p.tag = tagDoTime; 
                
                FootballBrain brain = p.GetComponent<FootballBrain>();
                if (brain != null) {
                    brain.Initialize(elenco[i], bola, ataque, defesa, listaInstancias, tX, tZ);
                    listaInstancias.Add(brain); 
                }
            }
        }
    }

    public void RegistrarGol(string timeQueMarcou) {
        if (jogoAcabou) return; 
        if (timeQueMarcou == "Casa") placarCasa++;
        else placarVisitante++;

        if (UIManager.Instance != null) UIManager.Instance.AtualizarPlacar(placarCasa, placarVisitante);
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

        if (goleiroCasaObj != null) goleiroCasaObj.ResetarPosicao();
        if (goleiroVisitanteObj != null) goleiroVisitanteObj.ResetarPosicao();
    }

    void ResetarPosicoesDoTime(List<FootballBrain> time, Transform[] spawns) {
        int indexSpawn = 0;
        for (int i = 0; i < time.Count; i++) {
            if (time[i] != null) {
                
                if (indexSpawn < spawns.Length) {
                     time[i].transform.position = spawns[indexSpawn].position;
                     time[i].transform.rotation = spawns[indexSpawn].rotation;
                     indexSpawn++;
                }
                
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