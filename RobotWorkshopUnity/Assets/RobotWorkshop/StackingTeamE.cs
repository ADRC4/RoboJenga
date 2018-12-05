using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingTeamE : IStackable
{
    
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _placedTiles; } }

    readonly Rect _pickRect;
    readonly Rect _buildRect;
    readonly ICamera _camera;

    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);

    List<Orient> _placedTiles = new List<Orient>();

    

    public StackingTeamE(Mode mode)//E组堆叠模式
    {
        Message = "Simple vision stacking.";
        float m = 0.02f;
        _pickRect = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);//建立拾取区
        _buildRect = new Rect(0.7f + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);//建立建造区

        if (mode == Mode.Virtual)//虚拟相机模式
            _camera = new TeamECamera();
        else//相机模式
            _camera = new LiveCamera();
    }

    public PickAndPlaceData GetNextTargets()
    {
        // pick tiles

        var pickLayer = _camera.GetTiles(_pickRect);//拾取区，摄像头在拾取区获得块
        var buildLayer = _camera.GetTiles(_buildRect);//建造区

        if (pickLayer == null)//如果拾取区扫描不到块
        {
            Message = "Camera error.";
            return null;
        }

        if (pickLayer.Count == 0)//如果拾取区块拾取完了
        {
            Message = "No more tiles to pick.";
            return null;
        }

        var pick = pickLayer.First();//拾取在拾取区的块
        
        if (buildLayer == null)//如果建造区扫描不到块
        {
            Message = "Camera error.";
            return null;
        }

        if (buildLayer.Count == 0)//如果建造区块
        {
            Message = "No tiles in build area.";
            return null;
        }

        var closestEnds = ClosestEnds(buildLayer);

        //for (int i = 0; i < blocks.Count; i++)
        //{
        //    for (int j = 1; j < blocks.Count; j++)
        //    {
        //        var mid = Vector3.Lerp(uniquePairs[i], uniquePairs[j], 0.5f);
        //        var xAxis = uniquePairs[j] - uniquePairs[i];
        //    }
        //}


        var mid = Vector3.Lerp(closestEnds[0], closestEnds[1], 0.5f);
        var xAxis = closestEnds[1] - closestEnds[0];
        mid.y += _tileSize.y;
       
        var rotation = Quaternion.FromToRotation(Vector3.right, xAxis);

        var place = new Orient(mid, rotation);

        _placedTiles.Add(place);

        return new PickAndPlaceData { Pick = pick, Place = place, Retract = true };
    }


    Vector3[] GetEnds(Orient orient)//生成块的中点线
    {
        float halfLength = _tileSize.x * 0.5f-0.03f;//将块的中心店还原成线
        var direction = orient.Rotation * Vector3.right;//得到块的角度
        var delta = direction * halfLength;

        return new[] { orient.Center + delta, orient.Center - delta };
    }


    Vector3[] ClosestEnds(IList<Orient> blocks)//获取最近的点进行匹配
    {
        float closestDistance = float.MaxValue;
        Vector3[] currentClosestPair = null;//预设匹配的队列为空

        for (int i = 0; i < blocks.Count - 1; i++)
        {
            var endsA = GetEnds(blocks[i]);//设置块的端点A

            for (int j = i + 1; j < blocks.Count; j++)
            {
                var endsB = GetEnds(blocks[j]);//设置块的端点B

                var pairs = new[]//将两个块的四个中点进行循环比对，得出值
                {
                    new [] { endsA[i], endsB[j] },
                    new [] { endsA[j], endsB[i] },
                    new [] { endsA[i], endsB[j] },
                    new [] { endsA[j], endsB[i] },
                };

                for (int k = j; k < i*4; k++)//最近的进行匹配
                {
                    var distance = Vector3.Distance(pairs[k][i], pairs[k][j]);
                    if (distance < closestDistance && distance < _tileSize.x - 0.01f)
                    {
                        closestDistance = distance;
                        currentClosestPair = pairs[k];
                    }
                }
            }
        }



        var uniquePairs = new List<Vector3[]>();//得到uniquePairs的数列,确保每个块只会匹配一次
        float tol = 0.0001f;

        foreach (var pair in currentClosestPair.OrderBy(p=>Vector3.Distance(p[0],p[1])))//遍历循环
        {
            bool overlaps = false;//如果两个块交叠，则跳出，没有交叠则在数列中增加一个unique pair
            foreach (var end in uniquePairs.SelectMany(p=>p))
            {
               if(Vector3.Distance(end, pair[0]) < tol || Vector3.Distance(end, pair[1]) < tol)
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

   

    class TeamECamera : ICamera//虚拟相机
    {
        Queue<Orient[]> _sequence;
        Queue<Orient[]> _structure;

        public TeamECamera()//虚拟块的位置排列
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


        public IList<Orient> GetTiles(Rect rect)
        {
            return _sequence.Dequeue();
        }

        //public IList<Orient> PutTiles(Rect rect)
        //{
        //    return _structure.Dequeue();
        //}

       

    }
}