using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 
using System.Collections; // NOVO: Necessário para rodar a animação (Coroutine)

public class UIManager : MonoBehaviour {

    public static UIManager Instance; 

    [Header("Painéis de Placar")]
    public TextMeshProUGUI textoPlacarCasa;
    public TextMeshProUGUI textoPlacarVisitante;

    [Header("Cronômetro")]
    public TextMeshProUGUI textoTempo;

    [Header("Telas de Fim de Jogo")]
    public GameObject painelFimJogo; 
    public GameObject logoVitoria;   
    public GameObject logoDerrota;   
    public GameObject logoEmpate;    

    [Header("Efeito de Zoom (NOVO)")]
    [Tooltip("Tempo total da animação em segundos")]
    public float duracaoZoom = 0.5f;
    [Tooltip("Desenhe a curva de animação aqui (ex: começa no 0 e vai até o 1)")]
    public AnimationCurve curvaDeZoom = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Navegação")]
    public string nomeCenaMenu = "MenuPrincipal"; 

    void Awake() { 
        Instance = this; 
    }

    public void AtualizarPlacar(int placarCasa, int placarVisitante) {
        if (textoPlacarCasa != null) textoPlacarCasa.text = placarCasa.ToString();
        if (textoPlacarVisitante != null) textoPlacarVisitante.text = placarVisitante.ToString();
    }

    public void AtualizarTempo(float tempoAtual) {
        if (textoTempo != null) {
            int minutos = Mathf.FloorToInt(tempoAtual / 60F);
            int segundos = Mathf.FloorToInt(tempoAtual % 60F);
            textoTempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    public void MostrarFimDeJogo(int quemGanhou) {
        if (painelFimJogo != null) painelFimJogo.SetActive(true);
        
        if (logoVitoria) logoVitoria.SetActive(false);
        if (logoDerrota) logoDerrota.SetActive(false);
        if (logoEmpate) logoEmpate.SetActive(false);

        GameObject logoAtiva = null;

        if (quemGanhou == 1 && logoVitoria) logoAtiva = logoVitoria;
        else if (quemGanhou == 2 && logoDerrota) logoAtiva = logoDerrota;
        else if (quemGanhou == 0 && logoEmpate) logoAtiva = logoEmpate;

        if (logoAtiva != null) {
            // Inicia a mágica da animação!
            StartCoroutine(AnimarZoom(logoAtiva));
        }
    }

    // A ROTINA DE ANIMAÇÃO POR CÓDIGO
    IEnumerator AnimarZoom(GameObject logo) {
        // 1. Salva o tamanho original que você definiu na UI antes de encolher!
        Vector3 escalaOriginal = logo.transform.localScale;
        
        logo.SetActive(true);
        // 2. Encolhe a logo para tamanho zero para começar a animação
        logo.transform.localScale = Vector3.zero; 

        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracaoZoom) {
            tempoDecorrido += Time.deltaTime;
            float progresso = tempoDecorrido / duracaoZoom;

            // 3. Multiplica o valor da curva pelo tamanho original
            float multiplicador = curvaDeZoom.Evaluate(progresso);
            logo.transform.localScale = escalaOriginal * multiplicador;

            yield return null; 
        }

        // 4. Garante que terminou no tamanho perfeito que você escolheu na Unity
        logo.transform.localScale = escalaOriginal; 
    }
    
    public void BotaoVoltarParaMenu() {
        SceneManager.LoadScene(nomeCenaMenu);
    }

    public void BotaoReiniciarPartida() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}