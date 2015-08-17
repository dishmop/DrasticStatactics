using UnityEngine;
using System.Collections;

public class WallActivity : Activity {

    public int biasDoor = -1;
    public float biasAmount = 0.6f;
    private float[] doorProbabilities;
    private GameObject[] doors;
    private Vector3[] doorStartPositions;
    public PathPoint exitPathPoint;
    public bool hit;

	protected override void Start ()
    {
        base.Start();
        //Find all of the child doors
        Door[] doorScripts = GetComponentsInChildren<Door>();
        doors = new GameObject[doorScripts.Length];
        doorStartPositions = new Vector3[doors.Length];
        //nextPathPoints = new PathPoint[doors.Length];
        for (int d = doorScripts.Length - 1; d >= 0; d--)
        {
            doors[d] = doorScripts[d].gameObject;
            doorStartPositions[d] = doors[d].transform.position;
            doorScripts[d].nextPathPoints = new PathPoint[1] { exitPathPoint };
            //nextPathPoints[d] = doors[d].GetComponent<Door>();
        }
        doorProbabilities = new float[doors.Length];
        biasDoor = Random.Range(0, 4);
        SetBias();
        Reset();
	}
	
    protected override void Update()
    {
        base.Update();

        if (!hit)
        {
            for (int d = doors.Length - 1; d >= 0; d--)
            {
                doors[d].transform.position = Vector3.Lerp(doors[d].transform.position, doorStartPositions[d], 0.3f);
            }
        }
	}

    public override Vector3 GetKeyPosition(int number)
    {
        if (number >= 0 && number < nextPathCount)
        {
            return doorStartPositions[number] + Vector3.up * 2.0f;
        }
        return Vector3.zero;
    }

    public override PathPoint GetPathPoint(RunnerController runner)
    {
        PathPoint returnPoint = base.GetPathPoint(runner);
        if (!isReady)
        {
            for (int n = nextPathPoints.Length - 1; n >= 0; n--)
            {
                nextPathPoints[n].isReady = false;
            }
        }
        if (fireParticles) { runner.FireGoodParticles(); }
        return returnPoint;
    }

    protected override void UpdateReadiness()
    {
        if (isReady)
        {
            curResetTime = 0.0f;
        }
        else if (hit)
        {
            if (curResetTime < 0.0f)
            {
                curResetTime = 0.0f;
            }
            curResetTime += Time.deltaTime;
            if (curResetTime > resetTime)
            {
                Reset();
            }
        }
        else
        {
            curResetTime -= Time.deltaTime;
            if (curResetTime < resetTime * -4.0f)
            {
                //Apply explosion to clear any leftover runners?
                Reset();
            }
        }
    }

    protected override void Reset()
    {
        float randomValue = Random.Range(0.0f, 1.0f);
        float lowerBound = 0.0f;
        for (int d = doors.Length - 1; d >= 0; d--)
        {
            doors[d].transform.position = doorStartPositions[d] + Vector3.down * 3.0f;
            doors[d].transform.localRotation = Quaternion.identity;
            doors[d].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            if (randomValue > lowerBound && randomValue < lowerBound + doorProbabilities[d])
            {
                ((Door)nextPathPoints[d]).SetIsGood(true);
            }
            else
            {
                ((Door)nextPathPoints[d]).SetIsGood(false);
            }
            lowerBound += doorProbabilities[d];
        }
        curResetTime = 0.0f;
        isReady = true;
        hit = false;
    }

    private void SetBias()
    {
        if (biasDoor >= 0 && biasDoor < doorProbabilities.Length
            && biasAmount >= 0.0f && biasAmount <= 1.0f)
        {
            float otherProbability = (1.0f - biasAmount) / (doorProbabilities.Length - 1);
            for (int d = doorProbabilities.Length - 1; d >= 0; d--)
            {
                doorProbabilities[d] = otherProbability;
            }
            doorProbabilities[biasDoor] = biasAmount;
        }
        else
        {
            float probability = 1.0f / doorProbabilities.Length;
            for (int d = doorProbabilities.Length - 1; d >= 0; d--)
            {
                doorProbabilities[d] = probability;
            }
        }
    }

    public void NotifyHit()
    {
        isReady = false;
        hit = true;
        /*for (int d = doorTransforms.Length - 1; d >= 0; d--)
        {
            doorTransforms[d].GetComponent<Door>().isReady = false;
        }*/
    }
}
