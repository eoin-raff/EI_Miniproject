using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class LabanDescriptors : MonoBehaviour
{
    //Enum defining types of efforts
    private enum Efforts { Weight, Space, Time, Flow };

    //Kinect Dependencies
    public GameObject BodySourceManager;
    private BodySourceManager _BodyManager;

    // All Bodies tracked by the Kinect
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();

    //List of joints that will be used to measure efforts - can be altered to just get individual limbs or joints.

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

    // List of joints used for each specific weight
    private Dictionary<Efforts, List<JointType>> effortJoints = new Dictionary<Efforts, List<JointType>>();
    private List<JointType> weightJoints = new List<JointType>
        {
            JointType.SpineBase,
            JointType.FootLeft,
            JointType.FootRight,
            JointType.HandTipLeft,
            JointType.HandTipRight
        };
    private List<JointType> timeJoints = new List<JointType>
        {
            JointType.SpineBase,
            JointType.FootLeft,
            JointType.FootRight,
            JointType.HandTipLeft,
            JointType.HandTipRight
        };
    private List<JointType> spaceJoints = new List<JointType>
        {
            JointType.Head,
            JointType.ShoulderLeft,
            JointType.ShoulderRight
        };
    private List<JointType> flowJoints = new List<JointType>
        {
        //doesn't specify which joints to use for flow calculations
            JointType.SpineBase,
            JointType.FootLeft,
            JointType.FootRight,
            JointType.HandTipLeft,
            JointType.HandTipRight
        };

    // Transforms (Position & Rotation), Velocity, and Previous Positions (used for velocity calculations) of each Joint

    private Dictionary<ulong, Dictionary<JointType, Transform>> BodyJoints = new Dictionary<ulong, Dictionary<JointType, Transform>>();
    private Dictionary<ulong, Dictionary<JointType, Vector3>> BodyJoints_Velocity = new Dictionary<ulong, Dictionary<JointType, Vector3>>();
    private Dictionary<ulong, Dictionary<JointType, Vector3>> BodyJoints_PrevPos = new Dictionary<ulong, Dictionary<JointType, Vector3>>();

    // Frame by Frame values for each Effort per tracked body.

    private Dictionary<ulong, List<float>> WeightEffortCalculator = new Dictionary<ulong, List<float>>();
    private Dictionary<ulong, List<float>> TimeEffortCalculator = new Dictionary<ulong, List<float>>();
    private Dictionary<ulong, List<float>> SpaceEffortCalculator = new Dictionary<ulong, List<float>>();
    private Dictionary<ulong, List<float>> FlowEffortCalculator = new Dictionary<ulong, List<float>>();

    // Final Effort values for each tracked body

    public Dictionary<ulong, float> BodyWeightEffort = new Dictionary<ulong, float>();
    public Dictionary<ulong, float> BodyTimeEffort = new Dictionary<ulong, float>();
    public Dictionary<ulong, float> BodySpaceEffort = new Dictionary<ulong, float>();
    public Dictionary<ulong, float> BodyFlowEffort = new Dictionary<ulong, float>();

    //Public Accessors for Effort Values. Currently only returns for one body and needs to be refactored to allow more.
    public float WeightEffortValue { get; private set; }

    //Triggers for running Calculation Coroutines at distinct time intervals
    private Dictionary<Efforts, bool> effortCalculationsRunning = new Dictionary<Efforts, bool> {
        [Efforts.Weight] = false,
        [Efforts.Flow] = false,
        [Efforts.Time] = false,
        [Efforts.Space] = false,
    };

    private void Start()
    {
        effortJoints.Add(Efforts.Weight, weightJoints);
        effortJoints.Add(Efforts.Time, timeJoints);
        effortJoints.Add(Efforts.Space, spaceJoints);
        effortJoints.Add(Efforts.Flow, flowJoints);
    }

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
            WeightEffortCalculator[id].Add(CalculateEffort(id, Efforts.Weight));
            SpaceEffortCalculator[id].Add(CalculateEffort(id, Efforts.Space));
            TimeEffortCalculator[id].Add(CalculateEffort(id, Efforts.Time));
            FlowEffortCalculator[id].Add(CalculateEffort(id, Efforts.Flow));

            if (!effortCalculationsRunning[Efforts.Weight]) ;
                StartCoroutine(GetMaxOverTime(id, WeightEffortCalculator[id], 0.05f, Efforts.Weight));
            print("Effort: " + BodyWeightEffort[id]);
            //FIXME
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
        SpaceEffortCalculator.Add(id, new List<float>());
        TimeEffortCalculator.Add(id, new List<float>());
        FlowEffortCalculator.Add(id, new List<float>());

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

    private float TimeEffort(ulong id)
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
            //TODO: CALCULATIONS
        }
        return weight;
    }

    private float SpaceEffort(ulong id)
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

    private float CalculateEffort(ulong id, Efforts effort)
    {
        List<JointType> joints = effortJoints[effort];
        Dictionary<JointType, Transform> JointTransforms = BodyJoints[id];
        Dictionary<JointType, Vector3> JointVelocity = BodyJoints_Velocity[id];
        Dictionary<JointType, Vector3> JointPrevPos = BodyJoints_PrevPos[id];

        float weight = 0;
        foreach (JointType joint in joints)
        {
            switch (effort)
            {
                case Efforts.Weight:
                    weight += JointVelocity[joint].sqrMagnitude;
                    break;
                case Efforts.Space:
                    weight = 0;
                    break;
                case Efforts.Time:
                    //Sum of accelerations over time of representative joints (Root, Finger, Toes)
                    weight = 0;
                    break;
                case Efforts.Flow:
                    weight = 0;
                    break;
                default:
                    break;
            }
        }
        return weight;
    }

    IEnumerator GetMaxOverTime(ulong id, List<float> list, float seconds, Efforts effort)
    {
        //REMOVE ID AND JUST RETURN MAX ---- i.e. BodyWeightEffort[id] = StartCoroutine(GetMaxOverTime(List, seconds, trigger))
        effortCalculationsRunning[effort] = true;
        yield return new WaitForSeconds(seconds);

        float[] array = list.ToArray();
        float max = Mathf.Max(array);
        list.Clear();
        effortCalculationsRunning[effort] = false;
        switch (effort)
        {
            case Efforts.Weight:
                yield return BodyWeightEffort[id] = max;
                break;
            case Efforts.Space:
                break;
            case Efforts.Time:
                break;
            case Efforts.Flow:
                break;
            default:
                break;
        }
    }
}
