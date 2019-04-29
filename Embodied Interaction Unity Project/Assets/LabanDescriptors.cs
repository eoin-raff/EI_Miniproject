using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class LabanDescriptors : MonoBehaviour
{
    public GameObject BodySourceManager;
    private BodySourceManager _BodyManager;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private List<JointType> _Joints = new List<JointType>
    {
        JointType.FootLeft,
        JointType.AnkleLeft,
        JointType.KneeLeft, 
        JointType.HipLeft,

        JointType.FootRight, 
        JointType.AnkleRight, 
        JointType.KneeRight,
        JointType.HipRight,

        JointType.HandTipLeft,
        JointType.ThumbLeft,
        JointType.HandLeft,
        JointType.WristLeft,
        JointType.ElbowLeft,
        JointType.ShoulderLeft,

        JointType.HandTipRight, 
        JointType.ThumbRight, 
        JointType.HandRight, 
        JointType.WristRight,
        JointType.ElbowRight,
        JointType.ShoulderRight, 
        
        JointType.SpineBase,
        JointType.SpineMid,
        JointType.SpineShoulder, 
        JointType.Neck, 
        JointType.Head
    };

    private Dictionary<ulong, Dictionary<JointType, Transform>> BodyJoints = new Dictionary<ulong, Dictionary<JointType, Transform>>();
    private Dictionary<ulong, Dictionary<JointType, Vector3>> BodyJoints_Velocity = new Dictionary<ulong, Dictionary<JointType, Vector3>>();
    private Dictionary<ulong, Dictionary<JointType, Vector3>> BodyJoints_PrevPos = new Dictionary<ulong, Dictionary<JointType, Vector3>>();

    private Dictionary<ulong, List<float>> WeightEffortCalculator = new Dictionary<ulong, List<float>>();
    public Dictionary<ulong, float> BodyWeightEffort = new Dictionary<ulong, float>();

    public float WeightEffortValue { get; private set; }

    bool weightCRrunning = false;

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

        Body[] data = _BodyManager.GetData();
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

        #region Create Kinect Bodies
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

                UpdateBodyObject(body, body.TrackingId);
            }
        }
        #endregion

        #region Laban Descriptors
        foreach (ulong id in trackedIds)
        {
            WeightEffortCalculator[id].Add(WeightEffort(id));
            if (!weightCRrunning)
                StartCoroutine(GetMaxOverTime(id, WeightEffortCalculator[id], 0.05f, weightCRrunning));
            print("Effort: " + BodyWeightEffort[id]);
            // UI ONLY WORKS WITH ONE BODY
            if (BodyWeightEffort[id] > 0)
            {
                WeightEffortValue = BodyWeightEffort[id];
            }
        }
        #endregion
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("body_" + id);
        Dictionary<JointType, Transform> JointTransforms = new Dictionary<JointType, Transform>();
        Dictionary<JointType, Vector3> JointVelocity = new Dictionary<JointType, Vector3>();
        Dictionary<JointType, Vector3> JointPrevPos = new Dictionary<JointType, Vector3>();

        foreach (JointType joint in _Joints)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = joint.ToString();
            jointObj.transform.parent = body.transform;
            //Initialize Joint Dictionaries
            JointTransforms.Add(joint, jointObj.transform);
            JointVelocity.Add(joint, Vector3.zero);
            JointPrevPos.Add(joint, jointObj.transform.position);
        }
        //Initialize whole body Dictionaries
        BodyJoints.Add(id, JointTransforms);
        BodyJoints_Velocity.Add(id, JointVelocity);
        BodyJoints_PrevPos.Add(id, JointPrevPos);
        BodyWeightEffort[id] = 0;
        WeightEffortCalculator.Add(id, new List<float>());

        return body;
    }

    private void UpdateBodyObject(Body body, ulong id)
    {
        GameObject bodyObject = _Bodies[id];
        Dictionary<JointType, Transform> JointTransforms = BodyJoints[id];
        Dictionary<JointType, Vector3> JointVelocity = BodyJoints_Velocity[id];
        Dictionary<JointType, Vector3> JointPrevPos = BodyJoints_PrevPos[id];
        foreach (JointType joint in _Joints)
        {
            Joint sourceJoint = body.Joints[joint];
            Vector3 targetPosition = GetVector3FromJoint(sourceJoint);

            Transform jointObj = bodyObject.transform.Find(joint.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            JointTransforms[joint] = jointObj.transform;
            JointVelocity[joint] = (JointTransforms[joint].position - JointPrevPos[joint]) / Time.deltaTime;
            JointPrevPos[joint] = jointObj.transform.position;
        }
    }

    private static Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

    private float WeightEffort(ulong id)
    {
        List<JointType> joints = new List<JointType>
        {
            JointType.SpineBase,
            JointType.FootLeft,
            JointType.FootRight,
            JointType.HandTipLeft,
            JointType.HandTipRight
        };

        Dictionary<JointType, Transform> JointTransforms = BodyJoints[id];
        Dictionary<JointType, Vector3> JointVelocity = BodyJoints_Velocity[id];
        Dictionary<JointType, Vector3> JointPrevPos = BodyJoints_PrevPos[id];

        float weight = 0;
        foreach (JointType joint in joints)
        {
            weight += JointVelocity[joint].sqrMagnitude; //*alpha[joint] == weight coefficient (we call this 1 for now)
            //if (joint == JointType.SpineBase)
            //    print("Body: " + id + " \nJoint: " + joint.ToString() + " Velocity: " + JointVelocity[joint]);
        }
        return weight;
    }

    IEnumerator GetMaxOverTime(ulong id, List<float> list, float seconds, bool trigger)
    {
        //REMOVE ID AND JUST RETURN MAX ---- i.e. BodyWeightEffort[id] = StartCoroutine(GetMaxOverTime(List, seconds, trigger))
        trigger = true;
        yield return new WaitForSeconds(seconds);

        float[] array = list.ToArray();
        float max = Mathf.Max(array);
        list.Clear();
        trigger = false;
        yield return BodyWeightEffort[id] = max;
    }
}
