using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModbusApp.Models;
using System.Threading.Tasks;

namespace ModbusApp.ViewModels;

public partial class ClientViewModel : ObservableObject
{
    private readonly ModbusClient _modbusClient;

    [ObservableProperty] private string _ipAddress = "127.0.0.1";
    [ObservableProperty] private int _port = 502;
    [ObservableProperty] private byte _deviceId = 1;
    [ObservableProperty] private string _connectionStatus = "Disconnected";
    [ObservableProperty] private int _registerAddress = 0;
    [ObservableProperty] private int _registerCount = 1;
    [ObservableProperty] private string _writeValue = "";
    [ObservableProperty] private string _feedback = "";
    [ObservableProperty] private string _writeAddress = "";


    [ObservableProperty] private List<string> _modbusReadFunctions =
    [
        "Read Coils (FC01)",
        "Read Discrete Inputs (FC02)",
        "Read Holding Registers (FC03)",
        "Read Input Registers (FC04)",
    ];

    [ObservableProperty] private List<string> _modbusWriteFunctions =
    [
        "Write Single Coil (FC05)",
        "Write Single Register (FC06)",
        // "Write Multiple Coils (FC15)",
        // "Write Multiple Registers (FC16)",
    ];

    [ObservableProperty] private string _selectedWriteFunction;
    [ObservableProperty] private string _selectedReadFunction;

    public string ConnectButtonText => _modbusClient.IsConnected ? "Disconnect" : "Connect";

    public ClientViewModel()
    {
        _modbusClient = new ModbusClient();
    }

    [RelayCommand]
    private async Task ToggleConnectionAsync()
    {
        if (_modbusClient.IsConnected)
        {
            await _modbusClient.DisconnectAsync();
            ConnectionStatus = "Disconnected";
        }
        else
        {
            var success = await _modbusClient.ConnectAsync(IpAddress, Port);
            ConnectionStatus = success ? "Connected" : "Connection Failed";
        }

        OnPropertyChanged(nameof(ConnectButtonText));
    }

    [RelayCommand]
    private async Task WriteRegisterAsync()
    {
        if (!_modbusClient.IsConnected)
        {
            Feedback = "Client is not connected.";
            return;
        }

        try
        {
            // Parsowanie adresu rejestru
            if (!int.TryParse(WriteAddress, out var registerAddress))
            {
                Feedback = "Invalid register address. Please enter a valid number.";
                return;
            }

            switch (SelectedWriteFunction)
            {
                case "Write Single Coil (FC05)":
                    // Obsługa wpisania 1/0 jako true/false
                    if (WriteValue is "1" or "0")
                    {
                        var writeBoolValue = WriteValue == "1";
                        await _modbusClient.WriteSingleCoilAsync(DeviceId, registerAddress, writeBoolValue);
                        Feedback = "Write Successful";
                    }
                    else if (bool.TryParse(WriteValue, out var writeBoolValue))
                    {
                        await _modbusClient.WriteSingleCoilAsync(DeviceId, registerAddress, writeBoolValue);
                        Feedback = "Write Successful";
                    }
                    else
                    {
                        Feedback = "Invalid value for Write Single Coil. Please enter 'true', 'false', '1', or '0'.";
                    }

                    break;

                case "Write Single Register (FC06)":
                    if (ushort.TryParse(WriteValue, out var writeShortValue))
                    {
                        await _modbusClient.WriteSingleRegisterAsync(DeviceId, registerAddress, writeShortValue);
                        Feedback = "Write Successful";
                    }
                    else
                    {
                        Feedback = "Invalid value for Write Single Register. Please enter a valid number.";
                    }

                    break;

                // case "Write Multiple Coils (FC15)":
                //     var coils = WriteValue
                //         .Split(','); // Assuming coils are separated by commas, e.g., "true,false,true"
                //     var coilValues = coils.Select(c => bool.TryParse(c.Trim(), out var coil) ? coil : false).ToArray();
                //     if (coilValues.Length > 0)
                //     {
                //         await _modbusClient.WriteMultipleRegistersAsync(DeviceId, registerAddress,
                //             coilValues.Select(b => b ? (ushort)1 : (ushort)0).ToArray());
                //         Feedback = "Write Successful";
                //     }
                //     else
                //     {
                //         Feedback =
                //             "Invalid value for Write Multiple Coils. Please provide a comma-separated list of 'true'/'false'.";
                //     }
                //
                //     break;
                //
                // case "Write Multiple Registers (FC16)":
                //     var values = WriteValue.Split(','); // Assuming numbers are separated by commas, e.g., "100,200,300"
                //     var ushortValues = values.Select(v => ushort.TryParse(v.Trim(), out var reg) ? reg : (ushort)0)
                //         .ToArray();
                //     if (ushortValues.Length > 0)
                //     {
                //         await _modbusClient.WriteMultipleRegistersAsync(DeviceId, registerAddress, ushortValues);
                //         Feedback = "Write Successful";
                //     }
                //     else
                //     {
                //         Feedback =
                //             "Invalid value for Write Multiple Registers. Please provide a comma-separated list of numbers.";
                //     }
                //
                //     break;

                default:
                    Feedback = "Invalid Write Function selected.";
                    break;
            }
        }
        catch (Exception ex)
        {
            Feedback = $"Write operation failed: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task ReadRegisterAsync()
    {
        if (!_modbusClient.IsConnected) return;
        var values = await _modbusClient.ReadHoldingRegistersAsync(DeviceId, RegisterAddress, RegisterCount);
        Feedback = values != null ? string.Join(", ", values) : "Read Failed";
    }
}