using UnityEngine;
using System.Collections;

public class DeciderActivity : Activity {

    private DecidedActivity[] decidedActivities;
    private Transform[] obstacleStartTransforms;
    public PathPoint exitPathPoint;
    public Material good, bad;

	protected override void Start ()
    {
        base.Start();
        //Find all of the child doors
        decidedActivities = GetComponentsInChildren<DecidedActivity>();
        Reset();
	}
	
    protected override void Update()
    {
        base.Update();

	}

    public override PathPoint GetPathPoint(int runnerPathChoice = -1)
    {
        PathPoint returnPoint =  base.GetPathPoint();
        if (!isReady)
        {
            for (int n = nextPathPoints.Length - 1; n >= 0; n--)
            {
                nextPathPoints[n].isReady = false;
            }
        }
        return returnPoint;
    }

    protected override void UpdateReadiness()
    {
        if (isReady)
        {
            curResetTime = 0.0f;
        }
        else
        {
            //if (hit)
            {
                curResetTime += Time.deltaTime;
                if (curResetTime > resetTime)
                {
                    Reset();
                }
            }
        }
    }

    protected override void Reset()
    {
        /*float randomValue = Random.Range(0.0f, 1.0f);
        float lowerBound = 0.0f;
        //Transform currentTransform;
        for (int d = doors.Length - 1; d >= 0; d--)
        {
            /*currentTransform = doors[d].transform;
            if (doors[d] != null)
            {
                Destroy(doors[d]);
            }
            doors[d] = (GameObject)Instantiate(door, currentTransform.position, currentTransform.rotation);
            doors[d].transform.SetParent(transform);*
            doors[d].transform.position = doorStartTransforms[d].position;
            doors[d].transform.rotation = doorStartTransforms[d].rotation;
            doors[d].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            //nextPathPoints[d] = doors[d].GetComponent<Door>();
            //nextPathPoints[d].nextPathPoints = new PathPoint[1] { exitPathPoint };
            if (randomValue > lowerBound && randomValue < lowerBound + doorProbabilities[d])
            {
                ((Door)nextPathPoints[d]).SetIsGood(this, true, good);
            }
            else
            {
                ((Door)nextPathPoints[d]).SetIsGood(this, false, bad);
            }
            lowerBound += doorProbabilities[d];
        }*/
        isReady = true;
    }

    public void NotifyHit()
    {
        isReady = false;
        //hit = true;
        /*for (int d = doorTransforms.Length - 1; d >= 0; d--)
        {
            doorTransforms[d].GetComponent<Door>().isReady = false;
        }*/
    }
}
