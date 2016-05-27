using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class SudoMesh
{
    //representation of a mesh that has too many vertices to be rendered as a single mesh

    public string name;
    public List<Vector3[]> vertices;
    public List<int[]> triangles;
    public int vertexCount;
    public List<Color[]> colors;
    public int subMeshCount;

    private int triCount;

    private List<Vector2[]> originalUVs = new List<Vector2[]>();            //original uvs as they don't change
    private List<Vector3[]> originalNormals = new List<Vector3[]>();        //same for normals

    private const int vertexLimit = 65534;

    public SudoMesh(Mesh[] meshes)
    {
        initalize("newMesh", meshes);
    }

    public SudoMesh(string name, Mesh[] meshes)
    {
        initalize(name, meshes);
    }

    private void initalize(string name, Mesh[] meshes)
    {
        //creates one sudo mesh for the manipulating of the object (Unity has a vertex limit for a single object)
        //combining here means it can be manipulated, and then split up later

        this.name = name;

        vertices = new List<Vector3[]>();
        triangles = new List<int[]>();
        colors = new List<Color[]>();
        vertexCount = 0;
        triCount = 0;
        
        for (int i = 0; i < meshes.Length; i++)
        {
            vertices.Add(meshes[i].vertices);
            triangles.Add(meshes[i].triangles);
            colors.Add(meshes[i].colors);
                        
            //log the original uvs and normals
            originalUVs.Add(meshes[i].uv);
            originalNormals.Add(meshes[i].normals);

            vertexCount += meshes[i].vertexCount;
            triCount += meshes[i].triangles.Length;
        }

        subMeshCount = meshes.Length;
    }

    //Returns the mesh(es) of the sudomesh
    //As there is a hard vertex limit, it will pslit the mesh up to meet it
    public Mesh[] getMeshes()
    {
        Mesh[] meshes = new Mesh[subMeshCount];

        //seperate Color meshes
        
        for (int i = 0; i < subMeshCount; i++)
        {
            meshes[i] = new Mesh();

            meshes[i].name = name + (i+1).ToString();
            
            //retstore vertex arrays for mesh
            meshes[i].vertices = vertices[i];
            if (colors.Count > 0)
                meshes[i].colors = colors[i];
            meshes[i].triangles = triangles[i];
            
            //restore original uvs and normals
            meshes[i].uv = originalUVs[i];
            meshes[i].normals = originalNormals[i];
        }

        return meshes;
    }

    //returns triangles as a single array
    public int[] getTriangles()
    {
        int[] tris = new int[triCount];
        int offset = 0;
        for (int i = 0; i < triangles.Count; i++)
        {
            triangles[i].CopyTo(tris, offset);
            offset += triangles[i].Length;
        }

        return tris;
    }

    //just like getTriangles
    public Vector3[] getVertices()
    {
        Vector3[] verts = new Vector3[vertexCount];
        int offset = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i].CopyTo(verts, offset);
            offset += vertices[i].Length;
        }

        return verts;
    }
}
