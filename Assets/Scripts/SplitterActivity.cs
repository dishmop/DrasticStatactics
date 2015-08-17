using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplitterActivity : Activity {

    public Transform[] keyPositions;

    protected override void Start()
    {
        base.Start();
        nextPathCount = keyPositions.Length;
        nextSendPaths = new Queue<int>();
	}

    protected override void Update()
    {
        base.Update();
    }

    public override Vector3 GetKeyPosition(int number)
    {
        if (number >= 0 && number < nextPathCount)
        {
            return keyPositions[number].position;
        }
        return Vector3.zero;
    }

    public override PathPoint GetPathPoint(RunnerController runner)
    {
        if (nextSendPaths.Count > 0 && nextPathCount > 0 && isReady)
        {
            isReady = false;
            int nextPath = nextSendPaths.Dequeue();
            //if (nextPath >= 0 && nextPath < nextPathCount)
            {
                if (nextPath % 2 == 0)
                {
                    runner.nextChosenPath = 0;
                }
                else
                {
                    runner.nextChosenPath = 1;
                }
                return nextPathPoints[Mathf.FloorToInt(nextPath * 0.5f)];
            }
        }
        return this;
    }
}
