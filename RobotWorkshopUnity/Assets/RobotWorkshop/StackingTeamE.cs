﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingTeamE : IStackable
{
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _placedTiles; } }

    readonly Rect _buildRect;//定义一个块
    readonly ICamera _camera;//读取camera

    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);

    List<Orient> _placedTiles = new List<Orient>();


    public StackingTeamE(Mode mode)
    {
        Message = "Simple vision stacking.";
        float m = 0.02f;// m = 0.02
        _buildRect = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);//块大小， m*m,宽0.7-2m,高0.8-2m

        if (mode == Mode.Virtual)
            _camera = new TeamECamera();
        else
            _camera = new LiveCamera();
    }

    public PickAndPlaceData GetNextTargets()
    {
        var topLayer = _camera.GetTiles(_buildRect);

        if (topLayer == null)
        {
            Message = "Camera error.";
            return null;
        }

        if (topLayer.Count == 0)
        {
            Message = "No more tiles.";
            return null;
        }

        var closestEnds = (topLayer);

        for (int i = 0; i < closestEnds.Count; i++)
        {
            for (int j = 1; j < closestEnds.Count; i++)
            {
                var mid = Vector3.Lerp(closestEnds[0], closestEnds[1], 0.5f);
            }
        }

       

        mid.y += _tileSize.y;
        var xAxis = closestEnds[1] - closestEnds[0];
        
        var rotation = Quaternion.FromToRotation(Vector3.right, xAxis);

        var place = new Orient(mid, rotation);

        var pick = topLayer.First(); // change this

        _placedTiles.Add(place);

        return new PickAndPlaceData { Pick = pick, Place = place, Retract = true };
    }


    Vector3[] GetEnds(Orient orient)
    {
        float halfLength = _tileSize.x * 0.5f;
        var direction = orient.Rotation * Vector3.right;
        var delta = direction * halfLength;

        return new[] { orient.Center + delta, orient.Center - delta };
    }

    List<Vector3[]> ClosestEnds(IList<Orient> blocks)//获取最近的点进行匹配
    {
        float closestDistance = float.MaxValue;
        Vector3[][] currentClosestPair = null;//预设匹配的队列为空

        for (int i = 0; i < blocks.Count - 1; i++)
        {
            var endsA = GetEnds(blocks[i]);//设置块的端点A

            for (int j = i + 1; j < blocks.Count; j++)
            {
                var endsB = GetEnds(blocks[j]);//设置块的端点B

                var pairs = new[]{
                    new [] { endsA[0], endsB[0] },
                    new [] { endsA[1], endsB[0] },
                    new [] { endsA[0], endsB[1] },
                    new [] { endsA[1], endsB[1] },
                };

                for (int k = 0; k < 4; k++)//最近的进行匹配
                {
                    var distance = Vector3.Distance(pairs[k][0], pairs[k][1]);
                    if (distance < closestDistance && distance < _tileSize.x - 0.01f)
                    {
                        closestDistance = distance;
                        currentClosestPair = pairs;
                    }
                }
            }
        }



        var uniquePairs = new List<Vector3[]>();//得到uniquePairs的数列,确保每个块只会匹配一次
        float tol = 0.0001f;
    
        foreach (var pair in currentClosestPair.OrderBy(p => Vector3.Distance(p[0], p[1])))//遍历循环
        {
            bool overlaps = false;//如果两个块交叠，则跳出，没有交叠则在数列中增加一个unique pair
            foreach (var end in uniquePairs.SelectMany(p => p))
            {
                if (Vector3.Distance(end, pair[0]) < tol || Vector3.Distance(end, pair[1]) < tol)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
                uniquePairs.Add(pair);
        }
        
        return uniquePairs;//返回

    }

    class TeamECamera : ICamera
    {
        Queue<Orient[]> _sequence;

        public TeamECamera()
        {


            var t = new[]//虚拟块的属性
            {
             new Orient(0.2f,0.045f,0.2f,0.0f),
             new Orient(0.5f,0.045f,0.1f,0.0f),
             new Orient(0.3f,0.045f,0.4f,90f),
             new Orient(0.5f,0.045f,0.4f,0f),
             new Orient(0.1f,0.045f,0.6f,-90f),
             new Orient(0.4f,0.045f,0.6f,45f),
            };


            _sequence = new Queue<Orient[]>(new[]//虚拟块排列
            {
                new[] {t[0],t[1],t[2],t[3],t[4],t[5]},
                new[] {t[0],t[1],t[2],t[3],t[4]},
                new[] {t[0],t[1],t[2],t[3]},
                new[] {t[0],t[1],t[2]},
                new[] {t[0],t[1]},
                new Orient[0]
            });
        }

        public IList<Orient> GetTiles(Rect area)
        {
            return _sequence.Dequeue();
        }
    }
}