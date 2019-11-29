using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingOffline : IStackable
{
    public string Message { get; private set; }
    public ICollection<Pose> Display => _placeTiles;

    readonly Vector3 _pickPoint = new Vector3(0.2f, 0, 0.4f);
    readonly Vector3 _placePoint = new Vector3(1.2f, 0, 0.4f);
    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    readonly int _tileCount = 18;
    readonly float _gap = 0.005f;

    List<Pose> _pickTiles = new List<Pose>();
    List<Pose> _placeTiles = new List<Pose>();

    public StackingOffline(Mode mode)
    {
        MakePickTower();
    }

    public PickAndPlaceData GetNextTargets()
    {
        if (_pickTiles.Count == 0)
        {
            Message = "No tiles left.";
            return null;
        }

        var pick = _pickTiles.Last();

        var place = RandomLocation(_tileCount - _pickTiles.Count);
        place.position += _placePoint;

        _pickTiles.RemoveAt(_pickTiles.Count - 1);
        _placeTiles.Add(place);

        Message = $"Placing tile {_placeTiles.Count} out of {_tileCount}.";

        return new PickAndPlaceData { Pick = pick, Place = place };
    }

    void MakePickTower()
    {
        for (int i = 0; i < _tileCount; i++)
        {
            var next = JengaLocation(i);
            _pickTiles.Add(new Pose(_pickPoint + next.position, next.rotation));
        }
    }

    Pose JengaLocation(int index)
    {
        int count = index;
        int layer = count / 9;
        int row = count % 9;
        bool isEven = layer % 2 == 0;

        Vector3 position = new Vector3(0, (layer + 1) * _tileSize.y, (row - 1) * _tileSize.z);
        var rotation = Quaternion.Euler(0, isEven ? 0 : -45, 0);
        return new Pose(rotation * position, rotation);
    }

    Pose RandomLocation(int index)
    {
        int count = _placeTiles.Count;
        int layer = count / 2;
        int row = count % 2 * Random.Range(1, 2);
        bool isEven = layer % 2 == 0;

        Vector3 position = new Vector3(0, (layer + 1) * _tileSize.y, (row - 1) * _tileSize.z + _gap);
        var rotation = Quaternion.Euler(0, isEven ? 0 : -45, 0);
        return new Pose(rotation * position, rotation);
    }
}