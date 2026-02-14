using UnityEngine;

public class GoalTrigger : MonoBehaviour {
    public string timeQueMarcou; // Ex: "Time da Casa" ou "Visitante"
    private MatchManager manager;

    void Start() {
        manager = FindObjectOfType<MatchManager>();
    }

    void OnTriggerEnter(Collider other) {
        // Se a bola entrou no trigger
        if (other.CompareTag("Bola")) { // Lembre de colocar a Tag "Bola" na bola!
            manager.RegistrarGol(timeQueMarcou);
        }
    }
}