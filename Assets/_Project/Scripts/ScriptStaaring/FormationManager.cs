using UnityEngine;

public class FormationManager : MonoBehaviour {
    public static FormationManager Instance;

    [Header("Configuração do Campo")]
    public Transform bola;
    public float campoComprimento = 90f; // Ajuste conforme seu cenário
    public float campoLargura = 50f;

    void Awake() { Instance = this; }

    public Vector3 GetTacticalPosition(float xFactor, float zFactor, bool isTimeCasa) {
        if (bola == null) return Vector3.zero;

        // A posição base flutua com a bola (O time sobe e desce em bloco)
        float bolaX = Mathf.Clamp(bola.position.x, -campoComprimento/2, campoComprimento/2);
        
        // Se a bola avança, o time avança 60% do caminho (para não deixar a defesa exposta)
        float offsetX = bolaX * 0.6f; 

        // Calcula posição relativa à formação
        // Ex: Se xFactor é 0 (Zaga), ele fica atrás do centro do bloco
        float posX = (xFactor - 0.5f) * (campoComprimento * 0.8f);
        float posZ = (zFactor - 0.5f) * (campoLargura * 0.9f);

        // Se for visitante, inverte o campo (ataca para a esquerda)
        if (!isTimeCasa) {
            posX = -posX;
            offsetX = -offsetX; 
        }

        Vector3 finalPos = new Vector3(posX + offsetX, 0, posZ);
        
        // Trava dentro do campo
        finalPos.x = Mathf.Clamp(finalPos.x, -campoComprimento/2, campoComprimento/2);
        finalPos.z = Mathf.Clamp(finalPos.z, -campoLargura/2, campoLargura/2);

        return finalPos;
    }
}