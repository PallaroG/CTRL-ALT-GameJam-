using UnityEngine;

[CreateAssetMenu(fileName = "NovoPersonagem", menuName = "Futebol/Personagem")]
public class PlayerData : ScriptableObject {
    [Header("Identidade")]
    public Sprite fotoDoPersonagem;
    public string nomePersonagem;
    public Color corRepresentativa; 
    public GameObject visualModel; // <--- O CAMPO QUE FALTAVA
    
    [Header("Física de Movimento")]
    public float maxSpeed;   // Velocidade máxima
    public float turnSpeed;  // Força do Motor (Agilidade)
    public float mass;       // Peso
    
    [Header("Habilidades")]
    public float kickPower;    // Força do chute
    public float kickCooldown; // Tempo de recarga
}