using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class getJointPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {/*
        KinectInterop.JointType joint = KinectInterop.JointType.HandRight;
        KinectManager manager = KinectManager.Instance;

        if (manager && manager.IsInitialized())
        {
            if (manager.IsUserDetected())
            {
                long userId = manager.GetPrimaryUserID();

                if (manager.IsJointTracked(userId, (int)joint))
                {
                    Vector3 jointPos = manager.GetJointPosition(userId, (int)joint);
                    // do something with the joint position
                }
            }
        }*/
    }
}
