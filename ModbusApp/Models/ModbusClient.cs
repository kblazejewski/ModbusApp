using System.Net;
using FluentModbus;

namespace ModbusApp.Models;

public class ModbusClient
{
    private readonly ModbusTcpClient _modbusClient = new();
    private bool _isConnected;
    
    public bool IsConnected => _isConnected;

    public async Task<bool> ConnectAsync(string ipAddress, int port)
    {
        try
        {
            await Task.Run(() => _modbusClient.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ModbusEndianness.BigEndian));
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
    
    public async Task<ushort[]> ReadHoldingRegistersAsync(byte unitId, int address, int count)
    {
        EnsureConnected();
        return await Task.Run(() => _modbusClient.ReadHoldingRegisters<ushort>(unitId, address, count).ToArray());
    }

    public async Task WriteMultipleRegistersAsync(byte unitId, int address, ushort[] values)
    {
        EnsureConnected();
        await Task.Run(() => _modbusClient.WriteMultipleRegisters(unitId, address, values));
    }

    public async Task<byte[]> ReadCoilsAsync(byte unitId, int address, int count)
    {
        EnsureConnected();
        return await Task.Run(() => _modbusClient.ReadCoils(unitId, address, count).ToArray());
    }

    public async Task<byte[]> ReadDiscreteInputsAsync(byte unitId, int address, int count)
    {
        EnsureConnected();
        return await Task.Run(() => _modbusClient.ReadDiscreteInputs(unitId, address, count).ToArray());
    }

    public async Task<ushort[]> ReadInputRegistersAsync(byte unitId, int address, int count)
    {
        EnsureConnected();
        return await Task.Run(() => _modbusClient.ReadInputRegisters<ushort>(unitId, address, count).ToArray());
    }

    public async Task WriteSingleCoilAsync(byte unitId, int address, bool value)
    {
        EnsureConnected();
        await Task.Run(() => _modbusClient.WriteSingleCoil(unitId, address, value));
    }

    public async Task WriteSingleRegisterAsync(byte unitId, int address, ushort value)
    {
        EnsureConnected();
        await Task.Run(() => _modbusClient.WriteSingleRegister(unitId, address, value));
    }
}