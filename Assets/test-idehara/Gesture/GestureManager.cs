using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System.Linq;

public class GestureManager : MonoBehaviour
{
    public enum GestureType { STOP, SLOW, GO, NULL };
    public BodySourceManager _BodyManager;

    // 認識結果をインスペクタ上で↓この変数をチェックしながらコーディング
    public GestureType currentGesture;
    public float duration;
    public StickRecognizer sr;

    // Start is called before the first frame update
    void Start()
    {
        currentGesture = GestureType.NULL;
        duration = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (_BodyManager == null)
        {
            Debug.Log("_BodyManager == null");
            return;
        }

        // Bodyデータを取得する
        var data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        // 最初に追跡している人を取得する
        var body = data.FirstOrDefault(b => b.IsTracked);
        if (body == null)
        {
            return;
        }

        var Rhand = body.Joints[JointType.HandRight];
        var Relbow = body.Joints[JointType.ElbowRight];
        var Rshoulder = body.Joints[JointType.ShoulderRight];
        var Lhand = body.Joints[JointType.HandLeft];
        var Lelbow = body.Joints[JointType.ElbowLeft];
        var Lshoulder = body.Joints[JointType.ShoulderLeft];
        Debug.Log(Rhand);


        // StickRecognizer である sr に、棒の位置や動きを問い合わせ、
        // 関節情報などを加味して、ジェスチャを認識する。
        // ジェスチャを認識したら、currentGesture と duration を更新する
        // 認識できなければ、currentGesture は NULL に。
        // 新しいジェスチャが来たら、duration を 0 でリセット

        // デバッグ用
        if ( Input.GetKey( KeyCode.W) ) {
            if( currentGesture != GestureType.GO ) duration = 0;
            currentGesture = GestureType.GO;
        }
        else if( Input.GetKey( KeyCode.S) ) {
            if( currentGesture != GestureType.SLOW ) duration = 0;
            currentGesture = GestureType.SLOW;
        }
        else if( Input.GetKey( KeyCode.X) ) {
            if( currentGesture != GestureType.STOP ) duration = 0;
            currentGesture = GestureType.STOP;
        }
        else {
            currentGesture = GestureType.NULL;
        }


        duration += Time.deltaTime;
    }
}
