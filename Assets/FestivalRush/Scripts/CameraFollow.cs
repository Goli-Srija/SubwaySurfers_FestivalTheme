using UnityEngine;

namespace FestivalRush
{
    /// <summary>Third-person runner camera: behind and above the player.</summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float height = 4.3f;
        public float distance = 7.5f;
        public float lookAhead = 6f;
        public float sideFollow = 0.6f;

        public void SnapNow()
        {
            if (target == null) return;
            transform.position = Desired();
            Look();
        }

        Vector3 Desired()
        {
            Vector3 p = target.position;
            return new Vector3(p.x * sideFollow, height + p.y * 0.35f, p.z - distance);
        }

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 d = Desired();
            Vector3 cur = transform.position;
            cur.x = Mathf.Lerp(cur.x, d.x, Time.deltaTime * 8f);
            cur.y = Mathf.Lerp(cur.y, d.y, Time.deltaTime * 6f);
            cur.z = d.z; // never lag behind at high speed
            transform.position = cur;
            Look();
        }

        void Look()
        {
            Vector3 p = target.position;
            transform.LookAt(new Vector3(p.x * sideFollow, 1.2f, p.z + lookAhead));
        }
    }
}
