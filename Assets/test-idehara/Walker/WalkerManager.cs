using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerManager : MonoBehaviour
{
    static public float ARRIVAL_DIST = 0.5f;
    public float speed;
    public GameObject target;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(target)
        {
            Vector3 move = target.transform.position - gameObject.transform.position;
            if( move.magnitude < ARRIVAL_DIST )
            {
                speed = 0;
                Destroy(gameObject, 0.5f);
            }
            gameObject.transform.position += move.normalized * Time.deltaTime * speed;
        }
    }

    public void SetTarget(GameObject t)
    {
        target = t;
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("car"))
        {
            Debug.Log("collision!!");
            //gameObject.GetComponent<Rigidbody>().isKinematic = true;
            speed = 0;
        }
    }
}
