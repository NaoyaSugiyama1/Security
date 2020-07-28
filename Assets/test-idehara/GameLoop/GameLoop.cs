using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public enum GameState { Opening, Tutorial, Main, End };
    public GameState state;
    public int myscore;

    public GestureManager gm;

    // 最後にエスケープキーが押された時刻
    private DateTime lastEscape;

    // Start is called before the first frame update
    void Start()
    {
        state = GameState.Opening;
    }

    // Update is called once per frame
    void Update()
    {
        // ESC の２連打で終了
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (TimeSpan.FromSeconds(1.0f).CompareTo(DateTime.Now - lastEscape) > 0)
            {
                Debug.Log("QUIT");
                // これはビルドしたスタンドアロンでしか効かないので注意
                Application.Quit();
            }
            else
                lastEscape = DateTime.Now;
        }

        // マウスのクリックか、スペースキーかで、無条件に次に進む
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            NextState();

        switch( state )
        {
            case GameState.Main:
                var cars = GameObject.FindGameObjectsWithTag("car");
                // GestureManager に問い合わせ
                // Gesture が認識されていて、一定時間が経過していれば：
                //   Car 全員に指示をとばす
                if( gm.currentGesture != GestureManager.GestureType.NULL )
                {
                    foreach (var c in cars) {
                        c.GetComponent<CarController>().Order( gm.currentGesture );
                    }
                }

                foreach (var c in cars) {
                    if( c.GetComponent<CarController>().state == CarController.CarState.CRASHED )
                    {
                        // どれか１台でも衝突したら BADEND に移行
                    }
                }

                bool allArrived = true;
                foreach (var c in cars) {
                    if( c.GetComponent<CarController>().state != CarController.CarState.ARRIVED )
                    {
                        allArrived = false;
                        break;
                        // どれか１台でも到着していなかったら、allArrived は false
                    }
                }
                if( allArrived )
                {
                    // 全車到着で GOODEND に移行
                }

                break;
        }
    }

    private void NextState()
    {
        if (state == GameState.End)
            state = GameState.Opening;
        else
            state++;

        switch (state)
        {
            case GameState.Opening:
                Debug.Log("Opening");
                myscore = 0;

                // Release all unused assets
                Resources.UnloadUnusedAssets();
                GC.Collect();

                break;

            case GameState.Tutorial:
                Debug.Log("Tutorial");
                break;

            case GameState.Main:
                Debug.Log("Main");
                break;

            case GameState.End:
                Debug.Log("End");
                break;
        }
    }
}
