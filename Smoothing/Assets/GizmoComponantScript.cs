using UnityEngine;
using System.Collections;

public class GizmoComponantScript : MonoBehaviour {

    //just for colour and movement setting

    public Color color;
    public Vector3 influence;

	// Use this for initialization
	void Start () {
        GetComponent<Renderer>().material.color = color;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
