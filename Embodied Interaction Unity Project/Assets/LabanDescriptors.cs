using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Kinect = Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class LabanDescriptors : MonoBehaviour
{
    public GameObject BodySourceManager;
    private BodySourceManager _BodyManager;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
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
    private List<Kinect.JointType> _Joints = new List<Kinect.JointType>
    {
        Kinect.JointType.FootLeft,
        Kinect.JointType.AnkleLeft,
        Kinect.JointType.KneeLeft, 
        Kinect.JointType.HipLeft,

        Kinect.JointType.FootRight, 
        Kinect.JointType.AnkleRight, 
        Kinect.JointType.KneeRight,
        Kinect.JointType.HipRight,

        Kinect.JointType.HandTipLeft,
        Kinect.JointType.ThumbLeft,
        Kinect.JointType.HandLeft,
        Kinect.JointType.WristLeft,
        Kinect.JointType.ElbowLeft,
        Kinect.JointType.ShoulderLeft,
        Kinect.JointType.SpineShoulder,

        Kinect.JointType.HandTipRight, 
        Kinect.JointType.ThumbRight, 
        Kinect.JointType.HandRight, 
        Kinect.JointType.WristRight,
        Kinect.JointType.ElbowRight,
        Kinect.JointType.ShoulderRight, 
        
        Kinect.JointType.SpineShoulder,

        Kinect.JointType.SpineBase,
        Kinect.JointType.SpineMid,
        Kinect.JointType.SpineShoulder, 
        Kinect.JointType.Neck, 
        Kinect.JointType.Head
    };
    //Only works for one person!!!
    public Dictionary<Kinect.JointType, Transform> JointPositions = new Dictionary<Kinect.JointType, Transform>();


    void Update()
    {
        #region Get Kinect Data
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
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }
        #endregion

        #region Delete Kinect Bodies
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }
        #endregion

        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
                print(body.Joints.Values);
            }
        }

        print("root position: " + GetJointPosition(Kinect.JointType.SpineBase).position);

    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
            JointPositions.Add(jt, jointObj.transform);
        }

        return body;
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;

            if (_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }

            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
        }
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

    private Transform GetJointPosition(Kinect.JointType jointType)
    {
        if (!JointPositions.ContainsKey(jointType))
        {
            return null;
        }
        return JointPositions[jointType];
    }
}
