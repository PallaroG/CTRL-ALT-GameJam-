using UnityEngine;

public class FormationManager : MonoBehaviour {
    public static FormationManager Instance;

    [Header("Vínculos")]
    public Transform bola;

    [Header("Alinhamento do Mapa (GIZMOS)")]
    public Vector3 centroDoCampo = Vector3.zero; 

    [Header("Dimensões (CORRIGIDO: Z = Comprimento, X = Largura)")]
    public float campoComprimento = 90f; // Eixo Z (Gol a Gol)
    public float campoLargura = 50f;     // Eixo X (Laterais)
    
    [Header("Limites de Segurança")]
    public float margemFundo = 5.0f;     
    public float margemLateral = 2.0f;   

    void Awake() { Instance = this; }

    public Vector3 GetTacticalPosition(float taticaX, float taticaZ, bool isTimeCasa) {
        if (bola == null) return centroDoCampo; 

        // --- 1. EIXO Z (COMPRIMENTO / ATAQUE) ---
        float bolaZRelativa = bola.position.z - centroDoCampo.z;
        float bolaZClamped = Mathf.Clamp(bolaZRelativa, -campoComprimento/2, campoComprimento/2);
        float offsetZ = bolaZClamped * 0.6f; 

        float areaJogavelZ = campoComprimento - (margemFundo * 2); 
        float posZ = (taticaZ - 0.5f) * areaJogavelZ;
        float finalZ = posZ + offsetZ;

        // --- 2. EIXO X (LARGURA / LATERAIS) ---
        float areaJogavelX = campoLargura - (margemLateral * 2);
        float finalX = (taticaX - 0.5f) * areaJogavelX;

        // --- 3. INVERSÃO (VISITANTE) ---
        if (!isTimeCasa) {
            finalZ = -finalZ; 
            finalX = -finalX; 
            finalZ -= (offsetZ * 2); 
        }

        Vector3 finalPos = new Vector3(finalX + centroDoCampo.x, 0, finalZ + centroDoCampo.z);
        
        float limiteZ = (campoComprimento / 2) - margemFundo;
        float limiteX = (campoLargura / 2) - margemLateral;

        finalPos.z = Mathf.Clamp(finalPos.z, centroDoCampo.z - limiteZ, centroDoCampo.z + limiteZ);
        finalPos.x = Mathf.Clamp(finalPos.x, centroDoCampo.x - limiteX, centroDoCampo.x + limiteX);

        return finalPos;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        // Desenha a caixa no eixo correto (X = Largura, Z = Comprimento)
        Gizmos.DrawWireCube(centroDoCampo, new Vector3(campoLargura, 1, campoComprimento));
        
        Gizmos.color = Color.yellow;
        float sizeZ = campoComprimento - (margemFundo * 2);
        float sizeX = campoLargura - (margemLateral * 2);
        Gizmos.DrawWireCube(centroDoCampo, new Vector3(sizeX, 1, sizeZ));
    }
}