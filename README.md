# ⚡ AnalyzeMe - Advanced System Analyzer

![Version](https://img.shields.io/badge/version-1.2.0-00ffff?style=for-the-badge)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6?style=for-the-badge&logo=windows)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-00ff00?style=for-the-badge)

---

## 🌟 Overview

**AnalyzeMe** is a next-generation system monitoring and optimization tool for Windows, featuring a stunning cyberpunk-inspired UI. Built with WPF and .NET 8, it provides real-time insights into your PC's performance, hardware, network activity, and running processes.

### ✨ Key Features

- 🖥️ **Real-Time System Monitoring** - Live CPU, RAM, Disk, and GPU metrics
- 🔧 **Comprehensive Hardware Info** - Detailed specs for CPU, GPU, RAM, Storage, and Motherboard
- ⚡ **Performance Analytics** - Track system performance with visual graphs and statistics
- 🌐 **Network Bandwidth Monitor** - Real-time download/upload speeds and internet speed testing
- 📦 **Program Management** - Uninstall programs, manage startup items, and control Windows services
- 📊 **Process Manager** - Advanced task manager with process control (kill, suspend, set priority)
- 🔍 **System Diagnostics** - Health checks and system information export
- 💡 **Optimization Recommendations** - AI-powered suggestions to improve system performance

---

## 🚀 Installation

### Prerequisites
- **Windows 10/11** (64-bit)
- **.NET 8.0 Runtime** ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Administrator privileges** (for full functionality)

### Quick Start

1. **Download the latest release**
   ```bash
   # Clone the repository
   git clone https://github.com/yourusername/AnalyzeMe.git
   cd AnalyzeMe
   ```

2. **Build the project**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run AnalyzeMe**
   ```bash
   dotnet run
   ```
---

## 📋 Features Breakdown

### 📊 Dashboard
- **Real-time system overview** with animated metrics
- **CPU, RAM, Disk, Network usage** displayed live
- **System health score** and quick diagnostics
- **Temperature monitoring** for CPU and GPU

### 🔧 Hardware
- **Detailed component information:**
  - CPU: Model, cores, threads, clock speed, cache
  - GPU: Model, VRAM, driver version, DirectX support
  - RAM: Total/available memory, speed, type
  - Storage: Drives, capacity, free space, health status
  - Motherboard: Manufacturer, model, BIOS version

### ⚡ Performance
- **Historical performance data** with graphs
- **Resource usage trends** over time
- **Process impact analysis**
- **Bottleneck identification**

### 🌐 Network
- **Real-time bandwidth monitoring** (download/upload speeds)
- **Internet speed test** using Ookla Speedtest CLI
- **Session data tracking** (total data transferred)
- **Connection status and adapter information**

### 📦 Programs
- **Installed programs list** with details
- **One-click uninstall** functionality
- **Startup program management** (enable/disable)
- **Windows services control** (start/stop/modify startup type)

### 📊 Manager (Process Manager)
- **Real-time process monitoring** with live updates
- **CPU and memory usage per process**
- **Advanced process control:**
  - End task (kill process)
  - Suspend/Resume processes
  - Set process priority (Realtime, High, Normal, Low, Idle)
  - Open file location
  - View detailed properties
- **Search and filter** processes
- **Context menu** for quick actions

### 🔍 Diagnostics
- **System health checks**
- **Error log viewer**
- **System information export** (TXT format)
- **Driver verification**

### 💡 Optimize
- **Performance optimization suggestions**
- **Startup optimization** recommendations
- **Disk cleanup suggestions**
- **Memory optimization tips**

---

## 🎯 Usage

### Basic Navigation
- Use the **sidebar menu** to switch between different views
- All metrics update automatically in real-time
- **Right-click** on processes for advanced options
- Use the **search boxes** to filter results

### Process Manager
1. Navigate to **Manager** tab
2. View all running processes with live CPU/Memory stats
3. **Right-click** any process for options:
   - End Task
   - Suspend/Resume
   - Change Priority
   - Open File Location
4. **Double-click** a process to view detailed properties

### Network Speed Test
1. Go to **Network** tab
2. Click **🚀 RUN SPEED TEST**
3. Wait 30-60 seconds for results
4. View download/upload speeds and ping as well as other real-time network metrics

### Service Management
1. Navigate to **Programs** → **Services** tab
2. Search for the service you want to modify
3. Change startup type (Automatic/Manual/Disabled)
4. Click **✓ APPLY** to save changes

---

## ⚙️ Configuration

### Settings (Future Feature)
- Theme customization
- Update interval for metrics
- Temperature units (Celsius/Fahrenheit)
- Auto-start with Windows

---

## 🛠️ Development

### Tech Stack
- **Framework:** .NET 8.0 (WPF)
- **Language:** C# 12
- **UI:** XAML with custom styles
- **Architecture:** MVVM pattern
- **APIs Used:**
  - Windows Management Instrumentation (WMI)
  - Performance Counters
  - System.Diagnostics

### Project Structure
```
AnalyzeMe/
├── Models/              # Data models
├── Services/            # Business logic and system APIs
├── Views/               # XAML UI pages
├── Styles/              # Custom styles and themes
├── Tools/               # External tools (speedtest.exe)
├── MainWindow.xaml      # Main application window
└── App.xaml             # Application resources
```

### Building from Source
```bash
# Clone repository
git clone https://github.com/yourusername/AnalyzeMe.git
cd AnalyzeMe

# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Run
dotnet run --configuration Release
```

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork** the repository
2. Create a **feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. Open a **Pull Request**

### Code Style
- Follow standard C# naming conventions
- Use async/await for I/O operations
- Comment complex logic
- Keep methods focused and concise

---

## 🐛 Known Issues

- ⚠️ GPU Memory not accurate. Will get around to fixing that.
- ⚠️ Some system processes cannot be terminated due to Windows security
- ⚠️ GPU monitoring requires compatible hardware
- ⚠️ Network real-time monitoring may have slight delays on some systems
- ⚠️ Administrator privileges required for service management
- ⚠️ Users are seeing slight inaccuracies when it comes to speedtests. Working on a more advanced calculation method.

---

## 📝 Roadmap

### Version 1.3.0 (Planned)
- [ ] GPU performance monitoring and overclocking
- [ ] Storage analyzer with treemap visualization
- [ ] Battery health monitoring (laptops)
- [ ] Custom alerts and notifications
- [ ] Export reports to PDF/Excel

### Version 1.4.0 (Planned)
- [ ] System benchmarking suite
- [ ] Cloud sync and multi-PC dashboard
- [ ] Scheduled maintenance tasks
- [ ] Dark/Light theme switcher
- [ ] System restore point manager

### Future Ideas
- [ ] Voice commands
- [ ] Mobile companion app
- [ ] AI-powered optimization
- [ ] Gaming performance overlay
- [ ] Hardware upgrade recommendations

---

## 📜 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```text
MIT License

Copyright (c) 2025 Hrzenak Technologies

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## 👨‍💻 Author

**Hrzenak Technologies**

- Website: [Coming Soon]
- Email: Kyle@GoForti.com
- GitHub: [@TechGuyKy](https://github.com/TechGuyKy)

---

## 🙏 Acknowledgments

- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) - Hardware monitoring library
- [Ookla Speedtest CLI](https://www.speedtest.net/apps/cli) - Internet speed testing
- [WPF](https://github.com/dotnet/wpf) - UI framework
- [Termi] - Allowing me to use his system to test the software
---

## ⚖️ Disclaimer

**AnalyzeMe** is a system monitoring and management tool. Use caution when:
- Terminating system processes
- Modifying Windows services
- Changing process priorities
- Making system optimizations

**As the developer, I am not responsible for any system instability or data loss caused by improper use of this software.**

Always create a system restore point before making significant changes to your system.

---

## 💬 Support

Having issues? Need help?

- 📧 **Email:** Kyle@GoForti.com
- 🐛 **Bug Reports:** [GitHub Issues](https://github.com/TechGuyKy/AnalyzeMe/issues)
- 💡 **Feature Requests:** [GitHub Discussions](https://github.com/TechGuyKy/AnalyzeMe/discussions)

---

## 🎨 Screenshots

### Dashboard View
The main dashboard provides an at-a-glance view of your system's health and performance.
![Screenshot](https://i.imgur.com/g8XZw9O.png)

### Process Manager
Advanced process management with real-time CPU and memory monitoring, along with the ability to control process priority and lifecycle.
![Screenshot](https://i.imgur.com/VIssImP.png)

### Network Monitor
Track your network usage in real-time and perform internet speed tests directly from the application.
![Screenshot](https://i.imgur.com/pTxHnwn.png)

### Hardware Information
Comprehensive hardware details including CPU specifications, GPU information, memory configuration, and storage devices.
![Screenshot](https://i.imgur.com/VkoEXhC.png)


---

## 🔧 Troubleshooting

### Common Issues

**Application won't start**
- Ensure .NET 8.0 Runtime is installed
- Run as Administrator
- Check Windows Event Viewer for errors

**Cannot modify services**
- Run AnalyzeMe as Administrator
- Check Windows Service permissions

**Hardware info not showing**
- Some hardware requires specific drivers
- Update Windows and device drivers
- Check compatibility with LibreHardwareMonitor

---

<div align="center">
  <h3>Made with ❤️ by KyFu</h3>
  <p>If you find this project useful, please consider giving it a ⭐!</p>
  <p><strong>Version 1.2.0 BETA</strong> | © 2025 Hrzenak Technologies</p>
</div>
