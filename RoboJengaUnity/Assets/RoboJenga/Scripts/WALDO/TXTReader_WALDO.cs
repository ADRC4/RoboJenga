using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Threading;

public static class TXTReader_WALDO /*: MonoBehaviour*/
{
    static TXTReader_WALDO()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    public static Queue<Pose[]> GetPosesQueue(string testData)
    {

        Queue<Pose[]> ReadVectors = new Queue<Pose[]>();
        List<Pose> poses = new List<Pose>();

        var dataSet = Resources.Load<TextAsset>(testData);
        string[] data = dataSet.text.Split(new char[] { ';' });
        foreach (var s in data)
        {
            string[] coord = s.Split(new char[] { ',' });

            float x = float.Parse(coord[0], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(coord[2], CultureInfo.InvariantCulture.NumberFormat);
            float z = float.Parse(coord[1], CultureInfo.InvariantCulture.NumberFormat);
            float a = float.Parse(coord[3], CultureInfo.InvariantCulture.NumberFormat);

            poses.Add(Extensions.PoseFromRotation(x, y, z, a));
        }

        return CreateQueue(poses);
    }

    private static Queue<Pose[]> CreateQueue(List<Pose> poses)
    {
        Queue<Pose[]> queue = new Queue<Pose[]>();
        var p = poses;
        var length = poses.Count;
        for (int i = 0; i < length; i++)
        {
            queue.Enqueue(p.ToArray());
            p.RemoveRange(0, 1);
        }
        queue.Enqueue(new Pose[0]);
        return queue;
    }


    public static List<Pose> GetPoses(string testData)
    {
        
        List<Pose> poses = new List<Pose>();

        var dataSet = Resources.Load<TextAsset>(testData);
        string[] data = dataSet.text.Split(new char[] { ';' });
        foreach (var s in data)
        {
            string[] coord = s.Split(new char[] { ',' });

            float x = float.Parse(coord[0], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(coord[2], CultureInfo.InvariantCulture.NumberFormat);
            float z = float.Parse(coord[1], CultureInfo.InvariantCulture.NumberFormat);
            float a = float.Parse(coord[3], CultureInfo.InvariantCulture.NumberFormat);

            poses.Add(Extensions.PoseFromRotation(x, y, z, a));
        }
        return poses;
    }
}
