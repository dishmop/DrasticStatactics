using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour {

	static AudioController instance;

    MenuController mc;

    public float menuVolume = 0.5f, gameVolume = 0.3f;

    private AudioSource source;
    public float fadeFactor = 0.02f;
    private float stayFactor;
    public bool inMenu = true;

	void Awake()
	{
		if (instance == null) {
			//Debug.Log("Assigning instance of Audio Controller");
			instance = this;//new GameObject("Audio Controller").AddComponent<AudioController>();
            source = GetComponent<AudioSource>();
            stayFactor = 1 - fadeFactor;
		} else {
			Destroy (gameObject);
		}
		DontDestroyOnLoad(this);
        mc = GameObject.Find("Menu Controller").GetComponent<MenuController>();
	}
	
	public void OnApplicationQuit()
	{
		//Debug.Log("Audio Controller destroyed");
		instance = null;
		Destroy(this);
	}

    void Update()
    {
        if (mc.state == MenuController.MenuState.None)
        {
            source.volume = source.volume * stayFactor + gameVolume * fadeFactor;
        }
        else
        {
            source.volume = source.volume * stayFactor + menuVolume * fadeFactor;
        }
    }
}
