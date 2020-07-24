﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public enum GameState { Opening, Tutorial, Main, End };
    public GameState state;
    public int myscore;

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
