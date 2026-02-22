using UnityEngine;
using TMPro; // Biblioteca obrigatória para textos com boa resolução na Unity

public class UIManager : MonoBehaviour {

    public static UIManager Instance; 

    [Header("Painéis de Placar (Separados)")]
    [Tooltip("Arraste o TextMeshPro do placar do time da Casa aqui")]
    public TextMeshProUGUI textoPlacarCasa;
    
    [Tooltip("Arraste o TextMeshPro do placar do time Visitante aqui")]
    public TextMeshProUGUI textoPlacarVisitante;

    [Header("Cronômetro")]
    public TextMeshProUGUI textoTempo;

    [Header("Telas de Jogo")]
    public GameObject painelFimJogo;
    public TextMeshProUGUI textoResultadoFinal;

    void Awake() { 
        Instance = this; 
    }

    // O MatchManager chama essa função e manda os dois números isolados
    public void AtualizarPlacar(int placarCasa, int placarVisitante) {
        if (textoPlacarCasa != null) {
            textoPlacarCasa.text = placarCasa.ToString();
        }
        
        if (textoPlacarVisitante != null) {
            textoPlacarVisitante.text = placarVisitante.ToString();
        }
    }

    // Converte os segundos corridos em formato de relógio (MM:SS)
    public void AtualizarTempo(float tempoAtual) {
        if (textoTempo != null) {
            int minutos = Mathf.FloorToInt(tempoAtual / 60F);
            int segundos = Mathf.FloorToInt(tempoAtual % 60F);
            textoTempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    public void MostrarFimDeJogo(string resultado) {
        if (painelFimJogo != null) {
            painelFimJogo.SetActive(true);
        }
        
        if (textoResultadoFinal != null) {
            textoResultadoFinal.text = resultado;
        }
    }
}