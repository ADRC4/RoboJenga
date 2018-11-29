using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StackingTeamB : IStackable
{
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _placedTiles; } }
    readonly Rect _Jengarect;
    readonly Rect _HumanRect;
    readonly Rect _Robotrect;
    readonly Vector3 _pickPoint = new Vector3(1.1f, 0, 0.02f);
    int remainTiles = 45;

    int layer = 1;
    List<Vector3> gravityCenter = new List<Vector3>();
    List<detectHuman> firstLayer = new List<detectHuman>();
    List<Orient> secondLayer = new List<Orient>();
    int singleI = 0;


    readonly Rect _rect;
    readonly ICamera _camera;
    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    readonly int _tileCount = 45;
    readonly float _gap = 0.005f;

    List<Orient> _placedTiles = new List<Orient>();
    List<Orient> _pickTiles = new List<Orient>();

    private class detectHuman
    {
        public Orient Orient;
        public bool ifVacant = true;
    }

    public StackingTeamB(Mode mode)
    {
        Message = "Simple vision stacking.";
        float m = 0.02f;
        _HumanRect = new Rect(0 + m, 0 + m, 0.4f - m * 2, 0.8f - m * 2);
        // _Jengarect = new Rect(0.9f + m, 0 + m, 0.9f - m * 2, 0.8f - m * 2);
        _Robotrect = new Rect(0.4f + m, 0 + m, 0.4f - m * 2, 0.8f - m * 2);

        if (mode == Mode.Virtual)
            _camera = new VirtualCamera();
        else
            _camera = new LiveCamera();
        MakePickTower();


    }

    public Orient PutSameLayerNextTarget()
    {
        Orient place = new Orient();
        if (layer == 1)
        {
            List<Orient> _place = new List<Orient>();

            //扫描第一层人区，获得firstLayer
            detectFirstLayer();

            foreach (var i in firstLayer)
            {
                place = i.Orient;

                place.Center.x += 0.700f;
                place.Center.y += (layer - 0.5f) * _tileSize.y;


                _place.Add(place);
            }
            Vector3 calCenter = new Vector3();
            for (int i = 0; i < _place.Count; i++)
            {

                calCenter += new Vector3(_place[i].Center.x, 0.5f * _tileSize.y, _place[i].Center.z);

            }
            Vector3 thisCenter = new Vector3(calCenter.x / _place.Count, 0.5f * _tileSize.y, calCenter.z / _place.Count);

            gravityCenter.Add(thisCenter);
            //计算第1层位置，获得第一层gravityCenter
            place = placeLocation()[singleI];

        }
        else
        {
            //扫描新的人区
            detectSecondLayer();
            //确定取的位置
            place = placeLocation()[singleI];



            singleI++;
            if (singleI == placeLocation().Count)
            {
                layer += 1;
                //每当singleI==(placeLocation().Count-secondLayer.Count)





                //生成新的secondlayer

                //重新设定第一层布尔

                //执行一次placeLocation获取新一层的放置位置
                placeLocation();
            }
        }

        return place;
    }

    void MakePickTower()
    {
        for (int i = 0; i < _tileCount; i++)
        {
            var next = JengaLocation(i);
            _pickTiles.Add(new Orient(_pickPoint + next.Center, next.Rotation));
        }
    }

    void detectFirstLayer()
    {
        var topLayer = ScanConstruction(_HumanRect);
        //if (/*topLayer != null*/ )
        //{
        foreach (var detect in topLayer)
        {
            if (detect.Center.y < 0.045)
            {
                detectHuman element = new detectHuman();
                element.Orient = detect;
                element.ifVacant = true;
                firstLayer.Add(element);
            }

        }
        //}


    }

    void detectSecondLayer()
    {
        var topLayer = ScanConstruction(_HumanRect);

        foreach (var i in topLayer)
        {
            if (i.Center.y > 0.045)
            {
                secondLayer.Add(i);
            }

        }

    }

    public PickAndPlaceData GetNextTargets()
    {

        //if (layer==1)
        //{
        //detectFirstLayer();
        //}





        if (firstLayer == null)
        {
            Message = "Camera error.";
            return null;
        }

        if (remainTiles == 0)
        {
            Message = "No more tiles.";
            return null;
        }
        var pick = _pickTiles.Last();

        var place = PutSameLayerNextTarget();
        remainTiles -= 1;

        return new PickAndPlaceData { Pick = pick, Place = place };

    }

    //List<Orient> placeFirstLocation()
    //{
    //    List<Orient> _place = new List<Orient>();
    //    foreach (var i in firstLayer)
    //    {
    //        var place = i.Orient;
    //        if (i.ifVacant == true)
    //        {

    //            place.Center.x += 0.700f;
    //            place.Center.y += (layer - 0.5f) * _tileSize.y;
    //            _placedTiles.Add(place);
    //        }
    //        _place.Add(place);
    //    }
    //    return _place;
    //}


    public void detectVacancy()
    {
        foreach (var i in secondLayer)
        {
            var x2 = i.Center.x;
            var z2 = i.Center.z;

            foreach (var j in firstLayer)
            {
                float deltaX = System.Math.Abs(j.Orient.Center.x - x2);
                float deltaZ = System.Math.Abs(j.Orient.Center.z - z2);
                if (deltaX < 0.025f || deltaZ < 0.025f)
                {
                    j.ifVacant = false;
                }

            }
        }
    }

    //public List<Orient > placeOfFirstLocation()
    //{

    //    return 
    //}




    //将要放置的方块的位置列表
    public List<Orient> placeLocation()
    {
        List<Orient> _place = new List<Orient>();
        foreach (var i in firstLayer)
        {
            var place = i.Orient;
            int count = 0;
            float jiaoDu = 90;
            float angle = 3.1415f / 180 * jiaoDu;
            if (i.ifVacant)
            {
                count++;
                place.Center.x += 0.700f;
                place.Center.y += (layer - 0.5f) * _tileSize.y;

                //设定这一层的扭转角度

                bool isEven = layer % 2 == 0;
                Vector3 position = place.Center;

                //float tempoX = gravityCenter[layer].x - position.x;
                //float tempoZ = gravityCenter[layer].z - position.z;

                var rotation = Quaternion.Euler(0, isEven ? 0 : -90, 0);
                //Vector3 postPosition = new Vector3();
                //扭转角度 绕取到的中心点旋转
                position.x = (float)((position.x - gravityCenter[layer].x) * Math.Cos(angle) - (position.y - gravityCenter[layer].y) * Math.Sin(angle) + gravityCenter[layer].x);
                position.z = (float)((position.z - gravityCenter[layer].z) * Math.Cos(angle) - (position.z - gravityCenter[layer].z) * Math.Sin(angle) + gravityCenter[layer].z);
                place = new Orient(position, rotation);


                _place[count] = place;
            }

        }
        Vector3 calCenter = new Vector3();
        for (int i = 0; i < _place.Count; i++)
        {

            calCenter += new Vector3(_place[i].Center.x, (layer - 0.5f) * _tileSize.y, _place[i].Center.z);

        }
        Vector3 thisCenter = new Vector3(calCenter.x / _place.Count, (layer - 0.5f) * _tileSize.y, calCenter.z / _place.Count);

        gravityCenter.Add(thisCenter);

        singleI = 0;
        return _place;
    }

    IList<Orient> ScanConstruction(Rect rectangle)
    {

        var topLayer = _camera.GetTiles(rectangle);

        if (topLayer == null)
        {
            Message = "Camera error.";
            return null;
        }
        Message = "Scanning blocks";
        return topLayer;
    }

    Orient JengaLocation(int index)
    {
        int count = index;
        int layer = count / 3;
        int row = count % 3;
        bool isEven = layer % 2 == 0;

        Vector3 position = new Vector3(0, (layer + 1) * _tileSize.y, (row - 1) * _tileSize.z);
        var rotation = Quaternion.Euler(0, isEven ? 0 : -90, 0);
        return new Orient(rotation * position, rotation);
    }
}
