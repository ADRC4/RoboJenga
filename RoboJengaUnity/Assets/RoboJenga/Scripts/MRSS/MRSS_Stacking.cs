using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System;

public class MRSS_Stacking : IStackable
{
    public string Message { get; private set; }


    public ICollection<Pose> Display { get; } = new List<Pose>();

    //readonly Vector3 _placePoint = new Vector3(1.4f, 0.045f, 0.8f);

    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);


    readonly Rect _rectTarget;
    readonly Rect _PickPileRect;
    readonly Rect _rectStart;


    readonly float _gap = 0.01f;
    readonly ICamera _camera;


    int _tileCount = 1;
    int _tilelayer = 1;

    int _layerlengthx;
    int _layerlengthz;
    int _layerlength;
    int _step = 1;

    List<Pose> _pickTiles = new List<Pose>();


    Pose targetTower;
    Pose sourceTower;
    public MRSS_Stacking(Mode mode)
    {
        if (mode == Mode.Virtual)
            _camera = new MRSS_VirtualCamera();
        else
            _camera = new MotiveCamera();

        Message = "Running MRSS stacking.";

        float m = 0.02f;

        //Split the table into two.
        _PickPileRect = new Rect(1.1f + m, 0 + m, 0.3f - m * 2, 0.8f - m * 2);

        _rectTarget = new Rect(0 + m, 0 + m, 0.6f - m * 2, 0.8f - m * 2);
        _rectStart = new Rect(0.6f + m, 0 + m, 0.5f - m * 2, 0.8f - m * 2);


    }

    public PickAndPlaceData GetNextTargets()
    {
        // Top layer on the pickSide
        var topLayer = _camera.GetTiles(_PickPileRect);


        if (topLayer == null)
        {
            Message = "Camera error.";
            return null;
        }

        if (topLayer.Count == 0)
        {
            Message = "No tiles to pick.";
            return null;
        }

        var pick = topLayer.First();
        _pickTiles.Add(pick);


        //tower
        var towerBlockTarget = _camera.GetTiles(_rectTarget) ;
        var towerBlockSource = _camera.GetTiles(_rectStart);


        if (towerBlockTarget == null || towerBlockSource == null)
        {
            Message = "Camera error.";
            return null;
        }

        if (towerBlockTarget.Count == 0 || towerBlockSource.Count == 0)
        {
            Message = "No tower found.";
            return null;
        }

        targetTower = towerBlockTarget.First();
        sourceTower = towerBlockSource.First();


        Display.Add(sourceTower);
        Display.Add(targetTower);


        //_layerlengthx = Mathf.FloorToInt(( ) / _tileSize.x);

        //if(sourceTower.position.z > targetTower.position.z)
        //    _layerlengthz = Mathf.FloorToInt((  ) / _tileSize.z);
        //else if(sourceTower.position.z < targetTower.position.z)
        //    _layerlengthz = Mathf.FloorToInt((targetTower.position.z - sourceTower.position.z) / _tileSize.z);




        _layerlength = Mathf.FloorToInt((Mathf.Sqrt( (sourceTower.position.x - targetTower.position.x)* (sourceTower.position.x - targetTower.position.x) +
            (sourceTower.position.z - targetTower.position.z)* (sourceTower.position.z - targetTower.position.z))) / (_tileSize.x - _tileSize.z*0.5f));

        Debug.Log(_layerlength);


        var place = ConstructLocation(_tileCount );


        if (_tileCount < _layerlength - _step)
        {
            _tileCount++;
        }
        else
        {
            if (_tileSize.y * _tilelayer < targetTower.position.y)
            {
                _tilelayer++;
            }
            else
            {
                _tilelayer += 0;

                return null;
            }

            _tileCount = 1;
            //_layerlengthx--;
            _step++;
        }


        Display.Add(place);

        return new PickAndPlaceData { Pick = pick, Place = place };
    }



    Pose ConstructLocation(int index)
    {

        Debug.Log(targetTower.position);


        int  angle = index % 2 == 0 ? 45 : -45;
        float sign = _tilelayer % 2 == 0 ? 1: -1;


      

        Debug.Log(_layerlengthz);

        //Vector3 position = new Vector3(
        //    (targetTower.position.x) + _tileSize.x * index,
        //    _tileSize.y * _tilelayer,
        //    targetTower.position.z);

        float x = targetTower.position.x + ((sourceTower.position.x - targetTower.position.x) * index / _layerlength);
        float z = targetTower.position.z + ((sourceTower.position.z - targetTower.position.z) * index / _layerlength);

       
            Vector3 position = new Vector3(x , _tileSize.y * _tilelayer, z );
            var rotation = Quaternion.LookRotation(new Vector3(targetTower.position.x, 0, targetTower.position.z), Vector3.up) * Quaternion.Euler(0, sign * angle, 0);

            return new Pose(position, rotation);

  
        



    }

}