using UnityEngine;
using System.Collections.Generic;

public class VertexSelecter : MonoBehaviour {
    //Handels selecting of verts for ROI and gizmo

    public Material radialSelecterMat;

    private SudoMesh sudoMesh;
    private HEMesh heMesh;
    private MeshFilter[] meshFilters;
    private MeshCollider[] meshColliders;

    private GameObject gizmo;
    private int gizmodVert = -1;

    private GameObject radialSelector;
    private Vector3 rSScreenSpawn;
    private Vector3 previousMousePos;

    private List<int> selectedVerts = new List<int>();

    //called when everthing is initialised
    public void begin(SudoMesh sMesh, HEMesh hMesh, MeshFilter[] mFilters, MeshCollider[] mColliders, GameObject g)
    {
        sudoMesh = sMesh;
        heMesh = hMesh;
        meshFilters = mFilters;
        meshColliders = mColliders;
        gizmo = g;
    }

    //updates the vertices and color of the new submeshes
    private void updateMeshes()
    {
        Mesh[] newMeshes = sudoMesh.getMeshes();

        for (int i = 0; i < newMeshes.Length; i++)
            meshFilters[i].sharedMesh = newMeshes[i];
    }

    void Update()
    {
        gizmoSelectionHandeler();
        vertexSelectionHandeler();
        radialSelectionHandeler();

        paintVertices();
    }

    //deals with selecting vertex to be handel
    private void gizmoSelectionHandeler()
    {
        //selecting the gizmo point
        if (Input.GetButtonDown("Fire1"))
            setGizmo(pointOnMesh());
    }

    //deals with painting of vertices
    private void vertexSelectionHandeler()
    {
        //if space hit, then clear selected verts
        if (Input.GetKeyDown(KeyCode.Space))
            selectedVerts.Clear();

        //selecting the vertex to weight/pivot
        if (Input.GetButton("Fire2"))
            getSelectedVertex(pointOnMesh());
    }

