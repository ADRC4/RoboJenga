using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class MRSS_VirtualCamera : ICamera
{
    Queue<Pose[]> _pickSequence;
    Queue<Pose[]> _targetSequence;
    Queue<Pose[]> _SourceSequence;

    public MRSS_VirtualCamera()
    {
        var t = new[]
        {
           Extensions.PoseFromRotation(1.3f,0.135f,0.4f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.135f,0.7f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.135f,0.6f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.135f,0.2f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.09f,0.4f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.09f,0.7f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.09f,0.6f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.09f,0.2f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.045f,0.4f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.045f,0.7f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.045f,0.6f,0.0f),
           Extensions.PoseFromRotation(1.3f,0.045f,0.2f,0.0f)

        };

        var u = new[]
    {

           Extensions.PoseFromRotation(1.0f,0.045f,0.4f,90.0f),

        };

        var v = new[]
       {

           Extensions.PoseFromRotation(0.2f,0.27f,0.4f,90.0f),

        };

        _pickSequence = new Queue<Pose[]>(new[]
        {

           new[] {t[0],t[1],t[2],t[3],t[4],t[5],t[6],t[7],t[8],t[9],t[10]},
           new[] {t[1],t[2],t[3],t[4],t[5],t[6],t[7],t[8],t[9],t[10]},
           new[] {t[2],t[3],t[4],t[5],t[6],t[7],t[8],t[9],t[10]},
           new[] {t[3],t[4],t[5],t[6],t[7],t[8],t[9],t[10]},
           new[] {t[4],t[5],t[6],t[7],t[8],t[9],t[10]},
           new[] {t[5],t[6],t[7],t[8],t[9],t[10]},
           new[] {t[6],t[7],t[8],t[9],t[10]},
           new[] {t[7],t[8],t[9],t[10]},
           new[] {t[8],t[9],t[10]},
           new[] {t[9],t[10]},
           new[] {t[10]},

           new Pose[0]

        });

        _targetSequence = new Queue<Pose[]>(new[]
        {
            new[] {v[0]},
            new[] {v[0]},
            new[] {v[0]},
            new[] {v[0]},
            new[] {v[0]},
            new[] {v[0]},
            new[] {v[0]},
            new[] {v[0]},
            new[] {v[0]},
            new[] {v[0]},
            new[] {v[0]}
        });

        _SourceSequence = new Queue<Pose[]>(new[]
  {
          new[] {u[0]},
          new[] {u[0]},
          new[] {u[0]},
          new[] {u[0]},
          new[] {u[0]},
          new[] {u[0]},
          new[] {u[0]},
          new[] {u[0]},
          new[] {u[0]},
          new[] {u[0]},
          new[] {u[0]},

        });

    }

    public IList<Pose> GetTiles(Rect area)
    {
        if (area.x >= 1.1f)
            return _pickSequence.Dequeue();

        else if (area.x < 1.1f && area.x > 0.6f)
            return _SourceSequence.Dequeue();
        else if (area.x <= 0.6f)
            return _targetSequence.Dequeue();
        else return null;

    }
}
