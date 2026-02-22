using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Botões")]
    public GameObject jogar;
    public GameObject opcoes;
    public GameObject creditosBtn; // NOVO: Referência para o botão de Créditos
    public GameObject sair;
    
    [Header("Telas (Windows)")]
    public GameObject opcoesWindow;
    public GameObject creditosWindow; // NOVO: Referência para a tela de Créditos

    [Header("Config Cam")]
    Camera cam;
    private Vector3 destino = new Vector3(0.099f, 1.59f, -17.73f);
    private Vector3 destinoParede = new Vector3(-15.33f, 2.49f, 15.03f);
    private Vector3 créditosParede = new Vector3(-15.33f, 2.49f, 3.19f);
    
    private Quaternion rotacaoParede = Quaternion.Euler(0, -90, 0);
    private Quaternion rotacaoCreditos = Quaternion.Euler(0, -90, 0); // Assumindo que a parede de créditos tem a mesma rotação
    
    private Vector3 destinoInicio = new Vector3(0.099f, 3.25f, 6.36f);
    private Quaternion rotacaoInicio = Quaternion.Euler(12.245f, 0, 0);
    
    public float velocidadeInicio;
    public float velocidadeOpcoes;
    public float velocidadeRotacao;
    
    private bool camMove = false;
    private bool camMoveWall = false;
    private bool camMoveBack = false;
    private bool camMoveCredits = false; // NOVO: Flag para mover para os créditos

    public void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Movimento para início do jogo
        if(camMove) 
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, destino, velocidadeInicio * Time.deltaTime);

        // Movimento para parede de opções
        if(camMoveWall) 
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, destinoParede, velocidadeOpcoes * Time.deltaTime);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, rotacaoParede, velocidadeRotacao * Time.deltaTime);
        }

        // NOVO: Movimento para parede de créditos
        if(camMoveCredits) 
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, créditosParede, velocidadeOpcoes * Time.deltaTime);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, rotacaoCreditos, velocidadeRotacao * Time.deltaTime);
        }

        // Movimento de volta para o centro
        if(camMoveBack) 
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, destinoInicio, velocidadeOpcoes * Time.deltaTime);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, rotacaoInicio, velocidadeRotacao * Time.deltaTime);
        }

        // Parar os movimentos quando chegar no destino
        if(cam.transform.position == destino) camMove = false;
        else if(cam.transform.position == destinoInicio) camMoveBack = false;
        else if(cam.transform.position == destinoParede) camMoveWall = false;
        else if(cam.transform.position == créditosParede) camMoveCredits = false; // Para a câmera nos créditos
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

    // NOVO: Abrir Créditos
    public void OpenCreditsButton()
    {
        DesativarOutrosMovimentos();
        camMoveCredits = true;
        OcultarBotoesPrincipais();
        StartCoroutine(AtivarJanela(creditosWindow));
    }

    // NOVO: Fechar Créditos
    public void CloseCreditsButton()
    {
        DesativarOutrosMovimentos();
        creditosWindow.SetActive(false);
        camMoveBack = true;
        StartCoroutine(MostrarBotoesPrincipais());
    }

    // --- FUNÇÕES AUXILIARES PARA LIMPAR O CÓDIGO ---

    // Garante que a câmera não tente ir para dois lugares ao mesmo tempo se o jogador clicar rápido
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

    // Coroutine única que serve tanto para ligar Opções quanto Créditos
    IEnumerator AtivarJanela(GameObject janela)
    {
        yield return new WaitForSeconds(1f);
        if(janela) janela.SetActive(true);
    }

    // Coroutine única para religar os botões do menu
    IEnumerator MostrarBotoesPrincipais()
    {
        yield return new WaitForSeconds(1f);
        if(jogar) jogar.SetActive(true);
        if(opcoes) opcoes.SetActive(true);
        if(creditosBtn) creditosBtn.SetActive(true);
        if(sair) sair.SetActive(true);
    }
}