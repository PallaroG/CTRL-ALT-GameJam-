using UnityEngine;
using System.Collections.Generic;

public enum AIState { IDLE, CHASE_BALL, RETURN_POSITION, WITH_BALL }

public class FootballBrain : MonoBehaviour {

    [Header("Vínculos")]
    public PlayerData stats; 
    private SteeringAgent motor; 
    public SpriteRenderer meuSprite; 
    
    [Header("Consciência Tática")]
    public Transform bola;
    public Transform golAtaque;
    public Transform golDefesa;
    public List<FootballBrain> meusCompanheiros = new List<FootballBrain>(); 
    
    // REMOVI AS VARIÁVEIS "taticaX" e "taticaZ" DAQUI. 
    // AGORA ELAS VÊM DO "stats" (PlayerData).

    [Header("Configurações de IA")]
    public float raioDeVisao = 1000f; 
    public float distanciaDoChute = 3.0f; 
    public float cooldownPasse = 1.0f; 

    [Header("Debug - OLHE AQUI")]
    public AIState estadoAtual;
    public string ultimaDecisao = "Aguardando..."; 
    public float distanciaAtualDaBola; 
    public bool souOMaisProximo = false; 
    
    private float nextActionTime = 0f; 

    public void Initialize(PlayerData data, Transform _bolaIncorreta, Transform _ataque, Transform _defesa, List<FootballBrain> _time) {
        stats = data;
        golAtaque = _ataque;
        golDefesa = _defesa;
        
        meusCompanheiros = new List<FootballBrain>(_time);
        meusCompanheiros.Remove(this);

        GameObject bolaReal = GameObject.FindGameObjectWithTag("Bola");
        if (bolaReal != null) bola = bolaReal.transform;
        else bola = _bolaIncorreta; 

        if (meuSprite != null && stats.fotoDoPersonagem != null) {
            meuSprite.sprite = stats.fotoDoPersonagem;
            if (stats.mass > 100) meuSprite.transform.localScale = Vector3.one * 1.2f; 
            else meuSprite.transform.localScale = Vector3.one;
        }

        motor = GetComponent<SteeringAgent>();
        if (motor != null) {
            motor.maxSpeed = stats.maxSpeed;
            motor.maxForce = stats.agilidade;
            motor.mass = stats.mass;
            motor.isFlying = stats.podeVoar; 
        }
    }

    void Update() {
        if (bola == null) return;
        distanciaAtualDaBola = Vector3.Distance(transform.position, bola.position);
        TomarDecisao();
    }

    void TomarDecisao() {
        // --- 1. SOU GOLEIRO? ---
        if (stats.funcaoTatica == PosicaoTatica.Goleiro) {
            DefenderGol();
            return;
        }

        // --- 2. QUEM VAI NA BOLA? ---
        souOMaisProximo = VerificarSeSouOMaisProximo();

        // --- 3. ÁRVORE DE DECISÃO ---

        // A. Estou com a bola -> JOGA
        if (distanciaAtualDaBola <= distanciaDoChute) {
            estadoAtual = AIState.WITH_BALL;
            if (Time.time > nextActionTime) DecidirComBola();
        }
        // B. Sou o mais próximo -> CORRE PRA BOLA
        else if (souOMaisProximo) {
            estadoAtual = AIState.CHASE_BALL;
            motor.SetTarget(bola.position);
        }
        // C. Não sou o mais próximo -> DESMARCAR (Usando PlayerData!)
        else {
            estadoAtual = AIState.RETURN_POSITION;
            if (FormationManager.Instance) {
                // AQUI ESTÁ A MUDANÇA: Usamos stats.taticaX e stats.taticaZ
                Vector3 target = FormationManager.Instance.GetTacticalPosition(stats.taticaX, stats.taticaZ, IsTimeCasa());
                motor.SetTarget(target);
            } else {
                Vector3 direcao = (transform.position - bola.position).normalized;
                motor.SetTarget(bola.position + direcao * 10f);
            }
        }
    }

    bool VerificarSeSouOMaisProximo() {
        if (distanciaAtualDaBola <= distanciaDoChute) return true;

        foreach (var amigo in meusCompanheiros) {
            if (amigo == null) continue;
            if (amigo.stats.funcaoTatica == PosicaoTatica.Goleiro) continue;

            float distAmigo = Vector3.Distance(amigo.transform.position, bola.position);

            if (distAmigo < distanciaAtualDaBola - 0.5f) {
                return false; 
            }
        }
        return true; 
    }

