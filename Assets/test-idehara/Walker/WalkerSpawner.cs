using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerSpawner : MonoBehaviour
{
    public enum WalkerType { WALKER, BICYCLE };
    public GameObject walkerPrefab;
    public WalkerType walkerType;
    public GameObject target;
    public float maxSpeed;
    public float minSpeed;
    public float minInterval;
    public float maxInterval;
    // Start is called before the first frame update
    void Start()
    {
        StartSpawning();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartSpawning()
    {
        StartCoroutine( "SpawnWalker" );
    }

    private IEnumerator SpawnWalker()
    {
        while( true )
        {
            yield return new WaitForSeconds( minInterval + (maxInterval - minInterval) * Random.value);
            GameObject w = Instantiate( walkerPrefab, transform );
            WalkerManager wm = w.GetComponent<WalkerManager>();
            wm.speed = minSpeed + (maxSpeed-minSpeed) * Random.value;
            wm.target = target;
        }
    }
}
