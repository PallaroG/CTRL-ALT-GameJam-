using UnityEngine;

public class FormationManager : MonoBehaviour {
    public static FormationManager Instance;

    [Header("Vínculos")]
    public Transform bola;

    [Header("Alinhamento do Mapa (GIZMOS)")]
    // Use isso para mover a caixa verde/amarela até encaixar no gramado
    public Vector3 centroDoCampo = Vector3.zero; 

    [Header("Configuração das Dimensões")]
    public float campoComprimento = 90f; // Eixo Z
    public float campoLargura = 50f;     // Eixo X
    
    [Header("Limites de Segurança")]
    public float margemFundo = 5.0f;     
    public float margemLateral = 2.0f;   

    void Awake() { Instance = this; }

    public Vector3 GetTacticalPosition(float xFactor, float zFactor, bool isTimeCasa) {
        if (bola == null) return centroDoCampo; // Retorna o centro se não houver bola

        // --- 1. AJUSTE DO EIXO Z (COMPRIMENTO) ---
        // Acompanha a bola relativo ao centro do campo definido por você
        float bolaZRelativa = bola.position.z - centroDoCampo.z;
        float bolaZClamped = Mathf.Clamp(bolaZRelativa, -campoComprimento/2, campoComprimento/2);
        float offsetZ = bolaZClamped * 0.6f; 

        float areaJogavelZ = campoComprimento - (margemFundo * 2); 
        float posZ = (zFactor - 0.5f) * areaJogavelZ;
        float finalZ = posZ + offsetZ;

        // --- 2. AJUSTE DO EIXO X (LARGURA) ---
        float areaJogavelX = campoLargura - (margemLateral * 2);
        float posX = (xFactor - 0.5f) * areaJogavelX;

        // --- 3. INVERSÃO (VISITANTE) ---
        if (!isTimeCasa) {
            posX = -posX;
            finalZ = -finalZ;
            finalZ -= (offsetZ * 2); 
        }

        // --- 4. TRAVA E POSIÇÃO FINAL ---
        // Adicionamos 'centroDoCampo' no final para que os jogadores sigam o Gizmo movido
        Vector3 finalPos = new Vector3(posX + centroDoCampo.x, 0, finalZ + centroDoCampo.z);
        
        float limiteZ = (campoComprimento / 2) - margemFundo;
        float limiteX = (campoLargura / 2) - margemLateral;

        finalPos.x = Mathf.Clamp(finalPos.x, centroDoCampo.x - limiteX, centroDoCampo.x + limiteX);
        finalPos.z = Mathf.Clamp(finalPos.z, centroDoCampo.z - limiteZ, centroDoCampo.z + limiteZ);

        return finalPos;
    }

    void OnDrawGizmos() {
        // Desenha a partir do 'centroDoCampo' em vez do zero absoluto
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(centroDoCampo, new Vector3(campoLargura, 1, campoComprimento));
        
        Gizmos.color = Color.yellow;
        float sizeZ = campoComprimento - (margemFundo * 2);
        float sizeX = campoLargura - (margemLateral * 2);
        Gizmos.DrawWireCube(centroDoCampo, new Vector3(sizeX, 1, sizeZ));
    }
}