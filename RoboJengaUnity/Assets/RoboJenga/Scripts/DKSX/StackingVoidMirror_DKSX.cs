 using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingVisionVoidMirror : IStackable
{
    public string Message { get; private set; }
    public ICollection<Pose> Display { get; } = new List<Pose>();

    readonly Vector3 _placePoint = new Vector3(1.0f, 0, 0.4f);
    readonly Vector3 _TargetplacePoint = new Vector3(0.4f, 0, 0.4f);
    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    readonly Rect _rect;
    readonly float _gap = 0.01f;
    readonly ICamera _camera;

    bool _isScanning = true;
    int _targetCount = 0;
    int _tileCount = 0;
    List<Pose> _pickTiles = new List<Pose>();
    List<Pose> _mirroredTiles = new List<Pose>();

    public StackingVisionVoidMirror(Mode mode)
    {
        if (mode == Mode.Virtual)
            _camera = new VirtualCamera_DKSX();
        else
            _camera = new MotiveCamera();

        Message = "Void Mirror vision stacking.";
        float m = 0.20f;
        _rect = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);
    }

    public PickAndPlaceData GetNextTargets()
    {
        if (_isScanning)
        {
            return RememberBlocks();
        }
        else
        {
            return BuildVoidMirror();
        }
    }

    PickAndPlaceData RememberBlocks()
    {
        var topLayer = _camera.GetTiles(_rect);

        if (topLayer == null)
        {
            Message = "Camera error.";
            return null;
        }

        if (topLayer.Count == 0)
        {
            _isScanning = false;
            Message = "Finished scanning, building mirror.";
            _mirroredTiles = GenerateVoid(_pickTiles);
            _targetCount = _mirroredTiles.Count;
            return BuildVoidMirror();
        }

        var pick = topLayer.First();
        _pickTiles.Add(pick);
        var place = JengaLocation(_tileCount);
        _tileCount++;
        Display.Add(place);
        return new PickAndPlaceData { Pick = pick, Place = place };
    }

    PickAndPlaceData BuildVoidMirror()
    {
        if (_targetCount == 0) return null;
        var pick = JengaLocation(_tileCount - 1);
 
        //var place = _pickTiles[_tileCount - 1];
        var place = _mirroredTiles[_targetCount - 1];
        _targetCount--;
        _tileCount--;
        Display.Add(place);
        return new PickAndPlaceData { Pick = pick, Place = place };
    }



    Pose JengaLocation(int index)
    {
        int count = index;
        int layer = count / 3;
        int horiz = count % 3;
        bool isEven = layer % 2 == 0;

        Vector3 position = new Vector3(0, (layer + 1) * _tileSize.y, (horiz - 1) * (_tileSize.z + _gap));
        var rotation = Quaternion.Euler(0, isEven ? 0 : -60, 0) * Quaternion.Euler(0, 120f, 0);
        return new Pose(rotation * position + _placePoint, rotation);
    }


    List<Pose> GenerateVoid(IList<Pose> input)
    {
        //Sorting the input list in subsets of Y (up)

        var minX = input.Min(p => p.position.x);
        var minZ = input.First(p => p.position.x == minX).position.z - (_tileSize.x / 2);


        var towerToOrigin = input.Select(p =>
        {
            var px = p.position.x - minX;
            var py = p.position.y;
            var pz = p.position.z - minZ;
            var normalizedPosition = new Vector3(px, py, pz);
            return new Pose(normalizedPosition, p.rotation);
        });

        var ySortedTower = towerToOrigin.GroupBy(p => Mathf.FloorToInt(p.position.y/_tileSize.y))
            .Select(group => group.ToList()).ToList();

        int layerCount = ySortedTower.Count;
        Debug.Log(layerCount);
        //Boolean list of outcomes
        bool[,] hasBlock = new bool[layerCount, 3];

        //Analyze each layer
        for (int i = 0; i < layerCount; i++)
        {
            var layer = ySortedTower[i];

            //print(layer[0].rotation.eulerAngles.y);
            var layerRotation = layer[0].rotation.eulerAngles.y;

            //var t = layerRotation/90 






            if ((layerRotation >= 80f && layerRotation <= 100f) || (layerRotation >= 260 && layerRotation <= 280))
            {

                var xSrotedLayer = layer.OrderBy(l => l.position.x);
                foreach (var _pose in xSrotedLayer)
                {

                    if (_pose.position.x >= -0.01f && _pose.position.x <= 0.01f)
                    {
                        Debug.Log($"Found 0 on layer {i}");
                        hasBlock[i, 0] = true;
                    }
                    else if (_pose.position.x >= _tileSize.z && _pose.position.x < _tileSize.z * 2)
                    {
                        Debug.Log($"Found 1 on layer {i}");
                        hasBlock[i, 1] = true;
                    }
                    else if (_pose.position.x >= _tileSize.z * 2)
                    {
                        Debug.Log($"Found 2 on layer {i}");
                        hasBlock[i, 2] = true;
                    }
                }
            }
            else if ((layerRotation >= -10f && layerRotation <= 10f) || (layerRotation >= 170 && layerRotation <= 190f))
            {
                var zSrotedLayer = layer.OrderBy(l => l.position.z);
                foreach (var _pose in zSrotedLayer)
                {

                    if (_pose.position.z >= -0.01f && _pose.position.z <= 0.01f)
                    {
                        Debug.Log($"Found 2 on layer {i}");
                        hasBlock[i, 2] = true;
                    }
                    else if (_pose.position.z >= _tileSize.z && _pose.position.z < _tileSize.z * 2)
                    {
                        Debug.Log($"Found 1 on layer {i}");
                        hasBlock[i, 1] = true;
                    }
                    else if (_pose.position.z >= _tileSize.z * 2)
                    {
                        Debug.Log($"Found 0 on layer {i}");
                        hasBlock[i, 0] = true;
                    }
                }
            }

        }
        var referenceTower = JengaTower(layerCount);

        //Create new list of normalized void mirror poses
        List<Pose> vmPoses = new List<Pose>();

        for (int i = 0; i < layerCount; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (!hasBlock[i, j])
                {
                    //Debug.Log($"Block {i},{j} created");
                    vmPoses.Add(referenceTower[i, j]);
                }
            }
        }
        //vmPoses.Reverse();
        return vmPoses;

    }



    //Pose[,] JengaTower(int layerCount)
    //{
    //    Pose[,] tower = new Pose[layerCount, 3];
    //    int count = layerCount;
    //    for (int i = 0; i < count; i++)
    //    {
    //        bool isEven = i % 2 == 0;
    //        for (int j = 0; j < 3; j++)
    //        {
    //            Vector3 position = new Vector3(0, (i + 1) * _tileSize.y, (j - 1) * (_tileSize.z + _gap));
    //            var rotation = Quaternion.Euler(0, !isEven ? 0 : -90, 0) /** Quaternion.Euler(0, 180f, 0)*/;
    //            tower[i, j] = new Pose(rotation * position + _TargetplacePoint, rotation);

    //        }
    //    }

    //    return tower;
    //}

    Pose[,] JengaTower(int layerCount)
    {
        Pose[,] tower = new Pose[layerCount, 3];
        int count = layerCount;
        for (int i = 0; i < count; i++)
        {
            bool isEven = i % 2 == 0;
            for (int j = 0; j < 3; j++)
            {
                float yPos = (i + 1) * _tileSize.y;
                float zPos = (j - 1) * (_tileSize.z + _gap);

                Vector3 position = new Vector3(0, yPos, zPos);
                var rotation = Quaternion.Euler(0, !isEven ? 0 : -90, 0) * Quaternion.Euler(0, 180f, 0);
                tower[(count - 1) - i, j] = new Pose(rotation * position + _TargetplacePoint, rotation);

            }
        }

        return tower;
    }
}
