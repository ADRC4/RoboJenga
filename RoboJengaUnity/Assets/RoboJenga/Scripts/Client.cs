using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

class Client : IDisposable
{
    public bool Connected { get { return _client != null && _client.Connected; } }
    TcpClient _client;
    byte[] _bytes = new byte[6 * 4];

    public async Task ConnectAsync(string ip, int port)
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);
            Debug.Log($"Connected to: {_client.Client.RemoteEndPoint}");
        }
        catch (SocketException e)
        {
            Debug.Log($"SocketException: {e}");
        }

        _client.LingerState = new LingerOption(true, 5);
    }

    public async Task<Vector6> ReadAsync()
    {
        if (!Connected)
        {
            Debug.Log("Can't receive data, not connected.");
            return new Vector6();
        }

        var stream = _client.GetStream();

        while (stream.DataAvailable)
        {
            await stream.ReadAsync(_bytes, 0, _bytes.Length);
        }
        
        Vector6 joints = new Vector6();

        for (int i = 0; i < 6; i++)
        {
            joints[i] = BitConverter.ToSingle(_bytes, i*4);
        }

        return joints;
    }


    public Vector6 Read()
    {
        if (!Connected)
        {
            Debug.Log("Can't receive data, not connected.");
            return new Vector6();
        }

        var stream = _client.GetStream();

        while (stream.DataAvailable)
        {
            stream.Read(_bytes, 0, _bytes.Length);
        }

        Vector6 joints = new Vector6();

        for (int i = 0; i < 6; i++)
        {
            joints[i] = BitConverter.ToSingle(_bytes, i * 4);
        }

        return joints;
    }

    public void Dispose()
    {
        if (_client != null) _client.Dispose();
        Debug.Log("Disconnected.");
    }
}