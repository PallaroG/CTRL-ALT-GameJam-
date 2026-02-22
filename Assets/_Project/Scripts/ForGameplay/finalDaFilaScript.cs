using SuperAnimatedDialogue.Runtime;
using UnityEngine;

public class finalDaFilaScript : MonoBehaviour
{
    public GameObject Character;
    void OnTriggerEnter(Collider other)
    {
        PersonagemFila personagem = other.GetComponent<PersonagemFila>();

        if (personagem == null) return;

        Transform[] filhos = other.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in filhos)
        {
            if (t.CompareTag("dialogueBox"))
            {
                t.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        PersonagemFila personagem = other.GetComponent<PersonagemFila>();

        if (personagem == null) return;

        Transform[] filhos = other.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in filhos)
        {
            if (t.CompareTag("dialogueBox"))
            {
                t.gameObject.SetActive(false);
            }
        }
    }
}
