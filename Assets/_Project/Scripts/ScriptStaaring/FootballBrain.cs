using UnityEngine;
using System.Collections.Generic;

public enum AIState { IDLE, CHASE_BALL, RETURN_POSITION, SUPPORT, WAIT }

public class FootballBrain : MonoBehaviour {

    [Header("Vínculos")]
    public PlayerData stats; 
    private SteeringAgent motor; 

    [Header("Visual")]
    public SpriteRenderer meuSprite; 
    
    [Header("Consciência Tática")]
    public Transform bola;
    public Transform golAtaque;
    public Transform golDefesa;
    
    [Header("Configurações de IA")]
    public float raioDeVisao = 1000f; // Alto para garantir que vejam a bola
    public float distanciaChute = 2.5f;

    [Header("Estado Atual (Debug)")]
    public AIState estadoAtual;
    
    // Variáveis internas
    private float nextThinkTime = 0f;
    private float nextKickTime = 0f;
    private float timeToThink = 0.1f;

    public void Initialize(PlayerData data, Transform _bola, Transform _ataque, Transform _defesa) {
        stats = data;
        bola = _bola;
        golAtaque = _ataque;
        golDefesa = _defesa;

        // 1. CONFIGURA O VISUAL
        if (meuSprite != null && stats.fotoDoPersonagem != null) {
            meuSprite.sprite = stats.fotoDoPersonagem;
            if (stats.mass > 100) meuSprite.transform.localScale = Vector3.one * 2.0f; 
            else meuSprite.transform.localScale = Vector3.one;
        }

        // 2. CONFIGURA O MOTOR
        motor = GetComponent<SteeringAgent>();
        if (motor != null) {
            motor.maxSpeed = stats.maxSpeed;
            motor.maxForce = stats.agilidade;
            motor.mass = stats.mass;
            motor.isFlying = stats.podeVoar; 
        }
    }

    void Update() {
        if (Time.time < nextThinkTime) return;
        nextThinkTime = Time.time + timeToThink;

        TomarDecisao();
    }

    void TomarDecisao() {
        // --- CHECAGEM DE SEGURANÇA 1: BOLA ---
        if (bola == null) {
            // Tenta achar a bola sozinho se perdeu a referência
            GameObject objBola = GameObject.FindGameObjectWithTag("Bola");
            if (objBola != null) bola = objBola.transform;
            else {
                estadoAtual = AIState.IDLE;
                if(motor) motor.Stop();
                return;
            }
        }

        float distBola = Vector3.Distance(transform.position, bola.position);
        
        // --- 1. LÓGICA DE GOLEIRO ---
        if (stats.funcaoTatica == PosicaoTatica.Goleiro) {
            Vector3 alvoGoleiro = golDefesa.position + (bola.position - golDefesa.position).normalized * 5f;
            motor.SetTarget(alvoGoleiro);
            
            if (distBola < 10f) { // Sai do gol se chegar perto
                motor.SetTarget(bola.position);
                if (distBola < distanciaChute) TentarChutar();
            }
            return;
        }

        // --- 2. JOGADOR DE LINHA ---

        // Se estou vendo a bola (Raio de Visão)
        if (distBola < raioDeVisao) {
            
            // Lógica Simples: Corre pra bola
            estadoAtual = AIState.CHASE_BALL;
            motor.SetTarget(bola.position);
            
            // Se chegou perto, chuta
            if (distBola < distanciaChute) {
               TentarChutar();
            }
        } 
        else {
            // --- TÁTICA ---
            // Se a bola está muito longe, tenta voltar pra posição
            estadoAtual = AIState.RETURN_POSITION;
            
            // CHECAGEM DE SEGURANÇA 2: FORMATION MANAGER
            if (FormationManager.Instance != null) {
                // Se existe o gerente, pede a posição pra ele
                Vector3 targetTatico = FormationManager.Instance.GetTacticalPosition(0.5f, 0.5f, IsTimeCasa());
                motor.SetTarget(targetTatico);
            } else {
                // Se NÃO existe gerente (Esqueceu de criar), continua correndo pra bola pra não travar
                Debug.LogWarning("FormationManager não encontrado! Ignorando tática.");
                motor.SetTarget(bola.position);
            }
        }
    }

    bool IsTimeCasa() {
        if (golDefesa == null) return true;
        return golDefesa.position.x < 0; 
    }

    void TentarChutar() {
        if (Time.time < nextKickTime) return; 

        Rigidbody rbBola = bola.GetComponent<Rigidbody>();
        if (rbBola) {
            Vector3 dirGol = (golAtaque.position - bola.position).normalized;
            
            // Adiciona erro
            Vector3 erro = Random.insideUnitSphere * (1f - (stats.precisao / 100f));
            erro.y = 0; 
            
            Vector3 direcaoFinal = (dirGol + erro).normalized;
            
            // O Chute em si
            rbBola.AddForce(direcaoFinal * stats.kickPower, ForceMode.Impulse);
            
            // Efeito visual/debug
            Debug.DrawRay(bola.position, direcaoFinal * 5f, Color.yellow, 1f);
            nextKickTime = Time.time + 0.5f; 
        }
    }
}