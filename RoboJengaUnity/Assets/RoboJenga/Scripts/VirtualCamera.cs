using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class VirtualCamera : ICamera
{
    Queue<Pose[]> _sequence;

    public VirtualCamera()
    {        
        var t = new[]
        {
           Extensions.PoseFromRotation(0.1f,0.045f,0.3f,90.0f),
           Extensions.PoseFromRotation(0.2f,0.045f,0.1f,30.0f),
           Extensions.PoseFromRotation(0.4f,0.045f,0.3f,45.0f)
        };

        _sequence = new Queue<Pose[]>(new[]
        {
           new[] {t[0],t[1],t[2]},
           new[] {t[1],t[2]},
           new[] {t[2]},
           new Pose[0]
        });
    }

    public IList<Pose> GetTiles(Rect area)
    {
        return _sequence.Dequeue();
    }
}

