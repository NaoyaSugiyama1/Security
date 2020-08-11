using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System.Linq;
using System.Collections.Specialized;
using System;
using System.Diagnostics;
//using System.Diagnostics;

public class GestureManager : MonoBehaviour
{
    public enum GestureType { STOP, SLOW, GO, NULL };
    public BodySourceManager _BodyManager;

    // 認識結果をインスペクタ上で↓この変数をチェックしながらコーディング
    public GestureType currentGesture;
    public float duration;
    public StickRecognizer sr;
    Queue<Vector3> Rhand_Q;
    Queue<Vector3> Lhand_Q;
    Queue<Vector3> Relbow_Q;
    Queue<Vector3> Rshoulder_Q;
    
    // Start is called before the first frame update
    void Start()
    {
        currentGesture = GestureType.NULL;
        duration = 0;
        Rhand_Q = new Queue<Vector3>();
        Lhand_Q = new Queue<Vector3>();
        Relbow_Q = new Queue<Vector3>();
        Rshoulder_Q = new Queue<Vector3>();
    }

    // Update is called once per frame
    void Update()
    { 
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
        //else {
        //    currentGesture = GestureType.NULL;
        //}


        duration += Time.deltaTime;
    }

    void FixedUpdate ()
    {
        //Kinectの処理
        if (_BodyManager == null)
        {
           // Debug.Log("_BodyManager == null");
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
        //Debug.Log(Rhand.ToVector3());

        // 停止のキュー
        Rhand_Q.Enqueue(Rhand.ToVector3());
        if (Rhand_Q.Count > 60)
        {
            Rhand_Q.Dequeue();
        }
        float max = -999;
        float min = 999;
        foreach (Vector3 v in Rhand_Q)
        {
            if (max < v.x)
            {
                max = v.x;
            }
            if (min > v.x)
            {
                min = v.x;
            }
        }

        // 進行のキュー
        Lhand_Q.Enqueue(Lhand.ToVector3());
        if (Lhand_Q.Count > 60)
        {
            Lhand_Q.Dequeue();
        }
        float max2 = -999;
        float min2 = 999;
        foreach (Vector3 v2 in Lhand_Q)
        {
            var v3 = Math.Abs(v2.x);
            if (max2 < v3)
            {
                max2 = v3;
            }
            if (min2 > v3)
            {
                min2 = v3;
            }
        }

        // 停止のキュー
        //Relbow_Q.Enqueue(Relbow.ToVector3());
        //Rshoulder_Q.Enqueue(Rshoulder.ToVector3());
        //if (Relbow_Q.Count > 60)
        //{
        //    Relbow_Q.Dequeue();
        //}
        //if (Rshoulder_Q.Count > 60)
        //{
        //    Rshoulder_Q.Dequeue();
        //}
        var Rshoulder_y = Rshoulder.ToVector3().y;
        var Relbow_y = Relbow.ToVector3().y;
        var Rhand_y = Rhand.ToVector3().y;

        //if(Rhand.ToVector3().y > 0.3)
        //Debug.Log(max2 + " " + min2);
        if (max2 - min2 > 0.3)
        {
            if (currentGesture != GestureType.GO) duration = 0;
            currentGesture = GestureType.GO;
        }
        if (max - min > 0.2)
        {
            if (currentGesture != GestureType.SLOW) duration = 0;
            currentGesture = GestureType.SLOW;
        }
        else if (Relbow_y > 0.1 && Rshoulder_y > 0.1 && Rhand_y > 0.1)
        {
            if (currentGesture != GestureType.STOP) duration = 0;
            currentGesture = GestureType.STOP;
        }
        UnityEngine.Debug.Log(Rshoulder_y + " " + Relbow_y + " " + Rhand_y);
    }
}

public static class JointExtensions
{
    public static Vector3 ToVector3(this Windows.Kinect.Joint joint)
        => new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
}
