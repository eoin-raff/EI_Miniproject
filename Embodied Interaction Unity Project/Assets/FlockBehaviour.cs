using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockBehaviour : MonoBehaviour
{
    internal FlockController controller;
    private Rigidbody rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rigidbody.velocity += Steer() * Time.deltaTime;
        float mag = Mathf.Clamp(rigidbody.velocity.magnitude, controller.minVelocity, controller.maxVelocity);
        rigidbody.velocity = rigidbody.velocity.normalized * mag;
    }

    private Vector3 Steer()
    {
        Vector3 randomize = new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1);
        randomize.Normalize();
        float randomnessMultiplier = Mathf.Lerp(100, 1, Mathf.InverseLerp(0, 30, GraphicsController.Instance.Space));
        randomize *= randomnessMultiplier;

        Vector3 center = controller.flockCenter - transform.localPosition;
        Vector3 velocity = controller.flockVelocity - rigidbody.velocity;
        Vector3 avoid = transform.position - NearestObjectWithTag("Particle").transform.position;

        Vector3 follow;
        if (NearestObjectWithTag("Joint") == null)
        {
            follow = Vector3.zero;
        }
        else
        {
            follow = NearestObjectWithTag("Joint").transform.localPosition - transform.localPosition; //closest joint

        }
        return (center + velocity + follow + avoid* 2 + randomize);
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
