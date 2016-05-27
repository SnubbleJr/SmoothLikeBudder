using UnityEngine;
using System.Collections.Generic;

public class InitilizationScript : MonoBehaviour
{
    //converts the (potentially) sub mesh in the given gameobject into a sudomesh
    //then uses that to create a HE data structure

    //passes off sudo and he mesh after creation
    
    public GameObject objectToDeform;
    public GameObject gizmoPrefab;
    
    // Use this for initialization
    void Awake()
    {
        MeshFilter[] meshFilters = objectToDeform.GetComponentsInChildren<MeshFilter>();

        addColliders(meshFilters);

        MeshCollider[] meshColliders = objectToDeform.GetComponentsInChildren<MeshCollider>();
        
        SudoMesh sudoMesh = makeSudoMesh(meshFilters);

        //create HE data structure
        HEMesh heMesh = new HEMesh(sudoMesh);

        //spawn gizmo offscreen
        GameObject gizmo = Instantiate(gizmoPrefab, Vector3.one * 20, Quaternion.identity) as GameObject;

        //pass off to vertexSelecter
        GetComponent<VertexSelecter>().begin(sudoMesh, heMesh, meshFilters, meshColliders, gizmo);
    }

    //adds mesh colliders to filters
    private void addColliders(MeshFilter[] meshFilters)
    {
        foreach (MeshFilter meshFilter in meshFilters)
            if (meshFilter.GetComponent<MeshCollider>() == null)
                meshFilter.gameObject.AddComponent<MeshCollider>();
    }
    
    //extract the sub meshes from the mesh filters and parse into SudoMesh
    private SudoMesh makeSudoMesh(MeshFilter[] meshFilters)
    {
        Mesh[] originalMeshes = new Mesh[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
            originalMeshes[i] = meshFilters[i].mesh;

        //make sudo mesh
        SudoMesh mesh = new SudoMesh(originalMeshes);

        return mesh;
    }
}
