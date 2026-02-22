using UnityEngine;

public class GoleiroBrain : MonoBehaviour {

    [Header("Vínculos")]
    public PlayerData stats;
    private ArcadeMotor motor;
    public SpriteRenderer meuSprite;

    [Header("Tática da Baliza")]
    public Transform bola;
    public Transform golDefesa;
    public Transform golAtaque; 
    
    [Header("Área de Trânsito")]
    public float agressividadeDeAvanco = 0.6f; 
    public float distanciaDominio = 3.5f;
    public Collider zonaDeAtuacao;

    [Header("Reflexos (O Delay de Golo)")]
    [Tooltip("Quão rápido ele acompanha a bola. Valores menores = maior delay e mais fácil de fazer gol!")]
    public float velocidadeDeReacao = 3.5f; 
    private Vector3 alvoDeMovimento; 

    [Header("Debug")]
    public AIState estadoAtual = AIState.RETURN_POSITION;

    private Vector3 spawnInicial;
    private Quaternion rotacaoInicial;
    private float tempoProximaAcao = 0f;

    public void Initialize(PlayerData data, Transform _bola, Transform _defesa, Transform _ataque) {
        stats = data;
        bola = _bola;
        golDefesa = _defesa;
        golAtaque = _ataque;

        motor = GetComponent<ArcadeMotor>();
        if (meuSprite != null && stats.fotoDoPersonagem != null) meuSprite.sprite = stats.fotoDoPersonagem;

        spawnInicial = transform.position;
        rotacaoInicial = transform.rotation;
        alvoDeMovimento = transform.position; 

        Transform areaChild = golDefesa.Find("Area");
        if (areaChild != null) {
            zonaDeAtuacao = areaChild.GetComponent<Collider>();
        }

        if (bola != null) {
            Collider[] meusColisores = GetComponentsInChildren<Collider>();
            Collider colisorDaBola = bola.GetComponentInChildren<Collider>();
            if (meusColisores != null && colisorDaBola != null) {
                foreach(var c in meusColisores) Physics.IgnoreCollision(c, colisorDaBola, true);
            }
        }
    }

    void Update() {
        if (bola == null) return;
        float distBolaGoleiro = Vector3.Distance(transform.position, bola.position);
        motor.LookAtTarget(bola.position);

        if (distBolaGoleiro <= distanciaDominio) {
            if (estadoAtual != AIState.WITH_BALL) {
                estadoAtual = AIState.WITH_BALL;
                tempoProximaAcao = Time.time + 1.5f; 
            }

            motor.SetMovement(transform.position, Vector3.zero, 0); 
            
            Rigidbody rbBola = bola.GetComponentInParent<Rigidbody>();
            if (rbBola != null) {
                rbBola.linearVelocity = Vector3.zero;
                rbBola.angularVelocity = Vector3.zero;
                
                Vector3 direcaoFrente = (golAtaque.position - transform.position).normalized;
                Vector3 posicaoSegura = transform.position + (direcaoFrente * 1.5f);
                posicaoSegura.y = bola.position.y;
                bola.position = posicaoSegura;
            }

            if (Time.time >= tempoProximaAcao) {
                IsolarABola();
                estadoAtual = AIState.IDLE; 
            }
            return; 
        }

        if (estadoAtual == AIState.WITH_BALL) estadoAtual = AIState.IDLE;

        bool bolaNaArea = false;
        if (zonaDeAtuacao != null) {
            Vector3 pontoMaisProximo = zonaDeAtuacao.ClosestPoint(bola.position);
            if (Vector3.Distance(pontoMaisProximo, bola.position) < 0.1f) {
                bolaNaArea = true;
            }
        }

        if (bolaNaArea) {
            estadoAtual = AIState.CHASE_BALL;
            motor.SetMovement(bola.position, Vector3.zero, stats.maxSpeed);
            alvoDeMovimento = transform.position; 
            return;
        }

        estadoAtual = AIState.RETURN_POSITION;
        Vector3 posicaoDesejada;
        
        Rigidbody rbB = bola.GetComponentInParent<Rigidbody>();
        Vector3 velBola = rbB != null ? rbB.linearVelocity : Vector3.zero;

        if (velBola.magnitude > 8.0f) {
            Vector3 pontoFuturo = bola.position + (velBola * 0.5f);
            posicaoDesejada = pontoFuturo;
        } else {
            Vector3 direcaoDaBola = (bola.position - golDefesa.position).normalized;
            float distBolaGol = Vector3.Distance(golDefesa.position, bola.position);
            float avanco = distBolaGol * agressividadeDeAvanco;
            posicaoDesejada = golDefesa.position + (direcaoDaBola * avanco);
        }

        posicaoDesejada.y = transform.position.y; 

        if (zonaDeAtuacao != null) {
            posicaoDesejada = zonaDeAtuacao.ClosestPoint(posicaoDesejada);
            posicaoDesejada.y = transform.position.y; 
        }

        alvoDeMovimento = Vector3.Lerp(alvoDeMovimento, posicaoDesejada, Time.deltaTime * velocidadeDeReacao);

        float distParaIdeal = Vector3.Distance(transform.position, alvoDeMovimento);
        if (distParaIdeal > 0.3f) {
            motor.SetMovement(alvoDeMovimento, Vector3.zero, stats.maxSpeed * 0.8f); 
        } else {
            motor.Stop();
        }
    }

    void IsolarABola() {
        Vector3 direcao = (golAtaque.position - transform.position).normalized;
        direcao.y = 0.4f; 
        
        Rigidbody rbBola = bola.GetComponentInParent<Rigidbody>();
        if (rbBola != null) {
            rbBola.linearVelocity = Vector3.zero;
            rbBola.AddForce(direcao * stats.kickPower * 1.8f, ForceMode.Impulse); 
        }
    }

    public void ResetarPosicao() {
        transform.position = spawnInicial;
        transform.rotation = rotacaoInicial;
        alvoDeMovimento = transform.position;
        estadoAtual = AIState.RETURN_POSITION;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}