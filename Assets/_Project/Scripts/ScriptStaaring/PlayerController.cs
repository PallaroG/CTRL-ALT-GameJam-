using UnityEngine;
using System.Collections.Generic;

public enum PlayerState { IDLE, CHASE, ADJUSTING, DRIBBLE, KICK, PASS, RETURN, DEFEND, GOALKEEPER, SUPPORT }

public class PlayerController : MonoBehaviour {

    [Header("Visual & Debug")]
    public SpriteRenderer meuSpriteRenderer;
    public bool mostrarDebug = true;

    [Header("Configuração")]
    public PlayerData data;

    // Status Físicos
    private float currentMaxSpeed;
    private float currentAccel;
    private float currentKick;

    // Alvos
    private Transform ballTarget;
    private Transform myAttackGoal;
    private Transform myDefenseGoal;
    private Vector3 initialPos;
    private List<PlayerController> meusCompanheiros;
    
    // VARIÁVEL DE PASSE
    private Transform alvoDoPasse; // Quem vai receber a bola?
    private float passCooldownTimer = 0f; // <--- NOVO: Evita spam de passes

    private Rigidbody rb;
    private Rigidbody ballRb;
    
    public PlayerState currentState = PlayerState.IDLE;
    private float nextActionTime = 0f;

    // Controles Finos
    private float alcanceDoChute = 1.8f;
    private float anguloParaChute = 45f;

    public void Initialize(PlayerData stats, Transform ball, Transform attack, Transform defense, List<PlayerController> time) {
        data = stats;
        ballTarget = ball;
        myAttackGoal = attack;
        myDefenseGoal = defense;
        initialPos = transform.position;

        currentMaxSpeed = data.maxSpeed;
        currentAccel = data.aceleracao;
        currentKick = data.kickPower;

        rb = GetComponent<Rigidbody>();
        rb.mass = data.mass;
        rb.linearDamping = data.controleCorporal; 
        
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        GetComponent<Renderer>().material.color = data.corRepresentativa;
        
        if (ballTarget) ballRb = ballTarget.GetComponent<Rigidbody>();
        gameObject.name = data.nomePersonagem;

        if (meuSpriteRenderer != null && data.fotoDoPersonagem != null) {
            meuSpriteRenderer.sprite = data.fotoDoPersonagem;
        }

        meusCompanheiros = new List<PlayerController>(time);
        meusCompanheiros.Remove(this);

        if (data.funcaoTatica == PosicaoTatica.Goleiro) currentState = PlayerState.GOALKEEPER;
    }

    public void ResetPosition() {
        transform.position = initialPos;
        rb.linearVelocity = Vector3.zero;
        alvoDoPasse = null; 
        if (data.funcaoTatica == PosicaoTatica.Goleiro) currentState = PlayerState.GOALKEEPER;
        else currentState = PlayerState.IDLE;
    }

    void FixedUpdate() {
        if (!ballTarget) return;
        
        DecideStrategy();
        ExecuteState();
        VisualRotation();
        
        // Evita que jogadores fiquem grudados
        EvitarAglomeracao();
    }

    // --- CÉREBRO ---
    void DecideStrategy() {
        if (data.funcaoTatica == PosicaoTatica.Goleiro) { currentState = PlayerState.GOALKEEPER; return; }

        float distBall = Vector3.Distance(transform.position, ballTarget.position);
        
        // 1. ESTOU COM A BOLA? (Prioridade Absoluta)
        if (distBall <= alcanceDoChute) {
            if (EstouAlinhadoComAlvo(myAttackGoal.position)) {
                if (Vector3.Distance(transform.position, myAttackGoal.position) < 20f) {
                    currentState = PlayerState.KICK;
                } 
                else {
                    // Só procura passe se o cooldown permitar
                    if (Time.time > passCooldownTimer && ProcurarPasse()) {
                        currentState = PlayerState.PASS; 
                    } else {
                        currentState = PlayerState.DRIBBLE;
                    }
                }
            } else {
                currentState = PlayerState.ADJUSTING;
            }
            return;
        }

        // --- DEFESA ---
        float ballToDefense = Vector3.Distance(ballTarget.position, myDefenseGoal.position);
        float meToDefense = Vector3.Distance(transform.position, myDefenseGoal.position);

        if (ballToDefense < 30f && ballToDefense < meToDefense) {
            currentState = PlayerState.DEFEND;
            return;
        }

        // --- ANTI-ENXAME (Hierarquia) ---
        // Se tem alguém mais perto, eu dou suporte
        bool temAmigoNaFrente = false;
        foreach(var amigo in meusCompanheiros) {
            if(amigo.currentState == PlayerState.GOALKEEPER) continue;

            float distAmigo = Vector3.Distance(amigo.transform.position, ballTarget.position);
            
            // Se o amigo tem vantagem clara (1m mais perto), deixa ele ir
            if(distAmigo < distBall - 1.0f) {
                temAmigoNaFrente = true;
                break;
            }
        }

        if (temAmigoNaFrente) {
            currentState = PlayerState.SUPPORT; 
            return;
        }

        currentState = PlayerState.CHASE;
    }

