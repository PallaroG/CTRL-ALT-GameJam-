using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Botões")]
    public GameObject jogar;
    public GameObject opcoes;
    public GameObject creditosBtn;
    public GameObject sair;
    
    [Header("Telas (Windows)")]
    public GameObject opcoesWindow;
    public GameObject creditosWindow;

    [Header("Áudio")]
    public AudioSource musicaDoMenu; // NOVO: Referência para a música do menu
    public AudioSource musicaDoJogo; 

    [Header("Config Cam")]
    Camera cam;
    private Vector3 destino = new Vector3(0.099f, 1.59f, -17.73f);
    private Vector3 destinoParede = new Vector3(-15.33f, 2.49f, 15.03f);
    private Vector3 créditosParede = new Vector3(-15.33f, 2.49f, 3.19f);
    
    private Quaternion rotacaoParede = Quaternion.Euler(0, -90, 0);
    private Quaternion rotacaoCreditos = Quaternion.Euler(0, -90, 0);
    
    private Vector3 destinoInicio = new Vector3(0.099f, 3.25f, 6.36f);
    private Quaternion rotacaoInicio = Quaternion.Euler(12.245f, 0, 0);
    
    public float velocidadeInicio;
    public float velocidadeOpcoes;
    public float velocidadeRotacao;
    
    private bool camMove = false;
    private bool camMoveWall = false;
    private bool camMoveBack = false;
    private bool camMoveCredits = false;

    public void Start()
    {
        cam = Camera.main;
        Time.timeScale = 0f; 
    }

    void Update()
    {
        if(camMove) 
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, destino, velocidadeInicio * Time.unscaledDeltaTime);

        if(camMoveWall) 
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, destinoParede, velocidadeOpcoes * Time.unscaledDeltaTime);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, rotacaoParede, velocidadeRotacao * Time.unscaledDeltaTime);
        }

        if(camMoveCredits) 
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, créditosParede, velocidadeOpcoes * Time.unscaledDeltaTime);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, rotacaoCreditos, velocidadeRotacao * Time.unscaledDeltaTime);
        }

        if(camMoveBack) 
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, destinoInicio, velocidadeOpcoes * Time.unscaledDeltaTime);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, rotacaoInicio, velocidadeRotacao * Time.unscaledDeltaTime);
        }

        if(cam.transform.position == destino) camMove = false;
        else if(cam.transform.position == destinoInicio) camMoveBack = false;
        else if(cam.transform.position == destinoParede) camMoveWall = false;
        else if(cam.transform.position == créditosParede) camMoveCredits = false;
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void PlayButton()
    {
        DesativarOutrosMovimentos();
        camMove = true;
        OcultarBotoesPrincipais();
        
        Time.timeScale = 1f;
        
        // NOVO: Pausa a música do menu (se ela existir)
        if(musicaDoMenu != null)
        {
            musicaDoMenu.Pause(); 
            // Dica: Use musicaDoMenu.Stop() no lugar de Pause() se você não planeja voltar pro menu e quiser liberar memória.
        }

        // NOVO: Toca a música do jogo
        if(musicaDoJogo != null)
        {
            musicaDoJogo.Play();
        }
    }

    public void OpenOptionsButton()
    {
        DesativarOutrosMovimentos();
        camMoveWall = true;
        OcultarBotoesPrincipais();
        StartCoroutine(AtivarJanela(opcoesWindow));
    }

    public void CloseOptionsButton()
    {
        DesativarOutrosMovimentos();
        opcoesWindow.SetActive(false);
        camMoveBack = true;
        StartCoroutine(MostrarBotoesPrincipais());
    }

    public void OpenCreditsButton()
    {
        DesativarOutrosMovimentos();
        camMoveCredits = true;
        OcultarBotoesPrincipais();
        StartCoroutine(AtivarJanela(creditosWindow));
    }

    public void CloseCreditsButton()
    {
        DesativarOutrosMovimentos();
        creditosWindow.SetActive(false);
        camMoveBack = true;
        StartCoroutine(MostrarBotoesPrincipais());
    }

    private void DesativarOutrosMovimentos()
    {
        camMove = false;
        camMoveWall = false;
        camMoveBack = false;
        camMoveCredits = false;
    }

    private void OcultarBotoesPrincipais()
    {
        if(jogar) jogar.SetActive(false);
        if(opcoes) opcoes.SetActive(false);
        if(creditosBtn) creditosBtn.SetActive(false);
        if(sair) sair.SetActive(false);
    }

    IEnumerator AtivarJanela(GameObject janela)
    {
        yield return new WaitForSecondsRealtime(1f);
        if(janela) janela.SetActive(true);
    }

    IEnumerator MostrarBotoesPrincipais()
    {
        yield return new WaitForSecondsRealtime(1f);
        if(jogar) jogar.SetActive(true);
        if(opcoes) opcoes.SetActive(true);
        if(creditosBtn) creditosBtn.SetActive(true);
        if(sair) sair.SetActive(true);
    }
}