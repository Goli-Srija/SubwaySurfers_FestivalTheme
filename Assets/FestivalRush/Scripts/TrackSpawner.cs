using System.Collections.Generic;
using UnityEngine;

namespace FestivalRush
{
    /// <summary>Endless festival street: road, rangoli, string lights, lanterns, diyas, buildings, obstacles, coins.</summary>
    public class TrackSpawner : MonoBehaviour
    {
        public float segmentLength = 30f;
        public int segmentsAhead = 6;
        public float laneWidth = 2.5f;

        Transform player;
        float nextZ, nextRowZ;
        int lastFree = 1;
        readonly Queue<GameObject> segs = new Queue<GameObject>();
        readonly Queue<float> segEnds = new Queue<float>();

        float HalfRoad { get { return laneWidth * 1.5f + 0.6f; } }

        public void Init(Transform p)
        {
            player = p;
            ResetTrack();
        }

        public void ResetTrack()
        {
            while (segs.Count > 0)
            {
                GameObject g = segs.Dequeue();
                if (g != null) Destroy(g);
            }
            segEnds.Clear();
            nextZ = -30f;
            nextRowZ = 45f;
            lastFree = 1;
            Fill();
        }

        void Update()
        {
            if (player == null) return;
            Fill();
            while (segEnds.Count > 0 && segEnds.Peek() < player.position.z - 40f)
            {
                segEnds.Dequeue();
                Destroy(segs.Dequeue());
            }
        }

        void Fill()
        {
            while (nextZ < player.position.z + segmentLength * segmentsAhead) SpawnSegment();
        }

