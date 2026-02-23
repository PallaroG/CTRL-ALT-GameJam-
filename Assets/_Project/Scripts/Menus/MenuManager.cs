using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Botões Principais")]
    public GameObject jogar;
    public GameObject opcoes;
    public GameObject creditosBtn;
    public GameObject sair;

    [Header("Imagens Extras do Menu")]
    public GameObject[] imagensExtras; // ← ARRASTA AQUI AS IMAGENS

    [Header("Telas (Windows)")]
    public GameObject opcoesWindow;
    public GameObject creditosWindow;

    [Header("Áudio")]
    public AudioSource musicaDoMenu;
    public AudioSource musicaDoJogo;

    [Header("Config Cam")]
    Camera cam;

    private Vector3 destino = new Vector3(0.099f, 1.59f, -17.73f);
    private Vector3 destinoParede = new Vector3(-15.33f, 2.49f, 15.03f);
    private Vector3 creditosParede = new Vector3(-15.33f, 2.49f, 3.19f);

    private Quaternion rotacaoParede = Quaternion.Euler(0, -90, 0);
    private Quaternion rotacaoCreditos = Quaternion.Euler(0, -90, 0);

    private Vector3 destinoInicio = new Vector3(0.099f, 3.25f, 6.36f);
    private Quaternion rotacaoInicio = Quaternion.Euler(12.245f, 0, 0);

    public float velocidadeInicio = 3f;
    public float velocidadeOpcoes = 3f;
    public float velocidadeRotacao = 3f;

    private bool camMove = false;
    private bool camMoveWall = false;
    private bool camMoveBack = false;
    private bool camMoveCredits = false;

    void Start()
    {
        cam = Camera.main;
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (camMove)
            cam.transform.position = Vector3.MoveTowards(
                cam.transform.position,
                destino,
                velocidadeInicio * Time.unscaledDeltaTime
            );

        if (camMoveWall)
        {
            cam.transform.position = Vector3.MoveTowards(
                cam.transform.position,
                destinoParede,
                velocidadeOpcoes * Time.unscaledDeltaTime
            );

            cam.transform.rotation = Quaternion.Slerp(
                cam.transform.rotation,
                rotacaoParede,
                velocidadeRotacao * Time.unscaledDeltaTime
            );
        }

        if (camMoveCredits)
        {
            cam.transform.position = Vector3.MoveTowards(
                cam.transform.position,
                creditosParede,
                velocidadeOpcoes * Time.unscaledDeltaTime
            );

            cam.transform.rotation = Quaternion.Slerp(
                cam.transform.rotation,
                rotacaoCreditos,
                velocidadeRotacao * Time.unscaledDeltaTime
            );
        }

        if (camMoveBack)
        {
            cam.transform.position = Vector3.MoveTowards(
                cam.transform.position,
                destinoInicio,
                velocidadeOpcoes * Time.unscaledDeltaTime
            );

            cam.transform.rotation = Quaternion.Slerp(
                cam.transform.rotation,
                rotacaoInicio,
                velocidadeRotacao * Time.unscaledDeltaTime
            );
        }
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void PlayButton()
    {
        DesativarOutrosMovimentos();
        camMove = true;
        OcultarElementosPrincipais();

        Time.timeScale = 1f;

        if (musicaDoMenu != null)
            musicaDoMenu.Pause();

        if (musicaDoJogo != null)
            musicaDoJogo.Play();
    }

    public void OpenOptionsButton()
    {
        DesativarOutrosMovimentos();
        camMoveWall = true;
        OcultarElementosPrincipais();
        StartCoroutine(AtivarJanela(opcoesWindow));
    }

    public void CloseOptionsButton()
    {
        DesativarOutrosMovimentos();
        opcoesWindow.SetActive(false);
        camMoveBack = true;
        StartCoroutine(MostrarElementosPrincipais());
    }

    public void OpenCreditsButton()
    {
        DesativarOutrosMovimentos();
        camMoveCredits = true;
        OcultarElementosPrincipais();
        StartCoroutine(AtivarJanela(creditosWindow));
    }

    public void CloseCreditsButton()
    {
        DesativarOutrosMovimentos();
        creditosWindow.SetActive(false);
        camMoveBack = true;
        StartCoroutine(MostrarElementosPrincipais());
    }

    private void DesativarOutrosMovimentos()
    {
        camMove = false;
        camMoveWall = false;
        camMoveBack = false;
        camMoveCredits = false;
    }

    private void OcultarElementosPrincipais()
    {
        if (jogar) jogar.SetActive(false);
        if (opcoes) opcoes.SetActive(false);
        if (creditosBtn) creditosBtn.SetActive(false);
        if (sair) sair.SetActive(false);

        foreach (GameObject img in imagensExtras)
        {
            if (img != null)
                img.SetActive(false);
        }
    }

    IEnumerator MostrarElementosPrincipais()
    {
        yield return new WaitForSecondsRealtime(1f);

        if (jogar) jogar.SetActive(true);
        if (opcoes) opcoes.SetActive(true);
        if (creditosBtn) creditosBtn.SetActive(true);
        if (sair) sair.SetActive(true);

        foreach (GameObject img in imagensExtras)
        {
            if (img != null)
                img.SetActive(true);
        }
    }

    IEnumerator AtivarJanela(GameObject janela)
    {
        yield return new WaitForSecondsRealtime(1f);
        if (janela) janela.SetActive(true);
    }
}