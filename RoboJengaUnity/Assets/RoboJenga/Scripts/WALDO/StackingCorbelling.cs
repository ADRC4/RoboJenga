using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class StackingCorbelling : IStackable
{
    public string Message { get; private set; }
    public ICollection<Pose> Display { get; } = new List<Pose>();

    readonly Rect _rectPlace;
    readonly Rect _rectBuilders;
    readonly ICamera _camera;
    Pose pose0;
    Pose pose1;
    Pose pose2;
    Pose pose3;
    Pose pose4;
    Pose pose5;
    Pose pose6;
    Pose pose7;
    Pose pose8;
    Pose pose9;
    Pose pose10;
    Pose pose11;
    Pose pose12;
    Pose pose13;
    Pose pose14;
    Pose pose15;
    Pose pose16;
    Pose pose17;
    Pose pose18;
    Pose pose19;
    Pose pose20;
    Pose pose21;
    Pose pose22;
    Pose poseBlock1;
    Pose poseBlock2;
    IList<Pose> PlacePositions = new List<Pose>();
    IList<Pose> TwoPositions = null;

    Queue<Pose> TargetPose;

    public StackingCorbelling(Mode mode)
    {
        Message = "Simple vision stacking.";
        float m = 0.02f; //margin from the edge of the table
        _rectPlace = new Rect(0.4f, 0 + m, 1.0f - m * 2, 0.8f - m * 2); //place all blocks
        _rectBuilders = new Rect(0 + m, 0 + m, 0.4f - m * 2, 0.8f - m * 2); //place two blocks
                                                                            // TargetPose = PositionTarget();

        if (mode == Mode.Virtual)
            _camera = new VirtualCameraWaldo();
        else
            _camera = new MotiveCamera();
    }

    public PickAndPlaceData GetNextTargets()
    {
        if (TwoPositions == null)
        {
            TwoPositions = new List<Pose>();
            var topLayerTwo = _camera.GetTiles(_rectPlace);

            if (topLayerTwo == null)
            {
                Message = "Camera error.";
                return null;
            }

            if (topLayerTwo.Count != 2)
            {
                Message = "Tiles should be two.";
                return null;
            }

            TwoPositions = topLayerTwo;
            TargetPose = PositionTarget();
        }

        // camera
        var topLayerBuild = _camera.GetTiles(_rectBuilders);

        if (topLayerBuild == null)
        {
            Message = "Camera error.";
            return null;
        }

        if (topLayerBuild.Count == 0)
        {
            Message = "No more tiles.";
            return null;
        }

        var pick = topLayerBuild.First();

        // place

        if (TargetPose.Count == 0)
        {
            Message = "Bridge Completed!.";
            return null;
        }

        var place = TargetPose.Dequeue();

        Display.Add(place);

        return new PickAndPlaceData { Pick = pick, Place = place };
    }



    public Queue<Pose> PositionTarget()
    {
        var point2 = TwoPositions[0].position;
        var point1 = TwoPositions[1].position;
        //Vector3 point1 = new Vector3(1.3f, 0.045f, 0.1f);
        //Vector3 point2 = new Vector3(0.4f, 0.045f, 0.7f);
        Vector3 vector2Blocks = point1 - point2;
        var rotationVector2Blocks = Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(vector2Blocks);
        poseBlock1 = new Pose(point1, rotationVector2Blocks);
        poseBlock2 = new Pose(point2, rotationVector2Blocks);
        IList<Pose> TwoBlocks = new List<Pose>();
        TwoBlocks.Add(poseBlock1);
        TwoBlocks.Add(poseBlock2);
        Display.Add(poseBlock1);
        Display.Add(poseBlock2);
        var margin = .03f;
        var blockSize = .18f;
        var blockPlusMargin = margin + blockSize;
        int layers = 5;
        Vector3 normalized = vector2Blocks.normalized * blockPlusMargin;
        var offsetSize = ((((vector2Blocks.magnitude - blockPlusMargin) / 2) - blockPlusMargin) / (layers - 1));
        Vector3 offset = vector2Blocks.normalized * offsetSize;

        //var place0 = point1 - (normalized);
        var place1 = point1 - (normalized);
        var place2 = point2 + (normalized);
        var place3 = point1 - (offset);
        place3.y += 0.045f;
        var place4 = point2 + (offset);
        place4.y += 0.045f;
        var place5 = place3 - (normalized);
        var place6 = place4 + (normalized);
        var place7 = place3 - (offset);
        place7.y += 0.045f;
        var place8 = place4 + (offset);
        place8.y += 0.045f;
        var place9 = place7 - (normalized);
        var place10 = place8 + (normalized);
        var place11 = place7 - (offset);
        place11.y += 0.045f;
        var place12 = place8 + (offset);
        place12.y += 0.045f;
        var place13 = place11 - (normalized);
        var place14 = place12 + (normalized);
        var place15 = place11 - (offset);
        place15.y += 0.045f;
        var place16 = place12 + (offset);
        place16.y += 0.045f;
        var place17 = place15 - (normalized);
        var place18 = place16 + (normalized);
        var place19 = place15 - (offset);
        place19.y += 0.045f;
        var place20 = place16 + (offset);
        place20.y += 0.045f;
        var place21 = place19 - (normalized);
        var place22 = place20 + (normalized);



        //pose0 = new Pose(place0, rotationVector2Blocks);
        pose1 = new Pose(place1, rotationVector2Blocks);
        pose2 = new Pose(place2, rotationVector2Blocks);
        pose3 = new Pose(place3, rotationVector2Blocks);
        pose4 = new Pose(place4, rotationVector2Blocks);
        pose5 = new Pose(place5, rotationVector2Blocks);
        pose6 = new Pose(place6, rotationVector2Blocks);
        pose7 = new Pose(place7, rotationVector2Blocks);
        pose8 = new Pose(place8, rotationVector2Blocks);
        pose9 = new Pose(place9, rotationVector2Blocks);
        pose10 = new Pose(place10, rotationVector2Blocks);
        pose11 = new Pose(place11, rotationVector2Blocks);
        pose12 = new Pose(place12, rotationVector2Blocks);
        pose13 = new Pose(place13, rotationVector2Blocks);
        pose14 = new Pose(place14, rotationVector2Blocks);
        pose15 = new Pose(place15, rotationVector2Blocks);
        pose16 = new Pose(place16, rotationVector2Blocks);
        pose17 = new Pose(place17, rotationVector2Blocks);
        pose18 = new Pose(place18, rotationVector2Blocks);
        pose19 = new Pose(place19, rotationVector2Blocks);
        pose20 = new Pose(place20, rotationVector2Blocks);
        pose21 = new Pose(place21, rotationVector2Blocks);
        pose22 = new Pose(place22, rotationVector2Blocks);



        PlacePositions.Add(pose1);
        PlacePositions.Add(pose2);
        PlacePositions.Add(pose3);
        PlacePositions.Add(pose4);
        PlacePositions.Add(pose5);
        PlacePositions.Add(pose6);
        PlacePositions.Add(pose7);
        PlacePositions.Add(pose8);
        PlacePositions.Add(pose9);
        PlacePositions.Add(pose10);
        PlacePositions.Add(pose11);
        PlacePositions.Add(pose12);
        PlacePositions.Add(pose13);
        PlacePositions.Add(pose14);
        PlacePositions.Add(pose15);
        PlacePositions.Add(pose16);
        PlacePositions.Add(pose17);
        PlacePositions.Add(pose18);
        PlacePositions.Add(pose19);
        PlacePositions.Add(pose20);
        PlacePositions.Add(pose21);
        PlacePositions.Add(pose22);


        Debug.Log(PlacePositions.Count);


        //return PlacePositions;
        return new Queue<Pose>(PlacePositions);
    }






}