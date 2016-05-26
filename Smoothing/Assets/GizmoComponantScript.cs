using UnityEngine;
using System.Collections;

public class GizmoComponantScript : MonoBehaviour {

    //just for colour and movement setting

    public Color color;
    public Vector3 influence;

    private Renderer renderer;

	// Use this for initialization
	void Start ()
    {
        renderer = GetComponent<Renderer>();
        setColor(color);
	}

    public void setColor(Color color)
    {
        renderer.material.color = color;
    }

    public void resetColor()
    {
        renderer.material.color = color;
    }
}
