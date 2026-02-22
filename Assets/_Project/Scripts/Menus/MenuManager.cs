using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Botões")]
    public GameObject jogar;
    public GameObject sair;

    [Header("Config Cam")]
    Camera cam;
    private Vector3 destino = new Vector3(0.099f, 1.59f, -17.73f);
    public float velocidade;
    private bool camMove = false;

    public void Start()
    {
        cam = Camera.main;
    }
    void Update()
    {
        if(camMove)cam.transform.position = Vector3.MoveTowards(cam.transform.position,destino,velocidade * Time.deltaTime);
        if(cam.transform.position == destino) camMove = false;
    }
    public void ExitButton()
    {
        Application.Quit();
    }

    public void PlayButton()
    {
        camMove = true;
        jogar.SetActive(false);
        sair.SetActive(false);
    }
}
