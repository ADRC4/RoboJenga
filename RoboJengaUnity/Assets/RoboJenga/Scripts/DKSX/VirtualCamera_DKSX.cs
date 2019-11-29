using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class VirtualCamera_DKSX : ICamera
{
    Queue<Pose[]> _sequence;

    public VirtualCamera_DKSX()
    {
        _sequence = TXTReader.GetPosesQueue("DKSX/testData");
   
    }

    public IList<Pose> GetTiles(Rect area)
    {
        return _sequence.Dequeue();
    }
}

