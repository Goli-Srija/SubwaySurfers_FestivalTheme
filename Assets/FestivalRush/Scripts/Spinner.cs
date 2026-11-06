using UnityEngine;

namespace FestivalRush
{
    /// <summary>Rotates and optionally bobs an object (lanterns, power-ups, sparkler).</summary>
    public class Spinner : MonoBehaviour
    {
        public Vector3 axisSpeed = new Vector3(0f, 90f, 0f);
        public float bobAmp = 0f;
        public float bobSpeed = 2.5f;

        Vector3 basePos;
        float seed;

        void Start()
        {
            basePos = transform.localPosition;
            seed = Random.value * 10f;
        }

        void Update()
        {
            transform.Rotate(axisSpeed * Time.deltaTime, Space.World);
            if (bobAmp > 0f)
                transform.localPosition = basePos + Vector3.up * Mathf.Sin((Time.time + seed) * bobSpeed) * bobAmp;
        }
    }
}
