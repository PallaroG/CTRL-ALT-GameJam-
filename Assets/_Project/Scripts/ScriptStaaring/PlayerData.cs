using UnityEngine;

public enum PosicaoTatica { Linha, Goleiro }

[CreateAssetMenu(fileName = "NovoPersonagem", menuName = "Futebol/Personagem")]

public class PlayerData : ScriptableObject {
    
    [Header("Identidade")]
    public string nomePersonagem;
    public Sprite fotoDoPersonagem;
    public Color corRepresentativa; 
    public GameObject visualModel;
    public PosicaoTatica funcaoTatica;

    [Header("Mental (FM)")]
    [Range(0, 50)] public float visaoDeJogo;
    
    [Header("Física FM (Football Manager)")]
    [Range(0, 100)] public float precisao; // o quanto o jogador é bom em chutar na direção certa (0 a 100)
    public float maxSpeed;       // Velocidade Final 
    public float aceleracao;     // Arrancada (0 a 100) - Força do Motor
    public float controleCorporal; // (Drag) 0.5 = Sabão/Carro | 5 a 10 = Humano/Chuteira
    public float mass;           // Peso 
    public float agilidade;      //Capacidade de fazer curvas fechadas
    
    [Header("Habilidades")]
    public float kickPower;    
    public float kickCooldown; 
}