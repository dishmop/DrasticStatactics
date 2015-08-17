using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathPoint : MonoBehaviour {

    public float proximityRequiredSquared = 1.0f;
    public PathPoint previousPathPoint;
    public PathPoint[] nextPathPoints;
    protected bool setToGround = true;
    public int nextPathCount;
    protected bool haveNotified = false;
    public int pathNumberFromPrevious = -1;
    public float resetTime = 1.0f;
    public float curResetTime;
    public bool isReady = false;
    public bool isViewed = false;

    public bool fireParticles;

    protected virtual void Start()
    {
        if (previousPathPoint == null)
        {
            previousPathPoint = GetComponentInParent<PathPoint>();
            if (previousPathPoint == null)
            {
                previousPathPoint = this;
            }
        }
        //If this PathPoint doesn't have any explicit nextPathPoints
        if (nextPathPoints == null || nextPathPoints.Length == 0)
        {
            GetNextPathPointsFromChildren();
        }
        else//If this PathPoint has anything but 1 nextPathPoints
        {
            if (nextPathPoints.Length != 1)
            {
                GetNextPathPointsFromChildren();
            }
        }
        if (nextPathPoints == null)
        {
            //Debug.Log(name + " has no nextPathPoints (nextPathPoints == null)");
        }
        else if (nextPathPoints.Length == 0)
        {
            //Debug.Log(name + " has no nextPathPoints");
        }

        nextPathCount = nextPathPoints.Length;

        //Shift this PathPoint so it lies on the ground
        if (setToGround)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                Vector3 translation = hit.point - transform.position;
                transform.Translate(translation);
                foreach (Transform child in transform)
                {
                    child.Translate(-translation);
                }
                if (this is Activity)
                {
                    ((Activity)this).cameraTransform.position += translation;
                }
            }
        }
    }

    protected virtual void Update()
    {
	    if (!haveNotified)
        {
            NotifyNextPathPointsOfPreviousness();
        }
        UpdateReadiness();
	}

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Mathf.Sqrt(proximityRequiredSquared));
        if (nextPathPoints.Length > 0)
        {
            for (int p = nextPathPoints.Length - 1; p >= 0; p--)
            {
                if (nextPathPoints[p] != null)
                {
                    Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, nextPathPoints[p].transform.position + Vector3.up * 0.5f);
                }
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<PathPoint>() != null && !child.name.Contains("Main Exit"))
                {
                    Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, child.transform.position + Vector3.up * 0.5f);
                }
            }
        }
        Gizmos.color = Color.red;
        if (previousPathPoint != null)
        {
            Gizmos.DrawLine(transform.position + Vector3.up * 0.7f,
                Vector3.Lerp(transform.position + Vector3.up * 0.5f, previousPathPoint.transform.position + Vector3.up * 0.5f, 0.33f));
        }
    }

    protected void NotifyNextPathPointsOfPreviousness()
    {
        haveNotified = true;
        for (int p = nextPathPoints.Length - 1; p >= 0; p--)
        {
            if (nextPathPoints[p] != null)
            {
                nextPathPoints[p].previousPathPoint = this;
                if (nextPathPoints[p].pathNumberFromPrevious == -1)
                {
                    nextPathPoints[p].pathNumberFromPrevious = p;
                }
            }
            else
            {
                //Debug.Log(name + "'s nextPathPoint is null");
                haveNotified = false;
            }
        }
    }

    protected void GetNextPathPointsFromChildren()
    {
        //Fill a list with all of the child transforms, then add any that have a PathPoint to nextPathPoints
        List<PathPoint> tempNextPathPoints = new List<PathPoint>();
        PathPoint tempPathPoint;
        foreach (Transform child in transform)
        {
            tempPathPoint = child.GetComponent<PathPoint>();
            if (tempPathPoint != null && !tempPathPoint.name.Contains("Main Exit"))
            {
                tempNextPathPoints.Add(tempPathPoint);
            }
        }
        nextPathPoints = tempNextPathPoints.ToArray();
    }

    public virtual Vector3 GetKeyPosition(int number)
    {
        if (number >= 0 && number < nextPathCount)
        {
            return nextPathPoints[number].transform.position + Vector3.up * 3.0f;
        }
        return Vector3.zero;
    }

    public virtual PathPoint GetPathPoint(RunnerController runner)
    {
        if (nextPathPoints != null && isReady)
        {
            if (nextPathPoints.Length == 1)
            {
                isReady = false;
                if (fireParticles) { runner.FireGoodParticles(); }
                return nextPathPoints[0];
            }
            else if (runner.nextChosenPath >= 0 && runner.nextChosenPath < nextPathPoints.Length)
            {
                isReady = false;
                int next = runner.nextChosenPath;
                runner.nextChosenPath = -1;
                if (fireParticles) { runner.FireGoodParticles(); }
                return nextPathPoints[next];
            }
        }
        return this;
    }

    public Activity GetNextActivity(int pathChosen = 0, int attempt = 0)
    {
        Activity returnActivity = null;
        if (attempt > 25)
        {
            //Debug.Log("Recursive search for next activity timed out");
            return returnActivity;
        }
        if (pathChosen < 0)
        {
            if (previousPathPoint != null && previousPathPoint != this)
            {
                if (previousPathPoint is Activity)
                {
                    returnActivity = (Activity)previousPathPoint;
                }
                else
                {
                    returnActivity = previousPathPoint.GetNextActivity(-1, attempt + 1);
                }
            }
        }
        else if (pathChosen < nextPathPoints.Length)
        {
            if (nextPathPoints[pathChosen] is Activity)
            {
                returnActivity = (Activity)nextPathPoints[pathChosen];
            }
            else
            {
                returnActivity = nextPathPoints[pathChosen].GetNextActivity(0, attempt + 1);
            }
        }
        if (returnActivity == null && this is Activity)
        {
            //Debug.Log("Search for next Activity did something weird");
            returnActivity = (Activity)this;
        }
        return returnActivity;
    }
    public Activity GetSiblingActivity(int side = 1)
    {
        if (side == 1 || side == -1)
        {
            Activity lastFork = GetNextActivity(-1);
            //Debug.Log("lastFork is:" + lastFork);
            if (lastFork.nextPathCount > 1)
            {
                return lastFork.GetNextActivity((pathNumberFromPrevious + side + lastFork.nextPathPoints.Length)
                       % lastFork.nextPathPoints.Length);
            }
            else
            {
                //Debug.Log("This activity (" + this.name + ") has no siblings");
                return null;
            }
            /*return (Activity)previousPathPoint.nextPathPoints[(pathNumberFromPrevious + side + previousPathPoint.nextPathPoints.Length)
                % previousPathPoint.nextPathPoints.Length];*/
        }
        //Debug.Log("GetSiblingActivity was given an invalid argument: " + side);
        return null;
    }

    protected virtual void UpdateReadiness()
    {
        if (isReady)
        {
            curResetTime = 0.0f;
        }
        else
        {
            curResetTime += Time.deltaTime;
            if (curResetTime > resetTime)
            {
                Reset();
            }
        }
    }
    protected virtual void Reset()
    {
        curResetTime = 0.0f;
        isReady = true;
    }

    public void SetIsViewing(bool isViewing)
    {
        isViewed = isViewing;
        PathPoint childPathPoint;
        foreach (Transform child in transform)
        {
            childPathPoint = child.GetComponent<PathPoint>();
            if (childPathPoint != null)
            {
                childPathPoint.SetIsViewing(isViewing);
            }
        }
    }
}
