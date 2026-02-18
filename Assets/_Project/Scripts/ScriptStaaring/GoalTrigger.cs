using UnityEngine;

public class GoalTrigger : MonoBehaviour {
    
    [Tooltip("Escreva 'Casa' ou 'Visitante' aqui")]
    public string timeQueMarcou; 

    void OnTriggerEnter(Collider other) {
        VerificarGol(other);
    }

    void OnCollisionEnter(Collision collision) {
        VerificarGol(collision.collider);
    }

    void VerificarGol(Collider colisor) {
        // 1. Verifica se o objeto que tocou tem a tag BOLA
        bool ehBola = colisor.CompareTag("Bola");

        // 2. SE NÃO TIVER, verifica se o PAI dele (o Rigidbody) tem a tag BOLA
        // Isso resolve seu problema: O colisor está na Sphere (filho), mas o Rigidbody está no Pai (com a Tag)
        if (!ehBola && colisor.attachedRigidbody != null) {
            ehBola = colisor.attachedRigidbody.CompareTag("Bola");
        }

        if (ehBola) {
            Debug.Log($"<color=green>GOL! Detectado no {timeQueMarcou}.</color>");
            ConfirmarGol();
        }
    }

    void ConfirmarGol() {
        if (MatchManager.Instance != null) {
            MatchManager.Instance.RegistrarGol(timeQueMarcou);
        } else {
            // Tenta achar na marra se o Singleton falhar
            MatchManager acheiGerente = FindObjectOfType<MatchManager>();
            if(acheiGerente) acheiGerente.RegistrarGol(timeQueMarcou);
        }
    }
}