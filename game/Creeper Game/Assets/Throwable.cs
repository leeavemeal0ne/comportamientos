using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour {

    private bool updatePos;
    private Rigidbody r;
    private Vector3 force;

    // Use this for initialization
    void Start() {
        updatePos = false;
        r = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (updatePos) {
            Debug.Log("UPDATING LATA POSITION");
            Debug.Log(force);
            r.useGravity = true;
            r.AddForce(force);
            
            

        }
    }

    public void updatePhysics() {
        updatePos = true;
    }

    
    public void setForce(Vector3 source, Vector3 target, float angle)
    {
        Vector3 direction = target - source;
        float h = direction.y;
        direction.y = 0;
        float distance = direction.magnitude;
        float a = angle * Mathf.Deg2Rad;
        direction.y = distance * Mathf.Tan(a);
        distance += h / Mathf.Tan(a);

        // calculate velocity
        float velocity = Mathf.Sqrt(distance * Physics.gravity.magnitude / Mathf.Sin(2 * a));
        force = velocity * direction.normalized;
    }

}
