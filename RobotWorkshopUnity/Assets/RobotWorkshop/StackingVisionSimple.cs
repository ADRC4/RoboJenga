using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingVisionSimple : IStackable
{
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _placedTiles; } }

    readonly Rect _rect;
    readonly ICamera _camera;

    List<Orient> _placedTiles = new List<Orient>();

    public StackingVisionSimple(Mode mode)
    {
        Debug.Log("constructor");
        Message = "Simple vision stacking.";
        float m = 0.02f; //margin from the edge of the table
        _rect = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2); //half of the table

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

        var pick = topLayer.First();
        var place = pick;
        place.Center.x += 0.700f;
        _placedTiles.Add(place);

        return new PickAndPlaceData { Pick = pick, Place = place };
    }
}