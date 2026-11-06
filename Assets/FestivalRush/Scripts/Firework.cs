using System.Collections.Generic;
using UnityEngine;

namespace FestivalRush
{
    /// <summary>Simple glowing firework burst made of small spheres (no particle assets needed).</summary>
    public class Firework : MonoBehaviour
    {
        class Spark { public Transform t; public Vector3 v; }

        readonly List<Spark> sparks = new List<Spark>();
        float life;
        float maxLife = 1.6f;
        float size = 0.28f;

        public static void Burst(Vector3 pos, Color color, float scale)
        {
            GameObject go = new GameObject("Firework");
            go.transform.position = pos;
            go.AddComponent<Firework>().Make(color, scale);
        }

        void Make(Color color, float scale)
        {
            size *= scale;
            Color second = FRColors.Random();
            int count = 26;
            for (int i = 0; i < count; i++)
            {
                GameObject s = FRMat.Prim(PrimitiveType.Sphere, Vector3.zero, Vector3.one * size,
                                          i % 3 == 0 ? second : color, transform, 4f);
                sparks.Add(new Spark { t = s.transform, v = Random.onUnitSphere * Random.Range(6f, 9f) * scale });
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            life += dt;
            float k = Mathf.Clamp01(1f - life / maxLife);
            for (int i = 0; i < sparks.Count; i++)
            {
                Spark s = sparks[i];
                s.v += Vector3.down * 4f * dt;
                s.v *= 1f - 1.2f * dt; // air drag
                s.t.localPosition += s.v * dt;
                s.t.localScale = Vector3.one * size * k;
            }
            if (life >= maxLife) Destroy(gameObject);
        }
    }
}
