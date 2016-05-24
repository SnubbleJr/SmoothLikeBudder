using UnityEngine;
using System.Collections;

public class GizmoArrowScript : MonoBehaviour {

    //just for colour setting

    public Color color;

	// Use this for initialization
	void Start () {
        GetComponent<Renderer>().material.color = color;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
