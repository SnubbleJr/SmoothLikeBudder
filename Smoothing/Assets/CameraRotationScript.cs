using UnityEngine;
using System.Collections;

public class CameraRotationScript : MonoBehaviour {
    
    private Vector3 previousMousePos;
    private Camera[] cameras;

    private bool canRotate = true;

    private const float scale = 0.75f;

    // Use this for initialization
    void Start ()
    {
        previousMousePos = Input.mousePosition;
        cameras = GetComponentsInChildren<Camera>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (canRotate)
            mouseMovementHandler();

        scrollHandler();

        previousMousePos = Input.mousePosition;
    }

    //deals with rotating the camera around 0,0,0
    private void mouseMovementHandler()
    {
        if (Input.GetButton("Fire3"))
        {
            Vector3 delta = Input.mousePosition - previousMousePos;
            delta *= scale;

            transform.RotateAround(Vector3.zero, Vector3.up, delta.x * 0.5f);
            transform.RotateAround(Vector3.zero, transform.right, delta.y);
        }
    }

    //deals with zooming in and out
    private void scrollHandler()
    {
        Vector2 scrollWheel = Input.mouseScrollDelta;
        if (scrollWheel.sqrMagnitude > 0)
            foreach (Camera camera in cameras)
                if (camera.orthographicSize - (scrollWheel.y * scale) > 0.1f)
                    camera.orthographicSize -= (scrollWheel.y * scale);
    }

    //called by send messages, sets wiether to rotate or not
    public void setRotation(bool val)
    {
        canRotate = val;
    }
}
