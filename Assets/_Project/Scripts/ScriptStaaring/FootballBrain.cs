using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MemoriaJogador {
    public FootballBrain script;
    public string nome;
    public Vector3 ultimaPosicao;
    public float tempoDaLembranca;
}

public class FootballBrain : MonoBehaviour {

    [Header("Vínculos")]
    public PlayerData stats; 
    private ArcadeMotor motor; 
    public SpriteRenderer meuSprite; 
    
    [Header("Consciência Tática")]
    public Transform bola;
    public Transform golAtaque;
    public Transform golDefesa;
    public bool souOAtivo = false; // NOVO: Definido pelo MatchManager

    [Header("Sensores de IA")]
    public float raioDeDeteccao = 15.0f; 
    public float larguraDoSensor = 3.5f; 
    public float distanciaChuteArcade = 3.5f; 
    public float distanciaParaDriblar = 5.0f; 
    public float raioDePressao = 2.5f; 
    private float nextActionTime = 0f;

    [Header("Ajustes de Condução")]
    [Range(0.01f, 0.2f)] public float forcaMinima = 0.05f; 
    [Range(0.2f, 1.0f)] public float forcaMaxima = 0.4f;
    public float zonaDeFrenagem = 7.0f; 

    [Header("Malícia e Memória")]
    [Range(0, 100)] public float chanceDePassar = 100f; 
    public float distanciaBuscaCompanheiro = 30.0f; 
    public List<MemoriaJogador> mapaMentalDoTime = new List<MemoriaJogador>();
    private float nextScanTime = 0f;

    [Header("Contato Físico")]
    public float distanciaContatoFisico = 3.5f; 
    public float forcaDoEmpurrao = 50f;
    private float tempoAtordoado = 0f; 

    [Header("Debug")]
    public AIState estadoAtual;

    public void Initialize(PlayerData data, Transform _bolaReal, Transform _ataque, Transform _defesa, List<FootballBrain> _time) {
        stats = data;
        golAtaque = _ataque;
        golDefesa = _defesa;
        bola = _bolaReal; 
        motor = GetComponent<ArcadeMotor>();
        if (meuSprite != null && stats.fotoDoPersonagem != null) meuSprite.sprite = stats.fotoDoPersonagem;

        if (bola != null) {
            Collider[] meusColisores = GetComponentsInChildren<Collider>();
            Collider colisorDaBola = bola.GetComponentInChildren<Collider>();
            if (meusColisores != null && colisorDaBola != null) {
                foreach(var c in meusColisores) Physics.IgnoreCollision(c, colisorDaBola, true);
            }
        }
    }

    void Update() {
        if (Time.time < tempoAtordoado) {
            estadoAtual = AIState.TACKLE;
            return; 
        }

        if (bola == null) return;
        float distBola = Vector3.Distance(transform.position, bola.position);
        
        if (Time.time >= nextScanTime) {
            AtualizarMapaMental();
            nextScanTime = Time.time + 1.0f;
        }

        if (distBola <= distanciaChuteArcade) {
            estadoAtual = AIState.WITH_BALL;
            motor.SetMovement(transform.position, Vector3.zero, 0); 
            if (Time.time >= nextActionTime) ProcessarDecisaoDeCraque();
        } else {
            ProcessarInteligenciaSemBola(distBola);
        }
        motor.LookAtTarget(bola.position);
    }

    public void ReceberEmpurrao(Vector3 forca) {
        tempoAtordoado = Time.time + 0.8f; 
        motor.Stop(); 
        Rigidbody meuRb = GetComponent<Rigidbody>();
        if (meuRb != null) {
            meuRb.linearVelocity = Vector3.zero; 
            meuRb.AddForce(forca, ForceMode.Impulse); 
        }
    }

