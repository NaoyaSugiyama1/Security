using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Assets/KinectView/Scripts/ColorSourceView.cs
// Assets/KinectView/Scripts/ColorSourceManager.cs
// を参照して、適切に記述

// なにを返すべきかは、要打ち合わせ
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
    // 認識した両端は　Vector2 型のメンバ s1, s2 に入れる。
    public bool GetStickPosition( Texture2D frame )
    {
        formerFrame = currentFrame;
        currentFrame = frame;

        s1 = Vector2.zero;
        s2 = Vector2.up;

        return true;
    }
}
