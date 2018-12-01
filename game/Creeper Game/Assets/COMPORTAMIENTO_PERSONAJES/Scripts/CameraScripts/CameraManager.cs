using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    private Camera[] cameras;

	// Use this for initialization
	void Start () {
        cameras = Camera.allCameras;
        Debug.Log("Numero de camaras = " + cameras.Length);
        foreach(Camera c in cameras)
        {
            if (c.name.Equals("fpsCamera"))
            {
                c.enabled = true;
            }
            else
            {
                c.enabled = false;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.C))
        {
            foreach (Camera c in cameras)
            {
                if (c.name.Equals("fpsCamera"))
                {
                    c.enabled = !c.enabled;
                }
                else
                {
                    c.enabled = !c.enabled;
                }
            }
        }
	}
}
