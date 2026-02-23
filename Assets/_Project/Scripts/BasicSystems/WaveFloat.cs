using UnityEngine;

namespace BasicSystems.Effects
{
    public class WaveFloat : MonoBehaviour
    {
        public float amplitude = 0.5f;
        public float frequency = 1f;

        public bool randomizeOnStart = true;
        public Vector2 randomAmplitudeRange = new Vector2(0.2f, 1f);
        public Vector2 randomFrequencyRange = new Vector2(0.5f, 2f);

        private float randomOffset;
        private float previousOffset;

        void Start()
        {
            if (randomizeOnStart)
            {
                amplitude = Random.Range(randomAmplitudeRange.x, randomAmplitudeRange.y);
                frequency = Random.Range(randomFrequencyRange.x, randomFrequencyRange.y);
            }

            randomOffset = Random.Range(0f, 100f);
        }

        void Update()
        {
            float currentOffset = Mathf.Sin((Time.time + randomOffset) * frequency) * amplitude;

            // Remove offset anterior
            transform.position -= Vector3.up * previousOffset;

            // Aplica novo offset
            transform.position += Vector3.up * currentOffset;

            previousOffset = currentOffset;
        }
    }
}