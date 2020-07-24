using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    // 運転手いらいら度
    public float frustration;
    public float speed;
    public float accel;

    public GameObject target;   // 目的地（空ゲームオブジェクト推奨）

    // Start is called before the first frame update
    void Start()
    {
        frustration = 0;
        accel = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = target.transform.position - transform.position;
        transform.position += dir.normalized * speed * Time.deltaTime;
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
