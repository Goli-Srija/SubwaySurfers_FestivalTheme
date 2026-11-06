using UnityEngine;

namespace FestivalRush
{
    /// <summary>3-lane runner: change lane, jump, slide. Builds its own character out of primitives.</summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float laneWidth = 2.5f;
        public float laneChangeSpeed = 16f;
        public float jumpVelocity = 9.5f;
        public float gravity = 30f;
        public float slideDuration = 0.75f;

        int lane = 1;                 // 0 = left, 1 = middle, 2 = right
        float vy;
        bool grounded = true;
        float slideTimer, graceTimer, runPhase;

        BoxCollider box;
        Transform model, legL, legR, armL, armR, shieldRing;

        static readonly Vector3 StandCenter = new Vector3(0f, 0.9f, 0f);
        static readonly Vector3 StandSize   = new Vector3(0.8f, 1.8f, 1.1f);
        static readonly Vector3 SlideCenter = new Vector3(0f, 0.4f, 0f);
        static readonly Vector3 SlideSize   = new Vector3(0.8f, 0.8f, 1.1f);

        public bool IsSliding { get { return slideTimer > 0f; } }

        public void Build()
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            box = gameObject.AddComponent<BoxCollider>();
            box.center = StandCenter;
            box.size = StandSize;

            model = new GameObject("Model").transform;
            model.SetParent(transform, false);

            Color kurta = FRColors.Orange;
            Color pyjama = new Color(0.95f, 0.92f, 0.85f);
            Color skin = new Color(0.78f, 0.55f, 0.38f);
            Color cap = new Color(0.55f, 0.1f, 0.45f);

            legL = Limb(new Vector3(-0.15f, 0.72f, 0f), new Vector3(0.22f, 0.72f, 0.24f), pyjama);
            legR = Limb(new Vector3(0.15f, 0.72f, 0f), new Vector3(0.22f, 0.72f, 0.24f), pyjama);
            FRMat.Prim(PrimitiveType.Cube, new Vector3(0f, 1.1f, 0f), new Vector3(0.62f, 0.8f, 0.36f), kurta, model, 0f, false, true);
            FRMat.Prim(PrimitiveType.Cube, new Vector3(0f, 0.74f, 0f), new Vector3(0.64f, 0.1f, 0.38f), FRColors.Gold, model, 0.6f);
            FRMat.Prim(PrimitiveType.Sphere, new Vector3(0f, 1.7f, 0f), Vector3.one * 0.42f, skin, model, 0f, false, true);
            FRMat.Prim(PrimitiveType.Cylinder, new Vector3(0f, 1.92f, 0f), new Vector3(0.36f, 0.06f, 0.36f), cap, model);
            armL = Limb(new Vector3(-0.41f, 1.45f, 0f), new Vector3(0.16f, 0.62f, 0.18f), kurta);
            armR = Limb(new Vector3(0.41f, 1.45f, 0f), new Vector3(0.16f, 0.62f, 0.18f), kurta);

            // Phuljhari (sparkler) in right hand
            FRMat.Prim(PrimitiveType.Cylinder, new Vector3(0f, -0.62f, 0.25f), new Vector3(0.04f, 0.25f, 0.04f), Color.gray, armR)
                 .transform.localRotation = Quaternion.Euler(70f, 0f, 0f);
            GameObject spark = FRMat.Prim(PrimitiveType.Sphere, new Vector3(0f, -0.55f, 0.5f), Vector3.one * 0.16f, FRColors.Yellow, armR, 5f);
            spark.AddComponent<Spinner>().axisSpeed = new Vector3(500f, 300f, 0f);

