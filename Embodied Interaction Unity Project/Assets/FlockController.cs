using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockController : MonoBehaviour
{
    public float minVelocity = 5;
    public float maxVelocity = 20;
    public float Randomness { get; set; }
    public int flockSize = 20;
    public FlockBehaviour prefab;
    public Transform target;

    internal Vector3 flockCenter;
    internal Vector3 flockVelocity;
    internal Vector3 targetVelocity;

    public float Cohesion { get; set; }
    public float Steering {get; set;}   //High Space
    public float Seeking {get; set;}    //based on Low Flow
    public float Avoidance {get; set;}  //Based on High Flow
    //public float Directness {get; set;} //High Space Effort

    List<FlockBehaviour> particles = new List<FlockBehaviour>();

    void Start()
    {
        if (GameObject.FindGameObjectWithTag("Body"))
        {
            target = GameObject.FindGameObjectWithTag("Body").transform;
        }
        for (int i = 0; i < flockSize; i++)
        {
            CreateParticle();
        }
    }

    private void CreateParticle()
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
        Randomness = Mathf.Lerp(15, 2, Mathf.InverseLerp(0, 30, GraphicsController.Instance.Space));

        Cohesion = Mathf.Lerp(1f, -1f, Mathf.InverseLerp(200, 1500, GraphicsController.Instance.Flow));
        Avoidance = Mathf.LerpUnclamped(10, 1000, Mathf.InverseLerp(200, 1500, GraphicsController.Instance.Flow));
        Seeking = Mathf.LerpUnclamped(10, 0, Mathf.InverseLerp(200, 1500, GraphicsController.Instance.Flow));
        Steering = Mathf.Lerp(0, 10, Mathf.InverseLerp(0, 30, GraphicsController.Instance.Space));
    }
}
