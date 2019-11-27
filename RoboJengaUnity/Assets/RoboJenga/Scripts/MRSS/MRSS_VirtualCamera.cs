using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class MRSS_VirtualCamera : ICamera
{
    Queue<Pose[]> _pickSequence;
    Queue<Pose[]> _placeSequence;

    public MRSS_VirtualCamera()
    {
        var t = new[]
        {
           Extensions.PoseFromRotation(0.1f,0.045f,0.4f,90.0f),
           Extensions.PoseFromRotation(0.1f,0.045f,0.7f,90.0f),
           Extensions.PoseFromRotation(0.1f,0.045f,0.6f,90.0f),
           Extensions.PoseFromRotation(0.1f,0.045f,0.2f,90.0f),
        
        };

        var v = new[]
       {
          
           Extensions.PoseFromRotation(0.8f,0.045f,0.3f,90.0f),
         
        };

        _pickSequence = new Queue<Pose[]>(new[]
        {
            new[] {v[0]},
            new[] {v[0]},new[] {v[0]},new[] {v[0]},new[] {v[0]},new[] {v[0]}


        });

        _placeSequence = new Queue<Pose[]>(new[]
        {
           

            new[] {t[0],t[1],t[2]},
           new[] {t[1],t[2]},
           new[] {t[2]},
           new Pose[0]
        });


    }

    public IList<Pose> GetTiles(Rect area)
    {
        if (area.x <= 0.8f)
            return _pickSequence.Dequeue();

        else  return _placeSequence.Dequeue();

       
    }
}
