using System;
using System.Windows;
using System.Windows.Controls;
using AnalyzeMe.ViewModels;

namespace AnalyzeMe.Views
{
    public partial class HardwareView : Page
    {
        private MainViewModel ViewModel => (MainViewModel)Application.Current.MainWindow.DataContext;

        public HardwareView()
        {
            InitializeComponent();
            DataContext = ViewModel;
            Loaded += HardwareView_Loaded;
        }

        private void HardwareView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (ViewModel.SystemInfo == null) return;

            
            CpuModelText.Text = ViewModel.SystemInfo.ProcessorName;
            CpuCoresText.Text = $"{ViewModel.SystemInfo.ProcessorCores} Cores / {ViewModel.SystemInfo.ProcessorThreads} Threads";
            CpuBaseSpeedText.Text = $"{ViewModel.SystemInfo.ProcessorBaseSpeed:F2} GHz";
            CpuMaxSpeedText.Text = $"{ViewModel.SystemInfo.ProcessorMaxSpeed:F2} GHz";
            CpuArchText.Text = ViewModel.SystemInfo.SystemArchitecture;
            RamTotalText.Text = $"{ViewModel.SystemInfo.TotalRAM:F1} GB";
            RamAvailableText.Text = $"{ViewModel.SystemInfo.AvailableRAM:F1} GB ({100 - ViewModel.SystemInfo.RAMUsagePercentage:F1}%)";
            RamTypeText.Text = ViewModel.SystemInfo.MemoryType;
            RamSpeedText.Text = ViewModel.SystemInfo.MemorySpeed;
            RamSlotsText.Text = $"{ViewModel.SystemInfo.MemorySlotsUsed} / {ViewModel.SystemInfo.MemorySlots} slots used";
            DisksListView.ItemsSource = ViewModel.SystemInfo.Disks;
            GpuNameText.Text = ViewModel.SystemInfo.GraphicsCard;
            if (ViewModel.SystemInfo.GraphicsMemory > 0)
            {
                GpuMemoryText.Text = $"{ViewModel.SystemInfo.GraphicsMemory:F1} GB";
            }
            else
            {
                GpuMemoryText.Text = "Shared Memory";
            }
            MoboNameText.Text = ViewModel.SystemInfo.MotherboardName;
            BiosText.Text = ViewModel.SystemInfo.BiosVersion;
            BiosDateText.Text = $"Date: {ViewModel.SystemInfo.BiosDate}";
        }
    }
}