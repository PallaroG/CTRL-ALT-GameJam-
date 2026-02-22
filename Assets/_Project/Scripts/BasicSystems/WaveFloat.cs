using SuperAnimatedDialogue.Runtime;
using UnityEngine;


namespace BasicSystems.Effects
{
    public class WaveFloat : MonoBehaviour
    {
        public float amplitude = 0.5f;
        public float frequency = 1f;

        private Vector3 startPos;

        void Start()
        {
            startPos = transform.position;
        }

        void Update()
        {
            float offset = Mathf.Sin(Time.time * frequency) * amplitude;

            Vector3 basePos = transform.position;
            basePos.y += offset * Time.deltaTime;

            transform.position = basePos;
        }
    }
    
}
