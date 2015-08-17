using UnityEngine;
using System.Collections;

public class Door : PathPoint {

    WallActivity parentWall;
    bool isGood = false;
    Material materialDefault;
    public Material materialGood, materialBad;
    public float probabilityKill = 0.2f;
    bool explodeNextFrame = false;
    float explosionRadius = 3.0f;

    void Awake()
    {
        materialDefault = GetComponent<MeshRenderer>().material;
    }

	protected override void Start ()
    {
        setToGround = false;
        base.Start();
        /*if (previousPathPoint == null)
        {
            previousPathPoint = GetComponentInParent<PathPoint>();
            if (previousPathPoint == null)
            {
                previousPathPoint = this;
            }
        }*/
        parentWall = GetComponentInParent<WallActivity>();
        isReady = true;
	}
	
    protected override void Update()
    {
        base.Update();

        if (explodeNextFrame)
        {
            Explode();
            explodeNextFrame = false;
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Runner")
        {
            //Debug.Log(collision.transform.name + " has crashed into " + name + ", which is " + (isGood ? "good" : "bad"));
            isReady = false;
            parentWall.NotifyHit();
            if (collision.collider.GetComponent<Animator>().GetBool("OnGround"))
            {
                if (isGood)
                {
                    //Debug.Log(collision.transform.name + " has crashed into " + name + ", which is good");
                    GetComponent<MeshRenderer>().material = materialGood;
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    collision.collider.GetComponent<RunnerController>().currentPathPoint = nextPathPoints[0];
                    collision.collider.GetComponent<RunnerController>().FireGoodParticles();
                }
                else
                {
                    GetComponent<MeshRenderer>().material = materialBad;
                    if (Random.value < probabilityKill)
                    {
                        //Debug.Log(collision.transform.name + " has crashed into " + name + ", which is bad and causes death");
                        collision.collider.GetComponent<RunnerController>().Kill();
                        explodeNextFrame = true;
                    }
                    else
                    {
                        //Debug.Log(collision.transform.name + " has crashed into " + name + ", which is bad but non-lethal");
                        //collision.collider.GetComponent<RunnerController>().PushAway(transform.forward * -5.0f + Vector3.up * 5.0f);
                        collision.collider.GetComponent<RunnerController>().currentPathPoint = parentWall;
                    }
                }
            }
        }
    }

    protected override void UpdateReadiness() { }

    public void SetIsGood(bool good)
    {
        GetComponent<MeshRenderer>().material = materialDefault;
        isGood = good;
        isReady = true;
    }

    void Explode()
    {
        Vector3 explosionPos = transform.position - Vector3.up;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddExplosionForce(1500.0f, explosionPos, explosionRadius, 3.0F);

        }
    }
}
