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

        primeiro.gameObject.SetActive(false);

        AtualizarFila();
    }

    void AtualizarFila()
    {
        for (int i = 0; i < personagens.Count; i++)
        {
            personagens[i].DefinirPosicao(pontosFila[i]);
        }
    }

    // Retorna verdadeiro se o número de pessoas for igual ou maior que o número de pontos no chão
    public bool FilaCheia()
    {
        return personagens.Count >= pontosFila.Length;
    }

}
