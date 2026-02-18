using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Para reiniciar a cena

public class UIManager : MonoBehaviour {
    
    public static UIManager Instance;
    
    [Header("HUD")]
    public TextMeshProUGUI textoPlacar;
    public TextMeshProUGUI textoTempo; // <--- NOVO
    
    [Header("Fim de Jogo")]
    public GameObject painelFimJogo;   // <--- NOVO
    public TextMeshProUGUI textoVencedor; // <--- NOVO

    void Awake() {
        Instance = this;
    }

    public void AtualizarPlacar(int golsCasa, int golsVisitante) {
        if (textoPlacar != null) {
            textoPlacar.text = $"CASA {golsCasa} X {golsVisitante} VISITANTE";
            // Efeito visual de "Pop"
            textoPlacar.transform.localScale = Vector3.one * 1.5f;
            Invoke("ResetarEscala", 0.5f);
        }
    }
    
    // --- NOVO: Atualiza o relógio ---
    public void AtualizarTempo(float tempoRestante) {
        if (textoTempo != null) {
            // Formata para minutos:segundos (ex: 90:00)
            int minutos = Mathf.FloorToInt(tempoRestante / 60);
            int segundos = Mathf.FloorToInt(tempoRestante % 60);
            textoTempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    // --- NOVO: Mostra quem ganhou ---
    public void MostrarFimDeJogo(string mensagemVitoria) {
        if (painelFimJogo != null) {
            painelFimJogo.SetActive(true); // Liga o painel
            textoVencedor.text = mensagemVitoria;
        }
    }

    // --- NOVO: Função para o Botão (Ligue isso no OnClick do botão) ---
    public void ReiniciarCena() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ResetarEscala() {
        textoPlacar.transform.localScale = Vector3.one;
    }
}