    void DefenderGol() {
        Vector3 alvo = golDefesa.position + (bola.position - golDefesa.position).normalized * 5f;
        motor.SetTarget(alvo);
        if (distanciaAtualDaBola < 15f) { 
            motor.SetTarget(bola.position);
            if (distanciaAtualDaBola <= distanciaDoChute) ChutarProGol(); 
        }
    }

    void DecidirComBola() {
        float distGol = Vector3.Distance(transform.position, golAtaque.position);

        if (distGol < 35f) { 
            ChutarProGol();
        }
        else {
            FootballBrain amigoLivre = ProcurarAmigoLivre();
            if (amigoLivre != null) {
                PassarBola(amigoLivre);
            } else {
                ChutarPraFrente();
            }
        }
    }

    FootballBrain ProcurarAmigoLivre() {
        FootballBrain melhor = null;
        float melhorVantagem = -999f; 

        foreach (var amigo in meusCompanheiros) {
            if (amigo == null) continue;

            float distAmigo = Vector3.Distance(transform.position, amigo.transform.position);
            
            if (distAmigo < 4f || distAmigo > 60f) continue;

            float minhaDistGol = Vector3.Distance(transform.position, golAtaque.position);
            float amigoDistGol = Vector3.Distance(amigo.transform.position, golAtaque.position);
            float vantagem = minhaDistGol - amigoDistGol;

            if (vantagem > -5.0f) { 
                 Vector3 dir = (amigo.transform.position - transform.position).normalized;
                 Vector3 origem = transform.position + (Vector3.up * 0.5f) + (dir * 0.8f);
                 
                 Debug.DrawRay(origem, dir * (distAmigo - 1.0f), Color.yellow);

                 RaycastHit hit;
                 if (Physics.Raycast(origem, dir, out hit, distAmigo - 1.0f)) {
                     FootballBrain amigoAtingido = hit.collider.GetComponentInParent<FootballBrain>();
                     if (hit.collider.CompareTag("Bola") || amigoAtingido == amigo) {
                         if (vantagem > melhorVantagem) {
                             melhorVantagem = vantagem;
                             melhor = amigo;
                         }
                     }
                 } else {
                     if (vantagem > melhorVantagem) {
                         melhorVantagem = vantagem;
                         melhor = amigo;
                     }
                 }
            }
        }
        return melhor;
    }

    void PassarBola(FootballBrain alvo) {
        Rigidbody rbBola = bola.GetComponent<Rigidbody>();
        if (rbBola) {
            string msg = $"PASSE PARA {alvo.stats.nomePersonagem}";
            ultimaDecisao = msg;
            Debug.Log($"<color=cyan>{msg}</color>"); 
            Debug.DrawLine(transform.position, alvo.transform.position, Color.cyan, 2.0f);
            
            Vector3 dir = (alvo.transform.position - bola.position).normalized;
            rbBola.AddForce(dir * (stats.kickPower * 0.8f), ForceMode.Impulse); 
            nextActionTime = Time.time + cooldownPasse;
        }
    }
    
    void ChutarProGol() {
        Rigidbody rbBola = bola.GetComponent<Rigidbody>();
        if (rbBola) {
            ultimaDecisao = "CHUTE AO GOL!";
            Debug.Log($"<color=red>{ultimaDecisao}</color>"); 
            Vector3 dir = (golAtaque.position - bola.position).normalized;
            Vector3 erro = Random.insideUnitSphere * (1f - (stats.precisao / 100f));
            erro.y = 0;
            rbBola.AddForce((dir + erro).normalized * stats.kickPower, ForceMode.Impulse);
            nextActionTime = Time.time + 0.8f;
        }
    }

    void ChutarPraFrente() {
        Rigidbody rbBola = bola.GetComponent<Rigidbody>();
        if (rbBola) {
            ultimaDecisao = "DRIBLE";
            Debug.Log($"<color=yellow>{ultimaDecisao}</color>"); 
            Vector3 dir = (golAtaque.position - bola.position).normalized;
            rbBola.AddForce(dir * (stats.kickPower * 0.4f), ForceMode.Impulse);
            nextActionTime = Time.time + 0.5f;
        }
    }

    bool IsTimeCasa() {
        if (golDefesa == null) return true;
        return golDefesa.position.x < 0; 
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDoChute);
    }
}