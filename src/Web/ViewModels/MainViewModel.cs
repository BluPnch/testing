using System.Windows.Input;
using System.Collections.ObjectModel;
using Web.Commands;

namespace Web.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private double _temperature = 25.0;
        private double _humidity = 60.0;
        private bool _isVentilationActive;
        private bool _isHeatingActive;
        private bool _isIrrigationActive;

        public double Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }

        public double Humidity
        {
            get => _humidity;
            set => SetProperty(ref _humidity, value);
        }

        public bool IsVentilationActive
        {
            get => _isVentilationActive;
            set => SetProperty(ref _isVentilationActive, value);
        }

        public bool IsHeatingActive
        {
            get => _isHeatingActive;
            set => SetProperty(ref _isHeatingActive, value);
        }

        public bool IsIrrigationActive
        {
            get => _isIrrigationActive;
            set => SetProperty(ref _isIrrigationActive, value);
        }

        public ICommand ToggleVentilationCommand { get; }
        public ICommand ToggleHeatingCommand { get; }
        public ICommand ToggleIrrigationCommand { get; }

        public MainViewModel()
        {
            // Initialize commands
            ToggleVentilationCommand = new RelayCommand(ExecuteToggleVentilation);
            ToggleHeatingCommand = new RelayCommand(ExecuteToggleHeating);
            ToggleIrrigationCommand = new RelayCommand(ExecuteToggleIrrigation);
        }

        private void ExecuteToggleVentilation()
        {
            IsVentilationActive = !IsVentilationActive;
            // TODO: Реализовать логику управления вентиляцией
        }

        private void ExecuteToggleHeating()
        {
            IsHeatingActive = !IsHeatingActive;
            // TODO: Реализовать логику управления отоплением
        }

        private void ExecuteToggleIrrigation()
        {
            IsIrrigationActive = !IsIrrigationActive;
            // TODO: Реализовать логику управления поливом
        }
    }
} 