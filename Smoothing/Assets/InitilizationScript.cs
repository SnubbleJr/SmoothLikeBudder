using UnityEngine;
using System.Collections.Generic;

public class InitilizationScript : MonoBehaviour
{
    //converts the (potentially) sub mesh in the given gameobject into a sudomesh
    //then uses that to create a HE data structure

    public GameObject objectToDeform;

    private MeshFilter[] meshFilters;
    private SudoMesh sudoMesh;
    private HEMesh heMesh;
    
    // Use this for initialization
    void Start()
    {
        meshFilters = objectToDeform.GetComponentsInChildren<MeshFilter>();
        addColliders();

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
        getVerts(checkNewMesh());
    }

    //for testing atm, clicking on the mesh to select a vert on the sudomesh
    //return the clicked point on the mesh
    private RaycastHit checkNewMesh()
    {
        //get new object if clicked
        RaycastHit rayCastHit = new RaycastHit();
        if (Input.GetButtonDown("Fire1"))
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayCastHit))
                foreach (MeshCollider collider in objectToDeform.GetComponentsInChildren<MeshCollider>())
                    if (rayCastHit.collider == collider)
                        return rayCastHit;

        return new RaycastHit();
    }

    //again for testing
    private void getVerts(RaycastHit rayCastHit)
    {
        //get closest vert to ray cast
        //and color them
        if (rayCastHit.collider == null)
            return;

        //find the vertex that was selected and its neighbours
        int mainVert = findClosestVertex(rayCastHit);        
        List<HalfEdge> neighbours = findNeighbouringHalfEdges(mainVert);

        //now colour the vertex
        Color[] colors = new Color[sudoMesh.vertexCount];

        //colour the whole mesh white first
        for (int i = 0; i < sudoMesh.vertexCount; i++)
            colors[i] = Color.white;

        //then colour the neibghours
        foreach (HalfEdge neighbour in neighbours)
            colors[neighbour.vertexEnd] = Color.red;

        //finally colour the main vertex
        colors[mainVert] = Color.yellow;
        sudoMesh.colors = colors;

        updateMeshes();


        /*
        //now find nieboughrs and colour them in
        HalfEdge he = heMesh.vertToHE[mainVert];
        HalfEdge next = he;

        //colour tri
        //when return, jump to next tri via opposite

        //colour face
        //when end face, go to opposite and continue
        
        do
        {
            next = next.nextHalfEdge;
            if (next.oppositeHalfEdge != null)
            {
                Face face = heMesh.faces[next.oppositeHalfEdge.face];
                for (int i = 0; i < face.vertices.Length; i++)
                {
                    colors[face.vertices[i]] = Color.red;
                }
            }
        }
        while (next != he);
        */


        //if (distance < thresholdDist)
        //{
        //    colors[i] = Color.Lerp(Color.yellow, Color.blue, distance / thresholdDist);
        //    neighbours.Add(i, thresholdDist / distance);
        //}
        //else
        //    colors[i] = Color.white;


        //colors[mainVert] = Color.red;
        //mesh.colors = colors;
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

    //find all the neighbouring vertices, and return the corresponding half edge
    //means we can do face calculations as well
    private List<HalfEdge> findNeighbouringHalfEdges(int vertex)
    {
        List<HalfEdge> neighbours = new List<HalfEdge>();
        List<int> searchedFaces = new List<int>();            //for ensuring we don't add more than one HE per face
        List<int> searchedVertices = new List<int>();           //for ensuring we don't add a HE with the same vert ID
        HalfEdge currentHE = heMesh.vertToHE[vertex];           //set current HE to the next of verts HE, as we will visit this vert again
        List<HalfEdge> traversedHEs = new List<HalfEdge>();

        List<int> HEVerts = new List<int>();
        List<int> HEFaces = new List<int>();

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

            HEFaces.Add(currentHE.face);
            HEVerts.Add(currentHE.vertexEnd);

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
