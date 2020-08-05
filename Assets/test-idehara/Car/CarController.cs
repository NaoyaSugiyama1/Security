using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public enum CarState {NORMAL, CRASHED, ARRIVED};
    // 運転手いらいら度
    public float frustration;
    public float speed;
    public float accel;

    // 車の長さ
    public float carLength;

    // 人にぶつかったら CRASHED に移行
    public CarState state;
    // 目的地（空ゲームオブジェクト推奨）
    public GameObject target;

    private float wheelAngle;
    private float addAngleFactor = 12.0f;
    private Plane targetPlane;

    // Start is called before the first frame update
    void Start()
    {
        frustration = 0;
        accel = 0;
        state = CarState.NORMAL;
        wheelAngle = 0;
        // ターゲット設定コードを一回呼んで、インスペクタで指定されたターゲットも
        // ちゃんと計算
        SetTarget(target);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime /2;
        transform.Rotate( 0, Mathf.Tan(wheelAngle*3.14f/180.0f) * addAngleFactor * carLength * speed * Time.deltaTime, 0, Space.Self);
        transform.position += transform.forward * speed * Time.deltaTime /2;

        if( wheelAngle == 0 && Mathf.Abs(targetPlane.GetDistanceToPoint( transform.position)) < carLength * 1.3f)
            wheelAngle = -45.0f;

        Vector3 dir = target.transform.position - transform.position;
        float leftAngle = Vector3.Angle(dir, transform.forward);
        Debug.Log(leftAngle);
        if( Mathf.Abs(leftAngle) < 3 )
            wheelAngle *= 0.55f;

        if( state == CarState.ARRIVED )
        {
            // 目的地に到着したら、表示から消す
            // gameObject 自体を消滅させると、state が参照できなくなる
            gameObject.GetComponent<MeshRenderer>().enabled = false;
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
}
