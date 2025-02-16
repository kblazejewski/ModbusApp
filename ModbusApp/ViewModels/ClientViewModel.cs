using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModbusApp.Models;

namespace ModbusApp.ViewModels;

public partial class ClientViewModel : ObservableObject
{
    private readonly ModbusClient _modbusClient;

    [ObservableProperty] private string _ipAddress = "127.0.0.1";
    [ObservableProperty] private int _port = 502;
    [ObservableProperty] private byte _deviceId = 1;
    [ObservableProperty] private string _connectionStatus = "Disconnected";
    [ObservableProperty] private string _writeValue = "";
    [ObservableProperty] private string _feedback = "";
    [ObservableProperty] private string _writeAddress = "";
    [ObservableProperty] private string _readAddress = "";
    [ObservableProperty] private string _readValue = "";
    [ObservableProperty] private string _readFeedback = "";
    [ObservableProperty] private string _writeFeedback = "";
    
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
            if (!int.TryParse(WriteAddress, out var registerAddress))
            {
                WriteFeedback = "Invalid register address. Please enter a valid number.";
                return;
            }

            switch (SelectedWriteFunction)
            {
                case "Write Single Coil (FC05)":
                    if (WriteValue is "1" or "0")
                    {
                        var writeBoolValue = WriteValue == "1";
                        await _modbusClient.WriteSingleCoilAsync(DeviceId, registerAddress, writeBoolValue);
                        WriteFeedback = "Write Successful";
                    }
                    else if (bool.TryParse(WriteValue, out var writeBoolValue))
                    {
                        await _modbusClient.WriteSingleCoilAsync(DeviceId, registerAddress, writeBoolValue);
                        WriteFeedback = "Write Successful";
                    }
                    else
                    {
                        WriteFeedback = "Invalid value for Write Single Coil. Please enter 'true', 'false', '1', or '0'.";
                    }

                    break;

                case "Write Single Register (FC06)":
                    if (ushort.TryParse(WriteValue, out var writeShortValue))
                    {
                        await _modbusClient.WriteSingleRegisterAsync(DeviceId, registerAddress, writeShortValue);
                        WriteFeedback = "Write Successful";
                    }
                    else
                    {
                        WriteFeedback = "Invalid value for Write Single Register. Please enter a valid number.";
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
            WriteFeedback = $"Write operation failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ReadRegisterAsync()
    {
        if (!_modbusClient.IsConnected)
        {
            ReadFeedback = "Client is not connected.";
            return;
        }

        const int registerCount = 1;
        try
        {
            if (!int.TryParse(ReadAddress, out var registerAddress))
            {
                ReadFeedback = "Invalid register address. Please enter a valid number.";
                return;
            }

            switch (SelectedReadFunction)
            {
                case "Read Coils (FC01)":
                    var coils = await _modbusClient.ReadCoilsAsync(DeviceId, registerAddress, registerCount);
                    ReadValue = string.Join(", ", coils.Select(c => c == 1 ? "1" : "0"));
                    break;

                case "Read Discrete Inputs (FC02)":
                    var inputs = await _modbusClient.ReadDiscreteInputsAsync(DeviceId, registerAddress, registerCount);
                    ReadValue = string.Join(", ", inputs.Select(i => i == 1 ? "1" : "0"));
                    break;

                case "Read Holding Registers (FC03)":
                    var holdingValues =
                        await _modbusClient.ReadHoldingRegistersAsync(DeviceId, registerAddress, registerCount);
                    ReadValue = string.Join(", ", holdingValues);
                    break;

                case "Read Input Registers (FC04)":
                    var inputValues =
                        await _modbusClient.ReadInputRegistersAsync(DeviceId, registerAddress, registerCount);
                    ReadValue = string.Join(", ", inputValues);
                    break;

                default:
                    ReadFeedback = "Invalid Read Function selected.";
                    return;
            }

            ReadFeedback = "Read Successful";
        }
        catch (Exception ex)
        {
            ReadFeedback = $"Read operation failed: {ex.Message}";
        }
    }
}