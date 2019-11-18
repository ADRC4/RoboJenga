using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

class Robot
{
    readonly Mesh[] _joints;
    readonly Vector6 _a;
    readonly Vector6 _d;
    readonly Matrix4x4 _base;
    readonly Matrix4x4[] _init;
    readonly Mesh _tool;

    Matrix4x4[] _currentPose;

    private Robot(Vector6 a, Vector6 d, Matrix4x4 @base, Mesh[] joints, Mesh tool)
    {
        _a = a;
        _d = d;
        _base = @base;
        _joints = joints;
        _tool = tool;
        Forward(new Vector6(0, 0, 0, 0, 0, 0), ref _init);
    }

    public static Robot IRB1600(IList<float> origin)
    {
        var a = new Vector6(150, 700, 0, 0, 0, 0);
        var d = new Vector6(486.5f, 0, 0, 600, 0, 65);
        var @base = Base(origin);
        var joints = GetMeshes("IRB1600");
        var tool = GetMeshes("Gripper")[0];

        return new Robot(a, d, @base, joints, tool);
    }

    static Matrix4x4 Base(IList<float> n)
    {
        var pos = new Vector3(n[0], n[1], n[2]);
        var rot = new Quaternion(n[4], n[5], n[6], n[3]).normalized;
        var matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
        return matrix;
    }

    static Mesh[] GetMeshes(string model)
    {
        var prefab = Resources.Load<GameObject>(model);
        var instance = GameObject.Instantiate(prefab);
        var meshes = instance.GetComponentsInChildren<MeshFilter>().Map(f => FlipMesh(f.mesh));
        GameObject.Destroy(instance);
        return meshes;

        Mesh FlipMesh(Mesh mesh)
        {
            var result = new Mesh();
            result.SetVertices(mesh.vertices.Map(FlipVector));
            result.SetNormals(mesh.normals.Map(FlipVector));
            result.SetTriangles(mesh.triangles, 0, true);
            result.RecalculateTangents();
            return result;

            Vector3 FlipVector(Vector3 v) => new Vector3(v.x, v.z, v.y);
        }
    }

    public void DrawRobot(Vector6 jointsDeg, Material material)
    {
        Forward(jointsDeg, ref _currentPose);

        for (int i = 0; i < 7; i++)
        {
            var mesh = _joints[i];
            var transform = _base.inverse * (_currentPose[i] * _init[i].inverse);
            Graphics.DrawMesh(mesh, transform.ToLeftHanded(), material, 0);
        }

        Graphics.DrawMesh(_tool, (_base.inverse * _currentPose[6]).ToLeftHanded(), material, 0);
    }

    void Forward(Vector6 jointsDeg, ref Matrix4x4[] m)
    {
        Vector6 c = new Vector6();
        Vector6 s = new Vector6();

        for (int i = 0; i < 6; i++)
        {
            var jointRad = DegreeToRadian(jointsDeg[i], i);
            c[i] = Cos(jointRad);
            s[i] = Sin(jointRad);
        }

        var a = _a;
        var d = _d;

        var m0 = new Matrix4x4(new Vector4(c[0], 0, s[0], a[0] * c[0]), new Vector4(s[0], 0, -c[0], a[0] * s[0]), new Vector4(0, 1, 0, d[0]), new Vector4(0, 0, 0, 1)).transpose;
        var m1 = new Matrix4x4(new Vector4(c[1], -s[1], 0, a[1] * c[1]), new Vector4(s[1], c[1], 0, a[1] * s[1]), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1)).transpose;
        var m2 = new Matrix4x4(new Vector4(c[2], 0, s[2], a[2] * c[2]), new Vector4(s[2], 0, -c[2], a[2] * s[2]), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 0, 1)).transpose;
        var m3 = new Matrix4x4(new Vector4(c[3], 0, -s[3], 0), new Vector4(s[3], 0, c[3], 0), new Vector4(0, -1, 0, d[3]), new Vector4(0, 0, 0, 1)).transpose;
        var m4 = new Matrix4x4(new Vector4(c[4], 0, s[4], 0), new Vector4(s[4], 0, -c[4], 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 0, 1)).transpose;
        var m5 = new Matrix4x4(new Vector4(c[5], -s[5], 0, 0), new Vector4(s[5], c[5], 0, 0), new Vector4(0, 0, 1, d[5]), new Vector4(0, 0, 0, 1)).transpose;

        if (m == null)
            m = new Matrix4x4[7];

        m[0] = Matrix4x4.identity;
        m[1] = m[0] * m0;
        m[2] = m[1] * m1;
        m[3] = m[2] * m2;
        m[4] = m[3] * m3;
        m[5] = m[4] * m4;
        m[6] = m[5] * m5;
    }

    float DegreeToRadian(float degree, int i)
    {
        float radian = degree * (PI / 180f);
        if (i == 1) radian = -radian + PI * 0.5f;
        if (i == 2) radian *= -1;
        if (i == 4) radian *= -1;
        return radian;
    }
}