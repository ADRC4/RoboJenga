﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class TeamC_Loop : IStackable
{
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _placedTiles; } }

    readonly Rect _pickRect;
    readonly Rect _buildRect;
    readonly ICamera _camera;

    List<Orient> _placedTiles = new List<Orient>();

    int _loopCount = 0;
    float _mirrorPoint;

    public TeamC_Loop(Mode mode)
    {
        Message = "Simple vision stacking.";
        float m = 0.02f;
        _pickRect = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);
        _buildRect = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);

        if (mode == Mode.Virtual)
            _camera = new VirtualCamera();
        else
            _camera = new LiveCamera();
    }

    public PickAndPlaceData GetNextTargets()
    {
        for (int i = 0; i < 12; i++)
        {
            var pickTiles = _camera.GetTiles(_pickRect);

            if (pickTiles == null)
            {
                Message = "Camera error.";
                return null;
            }

            if (pickTiles.Count == 0)
            {
                Message = "No more tiles to pick.";
                return null;
            }

            var pick = pickTiles.First();


            var buildTiles = _camera.GetTiles(_pickRect);

            if (buildTiles == null)
            {
                Message = "Camera error.";
                return null;
            }

            if (buildTiles.Count == 0)
            {
                Message = "No tiles in build area.";
                return null;
            }

            var tile = buildTiles.First();

            if (_loopCount == 0)
            {
                float distance = 0.3f;
                _mirrorPoint = tile.Center.x + distance * 0.5f;
                return null;
            }


            var distanceToCenter = _mirrorPoint - tile.Center.x;
            var pos = tile.Center;
            pos.x = _mirrorPoint + distanceToCenter;

            var xAxis = tile.Rotation * Vector3.right;

            var angle = Vector3.SignedAngle(xAxis, Vector3.forward, Vector3.up);
            var rotation = Quaternion.Euler(0, -angle, 0);

            Orient place = new Orient(pos, rotation);

            _placedTiles.Add(place);

            return new PickAndPlaceData { Pick = pick, Place = place };
        }


        for (int j = 12; j <= 12; j++)
        {
            var pickTiles = _camera.GetTiles(_pickRect);

            if (pickTiles == null)
            {
                Message = "Camera error.";
                return null;
            }

            if (pickTiles.Count == 0)
            {
                Message = "No more tiles to pick.";
                return null;
            }

            var pick = pickTiles.First();


            var buildTiles = _camera.GetTiles(_pickRect);

            if (buildTiles == null)
            {
                Message = "Camera error.";
                return null;
            }

            if (buildTiles.Count == 0)
            {
                Message = "No tiles in build area.";
                return null;
            }

            var tile = buildTiles.First();

            if (_loopCount == 0)
            {
                float distance = 0.12f;
                _mirrorPoint = tile.Center.z + distance * 0.5f;
                return null;
            }


            var distanceToCenter = _mirrorPoint - tile.Center.z;
            var pos = tile.Center;
            pos.z = _mirrorPoint + distanceToCenter;

            var zAxis = tile.Rotation * Vector3.up;

            var angle = Vector3.SignedAngle(zAxis, Vector3.right, Vector3.up);
            var rotation = Quaternion.Euler(-angle, 0, 0);

            Orient place = new Orient(pos, rotation);

            _placedTiles.Add(place);

            return new PickAndPlaceData { Pick = pick, Place = place };
        }
        return null;
    }


    //IList<Orient> ScanConstruction(Rect rectangle)
    //{

    //    var topLayer = _camera.GetTiles(rectangle);

    //    if (topLayer == null)
    //    {
    //        Message = "Camera error.";
    //        return null;
    //    }

    //    Message = "Scanning blocks";
    //    return topLayer;

    //    var pick = topLayer.First();
    //    Vector3 get = new Vector3();
    //    get = pick.Center;

    //    Vector3 player_position = get.transform.position;

    //    float x = player_position.x;
    //    float y = player_position.y;
    //    float z = player_position.z;

    //    var distance = 0.120f;

    //}
}