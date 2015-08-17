using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    GameController gc;

    struct Menu
    {
        public Canvas canvas;
        public Button[] buttons;
        public RectTransform[] buttonTransforms;
        public Text[] buttonTexts;
    }
    public enum MenuState
    {
        Main, About, Quit, Pause, None,
    }
    public MenuState state;
    public static string websiteText = "http://divf.eng.cam.ac.uk/gam2eng/Main/WebHome";
    public float defaultLeftAnchor = 0.35f, defaultRightAnchor = 0.65f,
        extendedLeftAnchor = 0.25f, extendedRightAnchor = 0.75f;
    //public Color defaultTextColour, higlightedTextColour;

    public GameObject[] canvasGameObjects = new GameObject[4];

    public AudioClip soundMouseEnter, soundMouseExit, soundMouseClick;

    //Variables for menu background camera effects
    public Transform targetCameraTransform, cameraGimbalTransform;

    Menu[] menus;
    private int selectedButton = -1;

    // Use this for initialization
    void Start()
    {
        gc = GameObject.Find("Game Controller").GetComponent<GameController>();

        menus = new Menu[5];
        for (int m = 0; m < 4; m++)
        {
            menus[m] = new Menu();
            menus[m].canvas = canvasGameObjects[m].GetComponent<Canvas>();
            menus[m].buttons = menus[m].canvas.GetComponentsInChildren<Button>();
            menus[m].buttonTransforms = new RectTransform[menus[m].buttons.Length];
            menus[m].buttonTexts = new Text[menus[m].buttons.Length];
            for (int b = menus[m].buttons.Length - 1; b >= 0; b--)
            {
                menus[m].buttonTransforms[b] = menus[m].buttons[b].GetComponent<RectTransform>();
                menus[m].buttonTexts[b] = menus[m].buttons[b].GetComponentInChildren<Text>();
            }
            menus[m].canvas.enabled = false;
        }
        menus[4] = new Menu();
        menus[4].canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        state = MenuState.None;
        SetState(MenuState.Main);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state == MenuState.None)
            {
                PauseGame();
            }
            else if (state == MenuState.Pause)
            {
                ResumeGame();
            }
            else if (state == MenuState.Main)
            {
                GoToQuitMenu();
            }
            else
            {
                GoToMainMenu();
            }
        }
        if (state != MenuState.None)
        {
            MoveCamera(0.1f);
        }
    }

    public void MoveCamera(float lerpValue)
    {
        if (targetCameraTransform != null)
        {
            cameraGimbalTransform.localRotation
                = Quaternion.Euler(0.0f, cameraGimbalTransform.rotation.eulerAngles.y + 12.0f * Time.deltaTime, 0.0f);
            targetCameraTransform.localPosition = Vector3.Lerp(targetCameraTransform.localPosition,
                new Vector3(targetCameraTransform.localPosition.x, Terrain.activeTerrain.terrainData.GetHeight(
                    Mathf.RoundToInt(targetCameraTransform.position.x), Mathf.RoundToInt(targetCameraTransform.position.z)) + 15.0f,
                    targetCameraTransform.localPosition.z), lerpValue * 0.25f);
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraTransform.position, lerpValue);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetCameraTransform.rotation, lerpValue);
        }
        float minimumHeight = Terrain.activeTerrain.terrainData.GetHeight(Mathf.RoundToInt(Camera.main.transform.position.x),
            Mathf.RoundToInt(Camera.main.transform.position.z)) + 10.0f;
        if (Camera.main.transform.position.y < minimumHeight)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position,
            new Vector3(Camera.main.transform.position.x, minimumHeight, Camera.main.transform.position.z),
            Mathf.Min(lerpValue * 2.0f, 1.0f));
        }
    }

    public void SetState(MenuState newState)
    {
        //Do update things first
        menus[(int)state].canvas.enabled = false;
        if (state == MenuState.None)
        {
            gc.Pause();
        }
        state = newState;
        menus[(int)state].canvas.enabled = true;
        if (state == MenuState.None)
        {
            Cursor.visible = false;
            gc.Resume();
        }
        else
        {
            Cursor.visible = true;
        }

        if (soundMouseClick != null)
        {
            AudioSource.PlayClipAtPoint(soundMouseClick, transform.position, 0.5f);
        }
    }

    public void StartGame()
    {
        SetState(MenuState.None);
        //Perform setup
        gc.Initialise();
    }
    public void PauseGame()
    {
        SetState(MenuState.Pause);
    }
    public void ResumeGame()
    {
        SetState(MenuState.None);
        //Don't perform setup
    }
    public void GoToMainMenu()
    {
        SetState(MenuState.Main);
    }
    public void GoToAboutMenu()
    {
        SetState(MenuState.About);
    }
    public void GoToQuitMenu()
    {
        SetState(MenuState.Quit);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void CopyWebsiteText()
    {
        //To be filled in at some point maybe?
    }

    public void MouseEnter(GameObject callingButton)
    {
        //Debug.Log("Mouse has entered " + callingButton.ToString());
        if ((int)state < 4)
        {
            for (int n = menus[(int)state].buttons.Length - 1; n >= 0; n--)
            {
                if (menus[(int)state].buttons[n].Equals(callingButton))
                {
                    //Debug.Log(callingButton.ToString() + " matched with index " + n.ToString());
                    selectedButton = n;
                    menus[(int)state].buttonTransforms[selectedButton].anchorMax
                        = new Vector2(extendedRightAnchor, menus[(int)state].buttonTransforms[selectedButton].anchorMax.y);
                    menus[(int)state].buttonTransforms[selectedButton].anchorMin
                        = new Vector2(extendedLeftAnchor, menus[(int)state].buttonTransforms[selectedButton].anchorMin.y);
                    if (soundMouseEnter != null)
                    {
                        AudioSource.PlayClipAtPoint(soundMouseEnter, transform.position, 0.5f);
                    }
                    return;
                }
            }
        }
    }
    public void MouseExit(bool playSound = true)
    {
        if (selectedButton != -1)
        {
            if (selectedButton >= 0 && selectedButton < menus[(int)state].buttonTransforms.Length)
            {
                menus[(int)state].buttonTransforms[selectedButton].anchorMax
                    = new Vector2(defaultRightAnchor, menus[(int)state].buttonTransforms[selectedButton].anchorMax.y);
                menus[(int)state].buttonTransforms[selectedButton].anchorMin
                    = new Vector2(defaultLeftAnchor, menus[(int)state].buttonTransforms[selectedButton].anchorMin.y);
            }
            if (playSound && soundMouseExit != null)
            {
                AudioSource.PlayClipAtPoint(soundMouseExit, transform.position, 0.5f);
            }
        }
        selectedButton = -1;
    }
}
