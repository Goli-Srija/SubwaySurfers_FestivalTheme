using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FestivalRush
{
    /// <summary>Festive colour palette used everywhere in the game.</summary>
    public static class FRColors
    {
        public static readonly Color Orange  = new Color(1f, 0.5f, 0.05f);
        public static readonly Color Magenta = new Color(1f, 0.12f, 0.6f);
        public static readonly Color Yellow  = new Color(1f, 0.85f, 0.1f);
        public static readonly Color Cyan    = new Color(0.1f, 0.9f, 1f);
        public static readonly Color Green   = new Color(0.2f, 1f, 0.35f);
        public static readonly Color Red     = new Color(1f, 0.15f, 0.12f);
        public static readonly Color Purple  = new Color(0.6f, 0.2f, 1f);
        public static readonly Color Gold    = new Color(1f, 0.78f, 0.15f);

        public static readonly Color Road     = new Color(0.16f, 0.13f, 0.18f);
        public static readonly Color Sidewalk = new Color(0.45f, 0.32f, 0.28f);
        public static readonly Color Pole     = new Color(0.25f, 0.18f, 0.12f);
        public static readonly Color Clay     = new Color(0.6f, 0.28f, 0.12f);
        public static readonly Color Flame    = new Color(1f, 0.75f, 0.2f);
        public static readonly Color Sky      = new Color(0.06f, 0.03f, 0.14f);

        public static readonly Color[] Festive = { Orange, Magenta, Yellow, Cyan, Green, Red, Purple };

        public static readonly Color[] Buildings =
        {
            new Color(0.55f, 0.2f, 0.3f), new Color(0.3f, 0.22f, 0.5f), new Color(0.2f, 0.35f, 0.4f),
            new Color(0.6f, 0.4f, 0.2f),  new Color(0.4f, 0.18f, 0.45f), new Color(0.25f, 0.4f, 0.3f)
        };

        public static Color Random() { return Festive[UnityEngine.Random.Range(0, Festive.Length)]; }
        public static Color RandomBuilding() { return Buildings[UnityEngine.Random.Range(0, Buildings.Length)]; }
    }

    /// <summary>Creates materials and primitive shapes. Works with Built-in, URP and HDRP.</summary>
    public static class FRMat
    {
        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();
        static Shader lit;

        static Shader Lit()
        {
            if (lit == null)
            {
                lit = Shader.Find("Universal Render Pipeline/Lit");
                if (lit == null) lit = Shader.Find("HDRP/Lit");
                if (lit == null) lit = Shader.Find("Standard");
                if (lit == null) lit = Shader.Find("Diffuse");
            }
            return lit;
        }

        public static Material Get(Color c, float emission = 0f)
        {
            string key = ColorUtility.ToHtmlStringRGBA(c) + "_" + emission.ToString("0.00");
            Material m;
            if (cache.TryGetValue(key, out m) && m != null) return m;

            m = new Material(Lit());
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.35f);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.35f);

            if (emission > 0f)
            {
                Color e = c * emission;
                m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", e);
                if (m.HasProperty("_EmissiveColor")) m.SetColor("_EmissiveColor", e * 50f); // HDRP
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            cache[key] = m;
            return m;
        }

        /// <summary>Make a primitive. Position is LOCAL to parent. Colliders are triggers when kept.</summary>
        public static GameObject Prim(PrimitiveType type, Vector3 localPos, Vector3 scale, Color color,
                                      Transform parent, float emission = 0f, bool collider = false, bool shadows = false)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            Collider col = go.GetComponent<Collider>();
            if (col != null)
            {
                if (collider) col.isTrigger = true;
                else Object.Destroy(col);
            }
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;

            Renderer r = go.GetComponent<Renderer>();
            r.sharedMaterial = Get(color, emission);
            r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            return go;
        }
    }

    /// <summary>Keyboard + mouse/touch swipe input. Works with the old Input Manager AND the new Input System.</summary>
    public static class FRInput
    {
        static int frame = -1;
        static bool left, right, up, down, tap, confirm;
        static bool pointerHeld, swiped;
        static Vector2 startPos;

        public static bool Left    { get { Poll(); return left; } }
        public static bool Right   { get { Poll(); return right; } }
        public static bool Up      { get { Poll(); return up; } }
        public static bool Down    { get { Poll(); return down; } }
        public static bool Tap     { get { Poll(); return tap; } }
        public static bool Confirm { get { Poll(); return confirm; } }

        static void Poll()
        {
            if (frame == Time.frameCount) return;
            frame = Time.frameCount;
            left = right = up = down = tap = confirm = false;

            bool pDown = false, pHeld = false, pUp = false;
            Vector2 pos = Vector2.zero;

#if ENABLE_LEGACY_INPUT_MANAGER
            left    = Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A);
            right   = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
            up      = Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space);
            down    = Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S);
            confirm = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R);
            pDown = Input.GetMouseButtonDown(0);
            pHeld = Input.GetMouseButton(0);
            pUp   = Input.GetMouseButtonUp(0);
            pos   = Input.mousePosition;
