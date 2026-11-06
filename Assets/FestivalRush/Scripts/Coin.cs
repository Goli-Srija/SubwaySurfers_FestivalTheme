using UnityEngine;

namespace FestivalRush
{
    /// <summary>Spinning gold coin. Flies to the player while the Magnet power-up is active.</summary>
    public class Coin : MonoBehaviour
    {
        void Update()
        {
            float dt = Time.deltaTime;
            transform.Rotate(0f, 180f * dt, 0f, Space.World);

            GameManager gm = GameManager.I;
            if (gm != null && gm.MagnetActive && gm.State == GameState.Playing && gm.Player != null)
            {
                Vector3 target = gm.Player.transform.position + Vector3.up;
                Vector3 d = target - transform.position;
                if (d.magnitude < 12f && d.z > -3f)
                    transform.position += d.normalized * Mathf.Max(25f, gm.Speed * 2.5f) * dt;
            }
        }

        public void Collect()
        {
            Destroy(gameObject);
        }
    }
}
