using UnityEngine;

namespace FestivalRush
{
    public enum PowerUpType { Magnet, Shield, Double }

    /// <summary>Magnet (blue), Rangoli Shield (green), 2x Score (pink).</summary>
    public class PowerUp : MonoBehaviour
    {
        public PowerUpType type;
    }
}
