﻿using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;

// 車ゲームオブジェクトのコライダ設定
// ・車間距離を維持するためのトリガー（１ｍくらいモデルの外側）
// ・人や他の車との衝突判定に使うノントリガー（モデル表面）

public class CarController : MonoBehaviour
{
    public enum CarState {NORMAL, CRASHED, ARRIVED, WAITING};
    // 運転手いらいら度
    public float frustration;
    // 運転手反応速度 [s] （車間距離トリガに対する反応速度＝運転手の下手さ度合い）
    public float responseTime;
    // 現在速度 [m/s]
    public float speed;
    // 目標速度 [m/s]
    public float targetSpeed;

    // 車の長さ [m]
    public float carLength;

    // 人にぶつかったら CRASHED に移行
    public CarState state;
    // 目的地（空ゲームオブジェクト推奨）
    public GameObject target;
    // 前を走行する車
    public GameObject previousCar;

    private float wheelAngle;
    private float addAngleFactor = 12.0f;
    private Plane targetPlane;

    // Start is called before the first frame update
    void Start()
    {
        frustration = 0;
        targetSpeed = 0;
        state = CarState.NORMAL;
        wheelAngle = 0;
        // ターゲット設定コードを一回呼んで、インスペクタで指定されたターゲットもちゃんと計算しておく
        SetTarget(target);
    }

    void FixedUpdate()
    {
        float rotationAngle = Mathf.Tan(wheelAngle*3.14f/180.0f) * addAngleFactor * carLength * speed * Time.deltaTime;
        Vector3 dir = target.transform.position - transform.position;
        Vector3 orgforward = transform.forward;

        // 現在の向きで半分進んで
        transform.position += transform.forward * speed * Time.deltaTime /2;
        // 曲がる
        transform.Rotate( 0, rotationAngle, 0, Space.Self);
        // 曲がりすぎた結果、目的地が正面を通り過ぎたら
        if( Vector3.Cross(orgforward, dir).y * Vector3.Cross(transform.forward, dir).y < 0 )
        {
            // ハンドルを切り戻していたことにして向きを少し戻す
            wheelAngle *= 0.5f;
            transform.Rotate( 0, -rotationAngle*0.5f, 0, Space.Self);
            Debug.Log("Wheel Back");
        }
        // 回転後の向きで半分進む
        transform.position += transform.forward * speed * Time.deltaTime /2;

        // 曲がりはじめる条件。車体先端から 30% 先に目的地平面がやってきた
        if( wheelAngle == 0 && Mathf.Abs(targetPlane.GetDistanceToPoint(transform.position)) < carLength * 1.3f)
            wheelAngle = -45.0f;

        // 微調整
        Vector3 cr = Vector3.Cross(transform.forward.normalized, dir.normalized);
        float leftAngle = Vector3.Angle(dir, transform.forward);
        // 目的地が３度以下に見えているならハンドルをどんどん戻す
        if( Mathf.Abs(leftAngle) < 3 )
            wheelAngle *= 0.55f;
        // 目的地が４０度以下に見えているなら、ずれている角度に応じてハンドルを切り戻す
        else if( Mathf.Abs(leftAngle) < 40 )
            wheelAngle += cr.y * 0.8f;
    }

    // Update is called once per frame
    void Update()
    {
        switch( state )
        {
            case CarState.ARRIVED:
                // 目的地に到着したら、表示から消す
                // gameObject 自体を消滅させると、state が参照できなくなる
                gameObject.GetComponent<MeshRenderer>().enabled = false;
                break;
            case CarState.CRASHED:
                speed *= 0.9f;
                break;
        }
    }

    // 運転手が受け入れたら true
    public bool Order(GestureManager.GestureType gesture)
    {
        switch( gesture )
        {
            case GestureManager.GestureType.STOP:
                speed = 0;
                break;
            case GestureManager.GestureType.SLOW:
                speed = 1.0f;
                break;
            case GestureManager.GestureType.GO:
                speed = 3.0f;
                break;
        }
        return true;
    }

    public void SetTarget(GameObject t)
    {
        target = t;
        targetPlane = new Plane( transform.forward,  target.transform.position );
    }

    // 駐車場ゲートなどで一定時間停止
    public void WaitAtGate(float t)
    {
        state = CarState.WAITING;
    }

    public void OnCollisionEnter(Collision c)
    {
        if( c.gameObject.CompareTag("walker") )
        {
            Debug.Log("collision!!");

            // すぐに止めると実際っぽくないので、停止は Update 内で
            state = CarState.CRASHED;
            c.gameObject.GetComponent<WalkerManager>().Crash(gameObject, speed);
        }
        else if( c.gameObject.CompareTag("goal") )
        {
            Debug.Log("goal!!");
            state = CarState.ARRIVED;
        }
    }
}