    //deals with input of radial seleciton
    private void radialSelectionHandeler()
    {
        //create radial selector if either ctrl or alt is pressed down
        if (Input.GetKeyDown(KeyCode.LeftControl))
            createRadialSelector(true);

        if (Input.GetKeyDown(KeyCode.LeftAlt))
            createRadialSelector(false);

        //delete if either key is let go or if both are held
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt)
            || (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftAlt)))
            destroyRadialSelector();

        //radial selection, if ctrl held, then selecting, if alt held then unselection
        if (Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt))
            radialSelection(true);

        if (Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftControl))
            radialSelection(false);

        previousMousePos = Input.mousePosition;
    }

    //return the clicked point on the mesh
    private RaycastHit pointOnMesh()
    {
        //get new object if clicked
        RaycastHit rayCastHit = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayCastHit))
            foreach (MeshCollider collider in meshColliders) 
                if (rayCastHit.collider == collider)
                    return rayCastHit;

        return new RaycastHit();
    }

    //places a gizmo at the selected point
    private void setGizmo(RaycastHit rayCastHit)
    {
        if (rayCastHit.collider == null)
            return;

        gizmodVert = findClosestVertex(rayCastHit);
        gizmo.transform.position = rayCastHit.transform.TransformPoint(sudoMesh.getVertices()[gizmodVert]);
    }

    //Logs the closets vertex to the point clicked on mesh
    private void getSelectedVertex(RaycastHit rayCastHit)
    {
        if (rayCastHit.collider == null)
            return;

        //find the vertex that was selected and its neighbours
        selectedVerts.Add(findClosestVertex(rayCastHit));
    }
    
    private void createRadialSelector(bool selecting)
    {
        destroyRadialSelector();
        radialSelector = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        radialSelector.AddComponent<MeshCollider>();

        float scale = 50;
        float size = Camera.main.orthographicSize / 10f;

        //scale culinder so it will most probbally intersect with model
        radialSelector.transform.localScale = new Vector3(size, scale, size);

        //rotate cylinder so there is only ever a circle present
        radialSelector.transform.SetParent(Camera.main.transform, true);
        radialSelector.transform.localEulerAngles = Vector3.one * 90;
        radialSelector.transform.parent = null;
        
        //move cylinder so it'll be visible
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = scale + 5;
        radialSelector.transform.position = Camera.main.ScreenToWorldPoint(mousePos);

        rSScreenSpawn = Input.mousePosition;

        radialSelector.layer = LayerMask.NameToLayer("UI");

        Color color;

        if (selecting)
            color = Color.cyan;
        else
            color = Color.magenta;

        color.a = 0.5f;

        Renderer renderer = radialSelector.GetComponent<Renderer>();
        renderer.material = radialSelecterMat;
        renderer.material.color = color;

        //send message to camera controller, cannot controll camera while doing this
        Camera.main.SendMessageUpwards("setRotation", false);
    }

    private void destroyRadialSelector()
    {
        if (radialSelector != null)
        {
            Destroy(radialSelector);
            radialSelector = null;
        }

        //give control back to camera
        Camera.main.SendMessageUpwards("setRotation", true);
    }

    //Creates a sphere that scales with the mouse movement
    //either seelcts or deselcts verts
    private void radialSelection(bool selecting)
    {
        if (radialSelector == null)
            return;

        scaleRadialSelecter();
        getRadiallySelectedVertices();
    }
    
    //scale cylinder based on mouse movements   
    private void scaleRadialSelecter()
    {
        float deltaScale = Camera.main.orthographicSize * 0.015f;

        float previousDist = Vector3.Distance(previousMousePos, rSScreenSpawn);
        float currentDist = Vector3.Distance(Input.mousePosition, rSScreenSpawn);

        float delta = (currentDist - previousDist)* deltaScale;
        Vector3 scale = new Vector3(delta, 0, delta);
        radialSelector.transform.localScale += scale;
    }

    //finds Vertices in radial selecter bounds
    private void getRadiallySelectedVertices()
    {
        //bounds check within cylinder mesh collider
        Bounds bounds = radialSelector.GetComponent<MeshCollider>().bounds;

        Vector3[] vertices = sudoMesh.getVertices();

        for (int i = 0; i < sudoMesh.vertexCount; i++)
            if (bounds.Contains(vertices[i]))
                if (!selectedVerts.Contains(i))
                    selectedVerts.Add(i);
    }

    //Returns the closest vertex to the point hit
    private int findClosestVertex(RaycastHit rayCastHit)
    {
        Transform trans = rayCastHit.collider.transform;

        Vector3 localPoint = trans.InverseTransformPoint(rayCastHit.point);
        Vector3[] vertices = sudoMesh.getVertices();
        Vector3 diff;

        //find the nearest vert
        float closestDistance = Mathf.Infinity;

        int mainVert = 0;

        for (int i = 0; i < sudoMesh.vertexCount; i++)
        {
            diff = localPoint - vertices[i];
            float distSqr = diff.sqrMagnitude;
            if (distSqr < closestDistance)
            {
                closestDistance = distSqr;
                mainVert = i;
            }
        }

        return mainVert;
    }

    //paint the selected and neibghouring vertices
    private void paintVertices()
    {
        bool paintNeighbours = false;

        List<Color[]> colors = new List<Color[]>(sudoMesh.subMeshCount);

        //colour the whole mesh white first
        for (int i = 0; i < sudoMesh.subMeshCount; i++)
        {
            Color[] cols = new Color[sudoMesh.vertices[i].Length];
            for (int j = 0; j < sudoMesh.vertices[i].Length; j++)
                cols[j] = Color.white;
            colors.Add(cols);
        }

        int id = -1;
        if (paintNeighbours)
            //then colour the neibghours
            foreach (int mainVert in selectedVerts)
                foreach (HalfEdge neighbour in findNeighbouringHalfEdges(mainVert))
                {
                    for (int i = 0; i < sudoMesh.subMeshCount; i++)
                        if (neighbour.vertexEnd < sudoMesh.vertices[i].Length)
                            id = i;

                    colors[id][neighbour.vertexEnd] = Color.red;
                }

        //finally colour the main vertices
        id = -1;
        foreach (int mainVert in selectedVerts)
        {
            for (int i = 0; i < sudoMesh.subMeshCount; i++)
                if (mainVert < sudoMesh.vertices[i].Length)
                    id = i;

            colors[id][mainVert] = Color.red;
        }

        sudoMesh.colors = colors;

        updateMeshes();
    }

    //find all the neighbouring vertices, and return the corresponding half edge
    //means we can do face calculations as well
    private List<HalfEdge> findNeighbouringHalfEdges(int vertex)
    {
        List<HalfEdge> neighbours = new List<HalfEdge>();
        List<int> searchedFaces = new List<int>();            //for ensuring we don't add more than one HE per face
        List<int> searchedVertices = new List<int>();           //for ensuring we don't add a HE with the same vert ID
        HalfEdge currentHE = heMesh.vertToHE[vertex];           //set current HE to the next of verts HE, as we will visit this vert again
        List<HalfEdge> traversedHEs = new List<HalfEdge>();

        //loop thorugh half edge faces until we have arrived back at our vertex
        //and then get twin and repeat
        //if we don't have a twin, then search in the other direction
        //end search we we have returned to start or ended sarch in both directions

        bool oneSideSearched = false;
        bool searchFinished = false;

        do
        {
            //if we are back to the begining
            if (currentHE.vertexEnd == vertex)
                //enter the new face
                currentHE = currentHE.oppositeHalfEdge;

            //if no twin, then end this side of the search
            if (currentHE == null)
                //if this is the final search
                if (oneSideSearched)
                {
                    searchFinished = true;
                    break;
                }
                //else start searching in the other direction
                else
                {
                    currentHE = heMesh.vertToHE[vertex].oppositeHalfEdge;
                    oneSideSearched = true;
                    if (currentHE == null)
                    {
                        searchFinished = true;
                        break;
                    }
                }

            //add the new HE to the list if
            //we haven't already added one with the same Vert
            //and if it's not the one we are lookiing for
            //but only if we haven't use a HE from this face before
            //and if it's a unnique vertex
            if ((currentHE.vertexEnd != vertex)
                && (!searchedFaces.Exists(x => x == currentHE.face))
                && (!searchedVertices.Exists(x => x == currentHE.vertexEnd)))
            {
                neighbours.Add(currentHE);
                searchedFaces.Add(currentHE.face);
                searchedVertices.Add(currentHE.vertexEnd);
            }

            //if this HE has already been traversed, then end the search
            if (traversedHEs.Exists(x => x == currentHE))
                searchFinished = true;
            else
                //add the currentHE to the traversed list
                traversedHEs.Add(currentHE);

            currentHE = currentHE.nextHalfEdge;
        } while (!searchFinished);

        return neighbours;
    }
}
