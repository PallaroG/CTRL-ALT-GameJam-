using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [Header("Alvo")]
    public Transform alvo; // O script tenta achar sozinho se estiver vazio
    
    [Header("Configurações")]
    public float suavidade = 0.125f; // 0.1 = Rápido | 0.5 = Lento
    public Vector3 offset; // Distância que a câmera manterá da bola

    void Start() {
        // 1. SEGURANÇA: Se esqueceu de arrastar a bola, procura pela Tag
        if (alvo == null) {
            GameObject objBola = GameObject.FindGameObjectWithTag("Bola");
            if (objBola != null) {
                alvo = objBola.transform;
            } else {
                Debug.LogWarning("Câmera não achou a bola! Verifique a Tag 'Bola'.");
            }
        }

        // 2. CALCULA A DISTÂNCIA INICIAL
        // Ele pega a posição onde você deixou a câmera na cena e mantém essa distância
        if (alvo != null) {
            offset = transform.position - alvo.position;
        }
    }

    void LateUpdate() {
        // LateUpdate evita tremedeira (roda depois da física)
        if (alvo == null) return;

        // Onde a câmera quer ir? (Posição da bola + a distância original)
        Vector3 posicaoDesejada = alvo.position + offset;

        // Move suavemente (Lerp)
        Vector3 posicaoSuave = Vector3.Lerp(transform.position, posicaoDesejada, suavidade);

        // Aplica o movimento
        transform.position = posicaoSuave;

        // Opcional: Faz a câmera olhar levemente para a bola (bom para 3D)
        transform.LookAt(alvo); 
    }
}