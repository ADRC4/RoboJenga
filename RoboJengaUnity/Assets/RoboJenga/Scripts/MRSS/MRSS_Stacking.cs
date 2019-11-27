using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MRSS_Stacking : IStackable
{
    public string Message { get; private set; }


    public ICollection<Pose> Display { get; } = new List<Pose>();

    readonly Vector3 _placePoint = new Vector3(1.4f, 0.045f, 0.8f);

    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);


    readonly Rect _rectTarget;
    readonly Rect _PickPileRect;


    readonly float _gap = 0.01f;
    readonly ICamera _camera;


    int _tileCount = 1;
    int _tilelayer = 1;
    int _layerlength = 5;


    List<Pose> _pickTiles = new List<Pose>();
    IList<Pose> _existingBlocks = new List<Pose>();

    public IList<Pose> targetTower;

    public MRSS_Stacking(Mode mode)
    {
        if (mode == Mode.Virtual)
            _camera = new MRSS_VirtualCamera();
        else
            _camera = new MotiveCamera();

        Message = "Running MRSS stacking.";

        float m = 0.02f;

        //Split the table into two.
        _PickPileRect = new Rect(0.8f + m, 0 + m, 0.6f - m * 2, 0.8f - m * 2);
        _rectTarget = new Rect(0 + m, 0 + m, 0.8f - m * 2, 0.8f - m * 2);
    }

    public PickAndPlaceData GetNextTargets()
    {
        // Top layer on the pickSide
        var topLayer = _camera.GetTiles(_PickPileRect);

        targetTower = _camera.GetTiles(_rectTarget);

        if (topLayer == null)
        {
            Message = "Camera error.";
            return null;
        }

        if (topLayer.Count == 0)
        {
            return null;
        }


        var pick = topLayer.First();
        _pickTiles.Add(pick);

       

        
        var place = ConstructLocation(_tileCount);


        if (_tileCount < _layerlength) { _tileCount++; }
        else { _tilelayer++; _tileCount = 1; _layerlength--; }

        

        Display.Add(place);
        return new PickAndPlaceData { Pick = pick, Place = place };
    }





    Pose ConstructLocation(int index)
    {




        Debug.Log(targetTower.First<Pose>().position);

        var rotation = Quaternion.Euler(0, 0, 0) * Quaternion.Euler(0, 180f, 0);

        Vector3 position =  new Vector3((targetTower.First<Pose>().position.x) + _tileSize.x*index, _tileSize.y*_tilelayer, targetTower.First<Pose>().position.z) ;
        
        return new Pose( position , rotation);
    }



   
}