using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerManager : MonoBehaviour
{
    static public float ARRIVAL_DIST = 0.5f;
    public float speed;
    public GameObject target;

    private bool isArrived;
    private Vector3 move;

    // Start is called before the first frame update
    void Start()
    {
        isArrived = false;
        move = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if(target)
        {
            if( !isArrived )
                move = target.transform.position - gameObject.transform.position;
            if( move.magnitude < ARRIVAL_DIST )
            {
                isArrived = true;
                Destroy(gameObject, 0.5f);
            }
            gameObject.transform.position += move.normalized * Time.deltaTime * speed;
        }
    }

    public void SetTarget(GameObject t)
    {
        target = t;
        gameObject.transform.LookAt(t.transform.position, Vector3.up);
    }

    public void Crash(GameObject c, float speed)
    {
        speed = 0;
        target = null;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None; // no constraints, it rotates on any axis.
        Vector3 dir = gameObject.transform.position - c.transform.position;
        rb.AddForce(dir.normalized * speed * speed * 100.0f);
    }

    public void OnCollisionEnter(Collision c)
    {
        Vector3 f = gameObject.transform.position - c.gameObject.transform.position;
        f = f.normalized;
        if( Vector3.Dot(f, gameObject.transform.forward) < 0 )
        {
            gameObject.GetComponent<Rigidbody>().AddForce(f * 5.0f);
        }
    }

}
