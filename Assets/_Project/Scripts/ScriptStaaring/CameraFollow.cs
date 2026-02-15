using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [Header("Alvo")]
    public Transform alvo; // Arraste a Bola aqui
    
    [Header("Configurações")]
    public float suavidade = 0.125f; // Quanto menor, mais atrasada/suave a câmera (0.1 é bom)
    public Vector3 offset; // A distância/altura da câmera

    void Start() {
        // Se você não definir o offset manualmente no Inspector,
        // ele calcula automaticamente baseado na posição atual da câmera na cena.
        if (alvo != null) {
            offset = transform.position - alvo.position;
        }
    }

    void LateUpdate() {
        // LateUpdate roda DEPOIS que a física da bola já aconteceu (evita tremedeira)
        if (alvo == null) return;

        // 1. Onde a câmera quer estar? (Posição da bola + a altura/distância original)
        Vector3 posicaoDesejada = alvo.position + offset;

        // 2. Mover suavemente da posição atual para a desejada
        Vector3 posicaoSuave = Vector3.Lerp(transform.position, posicaoDesejada, suavidade);

        // 3. Aplicar
        transform.position = posicaoSuave;

        // Opcional: Se quiser que a câmera gire levemente
        // transform.LookAt(alvo); 
    }
}