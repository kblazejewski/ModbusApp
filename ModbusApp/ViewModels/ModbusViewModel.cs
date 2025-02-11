using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ModbusApp.Models;

namespace ModbusApp.ViewModels;

public class ModbusViewModel : INotifyPropertyChanged
{
    private readonly ModbusClient _modbusClient;

    public ObservableCollection<string> Log { get; set; } = new ObservableCollection<string>();
    public ModbusData ModbusData { get; set; } = new ModbusData();

    private bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            isLoading = value;
            OnPropertyChanged();
        }
    }
    
    public ICommand ReadCoilsCommand { get; }
    public ICommand ReadHoldingRegistersCommand { get; }
    
    public ModbusViewModel()
    {
        _modbusClient = new ModbusClient();
        //ReadCoilsCommand = new Command(async () => await ReadCoilsAsync());
        //ReadHoldingRegistersCommand = new Command(async () => await ReadHoldingRegistersAsync());
    }

    private async Task ReadCoilsAsync(byte unitIdentifier, int startingAddress, int count)
    {
        IsLoading = true;
        var boolData = await _modbusClient.ReadBoolAsync(unitIdentifier, startingAddress, count);
        ModbusData.BoolData = boolData;
        Log.Add("Read Coils: " + string.Join(", ", boolData));
        IsLoading = false;
    }
    
    private async Task ReadHoldingRegistersAsync(byte unitIdentifier, int startingAddress, int count)
    {
        IsLoading = true;
        var intData = await _modbusClient.ReadSignedIntegersAsync(unitIdentifier, startingAddress, count);
        ModbusData.IntData = intData;
        Log.Add("Read Holding Registers: " + string.Join(", ", intData));
        IsLoading = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}