    // ==========================================
    // LÓGICA DE DEFESA / SEM BOLA (A COLEIRA TÁTICA)
    // ==========================================
    void ProcessarInteligenciaSemBola(float distBola) {
        
        // SE EU NÃO SOU O JOGADOR DESIGNADO PARA CAÇAR A BOLA...
        if (!souOAtivo) {
            if (VerificarPosseDeBolaAliada()) {
                // Desmarcação de Apoio (Mantida)
                estadoAtual = AIState.SUPPORT;
                Vector3 direcaoParaGol = (golAtaque.position - bola.position).normalized;
                Vector3 direcaoParaMim = (transform.position - bola.position).normalized;
                Vector3 perpendicular = Vector3.Cross(Vector3.up, direcaoParaGol).normalized;
                
                float lado = Vector3.Dot(perpendicular, direcaoParaMim);
                Vector3 direcaoAbertura = (lado > 0) ? perpendicular : -perpendicular;

                Vector3 pontoDeSuporte = bola.position + (direcaoParaGol * 12.0f) + (direcaoAbertura * 8.0f);
                pontoDeSuporte.y = transform.position.y;

                Debug.DrawLine(transform.position, pontoDeSuporte, Color.green);
                float distProSuporte = Vector3.Distance(transform.position, pontoDeSuporte);
                motor.SetMovement(pontoDeSuporte, Vector3.zero, stats.maxSpeed * Mathf.Max(Mathf.Clamp01(distProSuporte / zonaDeFrenagem), 0.4f));
            } else {
                // A COLEIRA: Volto para a minha posição tática base!
                estadoAtual = AIState.RETURN_POSITION;
                Vector3 posicaoBase = CalcularPosicaoTaticaBase();
                
                // Desenha linha amarela na Scene para veres o jogador "preso" à posição
                Debug.DrawLine(transform.position, posicaoBase, Color.yellow);
                
                float distPraBase = Vector3.Distance(transform.position, posicaoBase);
                if (distPraBase > 2.0f) {
                    motor.SetMovement(posicaoBase, Vector3.zero, stats.maxSpeed * Mathf.Max(Mathf.Clamp01(distPraBase / zonaDeFrenagem), 0.4f));
                } else {
                    motor.Stop(); // Chegou na posição, fica a olhar o jogo
                }
            }
            return; // Interrompe aqui. Quem não é ativo não dá bote nem intercepta!
        }

        // --- AS LÓGICAS ABAIXO SÓ RODAM SE EU FOR O "SOU O ATIVO" (O MAIS PRÓXIMO) ---

        Rigidbody rbBola = bola.GetComponentInParent<Rigidbody>();
        Vector3 velBola = rbBola != null ? rbBola.linearVelocity : Vector3.zero;

        if (velBola.magnitude > 5.0f) {
            estadoAtual = AIState.CHASE_BALL;
            Vector3 pontoFuturo = bola.position + (velBola * 0.5f);
            pontoFuturo.y = transform.position.y; 
            Debug.DrawLine(bola.position, pontoFuturo, Color.cyan); 
            motor.SetMovement(pontoFuturo, Vector3.zero, stats.maxSpeed);
            return;
        }

        if (VerificarPosseDeBolaInimiga()) {
            estadoAtual = AIState.TACKLE;
            Vector3 direcaoParaMeuGol = (golDefesa.position - bola.position).normalized;
            Vector3 pontoDeBloqueio = bola.position + (direcaoParaMeuGol * 3.0f);
            Debug.DrawLine(bola.position, pontoDeBloqueio, Color.red); 

            float distProBloqueio = Vector3.Distance(transform.position, pontoDeBloqueio);
            motor.SetMovement(pontoDeBloqueio, Vector3.zero, stats.maxSpeed * Mathf.Max(Mathf.Clamp01(distProBloqueio / zonaDeFrenagem), 0.4f));
            return;
        }

        estadoAtual = AIState.CHASE_BALL;
        motor.SetMovement(bola.position, Vector3.zero, stats.maxSpeed * Mathf.Max(Mathf.Clamp01(distBola / zonaDeFrenagem), 0.35f));
    }

