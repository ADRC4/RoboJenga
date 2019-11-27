using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Formations
{
    public static Pose[,] Grid3x1(int layerCount)
    {
        Pose[,] grid = new Pose[layerCount, 3];

        for (int i = 0; i < layerCount; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (j % 2 == 0)
                {
                    grid[i, j] = Extensions.PoseFromRotation(0, i, 0, 0);
                }
                
            }
        }

        return grid;
    }
    //public static Pose JengaLocation(int index)
    //{
    //    int count = index;
    //    int layer = count / 3;
    //    int horiz = count % 3;
    //    bool isEven = layer % 2 == 0;

    //    Vector3 position = new Vector3(0, (layer + 1) * _tileSize.y, (horiz - 1) * (_tileSize.z + _gap));
    //    var rotation = Quaternion.Euler(0, isEven ? 0 : -90, 0) * Quaternion.Euler(0, 180f, 0);
    //    return new Pose(rotation * position + _placePoint, rotation);
    //}
}
