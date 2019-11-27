using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class VirtualCamera_WALDO : ICamera
{
    Queue<Pose[]> _sequence;

    public VirtualCamera_WALDO()
    {
        //_sequence = TXTReader.GetPosesQueue("WALDO/testData_W");

    }

    public IList<Pose> GetTiles(Rect area)
    {
        return _sequence.Dequeue();
    }
}

