using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public PathPoint currentPathPoint;
    public Transform targetCameraTransform;
    public bool paused = false;
    Image[] keys = new Image[6];
    public Color colourActiveKey, colourDisabledKey;

    public GameObject setOfRunners;
    public Transform runnerSpawnPoint;
    public Activity runnerStartActivity;
    bool alreadySpawned = true;

    public GameObject pathPointMarker, pathPointMarkerLink, pathPointMarkerArrow;
    private GameObject map;
    private Image currentActivityMarker, cameraMarker;
    Vector2 pathCentre;
    public float mapScale = 2.0f;//HUD size divieded by physical size
    private bool mapInitialised = false;

    private Text activityNameText, activityDescriptionText, runnerText;

	// Use this for initialization
	void Start()
    {

        //GameObject.Find("Canvas").GetComponent<Canvas>().enabled = true;

        activityNameText = GameObject.Find("Activity Name Text").GetComponent<Text>();
        activityDescriptionText = GameObject.Find("Activity Description Text").GetComponent<Text>();
        runnerText = GameObject.Find("Runner Text").GetComponent<Text>();
        keys[0] = GameObject.Find("Q").GetComponent<Image>();
        keys[1] = GameObject.Find("W").GetComponent<Image>();
        keys[2] = GameObject.Find("E").GetComponent<Image>();
        keys[3] = GameObject.Find("R").GetComponent<Image>();
        keys[4] = GameObject.Find("T").GetComponent<Image>();
        keys[5] = GameObject.Find("Y").GetComponent<Image>();

        currentActivityMarker = GameObject.Find("Current Activity Marker").GetComponent<Image>();
        cameraMarker = GameObject.Find("Camera Marker").GetComponent<Image>();
	}
	
	// Update is called once per frame
    void Update()
    {
        if (!mapInitialised)
        {
            InitialiseMap();
        }
        if (!paused)
        {
            MoveCamera(0.1f);

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                GoToPathPoint(currentPathPoint.GetNextActivity());
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                GoToPathPoint(currentPathPoint.GetNextActivity(-1));
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                GoToPathPoint(currentPathPoint.GetSiblingActivity(-1));
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                GoToPathPoint(currentPathPoint.GetSiblingActivity(1));
            }

            if (currentPathPoint != null)
            {
                if (currentPathPoint is Activity)
                {
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        ((Activity)currentPathPoint).SetNextSendPath(0);
                    }
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        ((Activity)currentPathPoint).SetNextSendPath(1);
                    }
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        ((Activity)currentPathPoint).SetNextSendPath(2);
                    }
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        ((Activity)currentPathPoint).SetNextSendPath(3);
                    }
                    if (Input.GetKeyDown(KeyCode.T))
                    {
                        ((Activity)currentPathPoint).SetNextSendPath(4);
                    }
                    if (Input.GetKeyDown(KeyCode.Y))
                    {
                        ((Activity)currentPathPoint).SetNextSendPath(5);
                    }
                    UpdateKeyInterface();
                    activityNameText.text = ((Activity)currentPathPoint).GetActivityName();
                    activityDescriptionText.text = ((Activity)currentPathPoint).GetActivityDescription();
                    runnerText.text = "Runners Left: " + GameObject.FindGameObjectsWithTag("Runner").Length.ToString();

                }
            }
        }
    }

    public void Pause()
    {
        //Do relevant things to pause the game
        paused = true;
    }
    public void Resume()
    {
        //Do relevant things to unpause the game
        paused = false;
    }
    
    public void MoveCamera(float lerpValue)
    {
        if (targetCameraTransform != null)
        {
            //Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraTransform.position, lerpValue);
            //Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetCameraTransform.rotation, lerpValue);
            float dist = (Camera.main.transform.position - targetCameraTransform.position).magnitude;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position,
                targetCameraTransform.position - targetCameraTransform.forward * dist * 0.65f, lerpValue);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetCameraTransform.rotation, lerpValue);
            //Keep camera out of the ground
            float minimumHeight = Terrain.activeTerrain.terrainData.GetHeight(Mathf.RoundToInt(Camera.main.transform.position.x),
                Mathf.RoundToInt(Camera.main.transform.position.z)) + 10.0f;
            if (Camera.main.transform.position.y < minimumHeight)
            {
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position,
                new Vector3(Camera.main.transform.position.x, minimumHeight, Camera.main.transform.position.z),
                Mathf.Min(lerpValue * 2.0f, 1.0f));
            }
        }
        if (mapInitialised)
        {
            cameraMarker.transform.localPosition = (new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z)
                - pathCentre)* mapScale;
            cameraMarker.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f - Camera.main.transform.rotation.eulerAngles.y);
        }
    }

    void UpdateKeyInterface()
    {
        if (!(currentPathPoint is SplitterActivity))
        {
            for (int n = 0; n < currentPathPoint.nextPathCount && n < keys.Length; n++)
            {
                if (currentPathPoint.nextPathPoints[n].isReady)
                {
                    keys[n].transform.position
                        = Camera.main.WorldToScreenPoint(((Activity)currentPathPoint).GetKeyPosition(n));
                    keys[n].color = colourActiveKey;
                }
                else
                {
                    keys[n].transform.position
                        = Camera.main.WorldToScreenPoint(((Activity)currentPathPoint).GetKeyPosition(n));
                    keys[n].color = colourDisabledKey;
                }
            }
        }
        else
        {
            for (int n = 0; n < currentPathPoint.nextPathCount && n < keys.Length; n++)
            {
                keys[n].transform.position
                    = Camera.main.WorldToScreenPoint(((Activity)currentPathPoint).GetKeyPosition(n));
                keys[n].color = colourActiveKey;
            }
        }
    }

    public void GoToPathPoint(PathPoint newPathPoint)
    {
        currentPathPoint.SetIsViewing(false);
        if (newPathPoint != null)
        {
            currentPathPoint = newPathPoint;
            if (currentPathPoint is Activity)
            {
                Activity currentActivity = (Activity)currentPathPoint;
                targetCameraTransform = currentActivity.cameraTransform;
                for (int n = 0; n < currentActivity.nextPathCount && n < keys.Length; n++)
                {
                    /*keys[n].transform.rotation = Quaternion.LookRotation(keys[n].transform.position - targetCameraTransform.position);
                    keys[n].transform.localScale
                        = Vector3.one * (keys[n].transform.position - targetCameraTransform.position).magnitude * 0.075f;*/
                    keys[n].enabled = true;
                }
                for (int n = currentPathPoint.nextPathCount; n < keys.Length; n++)
                {
                    keys[n].enabled = false;
                }
                currentActivityMarker.transform.localPosition
                    = (new Vector2(currentPathPoint.transform.position.x, currentPathPoint.transform.position.z) - pathCentre) * mapScale;
            }
            else
            {
                for (int n = 0; n < keys.Length; n++)
                {
                    keys[n].enabled = false;
                }
            }
        }
        else
        {
            //Debug.Log("New Activity given to GameController was null");
        }
        currentPathPoint.SetIsViewing(true);
    }

    private void InitialiseMap()
    {
        //Add markers to the map to show the location of activities
        PathPoint[] pathPoints;
        map = GameObject.Find("Map");
        GameObject[] tempPathPoints = GameObject.FindGameObjectsWithTag("Path Point");
        pathPoints = new PathPoint[tempPathPoints.Length];
        Button curButton;
        Transform curArrow;
        Vector2 currentPosition, currentDirection;
        pathCentre = new Vector2(GameObject.Find("Path").transform.position.x, GameObject.Find("Path").transform.position.z);
        for (int p = pathPoints.Length - 1; p >= 0; p--)
        {
            pathPoints[p] = tempPathPoints[p].GetComponent<PathPoint>();
            currentPosition
                = (new Vector2(tempPathPoints[p].transform.position.x, tempPathPoints[p].transform.position.z) - pathCentre) * mapScale;
            //Add lines to the map to show the connections to the next activities
            if (pathPoints[p].nextPathPoints != null)
            {
                if (pathPoints[p].nextPathPoints.Length > 0)
                {
                    for (int n = pathPoints[p].nextPathPoints.Length - 1; n >= 0; n--)
                    {
                        if (pathPoints[p].nextPathPoints[n] is Activity)
                        {
                            curArrow = ((GameObject)Instantiate(pathPointMarkerArrow)).transform;
                        }
                        else
                        {
                            curArrow = ((GameObject)Instantiate(pathPointMarkerLink)).transform;
                        }
                        curArrow.name = pathPoints[p].name + " - " + pathPoints[p].nextPathPoints[n].name + " Link";
                        curArrow.SetParent(map.transform, false);
                        currentDirection = (new Vector2(pathPoints[p].nextPathPoints[n].transform.position.x,
                            pathPoints[p].nextPathPoints[n].transform.position.z) - pathCentre) * mapScale - currentPosition;
                        curArrow.localPosition = currentPosition + currentDirection;
                        //curArrow.localScale = new Vector3(currentDirection.magnitude, 1.0f, 1.0f);
                        curArrow.GetComponentInChildren<Image>().fillAmount = currentDirection.magnitude * 0.005f;// / 200.0f;
                        curArrow.localRotation// = Quaternion.LookRotation(currentDirection, Vector3.forward);
                            = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg);
                        //curArrow.FindChild("Arrowhead").localScale = new Vector3(1.0f / currentDirection.magnitude, 1.0f, 1.0f);
                        //curArrow.GetComponentInChildren<Image>().color = Color.green;
                    }
                }
            }
            if (pathPoints[p] is Activity)
            {
                curButton = ((GameObject)Instantiate(pathPointMarker)).GetComponent<Button>();
                curButton.name = pathPoints[p].name + " Button";
                curButton.onClick.AddListener(delegate { GoToPathPoint(pathPoints[p]); });
                curButton.transform.SetParent(map.transform, false);
                curButton.transform.localPosition = currentPosition;
            }
        }
        GoToPathPoint(currentPathPoint);
        currentActivityMarker.transform.SetAsLastSibling();
        cameraMarker.transform.SetAsLastSibling();
        mapInitialised = true;
    }

    public void Initialise()
    {
        if (!alreadySpawned)
        {
            foreach (GameObject go_runner in GameObject.FindGameObjectsWithTag("Runner"))
            {
                Destroy(go_runner);
            }
            Instantiate(setOfRunners, runnerSpawnPoint.position, runnerSpawnPoint.rotation);
            foreach (GameObject go_runner in GameObject.FindGameObjectsWithTag("Runner"))
            {
                go_runner.GetComponent<RunnerController>().currentPathPoint = runnerStartActivity;
            }
        }
        alreadySpawned = false;
    }

}
