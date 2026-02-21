using UnityEngine;
using System.Collections.Generic;

public class FootballBrain : MonoBehaviour {

    [Header("Vínculos")]
    public PlayerData stats; 
    private ArcadeMotor motor; 
    public SpriteRenderer meuSprite; 
    
    [Header("Consciência Tática")]
    public Transform bola;
    public Transform golAtaque;
    public Transform golDefesa;
    public List<FootballBrain> meusCompanheiros = new List<FootballBrain>(); 

    [Header("Configurações Arcade")]
    public float raioDeDeteccao = 12.0f; 
    public float distanciaChuteArcade = 2.8f; 
    public float distanciaParaDriblar = 4.0f; // VALOR DE GATILHO PARA O DRIBLE
    private float nextActionTime = 0f;

    [Header("Ajustes de Condução")]
    [Range(0.01f, 0.2f)] public float forcaMinima = 0.05f; 
    [Range(0.2f, 1.0f)] public float forcaMaxima = 0.4f;  

    [Header("Debug")]
    public AIState estadoAtual;
    public float distanciaAtualDaBola; 

    public void Initialize(PlayerData data, Transform _bolaReal, Transform _ataque, Transform _defesa, List<FootballBrain> _time) {
        stats = data;
        golAtaque = _ataque;
        golDefesa = _defesa;
        meusCompanheiros = new List<FootballBrain>(_time);
        meusCompanheiros.Remove(this);
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
        if (bola == null) return;
        distanciaAtualDaBola = Vector3.Distance(transform.position, bola.position);
        
        if (distanciaAtualDaBola <= distanciaChuteArcade) {
            estadoAtual = AIState.WITH_BALL;
            if (Time.time >= nextActionTime) {
                AnalisarCaminhoProGol();
            }
        } else {
            estadoAtual = AIState.CHASE_BALL;
            motor.SetMovement(bola.position, Vector3.zero, stats.maxSpeed);
        }

        motor.LookAtTarget(bola.position);
    }

    void AnalisarCaminhoProGol() {
        Vector3 direcaoGol = (golAtaque.position - transform.position).normalized;
        string tagInimiga = (this.CompareTag("CASA")) ? "VISITANTE" : "CASA";
        
        RaycastHit hit;
        Vector3 origemRaio = transform.position + (direcaoGol * 1.5f) + Vector3.up;

        Debug.DrawRay(origemRaio, direcaoGol * raioDeDeteccao, Color.yellow, 0.1f);

        if (Physics.SphereCast(origemRaio, 2.0f, direcaoGol, out hit, raioDeDeteccao)) {
            if (hit.collider != null && hit.collider.attachedRigidbody != null && hit.collider.attachedRigidbody.CompareTag(tagInimiga)) {
                
                float distInimigo = hit.distance;

                // Transição de lógica baseada no valor de distanciaParaDriblar
                if (distInimigo <= distanciaParaDriblar) {
                    RealizarDribleLateral(direcaoGol);
                } else {
                    ConduzirControlado(direcaoGol, distInimigo);
                }
                return;
            }
        }

        ExecutarChuteDireto();
    }

    void ConduzirControlado(Vector3 direcao, float distInimigo) {
        // Cálculo da força baseado na proximidade
        float t = (distInimigo - distanciaParaDriblar) / (raioDeDeteccao - distanciaParaDriblar);
        float fatorFinal = Mathf.Lerp(forcaMinima, forcaMaxima, t);
        float forcaFinal = stats.kickPower * fatorFinal;

        // Debug específico para monitorar a diminuição da força
        string msg = $"Diminuindo força: Inimigo a {distInimigo:F1}m. Potência atual: {fatorFinal * 100:F0}%";
        AplicarForcaNaBola(direcao, forcaFinal, msg, "cyan");
        
        nextActionTime = Time.time + 0.4f; 
    }

    void RealizarDribleLateral(Vector3 direcaoGol) {
        Vector3 direcaoDrible = Vector3.Cross(Vector3.up, direcaoGol).normalized;
        Vector3 vetorFinal = (direcaoDrible + direcaoGol * 0.4f).normalized;

        AplicarForcaNaBola(vetorFinal, stats.kickPower * 0.5f, "Drible ativado! Distância crítica atingida.", "magenta");
        nextActionTime = Time.time + 0.7f;
    }

    void ExecutarChuteDireto() {
        Vector3 direcao = (golAtaque.position - bola.position).normalized;
        AplicarForcaNaBola(direcao, stats.kickPower, "Caminho limpo! Chute força máxima.", "red");
        nextActionTime = Time.time + 1.2f;
    }

    void AplicarForcaNaBola(Vector3 direcao, float forca, string logMsg, string logCor) {
        Debug.Log($"<color={logCor}>[IA {stats.nomePersonagem}]</color> {logMsg}");

        Rigidbody rbBola = bola.GetComponentInParent<Rigidbody>();
        if (rbBola != null) {
            rbBola.linearVelocity = Vector3.zero;
            rbBola.angularVelocity = Vector3.zero;
            direcao.y = 0.15f; 
            rbBola.AddForce(direcao * forca, ForceMode.Impulse);
        }
        estadoAtual = AIState.CHASE_BALL;
    }

    void PensarTaticamente() { }
    Vector3 CalcularVetorRepulsao() { return Vector3.zero; }
    void DecidirGoleiro() { }
    void ComportamentoSegurarBola() { }
    bool VerificarSeSouOMaisProximo() { return true; }
    bool IsTimeCasa() { return this.CompareTag("CASA"); }
    FootballBrain ProcurarAmigoLivre() { return null; }
}