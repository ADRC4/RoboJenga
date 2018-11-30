using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingTeamC : IStackable
{
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _placedTiles; } }

    readonly Rect _pickRect;
    readonly Rect _buildRect;
    readonly ICamera _camera;

    List<Orient> _placedTiles = new List<Orient>();

    List<Orient> _currentLayer = null;

    int _loopCount = 0;
    float _mirrorPoint;

    public StackingTeamC(Mode mode)
    {
        Message = "Simple vision stacking.";
        float m = 0.02f;
        _pickRect = new Rect(1f + m, 0 + m, 0.4f - m * 2, 0.8f - m * 2);
        _buildRect = new Rect(0 + m, 0 + m, 1f - m * 2, 0.8f - m * 2);

        if (mode == Mode.Virtual)
            _camera = new VirtualCamera();
        else
            _camera = new LiveCamera();
    }

    public PickAndPlaceData GetNextTargets()
    {
        if (_currentLayer == null)
        {
            _currentLayer = _camera.GetTiles(_buildRect).ToList();

            if (_currentLayer == null)
            {
                Message = "Camera error.";
                return null;
            }
        }

        if (_currentLayer.Count == 0)
        {
            Message = "Add a new layer of tiles.";
            _currentLayer = null;
            return null;
        }

        if (_loopCount == 0)
        {
            var rightMostTile = _currentLayer.OrderByDescending(t => t.Center.x).First();
            float distance = 0.3f;
            _mirrorPoint = rightMostTile.Center.x + distance * 0.5f;
        }

        var tile = _currentLayer.Last();
        _currentLayer.RemoveAt(_currentLayer.Count - 1);
        Message = $"{_currentLayer.Count} tiles left on current layer.";

        var distanceToCenter = _mirrorPoint - tile.Center.x;
        var pos = tile.Center;
        pos.x = _mirrorPoint + distanceToCenter;

        var xAxis = tile.Rotation * Vector3.right;

        var angle = Vector3.SignedAngle(xAxis, Vector3.forward, Vector3.up);
        var rotation = Quaternion.Euler(0, -angle, 0);

        Orient place = new Orient(pos, rotation);

        _placedTiles.Add(place);


        // pick tile
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

        _loopCount++;
        return new PickAndPlaceData { Pick = pick, Place = place };
    }
}