using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour {

    public bool isCharging;
    public bool isCharged;
    private float charge;   // charged seconds NOT [0,1] but [0, CHARGE_MAX]
    public HandManager otherHand;
    public bool isSender;

    private Vector3 posHand, posElbow, posShoulder;
    private Vector3[] handPositionHistory;
    private Vector3[] elbowPositionHistory;
    
    public float CHARGE_READY_MIN = 0.02f;
    public float CHARGE_MAX = 5.0f;
    public float CHARGE_ANGLE_COS = 0.2f;
    public float SHOOT_ANGLE_COS = 0.6f;
    public int HAND_HISTORY_NUMBER = 5;

    public float getNormalizedCharge()
    {
        return charge / CHARGE_MAX;
    }

    private GameObject bullet;
    private float masterVolume = 1.0f;

    private float lastCosTheta;
    private Vector3 crossingDir;

	// Use this for initialization
	void Start () {
        isCharging = false;
        charge = 0;
        handPositionHistory = new Vector3[HAND_HISTORY_NUMBER];
        elbowPositionHistory = new Vector3[HAND_HISTORY_NUMBER];

        if ((otherHand.isSender && isSender) || (!otherHand.isSender && !isSender)) {
            Debug.Log("One of the hands only must be designated sender");
        }
    }

    private void Awake()
    {
        bullet = transform.Find("bullet").gameObject;
        bullet.transform.position = Vector3.zero;
        bullet.GetComponent<Rigidbody>().velocity = Vector3.zero;

        masterVolume = GameObject.FindGameObjectWithTag("GameLoop").GetComponent<GameLoop>().MasterVolume;
        gameObject.GetComponent<AudioSource>().volume *= masterVolume;
    }

    // Update is called once per frame
    void Update () {
        bool valid = false;
        bool isOverhand = false;

        if (posHand.sqrMagnitude > 0 && posElbow.sqrMagnitude > 0 && posShoulder.sqrMagnitude > 0)
            valid = true;

        if( valid )
        {
            bullet.transform.position = posHand;
            QueueHandPosition(posHand);
            QueueElbowPosition(posElbow);
        }

        Vector3 vse = (posElbow - posShoulder).normalized;
        Vector3 veh = (posHand - posElbow).normalized;
        float costh = Vector3.Dot(vse, veh);
        float diffse = Mathf.Abs(posElbow.y - posShoulder.y);
        float diffeh = Mathf.Abs(posHand.y - posElbow.y);

        if (lastCosTheta * costh < 0)
            crossingDir = GetAverageVectorFromHandQueue().normalized;

        isOverhand = (getAverageElbowMovement() < getAverageHandMovement() / 4.0f);

 //       Debug.Log(costh);
        
        // Charge!
        if( costh < CHARGE_ANGLE_COS && valid)
        {
            if( !isCharging )   // just started to charge
            {
                GameLoop gl = GameObject.FindGameObjectWithTag("GameLoop").GetComponent<GameLoop>();
               // Debug.Log("Change target");
                gameObject.transform.parent.gameObject.GetComponent<ServoController>().SetNewTarget(gameObject);
                if (!gl.StateWithoutSound.Contains(gl.state))
                {
                    gameObject.GetComponent<AudioSource>().Play();
                }
            }

            isCharging = true;
            charge = charge + Time.deltaTime;
            if (charge > CHARGE_MAX)
            {
                charge = CHARGE_MAX;
            }
        }
        else
        {
            isCharging = false;
        }

        // Shoot!!
        if( costh > SHOOT_ANGLE_COS && isCharged && valid )
        {
            Vector3 shootDir;

            // Debug.Log(getAverageElbowMovement() + " " + getAverageHandMovement());

            GameLoop gl = GameObject.FindGameObjectWithTag("GameLoop").GetComponent<GameLoop>();

            // create a projectile and check if user using both hand
            if( isOverhand )
            {
//                Debug.Log("Single Overhand");
                shootDir = GetWeightedAverageVectorFromHandQueue();
            }
            else if (Vector3.Distance(otherHand.getHandPosition(), posHand) < 0.1f && false)
            {
                if (otherHand.isCharged && isSender)
                {
 //                   Debug.Log("Double Hand");
                    shootDir = (GetAverageVectorFromHandQueue() + otherHand.GetAverageVectorFromHandQueue()).normalized;
                }
                else
                {
 //                   Debug.Log("Close But Single");
                    shootDir = (vse * 2 + veh).normalized;
                }
            }
            else
            {
//                Debug.Log("Single Jolt");
                shootDir = (vse + veh).normalized; // + this.GetMaxVectorFromHandQueue().normalized / 3.0f;
            }

            // spread the direction of projectile in virtual world.
            if (!gl.isHMD)
            {
                Vector3 proj = Vector3.forward * Vector3.Dot(Vector3.forward, shootDir); // note that shootDir is normalized
                Vector3 outer = shootDir - proj; // ortho vector from v.forward to v.shoot
                shootDir = shootDir + outer * (gl.SpreadFactor - 1.0f);
                
                // tweak the direction accordint to the height
                shootDir.y += (posShoulder.y - 1.5f);
            }


            shootProjectile(shootDir.normalized * 50.0f);
        }

        // let the Phidget, which belongs to the parent, know that we need its focus
        if (!isCharged && charge > CHARGE_READY_MIN)
        {
            GameObject pm = gameObject.transform.parent.gameObject;
            pm.GetComponent<PhidgetController>().setFocus(gameObject);

            // if GameState is waiting for Charge, move to Shoot state
            GameLoop gl = GameObject.FindGameObjectWithTag("GameLoop").GetComponent<GameLoop>();
            gl.NextStateWithCheckCurrentState(GameLoop.GameState.TutorialCharge);
        }

        isCharged = (charge > CHARGE_READY_MIN);

        gameObject.transform.position = posHand;
        if (isCharged)
            gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        else
            gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
        gameObject.transform.localScale = new Vector3(1 + charge, 1 + charge, 1 + charge);

        BulletController b = gameObject.GetComponentInChildren<BulletController>();
        b.SetCharge(isCharged, getNormalizedCharge());
	}


    public void shootProjectile(Vector3 velocity, float projectCharge = 0.0f) {
        // create a projectile
        GameObject projectile = Instantiate(bullet);
        projectile.transform.position = gameObject.transform.position;
        projectile.GetComponent<Rigidbody>().velocity = velocity;
        var bc = projectile.GetComponent<BulletController>();
        bc.isFlying = true;
        bc.soundMulti = GameObject.FindGameObjectWithTag("GameLoop").GetComponent<GameLoop>().MasterVolume;
        if (projectCharge == 0.0f)
            projectCharge = getNormalizedCharge();
        bc.SetCharge(true, projectCharge);

        isCharging = false;
        isCharged = false;
        charge = 0;
    }

    private void QueueHandPosition(Vector3 vec)
    {
        if (handPositionHistory[0] == vec)
            return;
        for (int i = HAND_HISTORY_NUMBER - 1; i > 0; i--)
            handPositionHistory[i] = handPositionHistory[i - 1];
        handPositionHistory[0] = vec;
    }

    private void QueueElbowPosition(Vector3 vec)
    {
        if (elbowPositionHistory[0] == vec)
            return;
        for (int i = HAND_HISTORY_NUMBER - 1; i > 0; i--)
            elbowPositionHistory[i] = elbowPositionHistory[i - 1];
        elbowPositionHistory[0] = vec;
    }

    public Vector3 GetAverageVectorFromHandQueue() {
        Vector3 average = new Vector3();
        for (int i = 0; i < HAND_HISTORY_NUMBER-1; i++) {
            average = average * 1.2f + (handPositionHistory[i] - handPositionHistory[i + 1]);
        }
        return average.normalized;
    }

    public Vector3 GetWeightedAverageVectorFromHandQueue()
    {
        Vector3 average = new Vector3();
        for (int i = 0; i < HAND_HISTORY_NUMBER - 1; i++)
        {
            Vector3 v = (handPositionHistory[i] - handPositionHistory[i + 1]);
            average = average + v * v.magnitude;
        }
        return average.normalized;
    }

    public Vector3 GetMaxVectorFromHandQueue()
    {
        Vector3 result = Vector3.zero;
        for (int i = 0; i < HAND_HISTORY_NUMBER - 1; i++)
        {
            Vector3 newvector = (handPositionHistory[i] - handPositionHistory[i + 1]);
            if (result.magnitude < newvector.magnitude)
            {
                result = newvector;
            }
        }
        return result.normalized;
    }

    float getAverageHandMovement()
    {
        float result = 0;
        for (int i = 0; i < HAND_HISTORY_NUMBER - 1; i++)
        {
            result += (handPositionHistory[i] - handPositionHistory[i + 1]).magnitude;
        }
        return result;
    }


    float getAverageElbowMovement()
    {
        float result = 0;
        for(int i=0; i<HAND_HISTORY_NUMBER-1; i++)
        {
            result += (elbowPositionHistory[i] - elbowPositionHistory[i + 1]).magnitude;
        }
        return result;
    }

    public void SetPosition(Vector3 pos)
    {
        posHand = pos;
    }
    public Vector3 getHandPosition()
    {
        return posHand;
    }

    public void SetElbowPosition(Vector3 pos)
    {
        posElbow = pos;
    }

    public void SetShoulderPosition(Vector3 pos)
    {
        posShoulder = pos;
    }

    public float chargeRatio()
    {
        return charge / CHARGE_MAX;
    }

}