    // --- INTELIGÊNCIA DE PASSE (RAYCAST) ---
    bool ProcurarPasse() {
        float melhorNota = -1f;
        Transform melhorCandidato = null;

        foreach (var amigo in meusCompanheiros) {
            float dist = Vector3.Distance(transform.position, amigo.transform.position);

            if (dist > data.visaoDeJogo) continue;

            // TRAVA DE DISTÂNCIA: Evita tocar pra quem tá do lado (menos de 5m)
            if (dist < 5.0f) continue;

            float minhaDistGol = Vector3.Distance(transform.position, myAttackGoal.position);
            float amigoDistGol = Vector3.Distance(amigo.transform.position, myAttackGoal.position);
            
            float nota = (minhaDistGol - amigoDistGol); 
            if (nota < 0) nota /= 2; 

            Vector3 direcaoPasse = (amigo.transform.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direcaoPasse, out hit, dist)) {
                PlayerController obstaculo = hit.collider.GetComponent<PlayerController>();
                if (obstaculo != null && !meusCompanheiros.Contains(obstaculo) && obstaculo != this) {
                    continue; 
                }
            }

            if (nota > melhorNota) {
                melhorNota = nota;
                melhorCandidato = amigo.transform;
            }
        }

        if (melhorCandidato != null) {
            alvoDoPasse = melhorCandidato;
            return true; 
        }
        
