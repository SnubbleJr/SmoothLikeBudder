using UnityEngine;
using System.Collections.Generic;

public class MeshSculptingScript : MonoBehaviour {
    //This script lets a mesh be for deformed by the mouse

    private Transform trans;
    int mainVert;
    SortedDictionary<int, float> neighbours;
    Vector3 prevMousePos = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        getVerts(checkNewMesh());
        moveVerts();
    }

    private Vector3 checkNewMesh()
    {
        //get new object if clicked
        RaycastHit rayCastHit = new RaycastHit();
        if (Input.GetButtonDown("Fire1"))
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayCastHit))
                trans = rayCastHit.collider.transform;
            else
                return Vector3.zero;
        else
            return Vector3.zero;

        return rayCastHit.point;
    }

    private void getVerts(Vector3 selectedPoint)
    {
        //get closest vert to ray cast
        //and color them
        if (selectedPoint == Vector3.zero)
            return;

        neighbours = new SortedDictionary<int, float>();

        Mesh mesh = trans.GetComponent<MeshFilter>().mesh;

        Vector3 localPoint = selectedPoint - trans.position;
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];

        //nearest vert
        float closestDistance = 100f;

        //neighbouring verts
        float thresholdDist = 0.5f;

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Mathf.Abs(Vector3.Distance(vertices[i], localPoint));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                mainVert = i;
            }

            if (distance < thresholdDist)
            {
                colors[i] = Color.Lerp(Color.yellow, Color.blue, distance / thresholdDist);
                neighbours.Add(i, thresholdDist / distance);
            }
            else
                colors[i] = Color.white;
        }

        colors[mainVert] = Color.red;
        mesh.colors = colors;
    }

    private void moveVerts()
    {
        //move verts based on mouse movement
        if (trans == null)
            return;

        Mesh mesh = trans.GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = mesh.vertices;

        float scaleFactor = 0.01f;

        if (Input.GetButton("Fire2"))
        {
            Vector3 change = Input.mousePosition - prevMousePos;
            change *= scaleFactor;

            print(change);

            foreach (KeyValuePair<int, float> neighbour in neighbours)
                vertices[neighbour.Key] += change * neighbour.Value;

            vertices[mainVert] -= (change * neighbours[mainVert] * 0.25f);

            mesh.vertices = vertices;
        }

        prevMousePos = Input.mousePosition;
    }
}