        // ------------------------------------------------------------------ street
        void SpawnSegment()
        {
            float z0 = nextZ, L = segmentLength, zc = z0 + L * 0.5f, hr = HalfRoad;
            nextZ += L;

            Transform seg = new GameObject("Segment").transform;
            seg.SetParent(transform, false);
            segs.Enqueue(seg.gameObject);
            segEnds.Enqueue(z0 + L);

            // Road
            FRMat.Prim(PrimitiveType.Cube, new Vector3(0f, -0.25f, zc), new Vector3(hr * 2f, 0.5f, L), FRColors.Road, seg);

            // Glowing dashed lane lines
            for (float dz = 0f; dz < L; dz += 3f)
                for (int s = -1; s <= 1; s += 2)
                    FRMat.Prim(PrimitiveType.Cube, new Vector3(s * laneWidth * 0.5f, 0.02f, z0 + dz + 0.75f),
                               new Vector3(0.12f, 0.02f, 1.5f), FRColors.Gold, seg, 1.2f);

            // Rangoli circles in each lane
            for (float dz = 5f; dz < L; dz += 10f)
                for (int lane = 0; lane < 3; lane++)
                {
                    float x = (lane - 1) * laneWidth;
                    Color a = FRColors.Random(), b = FRColors.Random();
                    FRMat.Prim(PrimitiveType.Cylinder, new Vector3(x, 0.01f, z0 + dz), new Vector3(1.5f, 0.01f, 1.5f), a, seg, 0.4f);
                    FRMat.Prim(PrimitiveType.Cylinder, new Vector3(x, 0.02f, z0 + dz), new Vector3(1.0f, 0.01f, 1.0f), b, seg, 0.4f);
                    FRMat.Prim(PrimitiveType.Cylinder, new Vector3(x, 0.03f, z0 + dz), new Vector3(0.45f, 0.01f, 0.45f), FRColors.Yellow, seg, 0.8f);
                }

            for (int s = -1; s <= 1; s += 2)
            {
                // Sidewalk
                FRMat.Prim(PrimitiveType.Cube, new Vector3(s * (hr + 1.5f), -0.05f, zc), new Vector3(3f, 0.5f, L), FRColors.Sidewalk, seg);

                // Diyas (oil lamps) along the kerb
                for (float dz = 2.5f; dz < L; dz += 5f)
                {
                    float x = s * (hr + 0.35f);
                    FRMat.Prim(PrimitiveType.Cylinder, new Vector3(x, 0.28f, z0 + dz), new Vector3(0.3f, 0.08f, 0.3f), FRColors.Clay, seg);
                    FRMat.Prim(PrimitiveType.Sphere, new Vector3(x, 0.42f, z0 + dz), new Vector3(0.1f, 0.18f, 0.1f), FRColors.Flame, seg, 4f);
                }

                // Buildings decorated with light strips
                float z = z0;
                while (z < z0 + L - 1f)
                {
                    float w = Mathf.Min(Random.Range(5f, 9f), z0 + L - z);
                    float h = Random.Range(5f, 12f);
                    float d = 6f;
                    float bx = s * (hr + 3f + d * 0.5f);
                    FRMat.Prim(PrimitiveType.Cube, new Vector3(bx, h * 0.5f, z + w * 0.5f), new Vector3(d, h, w - 0.3f), FRColors.RandomBuilding(), seg);

                    Color strip = FRColors.Random();
                    float face = s * (hr + 3f - 0.03f);
                    for (float fy = 2.2f; fy < h; fy += 2.4f)
                        FRMat.Prim(PrimitiveType.Cube, new Vector3(face, fy, z + w * 0.5f), new Vector3(0.06f, 0.12f, w - 0.5f), strip, seg, 2.5f);
                    // glowing windows
                    for (float fy = 1.2f; fy < h - 1f; fy += 2.4f)
                        if (Random.value < 0.75f)
                            FRMat.Prim(PrimitiveType.Cube, new Vector3(face, fy, z + w * 0.5f), new Vector3(0.05f, 0.9f, 0.8f), new Color(1f, 0.8f, 0.45f), seg, 1.5f);
                    z += w;
                }
            }

            // Poles with string lights across the street + akash kandil lanterns
            for (float dz = 7.5f; dz < L; dz += 15f)
            {
                float zz = z0 + dz;
                for (int s = -1; s <= 1; s += 2)
                    FRMat.Prim(PrimitiveType.Cylinder, new Vector3(s * (hr + 0.6f), 2.75f, zz), new Vector3(0.18f, 2.75f, 0.18f), FRColors.Pole, seg);

                int n = 16;
                for (int i = 0; i <= n; i++)
                {
                    float t = (float)i / n;
                    float x = Mathf.Lerp(-(hr + 0.6f), hr + 0.6f, t);
                    float y = 5.4f - Mathf.Sin(t * Mathf.PI) * 1.1f;
                    FRMat.Prim(PrimitiveType.Sphere, new Vector3(x, y, zz), Vector3.one * 0.22f, FRColors.Festive[i % FRColors.Festive.Length], seg, 3f);
                }

                if (Random.value < 0.8f)
                {
                    float lx = Random.Range(-1, 2) * laneWidth;
                    float ly = 5.4f - Mathf.Sin(Mathf.InverseLerp(-(hr + 0.6f), hr + 0.6f, lx) * Mathf.PI) * 1.1f;
                    FRMat.Prim(PrimitiveType.Cylinder, new Vector3(lx, ly - 0.35f, zz), new Vector3(0.02f, 0.35f, 0.02f), Color.black, seg);
                    GameObject lantern = FRMat.Prim(PrimitiveType.Cube, new Vector3(lx, ly - 1.1f, zz), Vector3.one * 0.6f,
                                                    Random.value < 0.5f ? FRColors.Orange : FRColors.Red, seg, 2f);
                    lantern.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
                    lantern.AddComponent<Spinner>().axisSpeed = new Vector3(0f, 40f, 0f);
                }
            }

            // Obstacle rows
            while (nextRowZ < z0 + L)
            {
                if (nextRowZ >= z0) SpawnRow(nextRowZ, seg);
                float speed = GameManager.I != null ? Mathf.Max(GameManager.I.Speed, GameManager.I.startSpeed) : 12f;
                nextRowZ += Random.Range(16f, 24f) + speed * 0.3f;
            }
        }

