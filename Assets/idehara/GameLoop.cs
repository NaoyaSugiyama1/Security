using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameLoop : MonoBehaviour {

    public enum GameState { Opening, TutorialCharge, TutorialShoot, Mob, Boss, End, End2 };
    public List<GameLoop.GameState> StateWithoutSound = new List<GameLoop.GameState>
            { GameLoop.GameState.Opening, GameLoop.GameState.End };

    public GameState state;

    public float leftTime;
    public float MOBGAMETIME = 70;
    public float BOSSGAMETIME = 20;

    public Texture damageTexture;
    private bool isDamaged = false;

    public bool isRunTest = false;

    public bool isHMD = true;
    public float SpreadFactor = 1.0f;
    private float ScoreMulti = 1.0f;
    public float MasterVolume = 1.0f;

    List<int> scoresAll;
    List<int> scoresDay;
    private int myscore;

    private DateTime lastEscape;
    public bool toKillBoss;

    public GameObject scoreCanvas;
    public GameObject timeCanvas;

    public GameObject rightHand;

    private bool wasOvrButtonPressed;

    public GameObject baseSound;

    // Use this for initialization
    void Start () {
        state = GameState.Opening;
        lastEscape = DateTime.Now;
        scoresAll = new List<int>();
        scoresDay = new List<int>();

        // 追加ディスプレイの有効化
        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] is the primary, default display and is always ON.
        // Check if additional displays are available and activate each.
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        if (Display.displays.Length > 2)
            Display.displays[2].Activate();

        // PC別画面表示（VR画面をPCに表示しない）
        UnityEngine.XR.XRSettings.showDeviceView = false;

        // Game Log の読み込み
        if (!System.IO.File.Exists("gamelog.txt"))
            System.IO.File.Create("gamelog.txt").Close();
        string[] lines = System.IO.File.ReadAllLines("gamelog.txt");
        foreach(string s in lines)
        {
            string[] p = s.Split(' ');
            try
            {
                scoresAll.Add(Int32.Parse(p[2]));
                if (DateTime.Parse(p[0]).Date == DateTime.Now.Date)
                    scoresDay.Add(Int32.Parse(p[2]));
            }
            catch (Exception)
            {
            }
        }

        // Game Config の読み込み
        if (!System.IO.File.Exists("config.txt"))
            System.IO.File.Create("config.txt").Close();
        string[] configs = System.IO.File.ReadAllLines("config.txt");
        foreach (string s in configs)
        {
            string cline = s.Trim().ToUpper();
            if (cline.Length == 0)
                continue;
            string[] commands = cline.Split(' ');
            if (commands[0].StartsWith("#"))
                continue;

            if (commands[0] == "GAMEMODE")
            {
                if (commands[1] == "HMD")
                    isHMD = true;
                else if (commands[1] == "PROJECTOR")
                    isHMD = false;
                else
                    Application.Quit();
                UnityEngine.XR.XRSettings.enabled = isHMD;
                Debug.Log("Virtual Reality Mode :" + isHMD);
            }

            if (commands[0] == "GAMETIME")
            {
                MOBGAMETIME = Int32.Parse(commands[1]);
                BOSSGAMETIME = Int32.Parse(commands[2]);
                Debug.Log("GameTime : " + MOBGAMETIME + " / " + BOSSGAMETIME);
            }

            if (commands[0] == "SPREADFACTOR")
            {
                SpreadFactor = float.Parse(commands[1]);
                Debug.Log("Spread Factor : " + SpreadFactor);
            }

            if (commands[0] == "SCOREMULTI")
            {
                ScoreMulti = float.Parse(commands[1]);
                Debug.Log("Score Multiplier : " + ScoreMulti);
            }

            if (commands[0] == "MASTERVOLUME")
            {
                MasterVolume = float.Parse(commands[1]);
                Debug.Log("Master Volume : " + MasterVolume);
            }

            if (commands[0] == "DISPLAYBODY")
            {
                bool toShowBody = (int.Parse(commands[1]) != 0);
                GameObject.FindGameObjectWithTag("KinectController").GetComponent<beBodySourceView>().SetDisplayMode(toShowBody);
            }

            if (commands[0] == "DEBUGLIGHT")
            {
                bool lightDebugMode = (int.Parse(commands[1]) != 0);
                GameObject.FindGameObjectWithTag("HandController").GetComponent<ServoController>().DEBUG_MODE = lightDebugMode;
            }

            if (commands[0] == "KINECTPOSITION")
            {
                float px = (float.Parse(commands[1]));
                float py = (float.Parse(commands[2]));
                float pz = (float.Parse(commands[3]));
                Transform tr = GameObject.FindGameObjectWithTag("KinectController").GetComponent<Transform>();
                tr.localPosition = new Vector3(px, py, pz);
            }

            if (commands[0] == "KINECTROTATION")
            {
                float rx = (float.Parse(commands[1]));
                float ry = (float.Parse(commands[2]));
                float rz = (float.Parse(commands[3]));
                Transform tr = GameObject.FindGameObjectWithTag("KinectController").GetComponent<Transform>();
                tr.localRotation = Quaternion.Euler(rx, ry, rz);
            }
        }

        if ( isHMD )
        {
            gameObject.GetComponent<OpeningSwitcher>().setHomeCanvas(
                            GameObject.FindGameObjectWithTag("TamaLogoSub"));
            GameObject.FindGameObjectWithTag("TamaLogoMain").SetActive(false);
        }
        else
        {
            gameObject.GetComponent<OpeningSwitcher>().setHomeCanvas(
                            GameObject.FindGameObjectWithTag("TamaLogoMain"));
            GameObject.FindGameObjectWithTag("TamaLogoMain").SetActive(true);
        }

        // Sub-Camera is enabled on PC when in HDM mode. 
        if (isHMD)
        {
            GameObject.FindGameObjectWithTag("SubCamera").GetComponent<Camera>().depth = 2;

        }
        else
        {
            GameObject.FindGameObjectWithTag("SubCamera").GetComponent<Camera>().depth = -2;
            GameObject.FindGameObjectWithTag("LeftTimeCanvas").GetComponent<Canvas>().worldCamera =
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }
    }

    private void Awake()
    {
        wasOvrButtonPressed = false;
        GameObject.FindGameObjectWithTag("MainCamera").GetComponentInChildren<AudioSource>().volume *= MasterVolume;
    }

    // Update is called once per frame
    void Update () {
        OVRInput.Update(); // need to be called for checks below to work

        // Opening と Ending では、Remote のボタンで次に進む
//        if ((state == GameState.Opening || state == GameState.End) 
//            && OVRInput.Get(OVRInput.Button.One))
//        {
//            NextState();
//        }

        // Remote のボタンが押され始めたら次のステートへ（end2 は勝手に先に進む）
        if (OVRInput.Get(OVRInput.Button.One))
        {
            if (!wasOvrButtonPressed)
            {
                NextState();
            }
            wasOvrButtonPressed = true;
        }
        else
        {
            wasOvrButtonPressed = false;
        }


        if(state == GameState.End2 && !OVRInput.Get(OVRInput.Button.One))
        {
            NextState();
        }

        // Mob では、時間切れで次にすすむ
        if (state == GameState.Mob)
        {
            leftTime -= Time.deltaTime;
            if (leftTime < 0)
                NextState();
        }

        // Boss では、時間切れで「ボス瀕死状態」にする。
        if (state == GameState.Boss)
        {
            leftTime -= Time.deltaTime;
            if (leftTime < 0)
                toKillBoss = true;

            // 時間切れ後に正規時間の４分の１の時間が経過すると強制終了
            if (leftTime < -BOSSGAMETIME / 4)
                NextState();
        }

        // マウスのクリックか、スペースキーかで、無条件に次に進む
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            NextState();

        // ランテスト中はぐるぐる回す
        if (isRunTest && (state == GameState.Opening || state == GameState.TutorialCharge || state == GameState.TutorialShoot || state == GameState.End))
            NextState();

        // ESC の２連打で終了
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (TimeSpan.FromSeconds(1.0f).CompareTo(DateTime.Now - lastEscape) > 0)
            {
                Debug.Log("QUIT");
                Application.Quit();
            }
            else
                lastEscape = DateTime.Now;
        }


        // Body View の表示切り替え
        if (Input.GetKeyDown(KeyCode.B))
        {
            var bv = GameObject.FindGameObjectWithTag("KinectController").GetComponent<beBodySourceView>();
            bv.TriggerDisplay();
        }

        // 検出ターゲットのリセット（一番近いプレーヤーに再設定）
        if (Input.GetKeyDown(KeyCode.T))
        {
            var bv = GameObject.FindGameObjectWithTag("KinectController").GetComponent<beBodySourceView>();
            bv.ResetTrackingID();
        }

        // ゲーム時間強制延長
        if (Input.GetKeyDown(KeyCode.E))
        {
            this.leftTime += 10;
        }

        // Reset to Opening
        if (Input.GetKeyDown(KeyCode.R))
        {
            while( state != GameState.Opening )
            {
                NextState();
            }
        }

        // manual shoot
        GameObject manualTarget = null;
        if (state == GameState.Mob || state == GameState.Boss)
        {
            SpawnManager sm = GameObject.FindGameObjectWithTag("Spawner").GetComponent<SpawnManager>();
            if (Input.GetKeyDown(KeyCode.A))
                manualTarget = sm.getLeftmostEnemy();
            if (Input.GetKeyDown(KeyCode.S))
                manualTarget = sm.getCenterEnemy();
            if (Input.GetKeyDown(KeyCode.D))
                manualTarget = sm.getRightmostEnemy();

            if(manualTarget != null)
            {
                Vector3 projectVelocity = (manualTarget.transform.position - rightHand.transform.position).normalized * 50;
                rightHand.GetComponent<HandManager>().shootProjectile(projectVelocity, 1.0f);
            }

        }

    }

    public void StartGame()
    {
        state = GameState.Mob;
        leftTime = MOBGAMETIME;
    }

    public void NextStateWithCheckCurrentState(GameState cs)
    {
        if (state != cs)
            return;

        NextState();
    }

    private void NextState()
    {
        if (state == GameState.End2)
            state = GameState.Opening;
        else
            state++;

        ChestController cs = GameObject.FindGameObjectWithTag("Chest").GetComponent<ChestController>();
        SpawnManager sm = GameObject.FindGameObjectWithTag("Spawner").GetComponent<SpawnManager>();

        switch (state)
        {
            case GameState.Opening:
                Debug.Log("Opening");
                myscore = 0;
                toKillBoss = false;
                cs.Reset();
                GameObject world = GameObject.FindGameObjectWithTag("World");
                GameObject particles = world.transform.Find("Floating_Particles").gameObject;
                GameObject newps = Instantiate(Resources.Load("Floating_Particles"), world.transform) as GameObject;
                newps.name = "Floating_Particles";

                particles.GetComponent<ParticleSystem>().Clear();
                DestroyImmediate(particles.GetComponent<ParticleSystem>().GetComponent<Renderer>().materials[0]);
                DestroyImmediate(particles);


                // Release all unused assets
                Resources.UnloadUnusedAssets();
                GC.Collect();

                timeCanvas.SetActive(false);
                scoreCanvas.SetActive(false);

                if ( isHMD )
                    baseSound.GetComponent<AudioSource>().enabled = false;

                break;
            case GameState.TutorialCharge:
                Debug.Log("charge");

                baseSound.GetComponent<AudioSource>().enabled = true;

                break;
            case GameState.TutorialShoot:
                Debug.Log("shoot");

                break;

            case GameState.Mob:
                timeCanvas.SetActive(true);
                Debug.Log("Mob");
                leftTime = MOBGAMETIME;
                break;
            case GameState.Boss:
                Debug.Log("Boss");
                leftTime = BOSSGAMETIME;
                GameObject.FindGameObjectWithTag("Spawner").GetComponent<SpawnManager>().SpawnBoss();
                break;
            case GameState.End:
                Debug.Log("End");



                System.IO.File.AppendAllText("gamelog.txt",
                    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + myscore + "\r\n");
                scoresDay.Add(myscore);
                scoresAll.Add(myscore);

                int rankDaily = 1;
                foreach (int s in scoresDay)
                {
                    if (myscore < s)
                    {
                        rankDaily = rankDaily + 1;
                    }
                }
                int rankAll = 1;
                foreach (int s in scoresAll)
                {
                    if (myscore < s)
                    {
                        rankAll = rankAll + 1;
                    }
                }

                cs.OpenChest(myscore);
                StartCoroutine(showRank(myscore, rankDaily, rankAll));

                sm.Reset();
                break;

            case GameState.End2:
                Debug.Log("End2");
                break;
        }
    }

    public void PlayerDamage(float damage)
    {
        Invoke("TurnOnDamageDisplay", 0.5f);
    }

    private void TurnOnDamageDisplay()
    {
        isDamaged = true;
        StartCoroutine(TurnOffDamageDisplay());
    }

    private IEnumerator TurnOffDamageDisplay()
    {
        yield return new WaitForSeconds(0.1f);
        isDamaged = false;
    }

    public void OnGUI()
    {
        if( isDamaged )
        {
            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            GUI.DrawTexture(rect, damageTexture);
        }

        if (state != GameState.End)
        {
            int displayTime = (leftTime > 0 ? (int)leftTime : 0);
            if (state == GameState.Mob)
                displayTime += (int)BOSSGAMETIME;

            timeCanvas.GetComponent<Text>().text =
            (displayTime / 60).ToString("D2") + ":" + (displayTime % 60).ToString("D2");

        }
    }

    public void AddScore(int score)
    {
        myscore += (int)(score * ScoreMulti);
    }

    IEnumerator showRank(int myscore, int rankD, int rankA)
    {
        yield return new WaitForSeconds(7);

        timeCanvas.GetComponent<Text>().text =
            "score : " + myscore.ToString("N0") + "\r\n"
            + "rank(today) " + rankD.ToString("N0") + " / " + scoresDay.Count.ToString("N0") + "\r\n"
            + "rank( all ) " + rankA.ToString("N0") + " / " + scoresAll.Count.ToString("N0");

        if (!isHMD)
            scoreCanvas.SetActive(false);

        yield break;
    }
}