    // O CÁLCULO DA POSIÇÃO NO CAMPO BASEADO NO PLAYERDATA
    Vector3 CalcularPosicaoTaticaBase() {
        // Eixo da Profundidade (Zagueiro ou Atacante?)
        Vector3 profundidadeTatica = Vector3.Lerp(golDefesa.position, golAtaque.position, stats.taticaX);
        
        // Eixo Lateral (Esquerda ou Direita?)
        Vector3 direcaoGol = (golAtaque.position - golDefesa.position).normalized;
        Vector3 perpendicular = Vector3.Cross(Vector3.up, direcaoGol).normalized;
        
        // Assumindo um campo de +- 15 metros de largura do centro. Ajusta se o teu campo for maior!
        float largura = Mathf.Lerp(-15f, 15f, stats.taticaZ);
        Vector3 larguraTatica = perpendicular * largura;

        Vector3 baseFixa = profundidadeTatica + larguraTatica;
        
        // A formação desloca-se 30% em direção à bola para a equipa "bascular" em bloco
        Vector3 posicaoBolaLimitada = bola.position;
        posicaoBolaLimitada.y = transform.position.y;
        Vector3 baseFinal = Vector3.Lerp(baseFixa, posicaoBolaLimitada, 0.3f); 

        return baseFinal;
    }

    bool VerificarPosseDeBolaInimiga() {
        string tagInimiga = (this.CompareTag("CASA")) ? "VISITANTE" : "CASA";
        Collider[] cols = Physics.OverlapSphere(bola.position, distanciaChuteArcade + 1.0f);
        foreach(var c in cols) {
            if (c.attachedRigidbody != null && c.attachedRigidbody.CompareTag(tagInimiga)) return true;
        }
        return false;
    }

    bool VerificarPosseDeBolaAliada() {
        Collider[] cols = Physics.OverlapSphere(bola.position, distanciaChuteArcade + 1.0f);
        foreach(var c in cols) {
            if (c.attachedRigidbody != null && c.attachedRigidbody.CompareTag(this.tag) && c.attachedRigidbody.gameObject != this.gameObject) {
                return true; 
            }
        }
        return false;
    }

    // ==========================================
    // MAPA MENTAL
    // ==========================================
    void AtualizarMapaMental() {
        Collider[] encontrados = Physics.OverlapSphere(transform.position, distanciaBuscaCompanheiro);
        foreach (var col in encontrados) {
            if (col.attachedRigidbody == null) continue;
            GameObject donoDoCorpo = col.attachedRigidbody.gameObject;

            if (donoDoCorpo.CompareTag(this.tag)) {
                FootballBrain comp = donoDoCorpo.GetComponent<FootballBrain>();
                if (comp != null && comp != this) GravarNaMemoria(comp);
            }
        }
    }

    void GravarNaMemoria(FootballBrain comp) {
        MemoriaJogador mem = mapaMentalDoTime.Find(m => m.script == comp);
        if (mem == null) {
            mem = new MemoriaJogador { script = comp, nome = comp.stats.nomePersonagem };
            mapaMentalDoTime.Add(mem);
        }
        mem.ultimaPosicao = comp.transform.position;
        mem.tempoDaLembranca = Time.time;
    }

    // ==========================================
    // CASCATA DE DECISÃO (COM A BOLA)
    // ==========================================
    void ProcessarDecisaoDeCraque() {
        Vector3 direcaoGol = (golAtaque.position - transform.position).normalized;
        string tagInimiga = (this.CompareTag("CASA")) ? "VISITANTE" : "CASA";
        
        Collider[] pressaoColada = Physics.OverlapSphere(transform.position, distanciaContatoFisico);
        bool sofrendoPressao = false;

        foreach (var col in pressaoColada) {
            if (col.attachedRigidbody != null && col.attachedRigidbody.CompareTag(tagInimiga)) {
                FootballBrain inimigoBrain = col.attachedRigidbody.GetComponent<FootballBrain>();
                if (inimigoBrain != null) {
                    Vector3 direcaoEmpurrao = (col.attachedRigidbody.position - transform.position).normalized;
                    direcaoEmpurrao.y = 0.2f; 
                    inimigoBrain.ReceberEmpurrao(direcaoEmpurrao * forcaDoEmpurrao);
                    sofrendoPressao = true;
                }
            }
        }

        if (sofrendoPressao) {
            Transform alvoPasse = ProcurarMelhorOpcaoDePasse();
            if (alvoPasse != null && Random.Range(0, 100) < chanceDePassar) {
                ExecutarPasse(alvoPasse);
            } else {
                Vector3 direcaoFuga = (direcaoGol + transform.forward).normalized;
                AplicarForcaNaBola(direcaoFuga, stats.kickPower, "Sufocado! Bica!", "magenta");
                nextActionTime = Time.time + 0.8f;
            }
            return; 
        }

        Transform opcaoPasse = ProcurarMelhorOpcaoDePasse();
        if (opcaoPasse != null && Random.Range(0, 100) < chanceDePassar) {
            ExecutarPasse(opcaoPasse);
            return; 
        }

        RaycastHit hit;
        Vector3 origemRaio = transform.position + (direcaoGol * 0.5f) + Vector3.up;

        if (Physics.SphereCast(origemRaio, larguraDoSensor, direcaoGol, out hit, raioDeDeteccao)) {
            if (hit.collider != null && hit.collider.attachedRigidbody != null && hit.collider.attachedRigidbody.CompareTag(tagInimiga)) {
                if (hit.distance <= distanciaParaDriblar) {
                    RealizarDribleInteligente(direcaoGol, hit.collider.transform.position);
                } else {
                    ConduzirControlado(direcaoGol, hit.distance);
                }
                return; 
            }
        }

        ExecutarChuteDireto(); 
    }

