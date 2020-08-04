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
    private float addAngleFactor = 1.8f;
    private Plane targetPlane;

    // Start is called before the first frame update
    void Start()
    {
        frustration = 0;
        accel = 0;
        state = CarState.NORMAL;
        wheelAngle = -45.0f;
    }

    // Update is called once per frame
    void Update()
    {
        targetPlane = new Plane( transform.forward,  target.transform.position );

        Vector3 dir = target.transform.position - transform.position;
        transform.position += transform.forward * speed * Time.deltaTime;
        transform.Rotate( 0, Mathf.Tan(wheelAngle*addAngleFactor*3.14f/180.0f) * carLength * speed * Time.deltaTime * carLength, 0, Space.Self);
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
    }
}
