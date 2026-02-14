using UnityEngine;
using UnityEngine.Events;

namespace BasicSystems.UX
{

    /*
    script para fazer objetos com interação
    -------------------------------------------
    colocar o mouse sobre o objeto mostra a opção de interagir
    vai chamar algum evento após o clique
    vai ativar uma UI seguindo o mouse
    */
    public class InteractableScript : MonoBehaviour
    {
        [Header("evento após clicar")]
        public UnityEvent evento;
        
        void OnMouseDown() //pra clicar 
        {
            Debug.Log("clicou");
            evento.Invoke();
        }

        void OnMouseEnter()
        {
            
        }
    }
    
}
