using UnityEngine;
using System.Collections.Generic;

public class InitilizationScript : MonoBehaviour
{
    //converts the (potentially) sub mesh in the given gameobject into a sudomesh
    //then uses that to create a HE data structure
    
    public GameObject objectToDeform;

    public GameObject gizmoPrefab;

    private GameObject gizmo;
    private int gizmodVert = -1;

    private MeshFilter[] meshFilters;
    private SudoMesh sudoMesh;
    private HEMesh heMesh;

    private List<int> selectedVerts = new List<int>();

    // Use this for initialization
    void Start()
    {
        meshFilters = objectToDeform.GetComponentsInChildren<MeshFilter>();
        addColliders();

        spawnGizmo();

        sudoMesh = makeSudoMesh();

        //create HE data structure
        heMesh = new HEMesh(sudoMesh);
    }

    //add mesh colliders to filters
    private void addColliders()
    {
        foreach (MeshFilter meshFilter in meshFilters)
            if (meshFilter.GetComponent<MeshCollider>() == null)
                meshFilter.gameObject.AddComponent<MeshCollider>();
    }

    private void spawnGizmo()
    {
        gizmo = Instantiate(gizmoPrefab, Vector3.one * 20, Quaternion.identity) as GameObject;
    }

    //extract the sub meshes from the mesh filters and parse into SudoMesh
    private SudoMesh makeSudoMesh()
    {
        Mesh[] originalMeshes = new Mesh[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
            originalMeshes[i] = meshFilters[i].mesh;

        //make sudo mesh
        SudoMesh mesh = new SudoMesh(originalMeshes);

        return mesh;
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
        //if right clicked, then clear selected verts
        if (Input.GetKeyDown(KeyCode.Space))
        {
            selectedVerts.Clear();
            paintVertices();
        }

        //selecting the gizmo point
        if (Input.GetButtonDown("Fire1"))
            setGizmo(pointOnMesh());

        //selecting the vertex to weight/pivot
        if (Input.GetButton("Fire2"))
            getSelectedVertex(pointOnMesh());
    }
    
    //return the clicked point on the mesh
    private RaycastHit pointOnMesh()
    {
        //get new object if clicked
        RaycastHit rayCastHit = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayCastHit))
            foreach (MeshCollider collider in objectToDeform.GetComponentsInChildren<MeshCollider>())
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
        gizmo.transform.position = objectToDeform.transform.TransformPoint(sudoMesh.vertices[gizmodVert]);
    }

    //Logs the closets vertex to the point clicked on mesh
    private void getSelectedVertex(RaycastHit rayCastHit)
    {
        if (rayCastHit.collider == null)
            return;

        //find the vertex that was selected and its neighbours
        selectedVerts.Add(findClosestVertex(rayCastHit));

        paintVertices();
    }

    //Returns the closest vertex to the point hit
    private int findClosestVertex(RaycastHit rayCastHit)
    {
        SortedDictionary<int, float> neighbours = new SortedDictionary<int, float>();

        Transform trans = rayCastHit.collider.transform;

        Vector3 localPoint = trans.InverseTransformPoint(rayCastHit.point);
        Vector3[] vertices = sudoMesh.vertices;
        //find the nearest vert
        float closestDistance = Mathf.Infinity;

        int mainVert = 0;

        for (int i = 0; i < sudoMesh.vertexCount; i++)
        {
            Vector3 diff = localPoint - vertices[i];
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

        Color[] colors = new Color[sudoMesh.vertexCount];

        //colour the whole mesh white first
        for (int i = 0; i < sudoMesh.vertexCount; i++)
            colors[i] = Color.white;

        if (paintNeighbours)
            //then colour the neibghours
            foreach (int mainVert in selectedVerts)
                foreach (HalfEdge neighbour in findNeighbouringHalfEdges(mainVert))
                    colors[neighbour.vertexEnd] = Color.red;

        //finally colour the main vertices
        foreach (int mainVert in selectedVerts)
            colors[mainVert] = Color.red;

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
