using UnityEngine;

public enum PosicaoTatica { Linha, Goleiro }

[CreateAssetMenu(fileName = "NovoPersonagem", menuName = "Futebol/Personagem")]
public class PlayerData : ScriptableObject {
    
    [Header("Identidade")]
    public string nomePersonagem;
    public Sprite fotoDoPersonagem; // A imagem do Uno/Pombo
    public Color corRepresentativa; 
    public GameObject visualModel;  // O modelo 3D (opcional agora que usamos Sprite)
    public PosicaoTatica funcaoTatica;

    [Header("Física (Steering)")]
    public float mass;           // Peso (Uno = 800, Pombo = 5)
    public float maxSpeed;       // Velocidade Máxima
    public float agilidade;      // (MaxForce) Capacidade de fazer curvas fechadas
    public float aceleracao;     // Quão rápido chega na velocidade máxima

    [Header("Habilidades")]
    [Range(0, 100)] public float precisao; // 100 = Chuta onde quer, 0 = Chuta torto
    public float kickPower;    // Força do chute

    [Header("Especial (Caos)")]
    public bool podeVoar;      // <--- O ERRO ESTAVA AQUI (Faltava essa variável)
    public float alturaDeVoo = 3.0f; // Altura que o pombo fica do chão
}