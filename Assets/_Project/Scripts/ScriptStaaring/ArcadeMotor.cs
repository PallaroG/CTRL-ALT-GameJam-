using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeMotor : MonoBehaviour {
    private Rigidbody rb;
    private Vector3 currentVelocity;

    [Header("Configurações")]
    public float turnSpeed = 15f;
    public float stoppingDistance = 0.2f;
    public float forcaDesvioParede = 15f;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
        rb.freezeRotation = true; 
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public void SetMovement(Vector3 target, Vector3 repulsion, float speed) {
        target.y = transform.position.y;
        Vector3 diff = target - transform.position;

        Vector3 desvioParede = CalcularDesvioParede();

        if (diff.magnitude > stoppingDistance) {
            Vector3 moveDir = diff.normalized;
            currentVelocity = (moveDir * speed) + repulsion + desvioParede;
            
            rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);

            Quaternion lookRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * Time.fixedDeltaTime);
        } else {
            Stop();
        }
    }

    Vector3 CalcularDesvioParede() {
        RaycastHit hit;
        Vector3 origem = transform.position + Vector3.up * 0.5f; 
        Vector3 direcao = transform.forward;
        direcao.y = 0; 
        direcao.Normalize();
        
        float tamanhoRaio = 1.5f;

        Debug.DrawRay(origem, direcao * tamanhoRaio, Color.green);

        if (Physics.Raycast(origem, direcao, out hit, tamanhoRaio)) {
            if (!hit.collider.CompareTag("Bola") && !hit.collider.CompareTag("Chao") && hit.collider.GetComponent<FootballBrain>() == null) {
                Debug.DrawRay(origem, direcao * hit.distance, Color.red);
                Debug.DrawRay(hit.point, hit.normal * 2f, Color.yellow);
                return hit.normal * forcaDesvioParede;
            }
        }
        return Vector3.zero;
    }

    public void LookAtTarget(Vector3 lookPos) {
        Vector3 dir = (lookPos - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f) {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }
    }

    public void Stop() {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        rb.angularVelocity = Vector3.zero;
    }
}