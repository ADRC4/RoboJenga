using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingTeamD : IStackable
{
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _placedTiles; } }
    Rect _rect;

    readonly ICamera _camera;
    List<Orient> _placedTiles = new List<Orient>();
    List<Orient> _pickTiles = new List<Orient>();
    int _tileCount = 18;
    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    readonly float _gap = 0.005f;

    public StackingTeamD(Mode mode)
    {
        Message = "Simple vision stacking.";
        float m = 0.02f;
        _rect = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);

        if (mode == Mode.Virtual)
            _camera = new TeamDVirtualCamera();
        else
            _camera = new LiveCamera();
    }

    bool CheckCamera(IList<Orient> tiles)
    {
        if (tiles == null)
        {
            Message = "Camera error.";
            return false;
        }
        if (tiles.Count == 0)
        {
            Message = "No tiles left.";
            return false;
        }
        return true;
    }

    public PickAndPlaceData GetNextTargets()
    {
        float m = 0.02f;
        var topLayer = _camera.GetTiles(_rect);

        //if (topLayer == null)
        //{
        //    Message = "Camera error.";
        //    return null;
        //}

        //if (topLayer.Count == 0)
        //{
        //    Message = "No more tiles.";
        //    return null;
        //}

        if (_placedTiles.Count == 0)
        {
            var scanRect = new Rect(0.7f + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);
            var scanTiles = _camera.GetTiles(scanRect);

            var center = new Vector3(0.7f, 0f, 0.4f);
            if (!CheckCamera(scanTiles)) return null;
            if (scanTiles.Count > 1)
            {
                Message = "Place block. ";
                return null;
            }

            if (scanTiles.Count == 1)
            {
                var pick = _pickTiles.SingleOrDefault();
                var place = TowerB(_tileCount - _pickTiles.Count);
                place.Center.x += 0.700f;
                _pickTiles.RemoveAt(_pickTiles.Count - 1);
                _placedTiles.Add(pick);
                Message = $"Placing tile {_placedTiles.Count} out of {_tileCount}";
            }

            if (scanTiles.Count > 1)
            {
                var pick = _pickTiles.SingleOrDefault();
                var place = TowerA(_tileCount - _pickTiles.Count);
                place.Center.x += 0.700f;
                _pickTiles.RemoveAt(_pickTiles.Count - 1);
                _placedTiles.Add(pick);
                Message = $"Placing tile {_placedTiles.Count} out of {_tileCount}";
            }
        }


        return new PickAndPlaceData { };
    }

    Orient TowerA(int index)
    {
        int count = index;
        int layer = count / 1;
        int row = count % 1;

        Vector3 position = new Vector3(0, (layer + 1) * _tileSize.y, 1 / 3 * _tileSize.z + _gap);
        var rotation = Quaternion.Euler(0, -90, 0);
        return new Orient(position, rotation);
    }

    Orient TowerB(int index)
    {
        int count = index;
        int layer = count / 1;
        int row = count % 2;

        Vector3 position = new Vector3(0, (layer + 1) * _tileSize.y, (row - 2) * _tileSize.z + _gap);
        var rotation = Quaternion.Euler(0, -90, 0);
        return new Orient(position, rotation);
    }
}

class TeamDVirtualCamera : ICamera
{
    Queue<Orient[]> _sequence;
    IList<Orient> _strucutre = new[]
    {
        new Orient(0.1f,0.045f,0.3f,90.0f),
        new Orient(0.2f,0.045f,0.1f,30.0f),
        new Orient(0.4f,0.045f,0.3f,45.0f)
    };

    public TeamDVirtualCamera()
    {
        var t = new[]
        {
           new Orient(0.1f,0.045f,0.3f,90.0f),
           new Orient(0.2f,0.045f,0.1f,30.0f),
           new Orient(0.4f,0.045f,0.3f,45.0f)
        };

        _sequence = new Queue<Orient[]>(new[]
        {
           new[] {t[0],t[1],t[2]},
           new[] {t[1],t[2]},
           new[] {t[2]},
           new Orient[0]
        });
    }

    public IList<Orient> GetTiles(Rect area)
    {
        if (area.min.x < 5)
        {
            return _sequence.Dequeue();
        }
        else
        {
            return _strucutre;
        }

    }
}