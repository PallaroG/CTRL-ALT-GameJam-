using UnityEngine;

public class PersonagemFila : MonoBehaviour
{
    public float velocidade = 3f;
    private Transform alvo;
    private bool movendo = false;

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
