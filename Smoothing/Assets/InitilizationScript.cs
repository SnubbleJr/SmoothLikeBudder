using UnityEngine;
using System.Collections.Generic;

public class InitilizationScript : MonoBehaviour
{
    //Ed: converts the (potentially) sub mesh in the given gameobject into a sudomesh
    //Ed: then uses that to create a HE data structure
    
    public GameObject objectToDeform;

    public GameObject gizmoPrefab;

    private GameObject gizmo;
    private int gizmodVert = -1;

    private MeshFilter[] meshFilters;
    private SudoMesh sudoMesh;
    private HEMesh heMesh;

    private List<int> selectedVerts = new List<int>();

    //Ed:  Use this for initialization
    void Start()
    {
        meshFilters = objectToDeform.GetComponentsInChildren<MeshFilter>();
        addColliders();

        spawnGizmo();

        sudoMesh = makeSudoMesh();

        //Ed: create HE data structure
        heMesh = new HEMesh(sudoMesh);

        //Ed: add color to verts
        clearPaint();
    }

    //Ed: add mesh colliders to filters
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

    //Ed: extract the sub meshes from the mesh filters and parse into SudoMesh
    private SudoMesh makeSudoMesh()
    {
        Mesh[] originalMeshes = new Mesh[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
            originalMeshes[i] = meshFilters[i].mesh;

        //Ed: make sudo mesh
        SudoMesh mesh = new SudoMesh(originalMeshes);

        return mesh;
    }

    //Ed: updates the vertices and color of the new submeshes
    private void updateMeshes()
    {
        Mesh[] newMeshes = sudoMesh.getMeshes();

        for (int i = 0; i < newMeshes.Length; i++)
            meshFilters[i].sharedMesh = newMeshes[i];
    }

    void Update()
    {
        //Ed: if right clicked, then clear selected verts
        if (Input.GetKeyDown(KeyCode.Space))
        {
            clearPaint();
            selectedVerts.Clear();
            updateMeshes();
        }

        RaycastHit rayCastHit;

        if (Input.GetButtonDown("Fire1") || Input.GetButton("Fire2"))
        {
            rayCastHit = pointOnMesh();

            //Ed: selecting the gizmo point
            if (Input.GetButtonDown("Fire1"))
                setGizmo(rayCastHit);

            //Ed: selecting the vertex to weight/pivot
            if (Input.GetButton("Fire2"))
                getSelectedVertex(rayCastHit);
        }
    }
    
    //Ed: return the clicked point on the mesh
    private RaycastHit pointOnMesh()
    {
        //Ed: get new object if clicked
        RaycastHit rayCastHit = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayCastHit))
            foreach (MeshCollider collider in objectToDeform.GetComponentsInChildren<MeshCollider>())
                if (rayCastHit.collider == collider)
                    return rayCastHit;

        return new RaycastHit();
    }

    //Ed: places a gizmo at the selected point
    private void setGizmo(RaycastHit rayCastHit)
    {
        if (rayCastHit.collider == null)
            return;

        gizmodVert = findClosestVertex(rayCastHit);
        gizmo.transform.position = objectToDeform.transform.TransformPoint(sudoMesh.vertices[gizmodVert]);
    }

    //Ed: Logs the closets vertex to the point clicked on mesh
    private void getSelectedVertex(RaycastHit rayCastHit)
    {
        if (rayCastHit.collider == null)
            return;

        //Ed: find the vertex that was selected and its neighbours
        selectedVerts.Add(findClosestVertex(rayCastHit));

        paintVertices();
        updateMeshes();
    }

    //Ed: Returns the closest vertex to the point hit
    private int findClosestVertex(RaycastHit rayCastHit)
    {
        SortedDictionary<int, float> neighbours = new SortedDictionary<int, float>();

        Transform trans = rayCastHit.collider.transform;

        Vector3 localPoint = trans.InverseTransformPoint(rayCastHit.point);
        Vector3[] vertices = sudoMesh.vertices;
        //Ed: find the nearest vert
        float closestDistance = Mathf.Infinity;

        int mainVert = 0;
        
        float dist;

        for (int i = 0; i < sudoMesh.vertexCount; i++)
        {
            dist = Vector3.Distance(localPoint, vertices[i]);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                mainVert = i;
            }
        }

        return mainVert;
    }

    //Ed: paint all verts white
    private void clearPaint()
    {
        Color white = Color.white;

        //Ed: colour the whole mesh white first
        for (int i = 0; i < sudoMesh.vertexCount; i++)
            sudoMesh.updateVertexColor(i, white);
    }

    //Ed: paint the selected and neibghouring vertices
    private void paintVertices()
    {
        bool paintNeighbours = false;

        if (paintNeighbours)
            //Ed: then colour the neibghours
            foreach (int mainVert in selectedVerts)
                foreach (HalfEdge neighbour in findNeighbouringHalfEdges(mainVert))
                    sudoMesh.updateVertexColor(neighbour.vertexEnd, Color.red);

        foreach (int mainVert in selectedVerts)
            sudoMesh.updateVertexColor(mainVert, Color.red)
;    }

    //Ed: find all the neighbouring vertices, and return the corresponding half edge
    //Ed: means we can do face calculations as well
    private List<HalfEdge> findNeighbouringHalfEdges(int vertex)
    {
        List<HalfEdge> neighbours = new List<HalfEdge>();
        List<int> searchedFaces = new List<int>();            //Ed: for ensuring we don't add more than one HE per face
        List<int> searchedVertices = new List<int>();           //Ed: for ensuring we don't add a HE with the same vert ID
        HalfEdge currentHE = heMesh.vertToHE[vertex];           //Ed: set current HE to the next of verts HE, as we will visit this vert again
        List<HalfEdge> traversedHEs = new List<HalfEdge>();
        
        //Ed: loop thorugh half edge faces until we have arrived back at our vertex
        //Ed: and then get twin and repeat
        //Ed: if we don't have a twin, then search in the other direction
        //Ed: end search we we have returned to start or ended sarch in both directions

        bool oneSideSearched = false;
        bool searchFinished = false;

        do
        {
            //Ed: if we are back to the begining
            if (currentHE.vertexEnd == vertex)
                //Ed: enter the new face
                currentHE = currentHE.oppositeHalfEdge;

            //Ed: if no twin, then end this side of the search
            if (currentHE == null)
                //Ed: if this is the final search
                if (oneSideSearched)
                {
                    searchFinished = true;
                    break;
                }
                //Ed: else start searching in the other direction
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
            
            //Ed: add the new HE to the list if
            //Ed: we haven't already added one with the same Vert
            //Ed: and if it's not the one we are lookiing for
            //Ed: but only if we haven't use a HE from this face before
            //Ed: and if it's a unnique vertex
            if ((currentHE.vertexEnd != vertex)
                && (!searchedFaces.Exists(x => x == currentHE.face))
                && (!searchedVertices.Exists(x => x == currentHE.vertexEnd)))
            {
                neighbours.Add(currentHE);
                searchedFaces.Add(currentHE.face);
                searchedVertices.Add(currentHE.vertexEnd);
            }

            //Ed: if this HE has already been traversed, then end the search
            if (traversedHEs.Exists(x => x == currentHE))
                searchFinished = true;
            else
                //Ed: add the currentHE to the traversed list
                traversedHEs.Add(currentHE);

            currentHE = currentHE.nextHalfEdge;
        } while (!searchFinished);

        return neighbours;
    }
}
