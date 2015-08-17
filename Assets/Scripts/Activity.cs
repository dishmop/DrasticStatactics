using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Activity : PathPoint {

    public string activityName = "Activity Name", activityDescription = "Activity Description";
    public Transform cameraTransform;
    protected Queue<int> nextSendPaths;

    protected override void Start()
    {
        base.Start();
        nextSendPaths = new Queue<int>();
	}

    protected override void Update()
    {
        base.Update();
    }

    public override PathPoint GetPathPoint(RunnerController runner)
    {
        if (nextSendPaths.Count > 0 && isReady)
        {
            isReady = false;
            int nextPath = nextSendPaths.Dequeue();
            if (fireParticles) { runner.FireGoodParticles(); }
            return nextPathPoints[nextPath];
        }
        return this;
    }

    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    public void SetNextSendPath(int nextSendPath)
    {
        if (nextSendPath >= 0 && nextSendPath < nextPathCount)
        {
            nextSendPaths.Enqueue(nextSendPath);
        }
    }

    public string GetActivityName()
    {
        return activityName;
    }
    public string GetActivityDescription()
    {
        return activityDescription;
    }
}
