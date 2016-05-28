using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class SudoMesh
{
    //Ed: representation of a mesh that has too many vertices to be rendered as a single mesh

    public string name;
    public Vector3[] vertices;
    public int[] triangles;
    public int vertexCount;
    public Color[] colors;

    private int[] vertexSplit;              //Ed: this is where the auto split from unity camein (the length of the arrays)
    private int[] triSplit;                 //Ed: this is where the auto split from unity camein (the length of the arrays)

    private Dictionary<int, int[]> originalTris = new Dictionary<int, int[]>();                 //Ed: store the original tris as they don't change
    private Dictionary<int, Vector2[]> originalUVs = new Dictionary<int, Vector2[]>();          //Ed: same for uv
    private Dictionary<int, Vector3[]> originalNormals = new Dictionary<int, Vector3[]>();      //Ed: same for normals

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
        //Ed: creates one sudo mesh for the manipulating of the object (Unity has a vertex limit for a single object)
        //Ed: combining here means it can be manipulated, and then split up later

        this.name = name;

        //Ed: get vertex count for the whole mesh
        int vCount = 0;
        int tCount = 0;

        for (int i = 0; i < meshes.Length; i++)
        {
            vCount += meshes[i].vertexCount;
            tCount += meshes[i].triangles.Length;
        }

        vertices = new Vector3[vCount];
        triangles = new int[tCount];
        colors = new Color[vCount];

        vertexSplit = new int[meshes.Length];
        triSplit = new int[meshes.Length];

        int vOffset = 0;
        int tOffset = 0;
        
        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].vertices.CopyTo(vertices, vOffset);

            meshes[i].colors.CopyTo(colors, vOffset);
            
            //Ed: create the new triangle array, correctly offset
            List<int> list = new List<int>();
            meshes[i].triangles.ToList().ForEach(x => list.Add(x + tOffset));
            list.ToArray().CopyTo(triangles, tOffset);
            
            //Ed: log where the splits in the arrays occur
            vertexSplit[i] = meshes[i].vertexCount;
            vOffset += vertexSplit[i];
            triSplit[i] = meshes[i].triangles.Length;
            tOffset += triSplit[i];
            
            //Ed: log the original triangles uvs and normals
            originalTris.Add(i, meshes[i].triangles);
            originalUVs.Add(i, meshes[i].uv);
            originalNormals.Add(i, meshes[i].normals);
        }

        vertexCount = vertices.Length;
    }

    //Ed: Returns the mesh(es) of the sudomesh
    //Ed: As there is a hard vertex limit, it will pslit the mesh up to meet it
    public Mesh[] getMeshes()
    {
        Mesh[] meshes = new Mesh[vertexSplit.Length];

        //Ed: seperate Color meshes

        int vOffset = 0;

        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i] = new Mesh();

            meshes[i].name = name + (i+1).ToString();
            
            //Ed: retstore vertex arrays for mesh
            Vector3[] tempVerts = new Vector3[vertexSplit[i]];
            Array.Copy(vertices, vOffset, tempVerts, 0, tempVerts.Length);
            meshes[i].vertices = tempVerts;
            
            //Ed: restore colors
            Color[] tempColors = new Color[vertexSplit[i]];
            Array.Copy(colors, vOffset, tempColors, 0, tempColors.Length);
            meshes[i].colors = tempColors;

            vOffset += vertexSplit[i];

            //Ed: restore original triangles uvs and normals
            meshes[i].triangles = originalTris[i];
            meshes[i].uv = originalUVs[i];
            meshes[i].normals = originalNormals[i];
        }

        return meshes;
    }

    //Ed: single index update
    public void updateVertexPosition(int index, Vector3 pos)
    {
        vertices[index] = pos;
    }

    //Ed: single index update
    public void updateVertexColor(int index, Color color)
    {
        colors[index] = color;
    }
}
