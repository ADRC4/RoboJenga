using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StackingTeamD : IStackable
{
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _placedTiles; } }
    Rect _rect;

    Rect _pickrect;
    Rect _placedRect;

    readonly ICamera _camera;
    List<Orient> _placedTiles = new List<Orient>();
    List<Orient> _pickTiles = new List<Orient>();

    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);

    int _tileCount = 18;
    readonly float _gap = 0.005f;


    readonly Vector3 _placePoint = new Vector3(0.9f, 0, 0.4f);

    //readonly float _gap = 0.005f;
    //int _tileCount = 6;

    //picks the virtual or live mode to run the program
    public StackingTeamD(Mode mode)
    {
        Message = "TeamD Stacking.";
        float m = 0.02f;
        _pickrect = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);
        _placedRect = new Rect(0.7f + m, 0f + m, 0.7f - m * 2, 0.8f - m * 2);

        if (mode == Mode.Virtual)
            _camera = new TeamDVirtualCamera();
        else
            _camera = new LiveCamera();
    }


    public PickAndPlaceData GetNextTargets()
    {

        var _pickTiles = _camera.GetTiles(_pickrect);
        if (_pickTiles == null)
        {
            Message = "Camera error.";
            return null;
        }
        Debug.Log($"{_pickTiles.Count()} number of blocks scanned");

        Message = "Scanning blocks";

        float m = 0.02f;

        if (_pickTiles.Count >= 1)
        {
            Message = "Place tiles.";
            var placelayer = _camera.GetTiles(_placedRect);

            if (placelayer.Count == 0)
            {
                Message = "Told you! Place tiles!";
                placelayer = null;
                return null;
            }

            if (placelayer.Count == 1)
            {
                var pick = _pickTiles.First();
                var rotation = Quaternion.Euler(0, -90, 0);

                var position = new Vector3();
                var place = new Orient(position, rotation);
                _placedTiles.Add(pick);
                return new PickAndPlaceData { Pick = pick, Place = place };
            }

            if (placelayer.Count > 1)
            {
                var pick = _pickTiles.First();
                var position = new Vector3(1 / 3 * _tileSize.x, _tileSize.y, _tileSize.z);
                var rotation = Quaternion.Euler(0, -90, 0);
                var place = new Orient(position, rotation);
                //var rotation = 
                _placedTiles.Add(pick);
                return new PickAndPlaceData { Pick = pick, Place = place };
            }
        }


        return new PickAndPlaceData { };
    }

    //Orient TowerA(int index)
    //{
    //    int count = index;
    //    int layer = count / 1;
    //    int row = count % 1;

    //    Vector3 position = new Vector3(0, (layer + 1) * _tileSize.y, 1 / 3 * _tileSize.z + _gap);
    //    var rotation = Quaternion.Euler(0, -90, 0);
    //    return new Orient(position, rotation);
    //}

    //Orient TowerB(int index)
    //{
    //    int count = index;
    //    int layer = count / 1;
    //    int row = count % 2;

    //    Vector3 position = new Vector3(0, (layer + 1) * _tileSize.y, (row - 2) * _tileSize.z + _gap);
    //    var rotation = Quaternion.Euler(0, -90, 0);
    //    return new Orient(position, rotation);
    //}

}

class TeamDVirtualCamera : ICamera
{
    Queue<Orient[]> _sequence;

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
        return _sequence.Dequeue();
    }
}



