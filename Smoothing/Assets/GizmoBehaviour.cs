using UnityEngine;
using System.Collections;

public class GizmoBehaviour : MonoBehaviour {

    private Vector3 previousMousePos;
    private GizmoComponantScript selectedComponent;
    private bool mooving = false;

	// Use this for initialization
	void Start ()
    {
        previousMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
	
	// Update is called once per frame
	void Update () {

        GizmoComponantScript component = pointOnGizmo();
        if (Input.GetButtonDown("Fire1"))
            if (component != null)
            {
                mooving = true;
                selectedComponent = component;
                selectedComponent.setColor(Color.yellow);
            }
            else
                mooving = false;

        if (Input.GetButtonUp("Fire1"))
        {
            mooving = false;
            selectedComponent.resetColor();
        }

        if (mooving)
            moveGizmo();

        previousMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    //return the gizmo componant selected
    private GizmoComponantScript pointOnGizmo()
    {
        //get gizmo part
        RaycastHit rayCastHit = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayCastHit))
            if (rayCastHit.collider.CompareTag("Gizmo"))
                return rayCastHit.collider.GetComponent<GizmoComponantScript>(); ;

        return null;
    }

    //move according to mouse movments and what componet was selcted
    private void moveGizmo()
    {
        Vector3 delta = Camera.main.ScreenToWorldPoint(Input.mousePosition) - previousMousePos;
        
        //contrain movmenet by the axis we have selected
        delta = Vector3.Scale(delta, selectedComponent.influence);

        transform.position += delta;
    }
}