#elif ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                left    = kb.leftArrowKey.wasPressedThisFrame  || kb.aKey.wasPressedThisFrame;
                right   = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
                up      = kb.upArrowKey.wasPressedThisFrame    || kb.wKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
                down    = kb.downArrowKey.wasPressedThisFrame  || kb.sKey.wasPressedThisFrame;
                confirm = kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.rKey.wasPressedThisFrame;
            }
            var p = UnityEngine.InputSystem.Pointer.current;
            if (p != null)
            {
                pDown = p.press.wasPressedThisFrame;
                pHeld = p.press.isPressed;
                pUp   = p.press.wasReleasedThisFrame;
                pos   = p.position.ReadValue();
            }
#endif
            // Swipe detection (mouse drag in editor, finger swipe on phone)
            if (pDown) { startPos = pos; pointerHeld = true; swiped = false; }
            if (pointerHeld && !swiped && (pHeld || pUp))
            {
                Vector2 d = pos - startPos;
                float threshold = Mathf.Max(40f, Screen.height * 0.05f);
                if (d.magnitude > threshold)
                {
                    swiped = true;
                    if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) { if (d.x > 0) right = true; else left = true; }
                    else { if (d.y > 0) up = true; else down = true; }
                }
            }
            if (pUp)
            {
                if (pointerHeld && !swiped) tap = true;
                pointerHeld = false;
            }
        }
    }

    /// <summary>Tiny procedural sound effects, so the game has audio with zero imported files.</summary>
    public static class FRAudio
    {
        static AudioSource src;
        static AudioClip coin, jump, hit, power, slide;
        const int SR = 44100;

        public static void Init(GameObject host)
        {
            src = host.GetComponent<AudioSource>();
            if (src == null) src = host.AddComponent<AudioSource>();
            src.playOnAwake = false;
            coin  = Tones("coin",  new[] { 1318f, 1760f }, 0.07f);
            power = Tones("power", new[] { 523f, 659f, 784f, 1046f }, 0.08f);
            jump  = Sweep("jump", 350f, 900f, 0.18f);
            slide = Sweep("slide", 600f, 200f, 0.2f);
            hit   = Noise("hit", 0.4f);
        }

        public static void Coin()  { Play(coin, 0.5f); }
        public static void Power() { Play(power, 0.6f); }
        public static void Jump()  { Play(jump, 0.4f); }
        public static void Slide() { Play(slide, 0.35f); }
        public static void Hit()   { Play(hit, 0.8f); }

        static void Play(AudioClip c, float v) { if (src != null && c != null) src.PlayOneShot(c, v); }

        static AudioClip Tones(string name, float[] freqs, float each)
        {
            int per = (int)(SR * each);
            float[] d = new float[per * freqs.Length];
            for (int k = 0; k < freqs.Length; k++)
                for (int i = 0; i < per; i++)
                {
                    float env = 1f - (float)i / per;
                    d[k * per + i] = Mathf.Sin(2f * Mathf.PI * freqs[k] * i / SR) * env * 0.4f;
                }
            return Make(name, d);
        }

        static AudioClip Sweep(string name, float f0, float f1, float len)
        {
            int n = (int)(SR * len);
            float[] d = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                phase += 2f * Mathf.PI * Mathf.Lerp(f0, f1, t) / SR;
                d[i] = Mathf.Sin(phase) * (1f - t) * 0.4f;
            }
            return Make(name, d);
        }

        static AudioClip Noise(string name, float len)
        {
            int n = (int)(SR * len);
            float[] d = new float[n];
            float last = 0f;
            System.Random rng = new System.Random(7);
            for (int i = 0; i < n; i++)
            {
                float t = 1f - (float)i / n;
                last = Mathf.Lerp(last, (float)(rng.NextDouble() * 2 - 1), 0.25f); // low-passed noise
                d[i] = last * t * t * 0.9f;
            }
            return Make(name, d);
        }

        static AudioClip Make(string name, float[] data)
        {
            AudioClip c = AudioClip.Create(name, data.Length, 1, SR, false);
            c.SetData(data, 0);
            return c;
        }
    }
}
