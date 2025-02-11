using System.Net;
using FluentModbus;

namespace ModbusApp.Models;

public class ModbusClient
{
    private readonly ModbusTcpClient _modbusClient = new();
    private bool _isConnected;

    public async Task<bool> ConnectAsync(string ipAddress, int port)
    {
        try
        {
            await Task.Run(() => _modbusClient.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port)));
            _isConnected = true;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Modbus Connection Error: {ex.Message}");
            _isConnected = false;
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (!_isConnected) return;

        await Task.Run(() =>
        {
            _modbusClient.Disconnect();
            _isConnected = false;
        });
    }

    private void EnsureConnected()
    {
        if (!_isConnected)
            throw new InvalidOperationException("Modbus client is not connected.");
    }

    public async Task<ushort[]> ReadHoldingRegistersAsync(byte unitIdentifier, int startingAddress, int count)
    {
        EnsureConnected();
        return await Task.Run(() =>
            _modbusClient.ReadHoldingRegisters<ushort>(unitIdentifier, startingAddress, count).ToArray());
    }

    public async Task<short[]> ReadSignedIntegersAsync(byte unitIdentifier, int startingAddress, int count)
    {
        EnsureConnected();
        return await Task.Run(() =>
            _modbusClient.ReadHoldingRegisters<short>(unitIdentifier, startingAddress, count).ToArray());
    }

    public async Task<float[]> ReadFloatsAsync(byte unitIdentifier, int startingAddress, int count)
    {
        EnsureConnected();
        return await Task.Run(() =>
            _modbusClient.ReadHoldingRegisters<float>(unitIdentifier, startingAddress, count).ToArray());
    }

    public async Task<bool[]> ReadBoolAsync(byte unitIdentifier, int startingAddress, int count)
    {
        EnsureConnected();
        return await Task.Run(() =>
        {
            var bytes = _modbusClient.ReadCoils(unitIdentifier, startingAddress, count).ToArray();
            return bytes.Select(b => b > 0).ToArray();
        });
    }

    public async Task WriteIntegerAsync(byte unitIdentifier, int startingAddress, short value)
    {
        EnsureConnected();
        await Task.Run(() => _modbusClient.WriteSingleRegister(unitIdentifier, startingAddress, value));
    }

    public async Task WriteMultipleIntegersAsync(byte unitIdentifier, int startingAddress, short[] values)
    {
        EnsureConnected();
        await Task.Run(() => _modbusClient.WriteMultipleRegisters(unitIdentifier, startingAddress, values));
    }

    public async Task WriteFloatAsync(byte unitIdentifier, int startingAddress, float value)
    {
        EnsureConnected();
        var bytes = BitConverter.GetBytes(value);
        var registers = new ushort[2]
        {
            BitConverter.ToUInt16(bytes, 0),
            BitConverter.ToUInt16(bytes, 2)
        };
        await Task.Run(() => _modbusClient.WriteMultipleRegisters(unitIdentifier, startingAddress, registers));
    }

    public async Task WriteMultipleFloatsAsync(byte unitIdentifier, int startingAddress, float[] values)
    {
        EnsureConnected();
        var registers = values.SelectMany(v =>
        {
            var bytes = BitConverter.GetBytes(v);
            return new ushort[] { BitConverter.ToUInt16(bytes, 0), BitConverter.ToUInt16(bytes, 2) };
        }).ToArray();
        await Task.Run(() => _modbusClient.WriteMultipleRegisters(unitIdentifier, startingAddress, registers));
    }

    public async Task WriteBoolAsync(byte unitIdentifier, int startingAddress, bool value)
    {
        EnsureConnected();
        await Task.Run(() => _modbusClient.WriteSingleCoil(unitIdentifier, startingAddress, value));
    }

    public async Task WriteMultipleBoolsAsync(byte unitIdentifier, int startingAddress, bool[] values)
    {
        EnsureConnected();
        await Task.Run(() => _modbusClient.WriteMultipleCoils(unitIdentifier, startingAddress, values));
    }
}