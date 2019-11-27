using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingCorbelling : IStackable
{
    public string Message { get; private set; }
    public ICollection<Pose> Display => _placeTiles;

    readonly Vector3 _pickPoint = new Vector3(0.2f, 0, 0.4f);
    readonly Vector3 _placePoint = new Vector3(1.2f, 0, 0.4f);
    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    int _tileCount = 15;
    readonly float _gap = 0.005f;
    readonly ICamera _camera;
    readonly Rect _rectBridge;
    readonly Rect _rectPlace;
    bool _isScanning = true;

    List<Pose> _pickTiles = new List<Pose>();
    List<Pose> _placeTiles = null;

    public StackingCorbelling(Mode mode)
    {
        Message = "Bridging Corbelling";
        float m = 0.02f; //margin from the edge of the table
        _rectBridge = new Rect(0 + m, 1.0f + m, 1.0f - m * 2, 0.8f - m * 2); //stacking rectangle
        _rectPlace = new Rect(0 + m, 0 + m, 0.4f - m * 2, 0.8f - m * 2); // placing rectangle

        if (mode == Mode.Virtual)
            _camera = new VirtualCamera_WALDO();
        else
            _camera = new MotiveCamera();
        List<Pose> _pickTiles = new List<Pose>();

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

    public PickAndPlaceData RememberBlocks()
    {

        var topLayer = _camera.GetTiles(_rectBridge); // list of tiles?

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

        if (topLayer.Count > 1)
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



