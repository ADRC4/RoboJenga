using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

class Server : IDisposable
{
    public bool Connected { get { return _client != null && _client.Connected; } }
    TcpListener _server;
    TcpClient _client;

    public Server()
    { }

    public async Task ConnectAsync(string ip, int port)
    {
        try
        {
            _server = new TcpListener(IPAddress.Parse(ip), port);
            _server.Start();
            _client = await _server.AcceptTcpClientAsync();

            Log($"Connected to - {_client.Client.RemoteEndPoint}");
        }
        catch (SocketException e)
        {
            Log($"SocketException - {e}");
        }

        _server.Server.LingerState = new LingerOption(true, 60);
    }

    public async Task SendTargetsAsync(int info, Pose pick, Pose place)
    {
        var floats = new List<float>(14);
        floats.AddRange(pick.ToFloats());
        floats.AddRange(place.ToFloats());

        var bytes = new List<byte>(15 * 4);

        bytes.AddRange(BitConverter.GetBytes(info));

        foreach (float number in floats)
            bytes.AddRange(BitConverter.GetBytes(number));

        await SendAsync(bytes.ToArray());
    }

    public async Task<int> ReadAsync()
    {
        byte[] bytes = new byte[4];

        if (!Connected)
        {
            Log("Can't receive data, not connected.");
            return -1;
        }

        var stream = _client.GetStream();

        do
        {
            await stream.ReadAsync(bytes, 0, bytes.Length);
        }
        while (stream.DataAvailable);

        var info = BitConverter.ToInt32(bytes, 0);
        Log($"Info no. {info}");
        return info;
    }

    async Task SendAsync(byte[] bytes)
    {
        if (!Connected)
        {
            Log("Can't send data, not connected.");
            return;
        }

        var stream = _client.GetStream();
        await stream.WriteAsync(bytes, 0, bytes.Length);
    }

    void Log(string text)
    {
        Debug.Log($"Server: {text}");
    }

    public void Dispose()
    {
        if (_client == null) return;

        _client.Close();
        _client.Dispose();
        _server.Stop();        
        Debug.Log("Disconnected.");
    }
}