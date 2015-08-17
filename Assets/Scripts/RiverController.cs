using UnityEngine;
using System.Collections;

public class RiverController : MonoBehaviour {

    public float force = 1000.0f, flowSpeed = 5.0f;

    void OnTriggerStay(Collider collider)
    {
        Rigidbody rb = collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (rb.velocity.sqrMagnitude < flowSpeed * flowSpeed)
            {
                rb.AddForce(transform.right * force);
            }
            rb.AddForce(Physics.gravity * rb.mass * -1.3f);
            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
        }
    }
    void OnTriggerLeave(Collider collider)
    {
        Rigidbody rb = collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.drag = 0.0f;
            rb.angularDrag = 0.05f;
        }
    }
}
