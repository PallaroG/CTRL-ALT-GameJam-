using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Botões")]
    public GameObject jogar;
    public GameObject opcoes;
    public GameObject sair;
    public GameObject opcoesWindow;

    [Header("Config Cam")]
    Camera cam;
    private Vector3 destino = new Vector3(0.099f, 1.59f, -17.73f);
    private Vector3 destinoParede = new Vector3(-15.33f, 2.49f, 15.03f);
    private Quaternion rotacaoParede = Quaternion.Euler(0, -90, 0);
    private Vector3 destinoInicio = new Vector3(0.099f, 3.25f, 6.36f);
    private Quaternion rotacaoInicio = Quaternion.Euler(12.245f, 0, 0);
    public float velocidadeInicio;
    public float velocidadeOpcoes;
    public float velocidadeRotacao;
    private bool camMove = false;
    private bool camMoveWall = false;
    private bool camMoveBack = false;

    public void Start()
    {
        cam = Camera.main;
    }
    void Update()
    {
        if(camMove)cam.transform.position = Vector3.MoveTowards(cam.transform.position,destino,velocidadeInicio * Time.deltaTime); // Move para inicio do jogo

        if(camMoveWall) // Move para parede de opções
        {
            Debug.Log("moving");
            cam.transform.position = Vector3.MoveTowards(cam.transform.position,destinoParede,velocidadeOpcoes * Time.deltaTime);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation,rotacaoParede,velocidadeRotacao * Time.deltaTime);
        }

        if(camMoveBack) // Move de volta para o centro
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position,destinoInicio,velocidadeOpcoes * Time.deltaTime);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation,rotacaoInicio,velocidadeRotacao * Time.deltaTime);
        }

        if(cam.transform.position == destino)camMove = false;
        else if(cam.transform.position == destinoInicio) camMoveBack = false;
        else if(cam.transform.position == destinoParede) camMoveWall = false;
    }
    public void ExitButton()
    {
        Application.Quit();
    }

    public void PlayButton()
    {
        camMove = true;
        jogar.SetActive(false);
        opcoes.SetActive(false);
        sair.SetActive(false);
    }

    public void OpenOptionsButton()
    {
        jogar.SetActive(false);
        opcoes.SetActive(false);
        sair.SetActive(false);
        StartCoroutine(AtivarOpcoes());
        camMoveWall = true;
    }

    public void CloseOptionsButton()
    {
        opcoesWindow.SetActive(false);
        StartCoroutine(DesativarOpcoes());
        camMoveBack = true;
    }

    IEnumerator AtivarOpcoes()
    {
        yield return new WaitForSeconds(1f);
        opcoesWindow.SetActive(true);
    }

    IEnumerator DesativarOpcoes()
    {
        yield return new WaitForSeconds(1f);
        jogar.SetActive(true);
        opcoes.SetActive(true);
        sair.SetActive(true);
    }

}
