using UnityEngine;
using System.Collections.Generic;

public class PersonagemFila : MonoBehaviour
{
    [Header("IDENTIDADE")]
    public string ID; // ← COLOCA UM ID ÚNICO NO INSPECTOR

    public float velocidade = 3f;
    public GameObject CharacterButton;

    [Header("Relacionamentos")]
    public List<PersonagemFila> naoGostaDe = new List<PersonagemFila>();

    private Transform alvo;
    private bool movendo = false;

    public bool OdeiaID(string outroID)
    {
        foreach (var p in naoGostaDe)
        {
            if (p != null && p.ID == outroID)
                return true;
        }
        return false;
    }

    public void DefinirPosicao(Transform ponto)
    {
        alvo = ponto;
        movendo = true;
    }

    void Update()
    {
        if (movendo && alvo != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                alvo.position,
                velocidade * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, alvo.position) < 0.01f)
            {
                transform.position = alvo.position;
                movendo = false;
            }
        }
    }

    private void Start()
    {
        filaManager fila = FindObjectOfType<filaManager>();
        fila.EntrarNaFila(this);
    }
}