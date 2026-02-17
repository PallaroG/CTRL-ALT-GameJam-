using UnityEngine;

public class SteeringAgent : MonoBehaviour {

    [Header("Configurações do Corpo")]
    public float maxSpeed = 10f;
    public float maxForce = 5f; // "Cavalos de força" / Capacidade de fazer curva
    public float mass = 1f;
    public bool isFlying = false; // Para o pombo!

    [Header("Debug")]
    public Vector3 velocity;
    public Vector3 currentSteeringForce;

    private Rigidbody rb;
    private Vector3 targetPos;
    private bool shouldMove = false;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Nós controlamos a rotação via código
        rb.useGravity = !isFlying; // Pombos não caem (tanto)
        rb.linearDamping = 0; // Unity 6. Se for Unity antiga, use rb.drag = 0;
    }

    // O Cérebro chama isso para definir para onde ir
    public void SetTarget(Vector3 worldPos) {
        targetPos = worldPos;
        // Mantém a altura correta dependendo se voa ou anda
        if (!isFlying) targetPos.y = transform.position.y; 
        shouldMove = true;
    }

    public void Stop() {
        shouldMove = false;
        rb.linearVelocity = Vector3.zero; // Unity 6. Antiga: rb.velocity
    }

    void FixedUpdate() {
        if (!shouldMove) return;

        // 1. Steering: Seek (Perseguir)
        // Calcula a velocidade desejada (Linha reta até o alvo)
        Vector3 desiredVelocity = (targetPos - transform.position).normalized * maxSpeed;

        // 2. Calcula a força de direção (Steering = Desejada - Atual)
        Vector3 steering = desiredVelocity - rb.linearVelocity; // Unity 6. Antiga: rb.velocity
        
        // 3. Limita a força (Isso define a "agilidade". Carros têm maxForce baixo para curvas, Pombos alto)
        currentSteeringForce = Vector3.ClampMagnitude(steering, maxForce);
        currentSteeringForce /= mass;

        // 4. Aplica
        rb.AddForce(currentSteeringForce, ForceMode.Acceleration);

        // 5. Rotação (Olhar para onde anda)
        if (rb.linearVelocity.magnitude > 0.1f) {
            Quaternion lookRot = Quaternion.LookRotation(rb.linearVelocity.normalized);
            // Pombos giram rápido, Carros giram lento. Usamos o maxForce como base.
            float turnSpeed = maxForce * 2f; 
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.fixedDeltaTime * turnSpeed);
        }
    }

    void OnDrawGizmos() {
        if (shouldMove) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPos);
            Gizmos.color = Color.blue; // Vetor de velocidade atual
            Gizmos.DrawRay(transform.position, velocity);
        }
    }
}