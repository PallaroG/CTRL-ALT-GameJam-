using UnityEngine;

public enum PlayerState { IDLE, CHASE, DRIBBLE, KICK, RETURN, DEFEND }

public class PlayerController : MonoBehaviour {

    [Header("Visual")]
    public SpriteRenderer meuSpriteRenderer; // Arraste o objeto "Foto" do prefab aqui
    
    [Header("Configuração")]
    public PlayerData data;
    
    // Status atuais (Físicos)
    private float currentSpeed;
    private float currentKick;
    private float currentTurn;

    // Alvos Táticos
    private Transform ballTarget;
    private Transform myAttackGoal;
    private Transform myDefenseGoal;
    private Vector3 initialPos; // Posição original (Formação)

    private Rigidbody rb;
    private Rigidbody ballRb;
    public PlayerState currentState = PlayerState.IDLE;
    private float nextActionTime = 0f;

    public void Initialize(PlayerData stats, Transform ball, Transform attack, Transform defense) {
        data = stats;
        ballTarget = ball;
        myAttackGoal = attack;
        myDefenseGoal = defense;
        initialPos = transform.position; // Grava onde nasceu

        // Carrega status
        currentSpeed = data.maxSpeed;
        currentKick = data.kickPower;
        currentTurn = data.turnSpeed;

        // Configura Física
        rb = GetComponent<Rigidbody>();
        rb.mass = data.mass;
        GetComponent<Renderer>().material.color = data.corRepresentativa;
        
        if (ballTarget) ballRb = ballTarget.GetComponent<Rigidbody>();
        gameObject.name = data.nomePersonagem;

        // --- AQUI ESTÁ A LÓGICA DA FOTO ---
        if (meuSpriteRenderer != null && data.fotoDoPersonagem != null) {
            meuSpriteRenderer.sprite = data.fotoDoPersonagem;
        }
    }

    public void ResetPosition() {
        // Teletransporta de volta pro inicio quando sair gol
        transform.position = initialPos;
        rb.linearVelocity = Vector3.zero; // Zerar velocidade (ajustado para versão estável)
        currentState = PlayerState.IDLE;
    }

    void FixedUpdate() {
        if (!ballTarget) return;

        DecideStrategy();
        ExecuteState();
        RotateBody();
    }

    void DecideStrategy() {
        float distBall = Vector3.Distance(transform.position, ballTarget.position);
        float distHome = Vector3.Distance(transform.position, initialPos);
        
        // --- LÓGICA DE DEFESA ---
        // Se a bola está mais perto do MEU gol do que eu...
        float ballToDefense = Vector3.Distance(ballTarget.position, myDefenseGoal.position);
        float meToDefense = Vector3.Distance(transform.position, myDefenseGoal.position);

        // Se a bola está perigosamente perto da minha defesa (30m) e eu estou fora de posição
        if (ballToDefense < 30f && ballToDefense < meToDefense) {
            currentState = PlayerState.DEFEND;
            return;
        }

        // --- LÓGICA DE ATAQUE ---
        if (distBall < 1.5f) {
            // Estou com a bola. Perto do gol? (20m)
            if (Vector3.Distance(transform.position, myAttackGoal.position) < 20f) 
                currentState = PlayerState.KICK;
            else 
                currentState = PlayerState.DRIBBLE;
        }
        else if (distBall < 20f) { // Visão de jogo (20 metros)
            currentState = PlayerState.CHASE;
        }
        else {
            currentState = PlayerState.RETURN; // Volta pra posição tática
        }
    }

    void ExecuteState() {
        switch (currentState) {
            case PlayerState.IDLE:
                rb.linearVelocity *= 0.9f; break;
                
            case PlayerState.CHASE:
                MoveTo(ballTarget.position, 1f); break;
                
            case PlayerState.DRIBBLE: // Leva a bola
                MoveTo(ballTarget.position, 0.9f);
                if(Vector3.Distance(transform.position, ballTarget.position) < 1.2f) {
                    Vector3 dir = (myAttackGoal.position - ballTarget.position).normalized;
                    ballRb.AddForce(dir * (currentKick * 0.15f), ForceMode.Impulse); // Toquinho
                }
                break;

            case PlayerState.KICK: // Chuta pro gol
                if (Time.time > nextActionTime) {
                    Vector3 dir = (myAttackGoal.position - ballTarget.position).normalized + Vector3.up * 0.2f;
                    ballRb.AddForce(dir * currentKick, ForceMode.Impulse);
                    nextActionTime = Time.time + 1.0f; // Cooldown
                }
                break;

            case PlayerState.DEFEND: // Corre para bloquear o gol
                Vector3 blockPoint = (myDefenseGoal.position + ballTarget.position) / 2;
                MoveTo(blockPoint, 1.1f); // Corre desesperado
                break;

            case PlayerState.RETURN: // Volta pra formação
                MoveTo(initialPos, 0.8f); break;
        }
    }

    void MoveTo(Vector3 target, float mult) {
        Vector3 dir = (target - transform.position).normalized;
        Vector3 desired = dir * (currentSpeed * mult);
        Vector3 steer = desired - rb.linearVelocity;
        steer = Vector3.ClampMagnitude(steer, currentTurn);
        rb.AddForce(steer);
    }

    void RotateBody() {
        if(rb.linearVelocity.sqrMagnitude > 0.1f) 
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rb.linearVelocity.normalized), 0.2f));
    }
}