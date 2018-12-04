using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StackingTeamB : IStackable
{
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _placedTiles; } }
    readonly Rect _Pickrect;
    readonly Rect _HumanRect;
    readonly Rect _Robotrect;

    int layer = 0;
    int singleI = 0;
    List<Vector3> gravityCenter = new List<Vector3>();
    List<detectHuman> firstLayer = new List<detectHuman>();
    List<Orient> secondLayer = new List<Orient>();

    readonly Rect _rect;
    readonly ICamera _camera;
    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    readonly int _tileCount = 45;

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

        //define a rectangle area
        _HumanRect = new Rect(0 + m, 0 + m, 0.4f - m * 2, 0.8f - m * 2);
        _Pickrect = new Rect(1.0f + m, 0 + m, 0.4f - m * 2, 0.8f - m * 2);
        // _Jengarect = new Rect(0.9f + m, 0 + m, 0.9f - m * 2, 0.8f - m * 2);
        // _RobotRect = new Rect(0.4f + m, 0 + m, 0.4f - m * 2, 0.8f - m * 2);

        //scan according to mode
        if (mode == Mode.Virtual)
            _camera = new VirtualCamera();
        else
            _camera = new LiveCamera();
        

    }

    //每次在robot里面run一次pick&place
    //这里让机器人动起来，要看有没有output这些error
    public PickAndPlaceData GetNextTargets()
    {

        Orient place = new Orient();
        List<Orient> _place = new List<Orient>();
        if (singleI==0)
        {
            detectFirstLayer();
        }
        //识别用于搭建的木块
        var pickrectTiles = _camera.GetTiles(_Pickrect);

        if (pickrectTiles == null)
        {
            Message = "Camera error.";
            return null;
        }

        if (pickrectTiles.Count == 0)
        {
            Message = "No more tiles to pick.";
            return null;
        }
        //pick 
        var pick = pickrectTiles.First();

        if (firstLayer == null)
        {
            Message = "Camera error.";
            return null;
        }

        //when put first layer//place
        if (layer == 0)
        {

            foreach (var i in firstLayer)
            {
                var pieceOfFirstLayer = i.Orient;
                //改变值放到place这个list里
                //calculate the position of the first layer
                pieceOfFirstLayer.Center.x += 0.500f;
                _place.Add(pieceOfFirstLayer);
            }

            //calculate the gravity centre of first layer
            calculateNewGravityCenter(_place);

            //放下的position  要得到place的orient 所以call了
            place = _place[singleI];
            singleI++;

            if (singleI == firstLayer.Count)
            {
                layer++;
                singleI = 0;

            }

            return new PickAndPlaceData { Pick = pick, Place = place }; //get layer1 success first
        }

        else
        {
            if (singleI == 0)
            {
                detectSecondLayer();
                detectVacancy();
                _place = placeLocation();
                calculateNewGravityCenter(_place);
            }
            place = _place[singleI];
            if (singleI == placeLocation().Count)
            {
                singleI = 0;
                layer += 1;
            }
            return new PickAndPlaceData { Pick = pick, Place = place };
        }


    }


    //检测第一层位置method
    void detectFirstLayer()
    {//pieces in the area
        var topLayer = ScanConstruction(_HumanRect);
        //may return null，!null check
        if (topLayer != null)
        {
            foreach (var detect in topLayer)
            {     //y是不是小于0.045 要验证所有条件判断
                if (detect.Center.y < 0.05)
                {
                    detectHuman element = new detectHuman();
                    element.Orient = detect;
                    ////new default is true
                    firstLayer.Add(element);
                    //not detected write error
                }

            }
        }
    }

    //检测第二层位置method
    void detectSecondLayer()
    {

        var topLayer = ScanConstruction(_HumanRect);

        foreach (var i in topLayer)
        {
            if (i.Center.y > 0.05)
            {
                secondLayer.Add(i);
            }
        }
    }


    //new gravity centre
    void calculateNewGravityCenter(List<Orient> _place)
    {
        Vector3 calCenter = new Vector3();
        foreach (var i in _place)
        {
            calCenter += new Vector3(i.Center.x, 0.5f * _tileSize.y, i.Center.z);
        }

        Vector3 thisCenter = new Vector3(calCenter.x / _place.Count, 0.5f * _tileSize.y, calCenter.z / _place.Count);
        gravityCenter.Add(thisCenter);
    }

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

    //define a new orient go through firstlayer
    public List<Orient> placeLocation()
    {
        List<Orient> _place = new List<Orient>();
        float jiaoDu = 90;
        float angle = 3.1415f / 180 * jiaoDu;

        foreach (var i in firstLayer) //遍历firstlayer
        {
            var place = i.Orient;

            if (i.ifVacant)
            {

                Vector3 position = place.Center;

                Vector3 gravityCenterHumanFirst = gravityCenter[0];
                gravityCenterHumanFirst.x -= 0.5f;

                //move to place area
                float deltaX = position.x - gravityCenterHumanFirst.x;
                float deltaZ = position.z - gravityCenterHumanFirst.z;
                position.x = gravityCenter[layer].x + deltaX;
                position.z = gravityCenter[layer].z + deltaZ;



                position.y = (layer + 1) * _tileSize.y;
                bool isEven = true;
                if (layer % 2 != 0)
                {
                    isEven = false;
                    var rotation = Quaternion.Euler(0, isEven ? 0 : -1 * jiaoDu, 0); //define the angle of rotation，explain:isEven；三段表达 ？千面有个表达式true or false 如果是true 返回0；如果false 返回-90，0

                    //rotate angle, and rotate centre //return（0，0，0）；（0，-90，0）
                    //spin
                    position.x = (float)((position.x - gravityCenter[layer].x) * Math.Cos(angle) - (position.x - gravityCenter[layer].x) * Math.Sin(angle) + gravityCenter[layer].x);
                    position.z = (float)((position.z - gravityCenter[layer].z) * Math.Cos(angle) - (position.z - gravityCenter[layer].z) * Math.Sin(angle) + gravityCenter[layer].z);


                    place.Center = position;
                    place.Rotation = rotation;
                }

                _place.Add(place);
            }

        }
        //计算新的重心 方法还没完  一通乱算后 

        calculateNewGravityCenter(_place);//gravity of this layer


        return _place;//返回place的数组
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


}
