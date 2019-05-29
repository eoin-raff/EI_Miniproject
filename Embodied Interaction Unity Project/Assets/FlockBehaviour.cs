using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class FlockBehaviour : MonoBehaviour
{
    internal FlockController controller;
    private Rigidbody rigidbody;

    private enum JointName
    {
        FootLeft,
        AnkleLeft,
        KneeLeft,
        HipLeft,

        FootRight,
        AnkleRight,
        KneeRight,
        HipRight,

        HandTipLeft,
        ThumbLeft,
        HandLeft,
        WristLeft,
        ElbowLeft,
        ShoulderLeft,

        HandTipRight,
        ThumbRight,
        HandRight,
        WristRight,
        ElbowRight,
        ShoulderRight,

        SpineBase,
        SpineMid,
        SpineShoulder,
        Neck,
        Head
    }
    private JointName targetJoint;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        targetJoint = (JointName)Random.Range(0, 25);
    }

    // Update is called once per frame
    void Update()
    {
        rigidbody.velocity += Steer() * Time.deltaTime;
        float mag = Mathf.Clamp(rigidbody.velocity.magnitude, controller.minVelocity, controller.maxVelocity);
        rigidbody.velocity = rigidbody.velocity.normalized * mag;

       /* transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            Mathf.Clamp(transform.position.z, 0, 10));// transform.position.z);
    */}

    private Vector3 Steer()
    {
        Vector3 randomize = new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1);
        randomize.Normalize();

        Vector3 baseDirection = GraphicsController.Instance.RootDirection;

        Vector3 center = (controller.flockCenter - transform.localPosition);

        Vector3 velocity = (controller.flockVelocity - rigidbody.velocity);

        Vector3 avoid = (transform.position - NearestObjectWithTag("Particle").transform.position);

        Vector3 follow;

        if (GameObject.Find(targetJoint.ToString()) == null)
        {
            follow = Vector3.zero;
        }
        else
        {
            follow = GameObject.Find(targetJoint.ToString()).transform.position - transform.localPosition;
        }

        /*
                if (NearestObjectWithTag("Joint") == null)
                {
                    follow = Vector3.zero;
                }
                else
                {
                    follow = (NearestObjectWithTag("Joint").transform.localPosition - transform.localPosition); //closest joint

                }*/
        return (baseDirection * controller.Steering) + (randomize * controller.Randomness) + (center * controller.Cohesion) + (velocity * controller.Steering) + (follow * controller.Seeking) + (avoid * controller.Avoidance);
    }

    private GameObject NearestObjectWithTag(string tag)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

}
