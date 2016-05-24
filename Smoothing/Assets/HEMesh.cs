using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class HEMesh
{
    public List<HalfEdge> allHalfEdges;
    public Dictionary<int, HalfEdge> vertToHE;
    public Face[] faces;
    
    private Dictionary<int, List<HalfEdge>> vertStartToHE;       //maps all the HEs that start a specific vert, for fast opp finding

    public HEMesh (Mesh mesh)
    {
        generateHalfEdges(mesh.triangles);

        //debugCheck(mesh.vertexCount);
    }
    
    public HEMesh(SudoMesh mesh)
    {
        generateHalfEdges(mesh.triangles);

        //debugCheck(mesh.vertexCount);
    }

    private void debugCheck(int vertexCount)
    {
        Debug.Log(faces.Length);

        Debug.Log(vertexCount + ", " + vertToHE.Keys.Count);
        for (int i = 0; i < vertexCount; i++)
            if (!vertToHE.ContainsKey(i))
                Debug.Log(i);
    }
    
    private void generateHalfEdges(int[] meshTriangles)
    {
        allHalfEdges = new List<HalfEdge>();
        vertToHE = new Dictionary<int, HalfEdge>();
        vertStartToHE = new Dictionary<int, List<HalfEdge>>();

        faces = generateFaces(meshTriangles);
        
        for (int i = 0; i < faces.Length; i++)
            generateHEInFace(i);
        
        findOppositeHE();
    }

    //returns a set of faces based of the give mesh triangles
    //mesh tris are CW, while faces are CCW
    private Face[] generateFaces(int[] triangles)
    {
        Face[] faces = new Face[triangles.Length / 3];

        //This looks nasty, but it's fast, it's just getting 3 elements in turn
        for (int i = 0; i < faces.Length; i++)
        {
            int[] verts = new int[3];
            Array.Copy(triangles, (i*3), verts, 0, 3);
            Array.Reverse(verts);
            faces[i] = new Face(verts);
        }

        return faces;
    }

    //generates the set of half edges for the given edge
    //sets up to 1 HalfEdge for an unset vertex within the face
    private void generateHEInFace(int faceID)
    {
        Face face = faces[faceID];

        HalfEdge[] hes = new HalfEdge[3];
        
        //generate all the HEs first so that they can be referenced
        for(int i = 0; i < hes.Length; i++)
            hes[i] = new HalfEdge();

        for (int i = 0; i < hes.Length; i++)
        {
            hes[i].vertexStart = face.vertices[i];
            hes[i].vertexEnd = face.vertices[(i + 1) % 3];
            hes[i].face = faceID;
            hes[i].nextHalfEdge = hes[(i + 1) % 3];
            hes[i].previousHalfEdge = hes[(i + 2) % 3];

            allHalfEdges.Add(hes[i]);
            
            if (!vertToHE.ContainsKey(hes[i].vertexStart))
                vertToHE.Add(hes[i].vertexStart, hes[i]);

            //add to the vert start list map

            if (!vertStartToHE.ContainsKey(hes[i].vertexStart))
                vertStartToHE.Add(hes[i].vertexStart, new List<HalfEdge>());

            vertStartToHE[face.vertices[i]].Add(hes[i]);
        }

        /*
        this was for maming sure that each vert referenced a HE in a different face, but it left holes

        //check to see if it has been set yet
        int unusedVert = -1;
        for (int i = 0; i < 3; i++)
            if (!vertToHE.ContainsKey(face.vertices[i]))
                unusedVert = i;

        if (unusedVert > -1)
            vertToHE.Add(face.vertices[unusedVert], hes[unusedVert]);
        */
    }
    /*
    fancy for loop for the hes does this
    
    hes[0].vertexStart = face.vertices[0];
    hes[1].vertexStart = face.vertices[1];
    hes[2].vertexStart = face.vertices[2];

    hes[0].vertexEnd = face.vertices[1];
    hes[1].vertexEnd = face.vertices[2];
    hes[2].vertexEnd = face.vertices[0];

    hes[0].face = faceID;
    hes[1].face = faceID;
    hes[2].face = faceID;

    hes[0].nextHalfEdge = hes[1];
    hes[1].nextHalfEdge = hes[2];
    hes[2].nextHalfEdge = hes[0];

    hes[0].previousHalfEdge = hes[2];
    hes[1].previousHalfEdge = hes[0];
    hes[2].previousHalfEdge = hes[1];
    */

    //finds and links the opposite HEs (if existant)
    private void findOppositeHE()
    {
        List<HalfEdge> freeNeighbours = allHalfEdges.ToList();

        int oppCount = 0;
        int nopCount = 0;
        foreach (HalfEdge he in allHalfEdges)
        {
            //skip this he if already has an opp
            if (he.oppositeHalfEdge != null)
                continue;

            //if an opp exists
            //if there are HEs that start at this HE's end
            List<HalfEdge> potentialOpps = vertStartToHE[he.vertexEnd];

            foreach (HalfEdge potentialOpp in potentialOpps)
                if (potentialOpp.vertexEnd == he.vertexStart)
                {
                    he.oppositeHalfEdge = potentialOpp;
                    potentialOpp.oppositeHalfEdge = he;

                    oppCount++;
                    oppCount++;
                }


            if (he.oppositeHalfEdge == null)
                nopCount++;


            //foreach (HalfEdge potentialOpp in freeNeighbours)
            //    if ((potentialOpp.vertexStart == he.vertexEnd) && (potentialOpp.vertexEnd == he.vertexStart))
            //    {
            //        freeNeighbours.Remove(he);
            //        freeNeighbours.Remove(potentialOpp);
            //        break;
            //    }
        }
    }
}
