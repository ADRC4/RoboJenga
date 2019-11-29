using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class VirtualCameraZZOF : ICamera
{
    Queue<Pose[]> _sequence;

    public VirtualCameraZZOF()
    {        
        var t = new[]
        {
           Extensions.PoseFromRotation(0.35f,0.045f,0.4f,0.0f),
           Extensions.PoseFromRotation(0.35f,0.045f,0.4f+0.07f,0.0f),
           Extensions.PoseFromRotation(0.35f-0.07f*0.5f,0.045f*2,0.4f,90.0f),
           Extensions.PoseFromRotation(0.35f+0.07f*0.5f,0.045f*2,0.4f,90.0f),
           Extensions.PoseFromRotation(0.35f,0.045f*3,0.4f,0.0f),
           Extensions.PoseFromRotation(0.35f,0.045f*3,0.4f+0.07f,0.0f),
        };

        _sequence = new Queue<Pose[]>(new[]
        {
           new[] {t[4],t[5]},
           new[] {t[5]},
           new[] {t[2],t[3]},
           new[] {t[3]},
           new[] {t[0],t[1]},
           new[] {t[1]},
           new Pose[0]
        });
    }

    public IList<Pose> GetTiles(Rect area)
    {
        return _sequence.Dequeue();
    }
}

