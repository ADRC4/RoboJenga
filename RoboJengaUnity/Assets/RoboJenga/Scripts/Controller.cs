using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Mathf;
using System.Threading.Tasks;

public class Controller : MonoBehaviour
{
    public string IP => _mode == Mode.Virtual ? "127.0.0.1" : "192.168.0.3";

    [SerializeField] Mesh _tile = null;
    [SerializeField] GUISkin _skin = null;

    Material _tileMaterial;
    Material _pickMaterial;
    Material _placeMaterial;
    Material _robotMaterial;

    Server _server;
    IStackable _stackable;

    Robot _robot;
    Client _client;

    PickAndPlaceData _currentData;
    bool _looping = false;
    bool _robotAwaiting = false;
    Mode _mode;

    int _stackableIndex;
    string[] _stackableNames;
    string _robotMessage = "Press connect.";

    void OnGUI()
    {
        GUI.skin = _skin;

        GUILayout.BeginArea(new Rect(16, 16, Screen.width, Screen.height));
        GUILayout.BeginVertical();

        if (_stackableNames == null)
            _stackableNames = GetStackables()
                .Select(s => s.Name)
                .ToArray();

        _stackableIndex = GUILayout.SelectionGrid(_stackableIndex, _stackableNames, 6);
        _mode = (Mode)GUILayout.SelectionGrid((int)_mode, Enum.GetNames(typeof(Mode)), 2);

        if (_server == null)
        {
            if (GUILayout.Button("Connect"))
                ConnectToRobot();
        }
        else if (_server.Connected)
        {
            if (_looping)
            {
                if (GUILayout.Button("Stop loop"))
                    StopLoop();
            }
            else
            {
                if (GUILayout.Button("Start loop"))
                    StartLoop();
            }
        }

        GUILayout.Label($"<b>Robot:</b> {_robotMessage}");
        GUILayout.Label($"<b>Stacking:</b> {_stackable?.Message}");
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void Update()
    {
        if (_stackable?.Display != null)
        {
            foreach (var pose in _stackable.Display.Take(_stackable.Display.Count - 1))
                DrawTile(pose, _tileMaterial);
        }

        if (_currentData != null)
        {
            DrawTile(_currentData.Pick, _pickMaterial);
            DrawTile(_currentData.Place, _placeMaterial);
        }

        if (_client != null)
        {
            var angles = _client.Read();
            _robot.DrawRobot(angles, _robotMaterial);
        }
    }

    void OnApplicationQuit()
    {
        _server?.Dispose();
        _client?.Dispose();
    }

    IEnumerable<Type> GetStackables()
    {
        var type = typeof(IStackable);
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => type.IsAssignableFrom(t) && t.IsClass);

        return types;
    }

    async void ConnectToRobot()
    {
        Initialize();

        _robotMessage = "Waiting for robot to connect...";

        _server = new Server();

        await _server.ConnectAsync(IP, 1025);

        if (_server.Connected)
        {
            if (_mode == Mode.Virtual)
                await ClientAsync();
            _robotMessage = "Robot connected.";
        }
        else
        {
            _robotMessage = "Robot connection error.";
            _server.Dispose();
            _server = null;
        }
    }

    async Task ClientAsync()
    {
        _client = new Client();
        await _client.ConnectAsync(IP, 1026);
    }

    void Initialize()
    {
        // robot 
        //var origin = new[] { 500.900f, 678.200f, 192.100f, -0.6991f, 0.0012f, 0.0028f, 0.7150f };
        var origin = new[] { 555.500f, 644.000f, 194.400f, 0.69813f, -0.00181f, 0.00000f, -0.71597f }; //This was copied from RobotStudio's updated AutoPickAndPlace
        _robot = Robot.IRB1600(origin);

        // materials
        _tileMaterial = Resources.Load<Material>("TileMaterial");
        _robotMaterial = Resources.Load<Material>("RobotMaterial");
        _pickMaterial = Resources.Load<Material>("PickMaterial");
        _placeMaterial = Resources.Load<Material>("PlaceMaterial");

        // stackable
        var stackableType = GetStackables().ElementAt(_stackableIndex);
        _stackable = Activator.CreateInstance(stackableType, _mode) as IStackable;
    }

    async void StartLoop()
    {
        _robotMessage = "Robot loop started.";
        _looping = true;

        while (_looping)
        {
            if (!_robotAwaiting)
                _robotAwaiting = await _server.ReadAsync() == 1;

            if (!_looping)
                return;

            if (_robotAwaiting)
            {
                _currentData = _stackable.GetNextTargets();

                if (_currentData == null || _currentData.StopLoop)
                {
                    StopLoop();
                    return;
                }

                await _server.SendTargetsAsync(
                    _currentData.Retract ? 2 : 1,
                    BestGrip(_currentData.Pick),
                    BestGrip(_currentData.Place)
                    );

                _robotAwaiting = false;
            }
        }
    }

    void StopLoop()
    {
        _robotMessage = "Robot loop stopped.";
        _looping = false;
    }

    Pose BestGrip(Pose pose)
    {
        float robotPos = 0.7025f;
        var left = new Vector3(1, 0, 1);
        var right = new Vector3(1, 0, -1);
        var bestPos = pose.position.x < robotPos ? left : right;

        var angle = Extensions.GetAngle(pose.rotation * Vector3.right, bestPos);
        if (Abs(angle) > 90f) pose.rotation *= Quaternion.Euler(0, 180f, 0);

        return pose;
    }

    void DrawTile(Pose orient, Material material)
    {
        Graphics.DrawMesh(_tile, orient.position, orient.rotation, material, 0);
    }
}