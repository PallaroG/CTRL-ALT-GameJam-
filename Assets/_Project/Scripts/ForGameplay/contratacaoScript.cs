using UnityEngine;

public class contratacaoScript : MonoBehaviour
{
    public finalDaFilaScript finalDaFilaScript;
    public filaManager filaManager;

    public void contratado()
    {
        if (!finalDaFilaScript.PodeContratar || finalDaFilaScript.personagem == null)
            return;

        PersonagemFila candidato = finalDaFilaScript.personagem;

        // 🔥 VERIFICA POR ID
        foreach (string idContratado in GameManager.Instance.personagensContratados)
        {
            if (candidato.OdeiaID(idContratado))
            {
                Debug.Log(candidato.ID + " odeia " + idContratado + " → dispensado.");

                filaManager.SairPrimeiro();
                finalDaFilaScript.ForcarRevalidacao();
                finalDaFilaScript.PodeContratar = false;
                finalDaFilaScript.personagem = null;
                return;
            }
        }

        // Pode contratar
        Debug.Log("Contratado: " + candidato.ID);

        if (candidato.CharacterButton != null)
            candidato.CharacterButton.SetActive(true);

        GameManager.Instance.AdicionarContratado(candidato);

        filaManager.SairPrimeiro();
        finalDaFilaScript.ForcarRevalidacao();
        finalDaFilaScript.PodeContratar = false;
        finalDaFilaScript.personagem = null;
    }
    public void dispensado()
    {
        if (!finalDaFilaScript.PodeContratar)
            return;

        filaManager.SairPrimeiro();
        finalDaFilaScript.ForcarRevalidacao();
        finalDaFilaScript.PodeContratar = false;
        finalDaFilaScript.personagem = null;
    }
}