using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;


public class StackingTeamA : IStackable
{
    private class Block
    {
        public Orient Orient;
        public int Layer;
        public Domain Base;
        public List<Block> ConnectedBaseBlocks = new List<Block>();
        public BaseMass ConnectedBase = new BaseMass();
        public BaseMass AvailableSpace = new BaseMass();
        public bool RobotBlock = false;

        public Block(Orient orient, Vector3 size, bool robotBlock)
        {
            Orient = orient;
            RobotBlock = robotBlock;
            if (RobotBlock)
            {
                Base = new Domain(Orient.Center.x - size.y / 2, Orient.Center.x + size.y / 2);
            }
            else
            {
                Base = new Domain(Orient.Center.x - size.x / 2, Orient.Center.x + size.x / 2);
            }


        }


    }

    private class Domain
    {
        public float Min, Max;
        public float Center, Size;
        public Block ConnectedBlock;

        public Domain()
        {
        }

        public Domain(float min, float max)
        {
            Min = min;
            Max = max;
            Center = Min + (Max - Min) / 2;
            Size = Mathf.Abs(Max - Min);
        }


    }

    private class BaseMass
    {
        public List<Domain> Bases = new List<Domain>();
        public Domain FirstBase { get { return Bases.First(); } }
        public Vector2 CenterWeight;

        public BaseMass()
        {
        }

        public BaseMass(float minX, float maxX)
        {
            AddDomain(new Domain(minX, maxX));
        }

        public void AddDomain(Domain dom)
        {
            Bases.Add(dom);
            CenterWeight = GetCenterAndWeight(Bases);// returns the center [0] and weight [1]

        }

        /// <summary>
        /// returns the global center (.x) and weight (.y)
        /// </summary>
        /// <param name="bases">List of all the relevant base domains</param>
        public static Vector2 GetCenterAndWeight(List<Domain> bases)
        {
            float totalCenter = 0;
            float totalWeight = 0;

            foreach (var dom in bases)
            {
                float center = dom.Center;
                float weight = dom.Size;
                if (totalCenter == 0)
                {
                    totalCenter = center;
                }
                else
                {
                    totalCenter = (totalCenter * totalWeight + center * weight) / (totalWeight + weight);
                }
                totalWeight += weight;
            }
            return new Vector2(totalCenter, totalWeight);
        }
    }

