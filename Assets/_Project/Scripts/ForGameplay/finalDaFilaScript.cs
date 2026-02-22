using UnityEngine;

public class finalDaFilaScript : MonoBehaviour
{
    public GameObject Character;
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("guarda o Ultimo da fila");
    }
}
