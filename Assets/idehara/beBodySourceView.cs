using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class beBodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
    public GameObject LeftHand;
    public GameObject RightHand;
    public GameObject MainCamera;
    public GameObject gameLoop;

    public bool isChecking = false;
    public bool isShowing = false;

    private int trackedId = -1;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    private Vector3 OffsetToWorld = Vector3.zero;
    
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    
    void Update () 
    {
        if (isChecking)
            isShowing = true;
            
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }
        Vector3 closestPosition = Vector3.zero;
        GameLoop gl = gameLoop.GetComponent<GameLoop>();

        //foreach(var body in data)
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] == null)
            {
                continue;
            }
            
            if(data[i].IsTracked)
            {
                //get kinect coodinate without offset
                Vector3 position = GetVector3FromJoint(data[i].Joints[Kinect.JointType.Head], false);

                // found new body
                if (!_Bodies.ContainsKey(data[i].TrackingId))
                {
                    _Bodies[data[i].TrackingId] = CreateBodyObject(data[i].TrackingId);
                }

                if (Mathf.Abs(position.x) < 0.3f && Mathf.Abs(position.z) < 1.5f)
                {
                    if (!gl.isHMD)
                    {
                        gl.NextStateWithCheckCurrentState(GameLoop.GameState.Opening);
                    }
                }

                // closest player detection
                if (Mathf.Abs(position.x) < 1.0f && gl.state != GameLoop.GameState.End)
                {
                    if (closestPosition == Vector3.zero || Mathf.Abs(closestPosition.z) > Mathf.Abs(position.z) )
                    {
                        trackedId = i;
                        closestPosition = position;
                    }
                }
                RefreshBodyObject(data[i], _Bodies[data[i].TrackingId]);
            } else {
                if (trackedId == i)
                    trackedId = -1;
            }
        }

        if (trackedId != -1 && data[trackedId] != null)
        {
            // Get the head position withouf offsetting to Oculus
            // and use it to determine the offset
            Vector3 posHeadKinect = GetVector3FromJoint(data[trackedId].Joints[Kinect.JointType.Head], false);
            if (gl.isHMD)
            {
                Vector3 posOculus = MainCamera.transform.position;
                OffsetToWorld = posOculus - posHeadKinect;
            }
            else
            {
                //OffsetToWorld = Vector3.zero;
                //OffsetToWorld.x = data[trackedId].Joints[Kinect.JointType.Head].Position.X * 10;
                MainCamera.transform.position = posHeadKinect;
            }
            sendSkeleton(data[trackedId]);
        }
        
        // automatic move to Opening
        if( trackedId == -1 || data[trackedId] == null || Mathf.Abs(GetVector3FromJoint(data[trackedId].Joints[Kinect.JointType.Head], false).x) > 1.0f)
        {
            if (!gl.isHMD)
            {
                gl.NextStateWithCheckCurrentState(GameLoop.GameState.End);
                trackedId = -1;
            }
        }

        foreach(var body in _Bodies)
        {
            (body.Value as GameObject).SetActive(isShowing);
        }
        
    }
    
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = BoneMaterial;
            lr.startWidth = lr.endWidth = 0.005f;
            
            jointObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        
        return body;
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.startColor = GetColorForState(sourceJoint.TrackingState);
                lr.endColor = GetColorForState(targetJoint.Value.TrackingState);
            }
            else
            {
                lr.enabled = false;
            }
        }
    }

    private void sendSkeleton(Kinect.Body body) 
    {
        RightHand.GetComponent<HandManager>().SetPosition(GetVector3FromJoint(body.Joints[Kinect.JointType.HandRight]));
        RightHand.GetComponent<HandManager>().SetElbowPosition(GetVector3FromJoint(body.Joints[Kinect.JointType.ElbowRight]));
        RightHand.GetComponent<HandManager>().SetShoulderPosition(GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderRight]));
        LeftHand.GetComponent<HandManager>().SetPosition(GetVector3FromJoint(body.Joints[Kinect.JointType.HandLeft]));
        LeftHand.GetComponent<HandManager>().SetElbowPosition(GetVector3FromJoint(body.Joints[Kinect.JointType.ElbowLeft]));
        LeftHand.GetComponent<HandManager>().SetShoulderPosition(GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderLeft]));
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }

    private Vector3 GetVector3FromJoint(Kinect.Joint joint, bool applyOffet = true)
    {
        Vector3 localPosition = new Vector3(joint.Position.X, joint.Position.Y, -joint.Position.Z);
        GameLoop gl = gameLoop.GetComponent<GameLoop>();
/*        if (!gl.isHMD)
        {
            localPosition.x *= gl.SpreadFactor;
            localPosition.y *= gl.SpreadFactor;
        }
*/
        Vector3 globalPosition = gameObject.transform.TransformPoint(localPosition);

        if (applyOffet)
            globalPosition += OffsetToWorld;

           

        return globalPosition;
    }

    public void SetDisplayMode(bool toShow, bool toCheck=false)
    {
        isShowing = toShow;
        isChecking = (isShowing && toCheck);
    }

    public void TriggerDisplay()
    {
        SetDisplayMode(!isShowing);
    }

    public void ResetTrackingID()
    {
        trackedId = -1;
    }
}
