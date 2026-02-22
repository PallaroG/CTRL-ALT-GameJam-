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
    
    [Header("Áudio da Partida (NOVO)")]
    public AudioSource sfxSource;       // Toca efeitos rápidos (Apito, Gol)
    public AudioSource torcidaSource;   // Toca o som ambiente (Torcida em loop)
    public AudioClip somApito;
    public AudioClip somGol;
    public AudioClip somTorcida;
    [Tooltip("Distância da bola para o gol onde a torcida começa a gritar mais alto")]
    public float distanciaEmpolgacao = 30f; 
    private float volumeMinimoTorcida = 0.3f; // Volume base quando a bola tá no meio campo
    private bool comemorandoGol = false;

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
        comemorandoGol = false;
        tempoAtual = tempoDePartida;

        if(UIManager.Instance != null) {
            UIManager.Instance.AtualizarPlacar(0,0);
            if(UIManager.Instance.painelFimJogo) UIManager.Instance.painelFimJogo.SetActive(false);
        }

        jogadoresCasa.Clear();
        jogadoresVisitante.Clear();

        SpawnarTime(timeCasa, spawnsCasa, coordenadasCasa, golDireita, golEsquerda, jogadoresCasa, "CASA"); 
        SpawnarTime(timeVisitante, spawnsVisitante, coordenadasVisitante, golEsquerda, golDireita, jogadoresVisitante, "VISITANTE"); 
        
        // APITO INICIAL E LIGA A TORCIDA
        if (sfxSource != null && somApito != null) sfxSource.PlayOneShot(somApito);
        if (torcidaSource != null && somTorcida != null) {
            torcidaSource.clip = somTorcida;
            torcidaSource.loop = true;
            torcidaSource.volume = volumeMinimoTorcida;
            torcidaSource.Play();
        }
        
        Debug.Log("APITA O ÁRBITRO! BOLA ROLANDO!");
    }

    void Update() {
        if (jogoAcabou) return;
        if (tempoAtual > 0) {
            tempoAtual -= Time.deltaTime;
            if(UIManager.Instance) UIManager.Instance.AtualizarTempo(tempoAtual);
            
            DefinirPapeisTaticos();
            ControlarVolumeTorcida(); // Chama a emoção da torcida!
        } else {
            ApitarFimDeJogo();
        }
    }

    // A MÁGICA DA TORCIDA DINÂMICA
    void ControlarVolumeTorcida() {
        if (comemorandoGol || bola == null || torcidaSource == null) return;

        // Mede a distância da bola para os dois gols e pega a menor
        float distCasa = Vector3.Distance(bola.position, golDireita.position);
        float distVis = Vector3.Distance(bola.position, golEsquerda.position);
        float menorDistancia = Mathf.Min(distCasa, distVis);

        if (menorDistancia < distanciaEmpolgacao) {
            // Quanto mais perto de zero (dentro do gol), mais o multiplicador chega perto de 1
            float empolgacao = 1f - (menorDistancia / distanciaEmpolgacao);
            // Vai do volume mínimo (0.3) até quase o máximo (0.8) durante a jogada
            torcidaSource.volume = Mathf.Lerp(volumeMinimoTorcida, 0.8f, empolgacao); 
        } else {
            // Bola longe, torcida calma
            torcidaSource.volume = volumeMinimoTorcida;
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
        
        // Define quem ganhou: 0 = Empate, 1 = Casa, 2 = Visitante
        int resultadoFinal = 0; 
        if (placarCasa > placarVisitante) resultadoFinal = 1;
        else if (placarVisitante > placarCasa) resultadoFinal = 2;

        if (UIManager.Instance) UIManager.Instance.MostrarFimDeJogo(resultadoFinal);
        
        if(bola) bola.GetComponent<Rigidbody>().isKinematic = true;

        // APITO FINAL
        if (sfxSource != null && somApito != null) sfxSource.PlayOneShot(somApito);
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
        // O CADEADO: Se o jogo acabou ou se já estamos comemorando, ignora os pontos fantasmas!
        if (jogoAcabou || comemorandoGol) return; 
        
        comemorandoGol = true; // Trava a dinâmica e deixa a torcida louca!

        // GRITO DE GOL E TORCIDA NO MÁXIMO
        if (torcidaSource != null) torcidaSource.volume = 1.0f; 
        if (sfxSource != null && somGol != null) sfxSource.PlayOneShot(somGol);

        // Limpa espaços em branco e deixa minúsculo para evitar erros de digitação no Inspector
        string timeFormatado = timeQueMarcou.Trim().ToLower();

        if (timeFormatado == "casa") placarCasa++;
        else placarVisitante++;

        if (UIManager.Instance != null) UIManager.Instance.AtualizarPlacar(placarCasa, placarVisitante);
        Rigidbody rb = bola.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        Invoke("ResetarCampo", 2.0f);
    }

    void ResetarCampo() {
        if (jogoAcabou) return;
        
        comemorandoGol = false; // A bola volta pro meio, a torcida acalma

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
        
        // APITO DE RECOMEÇO
        if (sfxSource != null && somApito != null) sfxSource.PlayOneShot(somApito);
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