using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CheckForBlindspots : IStackable
{
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _placedTiles; } }

    readonly Rect _rect;
    readonly ICamera _camera;

    List<Orient> _placedTiles = new List<Orient>();

    public CheckForBlindspots(Mode mode)
    {
        Debug.Log("constructor");
        Message = "CheckForBlindspots";
        float m = 0.02f; //margin from the edge of the table
        _rect = new Rect(0 + m, 0 + m, 1.4f - m * 2, 0.8f - m * 2); //half of the table

        if (mode == Mode.Virtual)
            _camera = new VirtualCamera();
        else
            _camera = new LiveCamera();
    }

    public PickAndPlaceData GetNextTargets()
    {
        Debug.Log("GetNextTargets");
        var topLayer = _camera.GetTiles(_rect);

        if (topLayer == null) //no blocks scanned
        {
            Message = "Camera error.";
            return null;
        }

        if (topLayer.Count == 0)
        {
            Message = "No more tiles.";
            return null;
        }

        Debug.Log("PlacedTiles");
        _placedTiles = topLayer.ToList();



        return null;
    }
}