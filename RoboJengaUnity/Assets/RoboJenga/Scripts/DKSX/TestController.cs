using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    //readonly Vector3 _placePoint = new Vector3(0.4f, 0, 0.4f);
    readonly Vector3 _placePoint = new Vector3(0f, 0, 0f);
    readonly Vector3 _tileSize = new Vector3(0.18f, 0.045f, 0.06f);
    readonly float _gap = 0.01f;
    [SerializeField] Material mat;

    void Start()
    {
        var poses = TXTReader.GetPoses("DKSX/testData");
        foreach (var item in poses)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = item.position;
            c.transform.localScale = _tileSize;
            c.transform.rotation = item.rotation;
            c.GetComponent<MeshRenderer>().material = mat;
        }

        //normalization test [working fine!]
        //var minX = poses.Min(p => p.position.x);
        //var minZ = poses.Min(p => p.position.z);
        //var towerNormalized = poses.Select(p =>
        //{
        //    var px = p.position.x - minX;
        //    var py = p.position.y;
        //    var pz = p.position.z - minZ;
        //    var normalizedPosition = new Vector3(px, py, pz);
        //    return new Pose(normalizedPosition, p.rotation);
        //});
        //foreach (var item in towerNormalized)
        //{
        //    print($"{item.position.x},{item.position.y},{item.position.z}");
        //}

        var normalPoses = GenerateVoid(poses);
        foreach (var item in normalPoses)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = item.position;
            c.transform.localScale = new Vector3(0.18f, 0.045f, 0.06f);
            c.transform.rotation = item.rotation;
            c.GetComponent<MeshRenderer>().material = mat;
        }


        //var tower = JengaTower(5);
        //foreach (var item in tower)
        //{
        //    var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    c.transform.position = item.position;
        //    c.transform.localScale = _tileSize;
        //    c.transform.rotation = item.rotation;
        //    c.GetComponent<MeshRenderer>().material = mat;
        //}
    }

    List<Pose> GenerateVoid(IList<Pose> input)
    {
        //Sorting the input list in subsets of Y (up)

        var minX = input.Min(p => p.position.x);
        var minZ = input.First(p => p.position.x == minX).position.z - (_tileSize.x/2);


        var towerNormalized = input.Select(p =>
        {
            var px = p.position.x - minX;
            var py = p.position.y;
            var pz = p.position.z - minZ;
            var normalizedPosition = new Vector3(px, py, pz);
            return new Pose(normalizedPosition, p.rotation);
        });

        var ySortedTower = towerNormalized.GroupBy(p => p.position.y)
            .Select(group => group.ToList()).ToList();

        int layerCount = ySortedTower.Count;

        //Boolean list of outcomes
        bool[,] hasBlock = new bool[layerCount, 3];

        //Analyze each layer
        for (int i = 0; i < layerCount; i++)
        {
            var layer = ySortedTower[i];

            //print(layer[0].rotation.eulerAngles.y);
            if (layer[0].rotation.eulerAngles.y >= 85f && layer[0].rotation.eulerAngles.y <= 95f)
            {

                var xSrotedLayer = layer.OrderBy(l => l.position.x);
                foreach (var _pose in xSrotedLayer)
                {
                    print(_pose.position.x);
                    if (_pose.position.x >= -0.01f && _pose.position.x <= 0.01f)
                    {
                        Debug.Log($"Found 0 on layer {i}");
                        hasBlock[i, 0] = true;
                    }
                    else if (_pose.position.x >= _tileSize.z && _pose.position.x < _tileSize.z * 2)
                    {
                        Debug.Log($"Found 1 on layer {i}");
                        hasBlock[i, 1] = true;
                    }
                    else if (_pose.position.x >= _tileSize.z * 2)
                    {
                        Debug.Log($"Found 2 on layer {i}");
                        hasBlock[i, 2] = true;
                    }
                }
            }
            else if (layer[0].rotation.eulerAngles.y >= -5f && layer[0].rotation.eulerAngles.y <= 5f)
            {
                var zSrotedLayer = layer.OrderBy(l => l.position.z);
                foreach (var _pose in zSrotedLayer)
                {
                    print(_pose.position.z);
                    if (_pose.position.z >= -0.01f && _pose.position.z <= 0.01f)
                    {
                        Debug.Log($"Found 2 on layer {i}");
                        hasBlock[i, 2] = true;
                    }
                    else if (_pose.position.z >= _tileSize.z && _pose.position.z < _tileSize.z * 2)
                    {
                        Debug.Log($"Found 1 on layer {i}");
                        hasBlock[i, 1] = true;
                    }
                    else if (_pose.position.z >= _tileSize.z * 2)
                    {
                        Debug.Log($"Found 0 on layer {i}");
                        hasBlock[i, 0] = true;
                    }
                }
            }
            
        }
        var referenceTower = JengaTower(layerCount);
        
        //Create new list of normalized void mirror poses
        List<Pose> vmPoses = new List<Pose>();

        for (int i = 0; i < layerCount; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (!hasBlock[i, j])
                {
                    //Debug.Log($"Block {i},{j} created");
                    vmPoses.Add(referenceTower[i, j]);
                }
            }
        }
        vmPoses.Reverse();
        return vmPoses;

    }

    Pose[,] JengaTower(int layerCount)
    {
        Pose[,] tower = new Pose[layerCount, 3];
        int count = layerCount;
        for (int i = 0; i < count; i++)
        {
            bool isEven = i % 2 == 0;
            for (int j = 0; j < 3; j++)
            {
                float yPos = (i) * _tileSize.y + _tileSize.y / 2;
                float zPos = (j - 1) * (_tileSize.z + _gap);

                Vector3 position = new Vector3(0, yPos, zPos);
                var rotation = Quaternion.Euler(0, !isEven ? 0 : -90, 0) * Quaternion.Euler(0, 180f, 0);
                tower[(count-1) - i, j] = new Pose(rotation * position + _placePoint, rotation);

            }
        }

        return tower;
    }
}
