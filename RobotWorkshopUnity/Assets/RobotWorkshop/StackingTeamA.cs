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

        public Block(Orient orient, Vector3 size)
        {
            Orient = orient;
            Base = new Domain(Orient.Center.x - size.x / 2, Orient.Center.x + size.x / 2);
        }
    }

    private class Domain
    {
        public float Min, Max;
        public float Center, Size;

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

    //GLOBAL FIELDS --------------------------------------------------------------------
    public string Message { get; private set; }
    public IEnumerable<Orient> Display { get { return _structure.Select(x => x.Orient); } }

    readonly Vector3 _placePoint = new Vector3(0.9f, 0, 0.4f);
    //Block Dimesions: 180mm, 60mm, 45mm
    public readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    readonly Rect _rectStack, _rectConstruction;
    readonly float _gap = 0.002f;
    readonly ICamera _camera;
    readonly float _tolerance = 0.01f;

    Mode _mode;

    List<Block> _structure;

    bool _human = true, _wait = true;
    int _numberOfLayers;
    float _gripperSpace = 0.03f;

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
        if (_wait)
        {
            _wait = false;
            Message = "Waiting for human interaction";
            return null;
        }
        if (_human)
        {
            if (!ScanForNewTiles()) return null;
            _human = false;
            return (GetNextOrient());
        }
        else return (GetNextOrient());
    }

    bool ScanForNewTiles()
    {
        var newTiles = ScanConstruction(_rectConstruction);
        if (newTiles.Count == 0)
        {
            Debug.Log("NO TILES SCANNED");
            return false;
        }

        //Make sure that there are no duplicates
        //get the layer of the scanned elements
        int scannedLayer = (int)((newTiles.First().Center.y - (_tileSize.y / 2)) / _tileSize.y);
        if (scannedLayer == _numberOfLayers && _mode == Mode.Live)
        {
            //remove the top layer
            var topLayer = _structure.Where(s => s.Layer == _numberOfLayers);
            foreach (var block in topLayer)
            {
                _structure.Remove(block);
            }
        }

        //Assign the scanned blocks to _structure
        _structure.AddRange(newTiles.Select(x => new Block(x, _tileSize)));
        Debug.Log($"{_structure.Count} blocks in structure");

        return true;
    }

    PickAndPlaceData GetNextOrient()
    {
        var topLayer = _structure.Where(s => s.Layer == _numberOfLayers).ToList();
        PickAndPlaceData nextPickAndPlace = new PickAndPlaceData();

        //order list ascending (small X to large X)
        topLayer = topLayer.OrderBy(o => o.Base.Center).ToList(); //checked and worked

        //check the distance between the two blocks
        if (topLayer.Count() == 2)
        {
            float distance = Mathf.Abs(topLayer.First().Base.Center - topLayer.Last().Base.Center) - _tileSize.x;

            if (distance > _tileSize.x / 2 - 2 * _tolerance && distance < _tileSize.x + 2 * _gripperSpace)
            {
                Message = "Scenario A";
                //Scenario A: check if the two blocks are too far away
                Orient place = new Orient(topLayer.First().Orient.Center.x + _gripperSpace + _tileSize.x, topLayer.First().Orient.Center.y, topLayer.First().Orient.Center.z, 0);
                nextPickAndPlace = new PickAndPlaceData { Pick = topLayer.Last().Orient, Place = place, Retract = true };
                _structure.Remove(topLayer.Last());
                _structure.Add(new Block(place, _tileSize));
                Debug.Log("A: Too far");
            }
            else if (distance > 2 * _tileSize.x - 2 * _tolerance)
            {
                Message = "Scenario B";
                //Scenario B: check if the blocks are too close to eachother to fit another block, but too far to use the topspace
                Orient place = new Orient(topLayer.First().Orient.Center.x + _gripperSpace + _tileSize.x, topLayer.First().Orient.Center.y, topLayer.First().Orient.Center.z, 0);
                nextPickAndPlace = new PickAndPlaceData { Pick = topLayer.Last().Orient, Place = place, Retract = true };
                _structure.Remove(topLayer.Last());
                _structure.Add(new Block(place, _tileSize));
                Debug.Log("B: Too close");
            }
            else if (distance > _tileSize.x)
            {
                Message = "Scenario C";
                //Scenario C: place the block in the middle of the other blocks
                Debug.Log("C: Perfect fit between Blocks");

                // Get a new block from the stack
                var availableStack = ScanConstruction(_rectStack);
                //check if it actually scanned blocks
                if (availableStack.Count() == 0)
                {
                    Debug.Log("Camera error when retrieving blocks from stack");
                    return null;
                }
                var pick = availableStack.First();

                //set the next block in the center of the given blocks
                var center = topLayer.First().Base.Center + (topLayer.Last().Base.Center - topLayer.First().Base.Center) / 2;
                var place = new Orient(center, topLayer.First().Orient.Center.y, topLayer.First().Orient.Center.z, 0);
                _structure.Add(new Block(place, _tileSize));

                nextPickAndPlace = new PickAndPlaceData { Pick = pick, Place = place };
            }
            else
            {
                //Scenario D: put a block on top (2 blocks base)
                nextPickAndPlace = BlockOnTop(topLayer);
            }
        }
        else if (topLayer.Count() == 3)
        {
            //Scenario D: put a block on top (3 blocks base)
            nextPickAndPlace = BlockOnTop(topLayer);
        }

        return nextPickAndPlace;
    }

    /// <summary>
    /// Requires the list of topblocks, sorted ascending  (from small X to big X)
    /// </summary>
    PickAndPlaceData BlockOnTop(List<Block> topLayer)
    {
        Message = "Scenario D";
        //now wait for human interaction
        _wait = true;
        _human = true;

        PickAndPlaceData nextPickAndPlace;
        //Scenario D: place the block in the middle of the other blocks
        Debug.Log("D: Placing a block on top");

        // Get a new block from the stack
        var availableStack = ScanConstruction(_rectStack);
        //check if it actually scanned blocks
        if (availableStack.Count() == 0)
        {
            Debug.Log("Camera error when retrieving blocks from stack");
            return null;
        }
        var pick = availableStack.First();

        //find the domain betwee the minimum and the maximum building areas of the toplayer
        Domain availableDomain = new Domain(topLayer.First().Base.Min + _tolerance, topLayer.Last().Base.Max - _tolerance);

        //Select a random location within the domain
        float placeX = Random.Range(availableDomain.Min * 1.00f, availableDomain.Max * 1.00f);

        //set the height Y one layer up + extra tollerance 
        float placeY = topLayer.First().Orient.Center.y + _tileSize.y + _gap;
        float placeZ = topLayer.First().Orient.Center.z;

        Orient place = new Orient(placeX, placeY, placeZ, 0);

        //add block to structure
        _structure.Add(new Block(place, _tileSize));

        nextPickAndPlace = new PickAndPlaceData { Pick = pick, Place = place };

        return nextPickAndPlace;
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

    class SimulatedCamera : ICamera
    {
        Queue<Orient[]> _sequence;
        List<Orient> _topLayer = new List<Orient>
        {
            //test Scenario A
            
            new Orient(0.98f,0.045f,0.3f,0),
            new Orient(0.7f,0.045f,0.3f,0)

            //test scenario B

            //test scenario C
            /*
            new Orient(1.15f,0.045f,0.3f,0),
            new Orient(0.7f,0.045f,0.3f,0)
            */
            //test scenario D
            /*
            new Orient(0.9f,0.045f,0.3f,0),
           new Orient(0.7f,0.045f,0.3f,0)
           */
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