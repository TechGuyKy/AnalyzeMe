using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnalyzeMe.Services
{
    public class SpeedTestResult
    {
        public double DownloadMbps { get; set; }
        public double UploadMbps { get; set; }
        public double PingMs { get; set; }
        public string? ServerName { get; set; }
        public string? ServerLocation { get; set; }
        public string? Isp { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class SpeedTestService
    {
        private readonly string _speedTestPath;

        public SpeedTestService()
        {
            //i've added speedtest.exe in Tools folder, this looks for both Tools folder and speedtest.exe
            _speedTestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "speedtest.exe");
        }

        public bool IsSpeedTestAvailable()
        {
            return File.Exists(_speedTestPath);
        }

        public async Task<SpeedTestResult> RunSpeedTestAsync()
        {
            var result = new SpeedTestResult();

            try
            {
                if (!IsSpeedTestAvailable())
                {
                    result.Success = false;
                    result.ErrorMessage = "Speedtest CLI not found. Please download from speedtest.net/apps/cli";
                    return result;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = _speedTestPath,
                    Arguments = "--accept-license --accept-gdpr --format=json",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    var jsonDoc = JsonDocument.Parse(output);
                    var root = jsonDoc.RootElement;

                    //added to parse download speed in bits per second then convert automatically to mbps
                    if (root.TryGetProperty("download", out var download))
                    {
                        if (download.TryGetProperty("bandwidth", out var downloadBandwidth))
                        {
                            var bps = downloadBandwidth.GetDouble();
                            result.DownloadMbps = (bps * 8) / 1_000_000; //this is the conversion for bytes/s to Mb/s
                        }
                    }

                    //added to parse upload speed, as accurate as I could make it
                    if (root.TryGetProperty("upload", out var upload))
                    {
                        if (upload.TryGetProperty("bandwidth", out var uploadBandwidth))
                        {
                            var bps = uploadBandwidth.GetDouble();
                            result.UploadMbps = (bps * 8) / 1_000_000;
                        }
                    }

                    //added to parse ping for those who care about latency
                    if (root.TryGetProperty("ping", out var ping))
                    {
                        if (ping.TryGetProperty("latency", out var latency))
                        {
                            result.PingMs = latency.GetDouble();
                        }
                    }

                    //might as well parse server info too
                    if (root.TryGetProperty("server", out var server))
                    {
                        if (server.TryGetProperty("name", out var name))
                            result.ServerName = name.GetString();

                        if (server.TryGetProperty("location", out var location))
                            result.ServerLocation = location.GetString();
                    }

                    //ISP
                    if (root.TryGetProperty("isp", out var isp))
                    {
                        result.Isp = isp.GetString();
                    }

                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = await process.StandardError.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }
}