        // ------------------------------------------------------------------ obstacles
        void SpawnRow(float z, Transform seg)
        {
            // Always leave one free lane, and keep it near the previous free lane so the run is always possible.
            int free = Mathf.Clamp(lastFree + Random.Range(-1, 2), 0, 2);
            lastFree = free;

            for (int lane = 0; lane < 3; lane++)
            {
                float x = (lane - 1) * laneWidth;
                if (lane == free)
                {
                    float r = Random.value;
                    if (r < 0.08f) SpawnPowerUp(x, z, seg);
                    else if (r < 0.75f) CoinLine(x, z - 6f, 6, seg);
                    continue;
                }

                float r2 = Random.value;
                if (r2 < 0.28f)
                {
                    LowBarrier(x, z, seg);
                    if (Random.value < 0.5f) CoinArc(x, z, seg);
                }
                else if (r2 < 0.52f) HighBarrier(x, z, seg);
                else if (r2 < 0.82f) FestivalFloat(x, z, seg);
                else if (r2 < 0.92f) CoinLine(x, z - 4f, 5, seg);
            }
        }

        Transform Root(string name, float x, float z, Transform seg)
        {
            Transform t = new GameObject(name).transform;
            t.SetParent(seg, false);
            t.localPosition = new Vector3(x, 0f, z);
            t.gameObject.AddComponent<Obstacle>();
            return t;
        }

        // Jump over it
        void LowBarrier(float x, float z, Transform seg)
        {
            Transform t = Root("Barricade", x, z, seg);
            FRMat.Prim(PrimitiveType.Cube, new Vector3(0f, 0.6f, 0f), new Vector3(2.1f, 0.5f, 0.35f), FRColors.Orange, t, 0.3f, true, true);
            for (int i = -1; i <= 1; i++)
                FRMat.Prim(PrimitiveType.Cube, new Vector3(i * 0.65f, 0.6f, -0.01f), new Vector3(0.3f, 0.5f, 0.36f), Color.white, t, 0.3f);
            for (int s = -1; s <= 1; s += 2)
                FRMat.Prim(PrimitiveType.Cube, new Vector3(s * 0.9f, 0.2f, 0f), new Vector3(0.15f, 0.4f, 0.15f), FRColors.Pole, t);
            for (float mx = -0.95f; mx <= 0.96f; mx += 0.32f) // marigold garland
                FRMat.Prim(PrimitiveType.Sphere, new Vector3(mx, 0.92f, 0f), Vector3.one * 0.18f, FRColors.Yellow, t, 0.8f);
        }

        // Slide under it (toran gate with hanging marigolds)
        void HighBarrier(float x, float z, Transform seg)
        {
            Transform t = Root("ToranGate", x, z, seg);
            for (int s = -1; s <= 1; s += 2)
                FRMat.Prim(PrimitiveType.Cylinder, new Vector3(s * 1.05f, 1.45f, 0f), new Vector3(0.15f, 1.45f, 0.15f), FRColors.Red, t, 0.3f);
            FRMat.Prim(PrimitiveType.Cube, new Vector3(0f, 2.7f, 0f), new Vector3(2.4f, 0.35f, 0.3f), FRColors.Magenta, t, 0.5f, false, true);
            for (float mx = -0.85f; mx <= 0.86f; mx += 0.283f)
                for (int k = 0; k < 4; k++)
                    FRMat.Prim(PrimitiveType.Sphere, new Vector3(mx, 2.35f - k * 0.27f, 0f), Vector3.one * 0.2f,
                               k % 2 == 0 ? FRColors.Orange : FRColors.Yellow, t, 0.6f);

            BoxCollider c = t.gameObject.AddComponent<BoxCollider>();
            c.isTrigger = true;
            c.center = new Vector3(0f, 2.05f, 0f); // blocks from 1.3 m up: standing hits, sliding passes
            c.size = new Vector3(2.1f, 1.5f, 0.4f);
        }