            // Warm glow that follows the player
            GameObject lg = new GameObject("Glow");
            lg.transform.SetParent(transform, false);
            lg.transform.localPosition = new Vector3(0f, 2.5f, 1.5f);
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.7f, 0.35f);
            l.range = 10f;
            l.intensity = 1.5f;

            // Rangoli shield: ring of glowing dots
            shieldRing = new GameObject("Shield").transform;
            shieldRing.SetParent(transform, false);
            shieldRing.localPosition = new Vector3(0f, 1f, 0f);
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * 2f / 10f;
                Color c = i % 2 == 0 ? FRColors.Green : FRColors.Yellow;
                FRMat.Prim(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.9f, 0f, Mathf.Sin(a) * 0.9f), Vector3.one * 0.18f, c, shieldRing, 3f);
            }
            shieldRing.gameObject.SetActive(false);
        }

        Transform Limb(Vector3 pivot, Vector3 size, Color c)
        {
            Transform p = new GameObject("Pivot").transform;
            p.SetParent(model, false);
            p.localPosition = pivot;
            FRMat.Prim(PrimitiveType.Cube, new Vector3(0f, -size.y * 0.5f, 0f), size, c, p, 0f, false, true);
            return p;
        }

        public void ResetPlayer()
        {
            lane = 1;
            vy = 0f;
            grounded = true;
            graceTimer = 0f;
            EndSlide();
            transform.position = Vector3.zero;
            model.localRotation = Quaternion.identity;
            model.localScale = Vector3.one;
            model.gameObject.SetActive(true);
        }

        void StartSlide()
        {
            slideTimer = slideDuration;
            box.center = SlideCenter;
            box.size = SlideSize;
            FRAudio.Slide();
        }

        void EndSlide()
        {
            slideTimer = 0f;
            if (box != null) { box.center = StandCenter; box.size = StandSize; }
        }

        void Update()
        {
            GameManager gm = GameManager.I;
            if (gm == null) return;
            float dt = Time.deltaTime;

            shieldRing.gameObject.SetActive(gm.ShieldActive);
            if (gm.ShieldActive) shieldRing.Rotate(0f, 200f * dt, 0f);

            if (gm.State == GameState.GameOver)
            {
                model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(-85f, 0f, 0f), dt * 6f);
                return;
            }
            if (gm.State != GameState.Playing)
            {
                float idle = Mathf.Sin(Time.time * 3f) * 15f;
                armL.localRotation = Quaternion.Euler(idle, 0f, 0f);
                armR.localRotation = Quaternion.Euler(-60f + idle, 0f, 0f); // wave the sparkler
                return;
            }

            // ----- input -----
            if (FRInput.Left)  lane = Mathf.Max(0, lane - 1);
            if (FRInput.Right) lane = Mathf.Min(2, lane + 1);
            if (FRInput.Up && grounded)
            {
                vy = jumpVelocity;
                grounded = false;
                EndSlide();
                FRAudio.Jump();
            }
            if (FRInput.Down)
            {
                if (!grounded) vy = -jumpVelocity * 1.6f; // fast-fall like Subway-style runners
                StartSlide();
            }

            if (slideTimer > 0f)
            {
                slideTimer -= dt;
                if (slideTimer <= 0f) EndSlide();
            }

            // ----- movement -----
            Vector3 p = transform.position;
            float targetX = (lane - 1) * laneWidth;
            p.z += gm.Speed * dt;
            p.x = Mathf.MoveTowards(p.x, targetX, laneChangeSpeed * dt);
            if (!grounded)
            {
                vy -= gravity * dt;
                p.y += vy * dt;
                if (p.y <= 0f) { p.y = 0f; vy = 0f; grounded = true; }
            }
            transform.position = p;

            Animate(dt, targetX - p.x, gm.Speed);

            // blink while invulnerable after the shield breaks
            if (graceTimer > 0f)
            {
                graceTimer -= dt;
                model.gameObject.SetActive(graceTimer <= 0f || Mathf.Repeat(graceTimer, 0.15f) > 0.075f);
            }
        }

        void Animate(float dt, float xOffset, float speed)
        {
            runPhase += dt * speed * 0.9f;
            float swing = grounded ? Mathf.Sin(runPhase) * 45f : 25f;
            legL.localRotation = Quaternion.Euler(swing, 0f, 0f);
            legR.localRotation = Quaternion.Euler(grounded ? -swing : -35f, 0f, 0f);
            armL.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            armR.localRotation = Quaternion.Euler(swing - 30f, 0f, 0f);

            Vector3 targetScale = IsSliding ? new Vector3(1f, 0.45f, 1.1f) : Vector3.one;
            model.localScale = Vector3.Lerp(model.localScale, targetScale, dt * 18f);
            model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(0f, xOffset * 12f, -xOffset * 6f), dt * 12f);
        }

        void OnTriggerEnter(Collider other)
        {
            GameManager gm = GameManager.I;
            if (gm == null || gm.State != GameState.Playing) return;

            Coin coin = other.GetComponent<Coin>();
            if (coin != null) { gm.AddCoin(); coin.Collect(); return; }

            PowerUp pu = other.GetComponent<PowerUp>();
            if (pu != null) { gm.ActivatePowerUp(pu.type); Destroy(pu.gameObject); return; }

            Obstacle ob = other.GetComponentInParent<Obstacle>();
            if (ob != null)
            {
                if (graceTimer > 0f) return;
                if (gm.ShieldActive)
                {
                    gm.BreakShield();
                    graceTimer = 1.2f;
                    ob.Smash();
                    return;
                }
                gm.EndRun();
            }
        }
    }
}