    //GLOBAL FIELDS --------------------------------------------------------------------
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _structure.Select(x => x.Orient); } }

    readonly Vector3 _placePoint = new Vector3(0.9f, 0, 0.4f);
    //Block Dimesions: 180mm, 60mm, 45mm
    public readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    readonly Rect _rectStack, _rectConstruction;
    readonly float _gap = 0.002f;
    readonly ICamera _camera;
    readonly float _tolerance = 0.02f;
    readonly float _minProblem = -1f;
    int _timesLooped = 0;
    Mode _mode;
    bool _placeRobotBlock = false;
    float _min, _max;

    List<Orient> _placedTiles = new List<Orient>();
    List<Block> _structure;

    bool _human = true;
    int _tileCount = 0;
    int _layers;
    List<Orient> _pickTiles = new List<Orient>();

    //GLOBAL CONSTRUCTOR_______________________________________________________________________
    public StackingTeamA(Mode mode)
    {

        Message = "TeamA Stacking";
        _mode = mode;
        float m = 0.02f;
        _rectStack = new Rect(0 + m, 0 + m, 0.5f - m * 2, 0.8f - m * 2);
        _rectConstruction = new Rect(0.5f + m, 0 + m, 0.9f - m * 2, 0.8f - m * 2);
        _structure = new List<Block>();

        if (mode == Mode.Virtual)
            _camera = new SimulatedCamera();
        else
            _camera = new LiveCamera();
    }


    public PickAndPlaceData GetNextTargets()
    {
        if (_human)
        {
            Debug.Log("Human");
            //search for new blocks in the top layer
            if (!ScanForNewTiles()) return null;
            
            SetLayer();

            if (_layers < 1) return null;
            _human = false;
            return null;
        }
        else
        {
            Debug.Log("Robot");
            _human = true;
            
            var topLayer = ScanConstruction(_rectStack);
            if (topLayer.Count() == 0) return null;
            var pick = topLayer.First();

            // check if there are at least 2 layers
           // if (_layers < 1) return null;

            //Get The previous block
            var topBlock = _structure.Where(x => x.Layer == _layers).First();
            var place = new Orient(GetNextOrientX(), (_layers + 2) * (_tileSize.y + _gap), topBlock.Orient.Center.z, 0);

            _structure.Add(new Block(place, _tileSize, false));
            SetLayer();

            _timesLooped++;
            //Debug.Log($"Times looped: {_timesLooped}");

            return new PickAndPlaceData { Pick = pick, Place = place, Retract = true };
        }

    }

    bool ScanForNewTiles()
    {
        var newTiles = ScanConstruction(_rectConstruction);
        if (newTiles.Count == 0)
        {
            Debug.Log("NO TILES SCANNED");
            return false;
        }

        _structure.AddRange(newTiles.Select(x => new Block(x, _tileSize, false)));
        Debug.Log($"{_structure.Count} blocks in structure");
        _min = _structure.First().Base.Min;
        _max = _structure.First().Base.Max;


        foreach (var block in _structure)
        {
            if (block.Base.Min > _min) _min = block.Base.Min;
            if (block.Base.Max < _max) _max = block.Base.Max;
        }

        Debug.Log(_structure.Count());
        return true;
    }

    float GetNextOrientX()
    {
        Block topBlock = _structure.Where(s => s.Layer == _layers).First();
        Block bottomBlock = _structure.Where(s => s.Layer == _layers - 1).First();
        Domain blockBase;

        if (topBlock.Base.Center > bottomBlock.Base.Center)
        {
            blockBase = new Domain(bottomBlock.Base.Max, topBlock.Base.Min);
            //Debug.Log($"blockbase{blockBase.Size}");
        }
        else
        {
            blockBase = new Domain(topBlock.Base.Max, bottomBlock.Base.Min);
            //Debug.Log($"blockbase{blockBase.Size}");
        }

        float problem = blockBase.Center - topBlock.Base.Center;
        return blockBase.Center + problem;
    }







    Domain GetIntersection(Domain top, Domain bottom)
    {
        Domain intersection;
        if (bottom.Max > top.Min && bottom.Max < top.Max && bottom.Min > top.Min && bottom.Min < top.Max)
        {
            intersection = new Domain(top.Max, top.Max);
        }
        if (bottom.Max > top.Min && bottom.Max < top.Max)
        {
            intersection = new Domain(top.Min, bottom.Max);
        }
        else if (bottom.Min > top.Min && bottom.Min < top.Max)
        {
            intersection = new Domain(bottom.Min, top.Max);
        }
        else
        {
            //Debug.Log("No Intersections");
            return null;
        }

        return intersection;
    }

    void SetLayer()
    {
        foreach (var block in _structure)
        {
            block.Layer = (int)((block.Orient.Center.y - (_tileSize.z / 2)) / _tileSize.y);
            if (block.Layer > _layers) _layers++;
        }
        //Debug.Log($"Setting Layers: Max layer = {_layers}");
    }

    List<Orient> ScanConstruction(Rect rectangle)
    {

        var topLayer = _camera.GetTiles(rectangle).ToList();

        if (topLayer == null)
        {
            Message = "Camera error.";
            return null;
        }
        Message = "Scanning blocks";
        return topLayer;
    }

    Vector3 GetCenterOfMass(List<Orient> orients)
    {
        Vector3 centerOfMass = Vector3.zero;

        foreach (var orient in orients)
        {
            centerOfMass += orient.Center;
        }
        centerOfMass /= orients.Count;
        Debug.Log($"Center of mass of {orients.Count} Blocks: {centerOfMass}");
        return centerOfMass;
    }




    class SimulatedCamera : ICamera
    {
        Queue<Orient[]> _sequence;
        List<Orient> _topLayer = new List<Orient>
        {
           new Orient(0.8f,0.045f,0.3f,0),
           new Orient(0.89f,0.09f,0.3f,0)
           /*,
           new Orient(1.05f,0.045f,0.3f,0),
           new Orient(0.9f,0.09f,0.3f,0),
           new Orient(1f,0.135f,0.3f,0),
           new Orient(0.8f,0.135f,0.3f,0)*//*,
           new Orient(1.1f,0.180f,0.3f,0),
           new Orient(1.2f,0.225f,0.3f,0),
           new Orient(1.3f,0.270f,0.3f,0),
           new Orient(1.2f,0.315f,0.3f,0),
           new Orient(1.4f,0.315f,0.3f,0)*/
           /*new Orient(0.8f,0.045f,0.3f,0),
           new Orient(1f,0.045f,0.3f,0),
           new Orient(1.2f,0.045f,0.3f,0),
           new Orient(0.7f,0.09f,0.3f,0),
           new Orient(0.9f,0.09f,0.3f,0),
           new Orient(1.1f,0.09f,0.3f,0),
           new Orient(1.2f,0.135f,0.3f,0)*/
        };

        public SimulatedCamera()
        {

            var t = new[]
            {
           new Orient(0.1f,0.045f,0.3f,90.0f),
           new Orient(0.2f,0.045f,0.1f,30.0f),
           new Orient(0.4f,0.045f,0.3f,45.0f),
           new Orient(0.2f,0.045f,0.2f,30.0f)
        };

            _sequence = new Queue<Orient[]>(new[]
            {
           new[] {t[0],t[1],t[2],t[3]},
           new[] {t[1],t[2],t[3]},
           new[] {t[2],t[3]},
           new[] {t[3]},
           new Orient[0]
        });
        }

        /* public void SetTopLayer(List<Block> topLayer, int layers)
        {
            
            foreach (var block in topLayer.Where(s => s.Layer == layers))
            {
                _topLayer.Add(block.Orient);
            }
        }*/

        public IList<Orient> GetTiles(Rect area)
        {
            if (area.xMin == 0.02f)
            {
                return _sequence.Dequeue();
            }
            else
            {
                return _topLayer;
            }
        }
    }
}