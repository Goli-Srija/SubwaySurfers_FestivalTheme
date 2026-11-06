using UnityEngine;

namespace FestivalRush
{
    public enum GameState { Menu, Playing, GameOver }

    /// <summary>
    /// FESTIVAL RUSH - Diwali Night Run.
    /// Put this one component on an empty GameObject in an empty scene and press Play.
    /// It builds the camera, lights, player, track and UI by itself.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        [Header("Speed")]
        public float startSpeed = 12f;
        public float maxSpeed = 32f;
        public float acceleration = 0.3f;

        [Header("Power-up duration (seconds)")]
        public float magnetDuration = 10f;
        public float doubleDuration = 10f;
        public float shieldDuration = 12f;

        [Header("Fireworks")]
        public bool fireworks = true;

        public GameState State { get; private set; }
        public float Speed { get; private set; }
        public float Distance { get; private set; }
        public int Coins { get; private set; }
        public int Score { get { return Mathf.FloorToInt(scoreF); } }
        public int Multiplier { get { return doubleTimer > 0f ? 2 : 1; } }
        public bool MagnetActive { get { return magnetTimer > 0f; } }
        public bool ShieldActive { get { return shieldTimer > 0f; } }
        public PlayerController Player { get; private set; }

        float magnetTimer, doubleTimer, shieldTimer;
        float fireworkTimer, stateTime, scoreF;
        int best;
        bool newBest;
        TrackSpawner spawner;
        Camera cam;
        GUIStyle style;

        void Awake()
        {
            I = this;
            Application.targetFrameRate = 60;
            best = PlayerPrefs.GetInt("FR_Best", 0);

            SetupEnvironment();
            FRAudio.Init(gameObject);

            Player = new GameObject("Player").AddComponent<PlayerController>();
            Player.Build();

            spawner = new GameObject("Track").AddComponent<TrackSpawner>();
            spawner.Init(Player.transform);

            CameraFollow follow = cam.GetComponent<CameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.target = Player.transform;
            follow.SnapNow();

            State = GameState.Menu;
        }

