using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModbusApp.Models;
using System.Threading.Tasks;

namespace ModbusApp.ViewModels;

public partial class ClientViewModel : ObservableObject
{
    private readonly ModbusClient _modbusClient;
    private CancellationTokenSource _connectionMonitorCancellationTokenSource;

    [ObservableProperty] private string ipAddress = "127.0.0.1";
    [ObservableProperty] private int port = 502;
    [ObservableProperty] private byte deviceId = 1;
    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private string connectionStatus = "Disconnected";
    [ObservableProperty] private bool boolValue;
    [ObservableProperty] private int boolRegisterAddress = 0;
    [ObservableProperty] private string integerValue = "";
    [ObservableProperty] private int integerRegisterAddress = 0;

    public string ConnectButtonText => IsConnected ? "Disconnect" : "Connect";

    public ClientViewModel()
    {
        _modbusClient = new ModbusClient();
    }

    [RelayCommand]
    public async Task ToggleConnectionAsync()
    {
        if (IsConnected) // Używamy getter'a
        {
            await DisconnectAsync();
        }
        else
        {
            await ConnectAsync();
        }
    }

    // Metoda do łączenia
    public async Task ConnectAsync()
    {
        try
        {
            var success = await _modbusClient.ConnectAsync(IpAddress, Port);
            if (success)
            {
                ConnectionStatus = "Connected";
                OnPropertyChanged(nameof(ConnectButtonText));

                // Rozpoczynamy monitorowanie połączenia po udanym połączeniu
                StartConnectionMonitor();
            }
            else
            {
                ConnectionStatus = "Connection Failed";
                OnPropertyChanged(nameof(ConnectButtonText));
            }
        }
        catch
        {
            ConnectionStatus = "Connection Failed";
            OnPropertyChanged(nameof(ConnectButtonText));
        }
    }

    // Metoda do rozłączania
    public async Task DisconnectAsync()
    {
        await _modbusClient.DisconnectAsync();
        ConnectionStatus = "Disconnected";
        IsConnected = false;
        OnPropertyChanged(nameof(ConnectButtonText)); // Zaktualizowanie tekstu przycisku

        // Zatrzymanie monitorowania po rozłączeniu
        _connectionMonitorCancellationTokenSource?.Cancel();
    }

    // Metoda monitorująca połączenie
    private void StartConnectionMonitor()
    {
        // Tworzymy nowy CancellationTokenSource
        _connectionMonitorCancellationTokenSource = new CancellationTokenSource();
        var token = _connectionMonitorCancellationTokenSource.Token;

        // Używamy Task do monitorowania połączenia w tle
        Task.Run(async () =>
        {
            while (!_modbusClient.IsConnected)
            {
                await Task.Delay(2000, token); // Sprawdzenie co 2 sekundy

                if (!_modbusClient.IsConnected) // Sprawdzamy, czy połączenie zostało utracone
                {
                    ConnectionStatus = "Lost Connection";
                    IsConnected = false;
                    OnPropertyChanged(nameof(ConnectButtonText)); // Odświeżenie statusu przycisku
                    break; // Zatrzymujemy monitorowanie po utracie połączenia
                }
            }
        }, token);
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