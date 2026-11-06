using UnityEngine;

namespace FestivalRush
{
    /// <summary>Marker for anything that ends the run when touched.</summary>
    public class Obstacle : MonoBehaviour
    {
        public void Smash()
        {
            Firework.Burst(transform.position + Vector3.up * 1.5f, FRColors.Random(), 0.6f);
            Destroy(gameObject);
        }
    }
}
