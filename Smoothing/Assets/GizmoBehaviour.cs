using UnityEngine;
using System.Collections;

public class GizmoBehaviour : MonoBehaviour {

    private Vector3 previousMousePos;
    private GizmoComponantScript selectedComponent;
    private bool mooving = false;

    private float originalScale;

	//Ed:  Use this for initialization
	void Start ()
    {
        previousMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        originalScale = Camera.main.orthographicSize;
    }
	
	//Ed:  Update is called once per frame
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
            if (selectedComponent != null)
                selectedComponent.resetColor();
        }

        if (mooving)
            moveGizmo();

        previousMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Ed: scale the gizmo to how far zoomed in we are
        transform.localScale = Vector3.one * (Camera.main.orthographicSize / originalScale);
    }

    //Ed: return the gizmo componant selected
    private GizmoComponantScript pointOnGizmo()
    {
        //Ed: get gizmo part
        RaycastHit rayCastHit = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayCastHit))
            if (rayCastHit.collider.CompareTag("Gizmo"))
                return rayCastHit.collider.GetComponent<GizmoComponantScript>(); ;

        return null;
    }

    //Ed: move according to mouse movments and what componet was selcted
    private void moveGizmo()
    {
        Vector3 delta = Camera.main.ScreenToWorldPoint(Input.mousePosition) - previousMousePos;
        
        //Ed: contrain movmenet by the axis we have selected
        delta = Vector3.Scale(delta, selectedComponent.influence);

        transform.position += delta;
    }
}
