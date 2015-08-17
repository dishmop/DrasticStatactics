using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Obstacle : PathPoint {

    //public PathPoint deathPathPoint;
    public float goodProbability = 0.5f;
    bool isGood = false;
    bool hit;
    public GameObject obstacleObject;
    Material materialDefault;
    public Material materialGood, materialBad;
    public GameObject uiText;
    public Transform uiTextTarget;
    //TextMesh textMesh;

	protected override void Start ()
    {
        base.Start();

        materialDefault = obstacleObject.GetComponent<MeshRenderer>().material;
        //uiText = (GameObject)Instantiate(uiText);
        //uiText.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform, false);
        uiText.GetComponentInChildren<Text>().text
            = uiText.GetComponentInChildren<Text>().text.Replace("x", Mathf.Round(goodProbability * 100.0f).ToString());
        //textMesh = GetComponent<TextMesh>();
        //textMesh.text = textMesh.text.Replace("x", Mathf.Round(goodProbability * 100.0f).ToString());
        Reset();
	}
	
    protected override void Update()
    {
        base.Update();
        if (isViewed)
        {
            uiText.SetActive(true);
            uiText.transform.position = Camera.main.WorldToScreenPoint(uiTextTarget.position) - Vector3.up * 35.0f;
        }
        else
        {
            uiText.SetActive(false);
        }
        uiText.GetComponent<Image>().color = Color.Lerp(uiText.GetComponent<Image>().color, Color.white, 0.1f);
        //textMesh.characterSize = 0.003f * (transform.position - Camera.main.transform.position).magnitude;
	}

    void OnTriggerEnter(Collider collider)
    {
        //Debug.Log(collider.transform.name + " has crashed into " + name + ", which is " + (isGood ? "good" : "bad"));
        if (collider.tag == "Runner")
        {
            //Debug.Log("Runner has crashed into " + name + ", which is " + (isGood ? "good" : "bad"));
            isReady = false;
            hit = true;
            //parentWall.NotifyHit();
            if (isGood)
            {
                obstacleObject.GetComponent<MeshRenderer>().material = materialGood;
                //collider.GetComponent<RunnerController>().FireGoodParticles();//This will look better from the PathPoint trigger
            }
            else
            {
                obstacleObject.GetComponent<MeshRenderer>().material = materialBad;
                collider.GetComponent<RunnerController>().Kill();
            }
        }
    }

    public override PathPoint GetPathPoint(RunnerController runner)
    {
        if (nextPathPoints != null && isReady)
        {
            if (nextPathPoints.Length == 1)
            {
                isReady = false;
                return nextPathPoints[0];
            }
        }
        return this;
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
                Reset();
            }
        }
    }
    protected override void Reset()
    {
        obstacleObject.GetComponent<MeshRenderer>().material = materialDefault;
        isGood = (Random.value < goodProbability ? true : false);
        uiText.GetComponent<Image>().color = Color.clear;
        isReady = true;
        hit = false;
        //Debug: so that we can see in advance which obstacles are good
        /*if (isGood)
        {
            obstacleObject.GetComponent<MeshRenderer>().material = materialGood;
        }
        else
        {
            obstacleObject.GetComponent<MeshRenderer>().material = materialBad;
        }*/
    }
}
