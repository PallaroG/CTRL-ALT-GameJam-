using UnityEngine;
using UnityEngine.Events;

namespace BasicSystems.UX
{

    /*
        script para fazer objetos com interação
        -------------------------------------------
        colocar o mouse sobre o objeto mostra a opção de interagir
        vai chamar algum evento após o clique
        vai ativar uma UI enquanto o mouse estiver por cima do objeto
    */
    public class InteractableScript : MonoBehaviour
    {

        [Header("materials configuration")]
        /*na hierarquia, no componente Mesh Renderer
        criar mais um element em materials
        com o mesmo material do objeto
        */
        public Renderer rend;
        public Material normalMaterial;
        public Material outlineMaterial;

        [Header("EVENTO CLICK")]


        public UnityEvent evento;
        public float descerClick = 0.1f;
        public float levantarClick = 0.1f;
        
        void Start()
        {
            rend = GetComponent<Renderer>();
            normalMaterial = rend.material;

        }

        void OnMouseDown() //clicar no objeto
        {
            Debug.Log("clicou");
            evento.Invoke();
            transform.position += Vector3.down * descerClick;

        }
        void OnMouseUp()
        {
            transform.position += Vector3.up * levantarClick;
        }

        void OnMouseEnter() //sobre o objeto
        {
   
            rend.material = outlineMaterial;
        }
        void OnMouseExit() 
        {

            rend.material = normalMaterial;
        }


    }
    
}
