using UnityEngine;
using System.Collections.Generic;

public class filaManager : MonoBehaviour
{
    public Transform[] pontosFila;

    private List<PersonagemFila> personagens = new List<PersonagemFila>();

    public void EntrarNaFila(PersonagemFila p)
    {
        personagens.Add(p);
        AtualizarFila();
    }

    public void SairPrimeiro()
    {
        if (personagens.Count == 0)
            return;

        PersonagemFila primeiro = personagens[0];

        personagens.RemoveAt(0);

        Destroy(primeiro.gameObject); // ele some da cena

        AtualizarFila();
    }

    void AtualizarFila()
    {
        for (int i = 0; i < personagens.Count; i++)
        {
            personagens[i].DefinirPosicao(pontosFila[i]);
        }
    }

}
