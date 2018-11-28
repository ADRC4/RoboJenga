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
    public IEnumerable<Orient> Display { get { return PlacedTiles(); } }

    readonly Vector3 _placePoint = new Vector3(0.9f, 0, 0.4f);
    //Block Dimesions: 180mm, 60mm, 45mm
    public readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    readonly Rect _rectStack, _rectConstruction;
    readonly float _gap = 0.01f;
    readonly ICamera _camera;
    readonly float _tolerance = 0.02f;
    readonly float _minProblem = -1f;
    int _timesLooped = 0;
    Mode _mode;
    bool _placeRobotBlock = false;

    List<Orient> _placedTiles = new List<Orient>();
    List<Block> _structure;

    bool _isScanning = true;
    int _tileCount = 0;
    int _layers;
    List<Orient> _pickTiles = new List<Orient>();

    //GLOBAL CONSTRUCTOR_______________________________________________________________________
    public StackingTeamA(Mode mode)
    {

        Message = "TeamA Stacking";
        _mode = mode;
        float m = 0.02f;
        _rectStack = new Rect(0 + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);
        _rectConstruction = new Rect(0.7f + m, 0 + m, 0.7f - m * 2, 0.8f - m * 2);


        if (mode == Mode.Virtual)
            _camera = new SimulatedCamera();
        else
            _camera = new LiveCamera();
    }


    public PickAndPlaceData GetNextTargets()
    {
        var topLayer = ScanConstruction(_rectStack);
        if (topLayer.Count() == 0) return null;

        //Debug.Log("GetNextTargets");
        //Scan the new structure and add the added blocks
        FindNewBlocks();
        var place = GetNextOrient(FindBiggestProblem());
        //find and place the next block

        var pick = topLayer.First();
        //new Orient(0.8f, 0.045f, 0.45f, 90);
        _timesLooped++;
        Debug.Log($"Times looped: {_timesLooped}");
        return new PickAndPlaceData { Pick = pick, Place = place };
    }

    void FindNewBlocks()
    {
        //Debug.Log("FindNewBlocks");
        var topLayer = ScanConstruction(_rectConstruction);

        var newBlocks = new List<Block>();
        //Check if it is the initial construction
        if (_structure == null)
        {
            _structure = new List<Block>();
            foreach (var orient in topLayer)
            {
                newBlocks.Add(new Block(orient, _tileSize, false));
            }
            //Debug.Log("Adding first stack");
        }
        else
        {
            // check for duplicates
            foreach (var newOrient in topLayer)
            {
                bool exists = false;
                foreach (var block in _structure)
                {
                    if (Vector3.Distance(newOrient.Center, block.Orient.Center) < _tolerance)
                    {
                        //Add new block to structure
                        exists = true;
                        break;
                    }
                }
                if (!exists) newBlocks.Add(new Block(newOrient, _tileSize, false));
            }
        }
        _structure.AddRange(newBlocks);
        SetLayer();
        GetBaseSurface();
        GetAvailableSpace();
        //Debug.Log($"{newBlocks.Count} new blocks scanned");


    }

    void GetAvailableSpace()
    {
        //reset all the available spaces
        foreach (var block in _structure)
        {
            block.AvailableSpace = new BaseMass();
        }

        //add the toplayer
        foreach (var block in _structure.Where(s => s.Layer == _layers))
        {
            block.AvailableSpace.AddDomain(block.Base);
        }

        //check the other layers
        for (int i = _layers; i > 0; i--)
        {
            var relevantTopBlocks = _structure.Where(s => s.Layer == i).ToList();
            //Debug.Log($"Unordered: First {relevantTopBlocks.First().Orient.Center.x} - Last {relevantTopBlocks.Last().Orient.Center.x}");
            relevantTopBlocks.OrderBy(o => o.Base.Min);
            //Debug.Log($"Ordered: First {relevantTopBlocks.First().Orient.Center.x} - Last {relevantTopBlocks.Last().Orient.Center.x}");
            List<Domain> availableDomains = new List<Domain>();

            //add available domains for the layer
            //add the domain befor the first block
            availableDomains.Add(new Domain(0, relevantTopBlocks.First().Base.Min));
            //add the domains between the blocks
            for (int j = 1; j < relevantTopBlocks.Count(); j++)
            {
                availableDomains.Add(new Domain(relevantTopBlocks[j - 1].Base.Max, relevantTopBlocks[j].Base.Min));
            }
            //add the domain after the last block
            availableDomains.Add(new Domain(relevantTopBlocks.Last().Base.Max, 2f));
            //Debug.Log($"available domains: {availableDomains.Count} layer {i}");

            //crossreference the available space with domain of the blocks in the layer underneath
            foreach (var block in _structure.Where(s => s.Layer == i - 1))
            {
                foreach (var domain in availableDomains)
                {
                    var intersection = GetIntersection(block.Base, domain);
                    if (intersection != null && intersection.Size > _tileSize.z + _gap)
                    {
                        block.AvailableSpace.AddDomain(intersection);
                        intersection.ConnectedBlock = block;
                        //Debug.Log($"AvailableSpaceFound: min {intersection.Min} max {intersection.Max} layer {i - 1}");
                    }
                }
            }
        }
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

    void GetBaseSurface()
    {

        foreach (var topBlock in _structure.Where(s => s.Layer > 0))
        {
            //reset connected bases & baseblocks
            topBlock.ConnectedBase = new BaseMass();
            topBlock.ConnectedBaseBlocks.Clear();
            //Debug.Log($"nr new blocks{newBlocks.Count}");


            foreach (var subBlock in _structure.Where(s => s.Layer == topBlock.Layer - 1))
            {
                Domain intersection = GetIntersection(topBlock.Base, subBlock.Base);
                if (intersection != null)
                {
                    topBlock.ConnectedBase.AddDomain(intersection);
                    topBlock.ConnectedBaseBlocks.Add(subBlock);
                    //Debug.Log($"Intersection: {intersection.Size}, Connected baseblocks {topBlock.ConnectedBaseBlocks.Count}");
                }
            }

        }

    }

    /// <summary>
    /// Returns a Vector3(biggestProblem, problemCenter, problemLayer)
    /// </summary>
    /// <returns></returns>
    Vector3 FindBiggestProblem()
    {
        //Debug.Log("FindBiggestProblem");
        float biggestProblem = 0;
        float problemCenter = 0;
        float problemLayer = 0;

        var OrientList = from block in _structure
                         where block.Layer > 0
                         select block.Orient;



        //IEnumerable<Domain> currentBase;
        var currentBase = (from block in _structure
                           where block.Layer == 1
                           select block.ConnectedBase.Bases).SelectMany(s => s).ToList();
        Debug.Log($"currentBase {currentBase.Count}");
        problemCenter = BaseMass.GetCenterAndWeight(currentBase).x;
        problemLayer = 1; // change this if i find the biggest problem
        biggestProblem = GetCenterOfMass(OrientList.ToList()).x - problemCenter;
        //Debug.Log(_structure.Where(s => s.Layer == _layers).Count());
        //Search for the center of mass farthest removed from center of base
        /*foreach (var topBlock in _structure.Where(s => s.Layer == _layers))
        {

            float center = topBlock.Orient.Center.x;
            float problem = topBlock.ConnectedBase.CenterWeight.x - center;
            if (firstBlock)
            {
                problemCenter = center;
                biggestProblem = problem;
                firstBlock = false;
            }
            List<Block> currentBlocks = new List<Block>();
            currentBlocks.Add(topBlock);
            currentBlocks.AddRange(topBlock.ConnectedBaseBlocks);

            for (int i = topBlock.Layer-1; i > 0; i--)
            {
                List<Orient> currentOrients = new List<Orient>();

                BaseMass currentBase = new BaseMass();
                foreach (var block in currentBlocks.Where(s => s.Layer == i))
                {
                    currentBase.Bases.AddRange(block.ConnectedBase.Bases);
                }

                Debug.Log($"nr of Current blocks {currentBlocks.Count} at layer: {i}");
                {
                    List<Block> connectedBaseBlocks = new List<Block>();
                    foreach (var block in currentBlocks)
                    {
                        currentOrients.Add(block.Orient);
                        connectedBaseBlocks.AddRange(block.ConnectedBaseBlocks);
                    }
                    currentBlocks.AddRange(connectedBaseBlocks.Distinct());
                }

                Vector3 centerOfMass = GetCenterOfMass(currentOrients);

                problem = centerOfMass.x - BaseMass.GetCenterAndWeight(currentBase.Bases).x;
                if (Abs(problem) > Abs(biggestProblem))
                {
                    biggestProblem = problem;
                    problemCenter = center;
                    problemLayer = i;
                }
            }
        }*/
        //if (biggestProblem < _minProblem) return Vector3.zero;
        Debug.Log($"The biggest problem is {biggestProblem}, problemcenter: {problemCenter}; problemLayer: {problemLayer}");
        return new Vector3(biggestProblem, problemCenter, problemLayer);
    }

    Orient GetNextOrient(Vector3 vecBiggestProblem)
    {
        Orient nextOrients = new Orient();
        //if (vecBiggestProblem == Vector3.zero) return nextOrients;
        float biggestProblem = vecBiggestProblem.x;
        float problemCenter = vecBiggestProblem.y;
        float problemLayer = vecBiggestProblem.z;

        // find the ultimate location to counterbalance the problem
        float bestLocation = problemCenter - biggestProblem;

        // find the top block closest to the center of mass of the biggest problem
        var availableSpaces = (from block in _structure
                               where block.Layer > 0
                               select (from dom in block.AvailableSpace.Bases
                                       select dom)).SelectMany(s => s).ToList();

        Debug.Log($"{availableSpaces.Count()} available spaces!");

        // check if the problem is within one of the available spaces
        var bestPlaces = (from dom in availableSpaces
                          where bestLocation > dom.Min && bestLocation < dom.Max
                          select dom);
        Domain bestPlace;
        if (bestPlaces.Count() > 0)
        {
            bestPlace = bestPlaces.First();
            Debug.Log("Perfect fit");
        }
        else
        {
            //order the available places by distance to the problem      
            availableSpaces.OrderBy(o => Mathf.Abs(o.Center - bestLocation));
            bestPlace = availableSpaces.First();
            Debug.Log("solution found");
        }

        //this should probably be integrated in the previous code...
        /*int newLayer = (from block in _structure
                        where (from dom in block.AvailableSpace.Bases
                               where dom == bestPlace
                               select dom).Count() > 0
                        select block).First().Layer + 1;*/
        int newLayer = bestPlace.ConnectedBlock.Layer + 2;
        Debug.Log($"layer {newLayer}");



        if (biggestProblem > 0) bestLocation = bestPlace.Min + _tileSize.z / 2 + _gap;
        else bestLocation = bestPlace.Max - _tileSize.z / 2 - _gap;


        nextOrients = new Orient(bestLocation,newLayer * 0.045f, 0.3f + 0.02f* _timesLooped, 90);
        _structure.Add(new Block(nextOrients, _tileSize, true));
        //Debug.Log($"Total amount of blocks {_structure.Count}");


        return nextOrients;
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

    /*Vector3 GetCenterOfMass(Orient orient)
    {
        Vector3 centerOfMass = orient.Center;
        centerOfMass.y -= _tileSize.y / 2;
        return centerOfMass;
    }*/

    List<Orient> PlacedTiles()
    {
        List<Orient> placedTiles = new List<Orient>();
        foreach (var block in _structure)
        {
            placedTiles.Add(block.Orient);
        }
        return placedTiles;
    }



    class SimulatedCamera : ICamera
    {
        Queue<Orient[]> _sequence;
        List<Orient> _topLayer = new List<Orient>
        {
           new Orient(0.8f,0.045f,0.3f,0),
           new Orient(1.05f,0.045f,0.3f,0),
           new Orient(0.9f,0.09f,0.3f,0),
           new Orient(1f,0.135f,0.3f,0),
           new Orient(0.8f,0.135f,0.3f,0)/*,
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