        void SetupEnvironment()
        {
            cam = Camera.main;
            if (cam == null)
            {
                GameObject cg = new GameObject("Main Camera");
                cg.tag = "MainCamera";
                cam = cg.AddComponent<Camera>();
                cg.AddComponent<AudioListener>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = FRColors.Sky;
            cam.fieldOfView = 62f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 300f;

            GameObject lg = new GameObject("Moon Light");
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(0.75f, 0.7f, 1f);
            l.intensity = 0.6f;
            l.shadows = LightShadows.Soft;
            lg.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.38f, 0.3f, 0.48f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = FRColors.Sky;
            RenderSettings.fogStartDistance = 60f;
            RenderSettings.fogEndDistance = 175f;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            stateTime += dt;

            switch (State)
            {
                case GameState.Menu:
                    if (FRInput.Tap || FRInput.Confirm) StartRun();
                    break;

                case GameState.Playing:
                    Speed = Mathf.Min(maxSpeed, Speed + acceleration * dt);
                    Distance += Speed * dt;
                    scoreF += Speed * dt * 0.5f * Multiplier;
                    magnetTimer = Mathf.Max(0f, magnetTimer - dt);
                    doubleTimer = Mathf.Max(0f, doubleTimer - dt);
                    shieldTimer = Mathf.Max(0f, shieldTimer - dt);
                    break;

                case GameState.GameOver:
                    if (stateTime > 0.8f && (FRInput.Tap || FRInput.Confirm)) StartRun();
                    break;
            }

            if (fireworks)
            {
                fireworkTimer -= dt;
                if (fireworkTimer <= 0f)
                {
                    fireworkTimer = Random.Range(0.5f, 1.6f);
                    float side = Random.value < 0.5f ? -1f : 1f;
                    Vector3 pz = Player.transform.position;
                    Firework.Burst(new Vector3(side * Random.Range(6f, 18f), Random.Range(10f, 18f),
                                   pz.z + Random.Range(35f, 80f)), FRColors.Random(), 1f);
                }
            }
        }

        void StartRun()
        {
            Player.ResetPlayer();          // reset player first so the track rebuilds around z = 0
            spawner.ResetTrack();
            Distance = 0f; scoreF = 0f; Coins = 0;
            magnetTimer = doubleTimer = shieldTimer = 0f;
            Speed = startSpeed;
            newBest = false;
            stateTime = 0f;
            State = GameState.Playing;
        }

        // ---------- called by the player ----------
        public void AddCoin()
        {
            Coins++;
            scoreF += 10 * Multiplier;
            FRAudio.Coin();
        }

        public void ActivatePowerUp(PowerUpType t)
        {
            switch (t)
            {
                case PowerUpType.Magnet: magnetTimer = magnetDuration; break;
                case PowerUpType.Double: doubleTimer = doubleDuration; break;
                case PowerUpType.Shield: shieldTimer = shieldDuration; break;
            }
            FRAudio.Power();
        }

        public void BreakShield()
        {
            shieldTimer = 0f;
            FRAudio.Hit();
        }

        public void EndRun()
        {
            if (State != GameState.Playing) return;
            State = GameState.GameOver;
            stateTime = 0f;
            Speed = 0f;
            FRAudio.Hit();
            if (Score > best)
            {
                best = Score;
                newBest = true;
                PlayerPrefs.SetInt("FR_Best", best);
                PlayerPrefs.Save();
            }
        }

        // ---------- UI (IMGUI, no Canvas / TextMeshPro needed) ----------
        void OnGUI()
        {
            float s = Screen.height / 1080f;
            float w = Screen.width, h = Screen.height;

            if (State == GameState.Menu)
            {
                Dim(0.45f);
                Text(new Rect(0, h * 0.14f, w, 150 * s), "FESTIVAL RUSH", 120, FRColors.Gold);
                Text(new Rect(0, h * 0.14f + 140 * s, w, 70 * s), "~ Diwali Night Run ~", 50, FRColors.Orange);
                if (Mathf.Repeat(Time.time, 1f) < 0.65f)
                    Text(new Rect(0, h * 0.45f, w, 70 * s), "TAP  or  press SPACE  to start", 46, Color.white);
                Text(new Rect(0, h * 0.58f, w, 50 * s), "Left / Right  (A / D)  :  change lane", 32, Color.white, false);
                Text(new Rect(0, h * 0.58f + 45 * s, w, 50 * s), "Up / W / Space  :  jump        Down / S  :  slide", 32, Color.white, false);
                Text(new Rect(0, h * 0.58f + 90 * s, w, 50 * s), "On phone: swipe left, right, up, down", 32, Color.white, false);
                Text(new Rect(0, h * 0.58f + 150 * s, w, 50 * s), "Power-ups:  BLUE = Magnet    GREEN = Rangoli Shield    PINK = 2x Score", 30, FRColors.Cyan, false);
                Text(new Rect(0, h * 0.88f, w, 60 * s), "BEST  " + best, 40, FRColors.Yellow);
            }
            else if (State == GameState.Playing)
            {
                Text(new Rect(0, 20 * s, w - 40 * s, 80 * s), Score.ToString("000000"), 64, Color.white, true, TextAnchor.UpperRight);
                Text(new Rect(0, 95 * s, w - 40 * s, 60 * s), "COINS  " + Coins, 44, FRColors.Gold, true, TextAnchor.UpperRight);
                if (Multiplier > 1)
                    Text(new Rect(0, 150 * s, w - 40 * s, 60 * s), "x2 SCORE", 40, FRColors.Magenta, true, TextAnchor.UpperRight);

                float y = 30 * s;
                if (MagnetActive) { Bar(y, "MAGNET", magnetTimer / magnetDuration, FRColors.Cyan); y += 70 * s; }
                if (ShieldActive) { Bar(y, "RANGOLI SHIELD", shieldTimer / shieldDuration, FRColors.Green); y += 70 * s; }
                if (doubleTimer > 0f) { Bar(y, "2X SCORE", doubleTimer / doubleDuration, FRColors.Magenta); }
            }
            else
            {
                Dim(0.55f);
                Text(new Rect(0, h * 0.18f, w, 130 * s), "GAME OVER", 110, FRColors.Red);
                Text(new Rect(0, h * 0.36f, w, 80 * s), "SCORE  " + Score, 64, Color.white);
                Text(new Rect(0, h * 0.36f + 80 * s, w, 60 * s), "COINS  " + Coins + "      DISTANCE  " + Mathf.FloorToInt(Distance) + " m", 40, FRColors.Gold);
                Text(new Rect(0, h * 0.36f + 145 * s, w, 60 * s), newBest ? "NEW BEST!  Happy Diwali!" : "BEST  " + best, 44, FRColors.Yellow);
                if (stateTime > 0.8f && Mathf.Repeat(Time.time, 1f) < 0.65f)
                    Text(new Rect(0, h * 0.75f, w, 70 * s), "TAP  or  press SPACE  to play again", 44, Color.white);
            }
        }

        void Dim(float a)
        {
            GUI.color = new Color(0f, 0f, 0f, a);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        void Bar(float y, string label, float t, Color c)
        {
            float s = Screen.height / 1080f;
            Text(new Rect(30 * s, y, 500 * s, 36 * s), label, 30, c, true, TextAnchor.UpperLeft);
            Rect bg = new Rect(30 * s, y + 38 * s, 320 * s, 16 * s);
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(bg, Texture2D.whiteTexture);
            GUI.color = c;
            GUI.DrawTexture(new Rect(bg.x, bg.y, bg.width * Mathf.Clamp01(t), bg.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        void Text(Rect r, string t, int size, Color c, bool bold = true, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label);
                style.wordWrap = false;
            }
            style.fontSize = Mathf.Max(10, Mathf.RoundToInt(size * Screen.height / 1080f));
            style.alignment = anchor;
            style.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;

            Rect shadow = r; shadow.x += 3; shadow.y += 3;
            style.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
            GUI.Label(shadow, t, style);
            style.normal.textColor = c;
            GUI.Label(r, t, style);
        }
    }
}
