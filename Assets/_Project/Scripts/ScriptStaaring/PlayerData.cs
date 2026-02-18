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

    [Header("Posicionamento Tático (IA)")]
    [Range(0f, 1f)] public float taticaX = 0.5f; // 0 = Defesa, 1 = Ataque
    [Range(0f, 1f)] public float taticaZ = 0.5f; // 0 = Esquerda, 1 = Direita

    [Header("Física (Steering)")]
    public float mass;           
    public float maxSpeed;       
    public float agilidade;      
    public float aceleracao;     

    [Header("Habilidades")]
    [Range(0, 100)] public float precisao; 
    public float kickPower;    

    [Header("Especial (Caos)")]
    public bool podeVoar;      
    public float alturaDeVoo = 3.0f; 
}