using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModbusApp.Models;

namespace ModbusApp.ViewModels;

public partial class ClientViewModel:ObservableObject
{
    private readonly ModbusClient _modbusClient;
    
    [ObservableProperty]
    private string ipAddress = "127.0.0.1";
    
    [ObservableProperty]
    private int port = 502;

    [ObservableProperty] private byte deviceId = 1;
    
    [ObservableProperty]
    private bool isConnected;
    
    [ObservableProperty]
    private string connectionStatus = "Disconnected";

    [ObservableProperty] private bool boolValue;
    
    [ObservableProperty] private int boolRegisterAddress = 0;
    
    [ObservableProperty]
    private string integerValue = "";
    
    [ObservableProperty] private int integerRegisterAddress = 0;

    public ClientViewModel()
    {
        _modbusClient = new ModbusClient();
    }

    [RelayCommand]
    public async Task ConnectAsync()
    {
        IsConnected = await _modbusClient.ConnectAsync(IpAddress, Port);
        ConnectionStatus = IsConnected ? "Connected" : "Disconnected";
    }
    
    [RelayCommand]
    public async Task DisconnectAsync()
    {
        await _modbusClient.DisconnectAsync();
        IsConnected = false;
        ConnectionStatus = "Disconnected";
    }
    
    [RelayCommand]
    public async Task WriteBoolAsync()
    {
        if (!IsConnected) return;
        await _modbusClient.WriteBoolAsync(DeviceId, boolRegisterAddress, BoolValue);
    }
    
    [RelayCommand]
    public async Task WriteIntegerAsync()
    {
        if (!IsConnected) return;
        if (short.TryParse(IntegerValue, out short value))
        {
            await _modbusClient.WriteIntegerAsync(DeviceId, integerRegisterAddress, value);
        }
    }
    
    
    
}