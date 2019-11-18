using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FlipAxes : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {
        var meshes = g.GetComponentsInChildren<MeshFilter>()
            .Select(f => f.sharedMesh);

        foreach (var mesh in meshes)
        {
            mesh.vertices = Flip(mesh.vertices);
            mesh.normals = Flip(mesh.normals);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }

        Debug.Log("Mesh inverted.");

        Vector3[] Flip(ICollection<Vector3> list) => 
            list.Select(FlipVector).ToArray();

        Vector3 FlipVector(Vector3 v) => new Vector3(-v.x, v.z, v.y);
    }
}
