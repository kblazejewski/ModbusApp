using System.Net;
using FluentModbus;

namespace ModbusApp.Models;

public class ModbusClient
{
    private readonly ModbusTcpClient _modbusClient = new();

    public async Task ConnectAsync(string ipAddress, int port)
    {
        await Task.Run(() => _modbusClient.Connect(new IPEndPoint((IPAddress.Parse(ipAddress)), port)));
    }

    public async Task DisconnectAsync()
    {
        await Task.Run(() => _modbusClient.Disconnect());
    }

    public async Task<ushort[]> ReadHoldingRegistersAsync(byte unitIdentifier, int startingAdress, int count)
    {
        return await Task.Run(() =>
        {
            var spanData = _modbusClient.ReadHoldingRegisters<ushort>(unitIdentifier, startingAdress, count);
            return spanData.ToArray();
        });
    }

    public async Task<ushort[]> ReadUnsignedIntegersAsync(byte unitIdentifier, int startingAdress, int count)
    {
        return await Task.Run(() =>
        {
            var spanData = _modbusClient.ReadHoldingRegisters<ushort>(unitIdentifier, startingAdress, count);
            return spanData.ToArray();
        });
    }

    public async Task<short[]> ReadSignedIntegersAsync(byte unitIdentifier, int startingAdress, int count)
    {
        return await Task.Run(() =>
        {
            var spanData = _modbusClient.ReadHoldingRegisters<short>(unitIdentifier, startingAdress, count);
            return spanData.ToArray();
        });
    }

    public async Task<float[]> ReadFloatsAsync(byte unitIdentifier, int startingAdress, int count)
    {
        return await Task.Run(() =>
        {
            var spanData = _modbusClient.ReadHoldingRegisters<float>(unitIdentifier, startingAdress, count);
            return spanData.ToArray();
        });
    }

    public async Task<bool[]> ReadBoolAsync(byte unitIdentifier, int startingAdress, int count)
    {
        return await Task.Run(() =>
        {
            var byteData = _modbusClient.ReadCoils(unitIdentifier, startingAdress, count).ToArray();
            // Convert byte data to bool data
            return Enumerable.Range(0, count).Select(i => ((byteData[i / 8] >> (i % 8)) & 1) > 0).ToArray();
        });
    }

    public async Task WriteIntegerAsync(byte unitIdentifier, int startingAdress, short value)
    {
        await Task.Run(() => _modbusClient.WriteSingleRegister(unitIdentifier, startingAdress, value));
    }

    public async Task WriteMultipleIntegersAsync(byte unitIdentifier, int startingAdress, short[] values)
    {
        await Task.Run(() => _modbusClient.WriteMultipleRegisters(unitIdentifier, startingAdress, values));
    }

    public async Task WriteFloatAsync(byte unitIdentifier, int startingAddress, float value)
    {
        await Task.Run(() => _modbusClient.WriteMultipleRegisters(unitIdentifier, unitIdentifier, [value]));
    }

    public async Task WriteMultipleFloatsAsync(byte unitIdentifier, int startingAddress, float[] values)
    {
        await Task.Run(() => _modbusClient.WriteMultipleRegisters(unitIdentifier, unitIdentifier, values));
    }

    public async Task WriteBoolAsync(byte unitIdentifier, int startingAddress, bool value)
    {
        await Task.Run(() => _modbusClient.WriteSingleCoil(unitIdentifier, startingAddress, value));
    }

    public async Task WriteMultipleBoolsAsync(byte unitIdentifier, int startingAddress, bool[] values)
    {
        await Task.Run(() => _modbusClient.WriteMultipleCoils(unitIdentifier, startingAddress, values));
    }
}