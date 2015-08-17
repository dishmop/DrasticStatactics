using System;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ThirdPersonCharacter))]
public class RunnerController : MonoBehaviour
{
    public NavMeshAgent agent { get; private set; } // the navmesh agent required for the path finding
    public ThirdPersonCharacter character { get; private set; } // the character we are controlling
    public PathPoint currentPathPoint; // target to aim for
    public int nextChosenPath = -1;
    public GameObject ragdoll;
    bool hasMadeRagdoll = false;
    public ParticleSystem goodEffect, badEffect;
    public AudioClip goodSound, badSound;

    GameController gc;

    // Use this for initialization
    private void Start()
    {
        // get the components on the object we need ( should not be null due to require component so no need to check )
        agent = GetComponentInChildren<NavMeshAgent>();
        character = GetComponent<ThirdPersonCharacter>();

        gc = GameObject.Find("Game Controller").GetComponent<GameController>();

        agent.updateRotation = false;
        agent.updatePosition = true;

        transform.position += UnityEngine.Random.insideUnitSphere * 0.1f + Vector3.up * 0.1f;
    }


    // Update is called once per frame
    private void Update()
    {
        if (currentPathPoint != null && agent.enabled)
        {
            if ((transform.position - currentPathPoint.transform.position).sqrMagnitude < currentPathPoint.proximityRequiredSquared
                || gc.paused)
            {
                //agent.SetDestination(transform.position);//Added; needs testing. Doesn't seem to fix the continued-running issue
                agent.Stop();
                currentPathPoint = currentPathPoint.GetPathPoint(this);
            }
            else
            {
                agent.SetDestination(currentPathPoint.transform.position);
                agent.Resume();
                GetComponent<Animator>().SetBool("OnGround", false);
                character.Move(agent.desiredVelocity, false, false);
            }
        }
        if (hasMadeRagdoll)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "Terrain")
        {
            agent.enabled = true;
        }
    }

    public void PushAway(Vector3 velocity)
    {
        agent.enabled = false;
        GetComponent<Rigidbody>().velocity = velocity;
    }

    public void Kill()
    {
        if (!hasMadeRagdoll)
        {
            GenerateRagdoll();
        }
        FireBadParticles();
        //Delete the runner object to which this script is attached
        Destroy(gameObject);
    }
    void GenerateRagdoll()
    {
        if (ragdoll != null)
        {
            //Instantiate a ragdoll and align it's transforms
            Transform curRagdoll = ((GameObject)Instantiate(ragdoll, transform.position, transform.rotation)).transform;
            KinetifyChildren(transform.FindChild("EthanSkeleton"), curRagdoll.FindChild("EthanSkeleton"), agent.velocity * 0.75f);
            badEffect.transform.SetParent(curRagdoll.transform);
        }
    }
    public void KinetifyChildren(Transform source, Transform destination, Vector3 velocity)
    {
        //Debug.Log("AlignTransform depth = " + depth);
        destination.position = source.position;
        destination.rotation = source.rotation;
        Rigidbody rb = destination.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = velocity;
        }
        foreach (Transform sourceChild in source)
        {
            KinetifyChildren(sourceChild, destination.FindChild(sourceChild.name), velocity);
        }
    }


    public void FireGoodParticles()
    {
        if (goodEffect != null)
        {
            goodEffect.Play();
        }
        if (goodSound != null)
        {
            AudioSource.PlayClipAtPoint(goodSound, transform.position);
        }
    }
    public void FireBadParticles()
    {
        if (badEffect != null)
        {
            badEffect.Play();
        }
        if (badSound != null)
        {
            AudioSource.PlayClipAtPoint(badSound, transform.position);
        }
    }
}
