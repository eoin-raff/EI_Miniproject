using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockController : MonoBehaviour
{
    public float minVelocity = 5;
    public float maxVelocity = 20;
    public float randomness = 1;
    public int flockSize = 20;
    public FlockBehaviour prefab;
    public Transform target;

    internal Vector3 flockCenter;
    internal Vector3 flockVelocity;
    internal Vector3 targetVelocity;

    List<FlockBehaviour> particles = new List<FlockBehaviour>();

    void Start()
    {
        if (GameObject.FindGameObjectWithTag("Body"))
        {
            target = GameObject.FindGameObjectWithTag("Body").transform;
        }
        for (int i = 0; i < flockSize; i++)
        {
            FlockBehaviour particle = Instantiate(prefab, transform.position, transform.rotation) as FlockBehaviour;
            Collider collider = GetComponent<Collider>();
            particle.transform.parent = transform;
            particle.transform.localPosition = new Vector3(
                            Random.value * collider.bounds.size.x,
                            Random.value * collider.bounds.size.y,
                            Random.value * collider.bounds.size.z) - collider.bounds.extents;
            particle.controller = this;
            particles.Add(particle);
        }
    }

    void Update()
    {
        if (target = null)
        {
            target = GameObject.FindGameObjectWithTag("Body").transform;
        }
        Vector3 center = Vector3.zero;
        Vector3 velocity = Vector3.zero;
        foreach (FlockBehaviour particle in particles)
        {
            center += particle.transform.localPosition;
            velocity += particle.GetComponent<Rigidbody>().velocity;
        }
        flockCenter = center / flockSize;
        flockVelocity = velocity / flockSize;
        targetVelocity = GraphicsController.Instance.RootDirection;
        randomness = GraphicsController.Instance.Flow;
        
    }
}
