using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Assets/KinectView/Scripts/ColorSourceView.cs
// Assets/KinectView/Scripts/ColorSourceManager.cs
// を参照して、適切に記述

// なにを返すべきかは、要打ち合わせ

public class StickRecognizer : MonoBehaviour
{
    public enum StickState { STILL, HMOVE, VMOVE, WAVE };
    public Vector3 stickPosition;

    // この型は OpenCV 用の型にすること！
    private Texture2D formerFrame;
    private Texture2D currentFrame;

    // Start is called before the first frame update
    void Start()
    {
        stickPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public StickRecognizer.StickState GetStickAction( Texture2D frame )
    {
        formerFrame = currentFrame;
        currentFrame = frame;

        return StickState.STILL;
    }
}
