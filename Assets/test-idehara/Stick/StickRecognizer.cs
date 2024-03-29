using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;


// Assets/KinectView/Scripts/ColorSourceView.cs
// Assets/KinectView/Scripts/ColorSourceManager.cs
// を参照して、適切に記述

// なにを返すべきか
// →　棒の画面上の２点
// →　呼び出し側で、その座標情報と、骨格情報からジェスチャ認識

public class StickRecognizer : MonoBehaviour
{
    public Vector2 s1, s2;

    // この型は OpenCV 用の型にすること！
    private Texture2D formerFrame;
    private Texture2D currentFrame;

    // Start is called before the first frame update
    void Start()
    {
        s1 = Vector2.zero;
        s2 = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 棒が認識されていれば true, 認識されていなければ false を返す
    // 認識した両端は Vector2 型のメンバ s1, s2 に入れる。
    // s1, s2 の座標系は、左上を (0,0) とするスクリーン座標
    public bool GetStickPosition( Texture2D frame )
    {
        formerFrame = currentFrame;
        currentFrame = frame;

        s1 = Vector2.zero;
        s2 = Vector2.up;

 

        //横 press Y
        if (Input.GetKeyDown(KeyCode.Y))
        {
            s1 = new Vector2(50, 100);
            s2 = new Vector2(150, 100);
            return true;
        }
        //斜め press H
        else if (Input.GetKeyDown(KeyCode.H))
        {
            s1 = new Vector2(70, 70);
            s2 = new Vector2(140, 140);
            return true;
        }
        //縦 press N
        else if (Input.GetKeyDown(KeyCode.N))
        {
            s1 = new Vector2(100, 50);
            s2 = new Vector2(100, 150);
            return true;
        }
        else
        {
            return false;
        }
 
    }
}
