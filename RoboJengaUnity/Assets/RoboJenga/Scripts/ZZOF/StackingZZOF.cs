using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingZZOF : IStackable
{
    public string Message { get; private set; }
    public ICollection<Pose> Display => _pickTiles;

    readonly Vector3 _placePoint = new Vector3(0.9f, 0, 0.4f);
    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    readonly Rect _rect;
    readonly float _gap = 0.01f;
    readonly ICamera _camera;

    bool _isScanning = true;
    int _tileCount = 0;
    List<Pose> _pickTiles = new List<Pose>();

    public StackingZZOF(Mode mode)
    {
        if (mode == Mode.Virtual)
            _camera = new VirtualCameraZZOF();
        else
            _camera = new MotiveCamera();

        Message = "Replicate vision stacking.";
        float m = 0.02f;
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
            return BuildBlocks();
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
            Message = "Finished scanning, rebuilding.";
            RotateTower();
            return BuildBlocks();
        }

        var pick = topLayer.First();
        _pickTiles.Add(pick);
        var place = JengaLocation(_tileCount);
        _tileCount++;
        return new PickAndPlaceData { Pick = pick, Place = place };
    }

    Vector3 TowerBase()
    {
        var bounds = new Bounds(_pickTiles.First().position, Vector3.zero);

        foreach (var tile in _pickTiles)
            bounds.Encapsulate(tile.position);

        var towerBase = bounds.center;
        towerBase.y = 0;
        return towerBase;
    }

    void RotateTower()
    {
        var towerBase = TowerBase();

        for (int i = 0; i < _pickTiles.Count; i++)
        {
            var tile = _pickTiles[i];

            // position
            var vector = tile.rotation * Vector3.right;
            var distance = _tileSize.x * 0.25f * 0.5f;
            vector *= distance;

            var random = Random.value;
            if (random < 0.5f)
                vector *= -1;

            tile.position += vector;

            // rotation
            var layerIndex = Mathf.RoundToInt((tile.position.y - _tileSize.y) / _tileSize.y);
            var isOdd = layerIndex % 2 == 1;
            var angle = 45 * 0.5f;

            if (isOdd)
            {
                angle *= -1;
            }

            var rotatedTile = tile.RotateAround(towerBase, angle);

            _pickTiles[i] = rotatedTile;
        }
    }

    PickAndPlaceData BuildBlocks()
    {
        if (_tileCount == 0) return null;
        var pick = JengaLocation(_tileCount - 1);
        var place = _pickTiles[_tileCount - 1];
        _tileCount--;
        return new PickAndPlaceData { Pick = pick, Place = place };
    }

    Pose JengaLocation(int index)
    {
        int count = index;
        int layer = count / 3;
        int horiz = count % 3;
        bool isEven = layer % 2 == 0;

        Vector3 position = new Vector3(0, (layer + 1) * _tileSize.y, (horiz - 1) * (_tileSize.z + _gap));
        var rotation = Quaternion.Euler(0, isEven ? 0 : -90, 0) * Quaternion.Euler(0, 180f, 0);
        return new Pose(rotation * position + _placePoint, rotation);
    }
}