using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingVisionSimple : IStackable
{
    public string Message { get; private set; }
    public ICollection<Pose> Display { get; } = new List<Pose>();

    readonly Rect _rect;
    readonly ICamera _camera;

    public StackingVisionSimple(Mode mode)
    {        
        Message = "Simple vision stacking.";
        float m = 0.02f; //margin from the edge of the table
        _rect = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2); //half of the table

        if (mode == Mode.Virtual)
            _camera = new VirtualCamera();
        else
            _camera = new MotiveCamera();
    }

    public PickAndPlaceData GetNextTargets()
    {
        var topLayer = _camera.GetTiles(_rect);

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

        var pick = topLayer.First();
        var place = pick;
        place.position.x += 0.700f;
        Display.Add(place);

        return new PickAndPlaceData { Pick = pick, Place = place };
    }
}