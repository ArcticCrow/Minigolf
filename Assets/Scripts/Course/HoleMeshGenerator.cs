using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HoleMeshGenerator : MonoBehaviour {

    private float groundHeight;
    private float width;
    private LineRenderer layout;

    private Mesh mesh;

    private int xSize, ySize, zSize;

    public void Initialize(float groundLevel, float trackWidth)
    {
        Debug.Log("groundLevel: " + groundLevel);
        groundHeight = groundLevel;
        layout = gameObject.GetComponent<LineRenderer>();

        CreateVertices();
        CreateTriangles();
        CreateColliders();
    }

    private void CreateColliders()
    {
        throw new NotImplementedException();
    }

    private void CreateTriangles()
    {
        throw new NotImplementedException();
    }

    private void CreateVertices()
    {
        /* Calculating corner vertices
         * A cube has 8 corners (vertices)
         * + 4 corners for every extra section (corners, obstacles)
         * = 8 [base] + 4 * points [4 for each section]
         * */
        int cornerVertices = 8 + 4 * layout.positionCount;

        /* Calculating edge verticies
         * A cube has 3 edges each require x/y/z vertices depending on their local direction times 4
         * - 3 for each already added corner vertice
         * + 1 x edge
         * + 4 z edges
         * + 2 y edges
         * = 4 * (xSize + ySize + zSize - 3) [base cube] + points * (2 * (2 * zSize + ySize - 3)  + xSize - 1)
         * */
        int edgeVertices = 4 * (xSize + ySize + zSize - 3);
    }
}
