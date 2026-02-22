using UnityEngine;

public class contratacaoScript : MonoBehaviour
{
    /*
        script para controlar os botões de contratação
        -----------------------------------------------
        verifica qual o personagem é o primeiro na fila
        método que contrata o personagem
        método que dispensa o personagem

    */

    public filaManager filaManager;
    public void contratado()
    {
        Debug.Log("Opa vem pra ca");
        filaManager.SairPrimeiro();
        //adicionar ele na tabela
    }

    public void dispensado()
    {
        Debug.Log("Sai pra lá");
        filaManager.SairPrimeiro();
    }


}