        // Change lane to avoid it (like the trains)
        void FestivalFloat(float x, float z, Transform seg)
        {
            Transform t = Root("FestivalFloat", x, z, seg);
            float len = 7f;
            Color body = Random.value < 0.5f ? FRColors.Purple : FRColors.Magenta;
            FRMat.Prim(PrimitiveType.Cube, new Vector3(0f, 1.5f, len * 0.5f), new Vector3(2.2f, 2.4f, len), body, t, 0.15f, true, true);
            FRMat.Prim(PrimitiveType.Cube, new Vector3(0f, 0.32f, len * 0.5f), new Vector3(2.25f, 0.12f, len), FRColors.Gold, t, 1f);
            FRMat.Prim(PrimitiveType.Sphere, new Vector3(0f, 3.0f, len * 0.5f), new Vector3(1.3f, 0.9f, 1.3f), FRColors.Gold, t, 0.6f, false, true);

            // Glowing rangoli on the front face
            GameObject face = FRMat.Prim(PrimitiveType.Cylinder, new Vector3(0f, 1.6f, -0.02f), new Vector3(1.5f, 0.02f, 1.5f), FRColors.Yellow, t, 1.5f);
            face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            GameObject inner = FRMat.Prim(PrimitiveType.Cylinder, new Vector3(0f, 1.6f, -0.04f), new Vector3(0.8f, 0.02f, 0.8f), FRColors.Red, t, 1.5f);
            inner.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            // Bulbs along the roof edges
            for (float bz = 0.3f; bz < len; bz += 0.9f)
                for (int s = -1; s <= 1; s += 2)
                    FRMat.Prim(PrimitiveType.Sphere, new Vector3(s * 1.05f, 2.75f, bz), Vector3.one * 0.18f, FRColors.Random(), t, 3f);

            // Wheels
            for (int s = -1; s <= 1; s += 2)
                for (float wz = 1f; wz < len; wz += len - 2f)
                {
                    GameObject wheel = FRMat.Prim(PrimitiveType.Cylinder, new Vector3(s * 1.05f, 0.35f, wz), new Vector3(0.6f, 0.08f, 0.6f), FRColors.Pole, t);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
        }

        // ------------------------------------------------------------------ pickups
        void CoinLine(float x, float zStart, int count, Transform seg)
        {
            for (int i = 0; i < count; i++) MakeCoin(new Vector3(x, 0.8f, zStart + i * 2f), seg);
        }

        void CoinArc(float x, float z, Transform seg)
        {
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                MakeCoin(new Vector3(x, 1.2f + Mathf.Sin(t * Mathf.PI) * 1.0f, z - 3f + t * 6f), seg);
            }
        }

        void MakeCoin(Vector3 pos, Transform seg)
        {
            GameObject c = FRMat.Prim(PrimitiveType.Cylinder, pos, new Vector3(0.7f, 0.06f, 0.7f), FRColors.Gold, seg, 1.5f);
            c.name = "Coin";
            c.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
            BoxCollider bc = c.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(1.2f, 14f, 1.2f);
            c.AddComponent<Coin>();
        }

        void SpawnPowerUp(float x, float z, Transform seg)
        {
            PowerUpType type = (PowerUpType)Random.Range(0, 3);
            Color c = type == PowerUpType.Magnet ? FRColors.Cyan : type == PowerUpType.Shield ? FRColors.Green : FRColors.Magenta;

            GameObject go = FRMat.Prim(PrimitiveType.Sphere, new Vector3(x, 1.1f, z), Vector3.one * 0.75f, c, seg, 2.5f, true);
            go.name = "PowerUp_" + type;
            ((SphereCollider)go.GetComponent<Collider>()).radius = 0.9f;
            go.AddComponent<PowerUp>().type = type;
            Spinner sp = go.AddComponent<Spinner>();
            sp.axisSpeed = new Vector3(0f, 120f, 0f);
            sp.bobAmp = 0.25f;
            // little orbiting ring so it reads as a power-up
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                FRMat.Prim(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.9f, 0f, Mathf.Sin(a) * 0.9f), Vector3.one * 0.2f, Color.white, go.transform, 3f);
            }
        }
    }
}