    Transform ProcurarMelhorOpcaoDePasse() {
        Transform melhorAlvo = null;
        float menorDistProGol = Vector3.Distance(transform.position, golAtaque.position);

        foreach (var mem in mapaMentalDoTime) {
            if (Time.time - mem.tempoDaLembranca > 3.0f) continue;
            float distAmigoProGol = Vector3.Distance(mem.ultimaPosicao, golAtaque.position);

            if (distAmigoProGol < menorDistProGol) {
                melhorAlvo = mem.script.transform;
                menorDistProGol = distAmigoProGol;
            }
        }
        return melhorAlvo;
    }

    void RealizarDribleInteligente(Vector3 direcaoGol, Vector3 posicaoInimigo) {
        Vector3 direcaoParaInimigo = (posicaoInimigo - transform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(Vector3.up, direcaoGol).normalized;
        float lado = Vector3.Dot(perpendicular, direcaoParaInimigo);
        Vector3 direcaoDrible = (lado > 0) ? -perpendicular : perpendicular;
        
        Vector3 vetorFinal = (direcaoDrible + direcaoGol * 0.3f).normalized;
        AplicarForcaNaBola(vetorFinal, stats.kickPower * 0.45f, "Corte Malicioso Frontal!", "magenta");
        nextActionTime = Time.time + 0.8f;
    }

    void ConduzirControlado(Vector3 direcao, float distInimigo) {
        float t = (distInimigo - distanciaParaDriblar) / (raioDeDeteccao - distanciaParaDriblar);
        float fatorFinal = Mathf.Lerp(forcaMinima, forcaMaxima, Mathf.Clamp01(t));
        AplicarForcaNaBola(direcao, stats.kickPower * fatorFinal, $"Conduzindo ({fatorFinal * 100:F0}%)", "cyan");
        nextActionTime = Time.time + 0.45f; 
    }

    void ExecutarPasse(Transform alvo) {
        Vector3 direcaoPasse = (alvo.position - bola.position).normalized;
        AplicarForcaNaBola(direcaoPasse, stats.kickPower * 0.7f, $"Passou a bola para {alvo.name}!", "yellow");
        nextActionTime = Time.time + 1.2f;
    }

    void ExecutarChuteDireto() {
        Vector3 direcao = (golAtaque.position - bola.position).normalized;
        AplicarForcaNaBola(direcao, stats.kickPower, "Bomba pro gol!", "red");
        nextActionTime = Time.time + 1.2f;
    }

    void AplicarForcaNaBola(Vector3 direcao, float forca, string logMsg = "", string logCor = "white") {
        if (!string.IsNullOrEmpty(logMsg)) Debug.Log($"<color={logCor}>[IA {stats.nomePersonagem}]</color> {logMsg}");
        Rigidbody rbBola = bola.GetComponentInParent<Rigidbody>();
        if (rbBola != null) {
            rbBola.linearVelocity = Vector3.zero;
            rbBola.angularVelocity = Vector3.zero;
            direcao.y = 0.12f; 
            rbBola.AddForce(direcao * forca, ForceMode.Impulse);
        }
    }
}