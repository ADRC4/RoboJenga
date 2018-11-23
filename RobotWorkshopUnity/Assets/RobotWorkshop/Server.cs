using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

class Server : IDisposable
{
    public bool Connected { get { return _client != null && _client.Connected; } }
    TcpListener _server;
    TcpClient _client;

    public Server(string ip, int port)
    {
        Connect(ip, port);
    }

    void Connect(string ip, int port)
    {
        try
        {
            _server = new TcpListener(IPAddress.Parse(ip), port);
            _server.Start();
            _client = _server.AcceptTcpClient();
            Debug.Log($"Connected to: {_client.Client.RemoteEndPoint}");
        }
        catch (SocketException e)
        {
            Debug.Log($"SocketException: {e}");
        }

        _server.Server.LingerState = new LingerOption(true, 60);
    }

    public int Read()
    {
        byte[] bytes = new byte[4];

        if (!Connected)
        {
            Debug.Log("Can't receive data, not connected.");
            return -1;
        }

        var stream = _client.GetStream();

        do
        {
            stream.Read(bytes, 0, bytes.Length);
        }
        while (stream.DataAvailable);

        var info = BitConverter.ToInt32(bytes, 0);
        Debug.Log($"Info: {info}");
        return info;
    }

    void Send(byte[] bytes)
    {
        if (!Connected)
        {
            Debug.Log("Can't send data, not connected.");
            return;
        }

        var stream = _client.GetStream();
        stream.Write(bytes, 0, bytes.Length);
    }

    public void Dispose()
    {
        if (_client != null) _client.Close();
        if (_client != null) _server.Stop();
        Debug.Log("Disconnected.");
    }

    public void SendTargets(int info, Orient pick, Orient place)
    {
        var floats = new List<float>(14);
        floats.AddRange(pick.ToFloats());
        floats.AddRange(place.ToFloats());

        var bytes = new List<byte>(15 * 4);

        bytes.AddRange(BitConverter.GetBytes(info));

        foreach (float number in floats)
            bytes.AddRange(BitConverter.GetBytes(number));

        Send(bytes.ToArray());
    }
}