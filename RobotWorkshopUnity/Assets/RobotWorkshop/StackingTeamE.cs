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

    public StackingTeamE(Mode mode)
    {
        Message = "Simple vision stacking.";
        float m = 0.02f;
        _pickRect = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);
        _buildRect = new Rect(0.7f + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);

        if (mode == Mode.Virtual)
            _camera = new TeamECamera();
        else
            _camera = new LiveCamera();
    }

    public PickAndPlaceData GetNextTargets()
    {
        var topLayer = _camera.GetTiles(_pickRect);
        var builtLayer = _camera.GetTiles(_buildRect);

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

        var closestEnds = ClosestEnds(topLayer);

        var mid = Vector3.Lerp(closestEnds[0], closestEnds[1], 0.5f);

        mid.y += _tileSize.y;
        var xAxis = closestEnds[1] - closestEnds[0];

        var rotation = Quaternion.FromToRotation(Vector3.right, xAxis);

        var place = new Orient(mid, rotation);

        var pick = topLayer.First(); // change this

        _placedTiles.Add(place);

        return new PickAndPlaceData { Pick = pick, Place = place, Retract = true };
    }


    Vector3[] GetJoints(Orient orient)
    {
        float halfLength = _tileSize.x * 0.5f;
        var direction = orient.Rotation * Vector3.right;
        var delta = direction * halfLength;

        return new[] { orient.Center + delta, orient.Center - delta };
    }

    //public float jointDistanceMax
    //{
    //    get { return jointDistanceMax; }
    //    set { if (value >= 0f || value <= 0.15f) { jointDistanceMax = value; } }
    //}

    Vector3[] ClosestEnds(IList<Orient> blocks)
    {

        float closestDistance = float.MaxValue;
        Vector3[] currentClosestPair = null;

        for (int i = 0; i < blocks.Count - 1; i++)
        {
            var jointsA = GetJoints(blocks[i]);

            for (int j = i + 1; j < blocks.Count; j++)
            {
                var jointsB = GetJoints(blocks[j]);

                var pairs = new[]
                {
                    new [] { jointsA[0], jointsB[0] },
                    new [] { jointsA[1], jointsB[0] },
                    new [] { jointsA[0], jointsB[1] },
                    new [] { jointsA[1], jointsB[1] },
                };

                for (int k = 0; k < 4; k++)
                {
                    var distance = Vector3.Distance(pairs[k][0], pairs[k][1]);
                    if (distance < closestDistance && distance < _tileSize.x - 0.01f)
                    {
                        closestDistance = distance;
                        currentClosestPair = pairs[k];
                    }
                }
            }
        }
        return currentClosestPair;
    }


    class TeamECamera : ICamera
    {
        Queue<Orient[]> _sequence;
        Queue<Orient[]> _placeSequence;

        public TeamECamera()
        {

            var t = new[]
            {
             new Orient(0.1f,0.045f,0.1f,0.0f),
             new Orient(0.3f,0.045f,0.2f,90f),
             new Orient(0.5f,0.045f,0.3f,45f),
             //new Orient(0.1f,0.045f,0.4f,0.0f),
             //new Orient(0.1f,0.045f,0.5f,0.0f),
             //new Orient(0.1f,0.045f,0.6f,0.0f),
            };


            _sequence = new Queue<Orient[]>(new[]
            {
                //new[] {t[0],t[1],t[2],t[3],t[4],t[5]},
                //new[] {t[0],t[1],t[2],t[3],t[4]},
                //new[] {t[0],t[1],t[2],t[3]},
                new[] {t[0],t[1],t[2]},
                new[] {t[0],t[1]},
                new Orient[0]
            });


            var v = new[]
            {
             new Orient(0.8f,0.045f,0.1f,0.0f),
             new Orient(1.0f,0.045f,0.2f,90f),
             new Orient(1.2f,0.045f,0.3f,45f),
            };

            _placeSequence = new Queue<Orient[]>(new[]
            {
                //new[] {t[0],t[1],t[2],t[3],t[4],t[5]},
                //new[] {t[0],t[1],t[2],t[3],t[4]},
                //new[] {t[0],t[1],t[2],t[3]},
                new[] {v[0],v[1],v[2]},
                new[] {v[0],v[1]},
                new Orient[0]
            });

        }


        public IList<Orient> GetTiles(Rect rect)
        {
            return _sequence.Dequeue();
        }

        //public IList<Orient> PutTiles(Rect rect)
        //{
        //    return _placeSequence.Dequeue();
        //}

    }
}