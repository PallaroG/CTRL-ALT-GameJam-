using SuperAnimatedDialogue.Runtime;
using UnityEngine;

namespace BasicSystems.Effects
{
    public class WaveFloat : MonoBehaviour
    {
        public float amplitude = 0.5f;
        public float frequency = 1f;

        [Header("Randomize")]
        public bool randomizeValues = false;
        public Vector2 amplitudeRange = new Vector2(0.2f, 1f);
        public Vector2 frequencyRange = new Vector2(0.5f, 2f);

        private Vector3 startPos;

        void Start()
        {
            if (randomizeValues)
            {
                amplitude = Random.Range(amplitudeRange.x, amplitudeRange.y);
                frequency = Random.Range(frequencyRange.x, frequencyRange.y);
            }

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