using UnityEngine;
using System.Collections;

public class CameraRotationScript : MonoBehaviour {
    
    private Vector3 previousMousePos;
    private Camera[] cameras;

    // Use this for initialization
    void Start ()
    {
        previousMousePos = Input.mousePosition;
        cameras = GetComponentsInChildren<Camera>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        float scale = 0.75f;
	    if (Input.GetButton("Fire3"))
        {
            Vector3 delta = Input.mousePosition - previousMousePos;
            delta *= scale;
            
            transform.RotateAround(Vector3.zero, Vector3.up, delta.x*0.5f);
            transform.RotateAround(Vector3.zero, transform.right, delta.y);
        }

        Vector2 scrollWheel = Input.mouseScrollDelta;
        if (scrollWheel.sqrMagnitude > 0)
            foreach (Camera camera in cameras)
                if (camera.orthographicSize - (scrollWheel.y * scale) > 0.1f)
                    camera.orthographicSize -= (scrollWheel.y * scale);

        previousMousePos = Input.mousePosition;
    }
}
