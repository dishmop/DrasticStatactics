using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DecidedActivity : Activity {

    DeciderActivity deciderActivity;

    protected override void Start()
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
        if (nextPathPoints == null)
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
        deciderActivity = GetComponentInParent<DeciderActivity>();
	}

    protected override void Update()
    {
        base.Update();
    }

    public override PathPoint GetPathPoint(int runnerPathChoice = -1)
    {
        if (isReady && runnerPathChoice >= 0 && runnerPathChoice < nextPathPoints.Length)
        {
            isReady = false;
            if (nextPathPoints[runnerPathChoice].isReady)
            {
                nextPathPoints[runnerPathChoice].isReady = false;
                return nextPathPoints[runnerPathChoice];
            }
        }
        return this;
    }

    private void GetNextPathPointsFromChildren()
    {
        //Fill a list with all of the child transforms, then add any that have a PathPoint to nextPathPoints
        List<PathPoint> tempNextPathPoints = new List<PathPoint>();
        PathPoint tempPathPoint;
        foreach (Transform child in transform)
        {
            tempPathPoint = child.GetComponent<PathPoint>();
            if (tempPathPoint != null)
            {
                tempNextPathPoints.Add(tempPathPoint);
            }
        }
        nextPathPoints = tempNextPathPoints.ToArray();
    }

    public Transform GetCameraTransform()
    {
        return deciderActivity.cameraTransform;
    }
}
