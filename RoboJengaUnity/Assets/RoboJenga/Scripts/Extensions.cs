using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICamera
{
    IList<Pose> GetTiles(Rect area);
}

public interface IStackable
{
    PickAndPlaceData GetNextTargets();
    string Message { get; }
    ICollection<Pose> Display { get; }
}

public class PickAndPlaceData
{
    public Pose Pick { get; set; }
    public Pose Place { get; set; }
    public bool Retract { get; set; } = false;
    public bool StopLoop { get; set; } = false;
}

public enum Mode { Virtual, Live };


static class Extensions
{
    public static Pose PoseFromRotation(float x, float y, float z, float angle)
    {
        var pos = new Vector3(x, y, z);
        var rotation = Quaternion.Euler(0, angle, 0);

        return new Pose(pos, rotation);
    }

    public static Pose RotateAround(this Pose pose, Vector3 origin, float angle)
    {
        var quat = Quaternion.Euler(0, angle, 0);
        var center = origin + quat * (pose.position - origin);
        var rotation = quat * pose.rotation;
        return new Pose(center, rotation);
    }

    public static float[] ToFloats(this Pose pose)
    {
        var p = pose.position;
        var r = pose.rotation;

        return new[]
        {
         p.x * 1000f,
         p.z * 1000f,
         p.y * 1000f,
         -r.w,
         r.x,
         r.z,
         r.y
         };
    }

    public static float GetAngle(Vector3 a, Vector3 b)
    {
        var v1 = new Vector2(a.x, a.z).normalized;
        var v2 = new Vector2(b.x, b.z).normalized;
        var angle = Mathf.Atan2(v1.x * v2.y - v1.y * v2.x, v1.x * v2.x + v1.y * v2.y) * (180f / Mathf.PI);
        return angle;
    }

    public static Matrix4x4 ToLeftHanded(this Matrix4x4 o)
    {
        var scale = Matrix4x4.Scale(new Vector3(0.001f, -0.001f, 0.001f));
        var rotate = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));
        var m = scale * rotate * o;
        return m;
    }

    public static K[] Map<T, K>(this ICollection<T> list, Func<T, K> projection)
    {
        var array = new K[list.Count];
        int i = 0;

        foreach (var item in list)
            array[i++] = projection(item);

        return array;
    }
}