        return false; 
    }

    void ExecuteState() {
        Vector3 targetPos;

        switch (currentState) {
            case PlayerState.IDLE: break;
            
            case PlayerState.CHASE:
                targetPos = GetPositionBehindBall(myAttackGoal.position); 
                MoveTo(targetPos, 1.0f); 
                break;

            case PlayerState.ADJUSTING:
                Vector3 alvoAlinhamento = (currentState == PlayerState.PASS) ? alvoDoPasse.position : myAttackGoal.position;
                targetPos = GetPositionBehindBall(alvoAlinhamento);
                MoveTo(targetPos, 1.0f);
                
                if (Vector3.Distance(transform.position, ballTarget.position) < alcanceDoChute * 0.8f) {
                    Vector3 afastar = (transform.position - ballTarget.position).normalized;
                    rb.AddForce(afastar * 30f, ForceMode.Acceleration);
                }
                break;

            case PlayerState.PASS: 
                rb.linearVelocity = Vector3.zero; 
                // Verifica cooldown antes de executar
                if (IsBallClose() && Time.time > nextActionTime && alvoDoPasse != null) {
                    RealizarPasse();
                    nextActionTime = Time.time + 1.0f;
                    currentState = PlayerState.IDLE;
                } else {
                    currentState = PlayerState.IDLE;
                }
                break;
                
            case PlayerState.DRIBBLE:
                MoveTo(ballTarget.position, 1.0f); 
                if(IsBallClose()) {
                    Vector3 dir = (myAttackGoal.position - ballTarget.position).normalized;
                    ballRb.AddForce(dir * (currentKick * 0.15f), ForceMode.Impulse);
                }
                break;

            case PlayerState.KICK:
                rb.linearVelocity = Vector3.zero;
                if (IsBallClose() && Time.time > nextActionTime) {
                    ChutarProGolComErro(); 
                    nextActionTime = Time.time + 1.2f; 
                    currentState = PlayerState.IDLE;
                }
                break;

            case PlayerState.DEFEND:
                Vector3 blockPoint = (myDefenseGoal.position + ballTarget.position) / 2;
                MoveTo(blockPoint, 1.1f);
                break;
            
            case PlayerState.SUPPORT:
                // Fica a 8 metros da bola (aumentei para abrir o jogo)
                Vector3 pontoApoio = ballTarget.position + (initialPos - ballTarget.position).normalized * 8.0f;
                MoveTo(pontoApoio, 0.9f); 
                break;

            case PlayerState.RETURN:
                MoveTo(initialPos, 0.7f); 
                break;

            case PlayerState.GOALKEEPER:
                LogicaDeGoleiro();
                break;
        }
    }

    void EvitarAglomeracao() {
        foreach (var amigo in meusCompanheiros) {
            float dist = Vector3.Distance(transform.position, amigo.transform.position);
            
            if (dist < 2.0f) {
                Vector3 empurrao = (transform.position - amigo.transform.position).normalized;
                rb.AddForce(empurrao * 15f, ForceMode.Acceleration);
            }
        }
    }

    // --- FUNÇÕES AUXILIARES ---

    Vector3 GetPositionBehindBall(Vector3 alvoFinal) {
        Vector3 dirAlvoPraBola = (ballTarget.position - alvoFinal).normalized;
        return ballTarget.position + (dirAlvoPraBola * 1.2f);
    }

    bool EstouAlinhadoComAlvo(Vector3 alvoPos) {
        Vector3 vetorEuBola = (ballTarget.position - transform.position).normalized;
        Vector3 vetorEuAlvo = (alvoPos - transform.position).normalized;
        float anguloAlinhamento = Vector3.Angle(vetorEuBola, vetorEuAlvo);
        return anguloAlinhamento < anguloParaChute;
    }

    void RealizarPasse() {
        if (alvoDoPasse == null) return;
        if (Time.time < passCooldownTimer) return; // Trava extra de segurança

        Vector3 direcao = (alvoDoPasse.position - ballTarget.position).normalized;
        float forcaDoPasse = currentKick * 0.6f;
        
        float erro = Random.Range(-5f, 5f) * (1.0f - (data.precisao / 100f));
        direcao = Quaternion.Euler(0, erro, 0) * direcao;

        ballRb.AddForce(direcao * forcaDoPasse, ForceMode.Impulse);
        
        passCooldownTimer = Time.time + 1.0f; // Define cooldown de 1 segundo
        Debug.Log($"<color=cyan>PASSE:</color> de {name} para {alvoDoPasse.name}");
    }
    
    bool IsBallClose() {
        return Vector3.Distance(transform.position, ballTarget.position) <= alcanceDoChute;
    }

    void MoveTo(Vector3 target, float effort) {
        Vector3 direction = (target - transform.position).normalized;
        if(mostrarDebug) Debug.DrawLine(transform.position, target, Color.green);
        rb.AddForce(direction * (currentAccel * effort), ForceMode.Acceleration);
        if (rb.linearVelocity.magnitude > currentMaxSpeed) {
            rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
        }
    }

    void VisualRotation() {
        if (rb.linearVelocity.magnitude > 0.1f) {
            Vector3 dir = rb.linearVelocity.normalized;
            dir.y = 0;
            if (dir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.fixedDeltaTime * 15f);
        }
    }

    void LogicaDeGoleiro() {
        Vector3 direcaoBola = (ballTarget.position - myDefenseGoal.position).normalized;
        float dist = Mathf.Clamp(Vector3.Distance(ballTarget.position, myDefenseGoal.position), 0, 5f);
        Vector3 target = myDefenseGoal.position + (direcaoBola * dist);
        MoveTo(target, 1.0f);
        if (IsBallClose() && Time.time > nextActionTime) {
            Vector3 chutao = (myAttackGoal.position - transform.position).normalized + Vector3.up;
            ballRb.AddForce(chutao * (currentKick * 0.8f), ForceMode.Impulse);
            nextActionTime = Time.time + 1.0f;
        }
    }

    void ChutarProGolComErro() {
        Vector3 direcaoPerfeita = (myAttackGoal.position - ballTarget.position).normalized;
        float erroMaximo = 45f * (1.0f - (data.precisao / 100f)); 
        float desvio = Random.Range(-erroMaximo, erroMaximo);
        Quaternion rot = Quaternion.Euler(0, desvio, 0);
        Vector3 finalDir = rot * direcaoPerfeita;
        finalDir += Vector3.up * 0.15f; 
        ballRb.AddForce(finalDir * currentKick, ForceMode.Impulse);
    }

    void OnDrawGizmos() {
        if (!mostrarDebug) return;

        // LINHAS VERDES: Rede de Conexão do Time (Prova que eles se conhecem)
        if (meusCompanheiros != null) {
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Verde transparente
            foreach (var amigo in meusCompanheiros) {
                if(amigo != null) Gizmos.DrawLine(transform.position, amigo.transform.position);
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcanceDoChute);
        
        // LINHA CIANO: Intenção de Passe
        if (alvoDoPasse != null && currentState == PlayerState.PASS) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, alvoDoPasse.position);
            Gizmos.DrawSphere(alvoDoPasse.position, 0.3f);
        }
    }
}