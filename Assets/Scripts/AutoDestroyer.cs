using UnityEngine;
using System.Collections;

public class AutoDestroyer : MonoBehaviour {

    public float lifeTime = 7.5f;
    public float horizontalRange = 1000.0f, downwardRange = 200.0f;
    private float curLifeTime;

	void Start () {
        curLifeTime = 0.0f;
	}

    void Update()
    {
        curLifeTime += Time.deltaTime;
        if (transform.position.y < -downwardRange || Mathf.Abs(transform.position.x) > horizontalRange
            || Mathf.Abs(transform.position.z) > horizontalRange || curLifeTime > lifeTime)
        {
            Destroy(gameObject);
        }
    }

}
