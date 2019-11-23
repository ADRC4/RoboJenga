using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Mathf;

class MotiveCamera : ICamera
{
    public IList<Pose> GetTiles(Rect area)
    {
        float tileHeight = 0.045f;
        var bounds = new Bounds(
            new Vector3(area.center.x, 0.3f, area.center.y),
            new Vector3(area.width, 0.6f - tileHeight, area.height)
            );

        List<Vector3> basePoints;
        List<Vector3> markers;

        using (var motive = new Motive())
        {
            if (!motive.Connected)
            {
                Log("Can't connect to camera.");
                return null;
            }

            basePoints = motive.GetFrameMarkers();
            markers = motive.GetUnlabledMarkers();
        }

        if (basePoints.Count < 3)
        {
            Log("Error in base frame markers.");
            return null;
        }

        var tiles = new List<Pose>();

        var sortedBasePoints = basePoints
            .OrderBy(m => m.magnitude)
            .ToList();

        var origin = sortedBasePoints[0];
        var vx = sortedBasePoints.Last() - origin;
        var vz = sortedBasePoints[1] - origin;
        var vy = Vector3.Cross(vz, vx);
        vz = Vector3.Cross(vx, vy);
        var q = Quaternion.LookRotation(vz, vy);
        var invq = Quaternion.Inverse(q);

        var sortedMarkers = markers.Select(m => invq * (m - origin))
                                   .Where(m => bounds.Contains(m))
                                   .OrderByDescending(m => m.y)
                                   .ToList();

        foreach (var marker in sortedMarkers)
            Debug.DrawRay(marker, Vector3.up * 0.005f, Color.red, 15);

        while (sortedMarkers.Count >= 3 && tiles.Count == 0)
        {
            var topHeight = sortedMarkers.First().y;
            var topLayerMarkers = sortedMarkers
                .Where(m => Abs(m.y - topHeight) < tileHeight * 0.5f)
                .ToList();

            sortedMarkers.RemoveAt(0);

            foreach (var marker in topLayerMarkers)
            {
                var tile = FindTile(marker, topLayerMarkers);
                if (tile != null) tiles.Add((Pose)tile);
            }
        }

        return tiles;
    }

    Pose? FindTile(Vector3 marker, IEnumerable<Vector3> topLayerMarkers)
    {
        float xLength = 0.180f - 0.008f;
        float zLength = 0.02f;
        float epsilon = 0.01f;
        float epsilonDeg = 5f;

        foreach (var otherX in topLayerMarkers)
        {
            float distanceX = (marker - otherX).magnitude;
            if (Abs(distanceX - xLength) > epsilon) continue;

            foreach (var otherZ in topLayerMarkers)
            {
                float distanceZ = (marker - otherZ).magnitude;
                if (Abs(distanceZ - zLength) > epsilon) continue;

                var vmx = otherX - marker;
                var vmz = otherZ - marker;
                var zAngle = Extensions.GetAngle(vmx, vmz);
                if (Abs(zAngle - 90f) > epsilonDeg) continue;

                var angle = Extensions.GetAngle(Vector3.right, vmx);
                var rotation = Quaternion.Euler(0, -angle, 0);
                var center = otherX * 0.5f + marker * 0.5f;
                return new Pose(center, rotation);
            }
        }

        return null;
    }

    void Log(string text)
    {
        Debug.Log($"MotiveCamera: {text}");
    }
}