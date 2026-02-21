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
    public GameObject prefabGoleiro; // NOVO: Prefab isolado do Goleiro

    [Header("Times")]
    public List<PlayerData> timeCasa; 
    public List<PlayerData> timeVisitante; 

    [Header("Spawns")]
    public Transform[] spawnsCasa;     
    public Transform[] spawnsVisitante; 

    private int placarCasa = 0;      
    private int placarVisitante = 0; 
    private Vector3 posicaoInicialBola;

    // Listas exclusivas para jogadores de linha
    private List<FootballBrain> jogadoresCasa = new List<FootballBrain>();
    private List<FootballBrain> jogadoresVisitante = new List<FootballBrain>();

    // Variáveis para guardar os goleiros e poder resetá-los
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

        SpawnarTime(timeCasa, spawnsCasa, golDireita, golEsquerda, jogadoresCasa, "CASA"); 
        SpawnarTime(timeVisitante, spawnsVisitante, golEsquerda, golDireita, jogadoresVisitante, "VISITANTE"); 
        
        Debug.Log("APITA O ÁRBITRO! BOLA ROLANDO!");
    }

    void Update() {
        if (jogoAcabou) return;
        if (tempoAtual > 0) {
            tempoAtual -= Time.deltaTime;
            if(UIManager.Instance) UIManager.Instance.AtualizarTempo(tempoAtual);
            
            // Define o "Caçador" apensa entre os jogadores de linha
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

    void SpawnarTime(List<PlayerData> elenco, Transform[] posicoes, Transform ataque, Transform defesa, List<FootballBrain> listaInstancias, string tagDoTime) {
        for (int i = 0; i < elenco.Count; i++) {
            if (i >= posicoes.Length) break;

            // MÁGICA DE ARQUITETURA: Verifica se é Goleiro ou Linha antes de Instanciar
            if (elenco[i].funcaoTatica == PosicaoTatica.Goleiro) {
                GameObject p = Instantiate(prefabGoleiro, posicoes[i].position, posicoes[i].rotation);
                p.name = elenco[i].nomePersonagem;
                p.tag = tagDoTime; 
                
                GoleiroBrain gb = p.GetComponent<GoleiroBrain>();
                if (gb != null) {
                    // Nota: Para o goleiro, o 'gol de defesa' é onde ele fica!
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
                    listaInstancias.Add(brain); // Adiciona na lista SOMENTE jogadores de linha
                }
            }
        }

        // Inicializa os jogadores de linha
        for (int i = 0; i < listaInstancias.Count; i++) {
            listaInstancias[i].Initialize(elenco[i], bola, ataque, defesa, listaInstancias);
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

        // Reseta os jogadores de linha
        ResetarPosicoesDoTime(jogadoresCasa, spawnsCasa);
        ResetarPosicoesDoTime(jogadoresVisitante, spawnsVisitante);

        // Reseta os Goleiros
        if (goleiroCasaObj != null) goleiroCasaObj.ResetarPosicao();
        if (goleiroVisitanteObj != null) goleiroVisitanteObj.ResetarPosicao();
        
        Debug.Log("BOLA NO CENTRO, RECOMEÇA O JOGO!");
    }

    void ResetarPosicoesDoTime(List<FootballBrain> time, Transform[] spawns) {
        // Encontra o spawn correspondente (pulando o índice do goleiro que pode estar no array)
        int indexSpawn = 0;
        for (int i = 0; i < time.Count; i++) {
            if (time[i] != null) {
                
                // Pula os spawns que são do goleiro (se por acaso a lógica colocar ele no meio do array)
                // Uma forma simples é apenas usar a posição atual deles como base, mas como estamos limitados pelo MatchManager,
                // Garantimos que os jogadores de linha assumam os primeiros spawns disponíveis.
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