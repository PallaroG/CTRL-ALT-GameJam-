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
    
    [Header("Áudio da Partida")]
    public AudioSource sfxSource;       
    public AudioSource torcidaSource;   
    public AudioClip somApito;
    public AudioClip somGol;
    public AudioClip somTorcida;
    [Tooltip("Distância da bola para o gol onde a torcida começa a gritar mais alto")]
    public float distanciaEmpolgacao = 30f; 
    private float volumeMinimoTorcida = 0.3f; 
    private bool comemorandoGol = false;

    [Header("Prefabs de Jogadores")]
    public GameObject prefabJogador; 
    public GameObject prefabGoleiro; 

    [Header("BANCO DE DADOS (NOVO)")]
    [Tooltip("Arraste TODOS os PlayerData possíveis de serem contratados no jogo aqui!")]
    public List<PlayerData> bancoDeJogadoresGeral;

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
        MontarTimeDaCasa(); // A MÁGICA ACONTECE AQUI ANTES DO JOGO INICIAR!
        IniciarPartida();
    }

    // LÓGICA DE INTEGRAÇÃO COM O GAME MANAGER
    void MontarTimeDaCasa() {
        if (GameManager.Instance == null) {
            Debug.LogWarning("<color=orange>AVISO:</color> GameManager não encontrado. Você deu Play direto na cena do campo?");
            return;
        }

        if (GameManager.Instance.personagensContratados.Count == 0) {
            Debug.LogWarning("<color=orange>AVISO:</color> A lista chegou vazia! Ninguém foi contratado no Menu.");
            return;
        }

        PlayerData goleiroFixo = null;
        if (timeCasa.Count > 0) {
            goleiroFixo = timeCasa[0]; 
        }

        timeCasa.Clear();
        if (goleiroFixo != null) timeCasa.Add(goleiroFixo);

        foreach (string idContratado in GameManager.Instance.personagensContratados) {
            string idLimpo = idContratado.Trim().ToLower();
            PlayerData jogadorEncontrado = bancoDeJogadoresGeral.Find(p => 
                p.nomePersonagem.Trim().ToLower() == idLimpo || 
                p.name.Trim().ToLower() == idLimpo
            );
            
            if (jogadorEncontrado != null) {
                timeCasa.Add(jogadorEncontrado);
            } else {
                Debug.LogError($"<color=red>ERRO:</color> O GameManager enviou '{idContratado}', mas não achei no Banco Geral!");
            }
        }

        // ==============================================================
        // A NOVA REGRA DOS GOLEIROS (Substituição Inteligente)
        // ==============================================================
        
        bool goleiroTitularDefinido = false;

        // 1. O primeiro goleiro contratado rouba a camisa 1 (Índice 0)
        for (int i = 1; i < timeCasa.Count; i++) {
            if (timeCasa[i] != null && timeCasa[i].funcaoTatica == PosicaoTatica.Goleiro) {
                if (!goleiroTitularDefinido) {
                    PlayerData temp = timeCasa[0];
                    timeCasa[0] = timeCasa[i];
                    timeCasa[i] = temp;
                    goleiroTitularDefinido = true;
                }
            }
        }

        // 2. Qualquer goleiro que ficou do Índice 1 em diante vai para a linha (versão _z)
        for (int i = 1; i < timeCasa.Count; i++) {
            if (timeCasa[i] != null && timeCasa[i].funcaoTatica == PosicaoTatica.Goleiro) {
                
                string nomeBuscaZ = timeCasa[i].nomePersonagem.Trim() + "_z";
                string nomeArquivoZ = timeCasa[i].name.Trim() + "_z";
                
                PlayerData versaoLinha = bancoDeJogadoresGeral.Find(p => 
                    p.nomePersonagem.Trim().ToLower() == nomeBuscaZ.ToLower() || 
                    p.name.Trim().ToLower() == nomeArquivoZ.ToLower()
                );
                
                if (versaoLinha != null) {
                    Debug.Log($"<color=magenta>[ADAPTAÇÃO DE GOLEIRO]</color> {timeCasa[i].nomePersonagem} perdeu a vaga e foi pro campo como {versaoLinha.nomePersonagem}!");
                    timeCasa[i] = versaoLinha;
                } else {
                    Debug.LogError($"<color=red>FALTOU O _Z:</color> O {timeCasa[i].nomePersonagem} é um goleiro extra, mas não achei a versão '{nomeBuscaZ}' no Banco Geral!");
                }
            }
        }
        // ==============================================================

        // PRINT PARA CONFERIR
        Debug.Log("<color=yellow>=== ESCALAÇÃO OFICIAL: TIME DA CASA ===</color>");
        for (int i = 0; i < timeCasa.Count; i++) {
            if (timeCasa[i] != null) {
                string tipo = timeCasa[i].funcaoTatica == PosicaoTatica.Goleiro ? "GOL" : "LINHA";
                Debug.Log($"Posição {i}: <color=cyan>{timeCasa[i].nomePersonagem}</color> ({tipo})");
            } else {
                Debug.Log($"<color=red>Posição {i}: VAZIO (Nenhum PlayerData encontrado)</color>");
            }
        }
        Debug.Log("<color=yellow>=========================================</color>");
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
        
        if (sfxSource != null && somApito != null) sfxSource.PlayOneShot(somApito);
        if (torcidaSource != null && somTorcida != null) {
            torcidaSource.clip = somTorcida;
            torcidaSource.loop = true;
            torcidaSource.volume = volumeMinimoTorcida;
            torcidaSource.Play();
        }
    }

    void Update() {
        if (jogoAcabou) return;
        if (tempoAtual > 0) {
            tempoAtual -= Time.deltaTime;
            if(UIManager.Instance) UIManager.Instance.AtualizarTempo(tempoAtual);
            
            DefinirPapeisTaticos();
            ControlarVolumeTorcida(); 
        } else {
            ApitarFimDeJogo();
        }
    }

    void ControlarVolumeTorcida() {
        if (comemorandoGol || bola == null || torcidaSource == null) return;

        float distCasa = Vector3.Distance(bola.position, golDireita.position);
        float distVis = Vector3.Distance(bola.position, golEsquerda.position);
        float menorDistancia = Mathf.Min(distCasa, distVis);

        if (menorDistancia < distanciaEmpolgacao) {
            float empolgacao = 1f - (menorDistancia / distanciaEmpolgacao);
            torcidaSource.volume = Mathf.Lerp(volumeMinimoTorcida, 0.8f, empolgacao); 
        } else {
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
        
        int resultadoFinal = 0; 
        if (placarCasa > placarVisitante) resultadoFinal = 1;
        else if (placarVisitante > placarCasa) resultadoFinal = 2;

        if (UIManager.Instance) UIManager.Instance.MostrarFimDeJogo(resultadoFinal);
        
        if(bola) bola.GetComponent<Rigidbody>().isKinematic = true;

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
        if (jogoAcabou || comemorandoGol) return; 
        
        comemorandoGol = true; 

        if (torcidaSource != null) torcidaSource.volume = 1.0f; 
        if (sfxSource != null && somGol != null) sfxSource.PlayOneShot(somGol);

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
        
        comemorandoGol = false; 

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