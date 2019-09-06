using PingLibrary;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PingMonitorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SendPing _ping;
        private bool _running;

        public MainWindow()
        {
            InitializeComponent();
            _running = false;
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (_running)
            {
                _ping.StopPings();
                _running = false;
                _ping.OnPing -= _ping_OnPing;
                btnStartStop.Content = "Start";
            }
            else
            {
                IPAddress address;
                if (!IPAddress.TryParse(textAddress.Text, out address))
                {
                    try
                    {
                        address = Dns.GetHostEntry(textAddress.Text).AddressList[0];
                    }
                    catch (Exception)
                    {
                        address = null;
                    }
                }

                if (address != null)
                {
                    if (integerInterval.Value.HasValue)
                    {
                        _ping = new SendPing(address, integerInterval.Value.Value);
                        btnStartStop.Content = "Stop";
                        _running = true;
                        chartPingData.ChartValues.Clear();
                        _ping.OnPing += _ping_OnPing;
                        _ping.StartPings();
                    }
                    else
                    {
                        // no interval
                    }
                }
                else
                {
                    // invalid
                }
            }
        }

        private void UpdateStats()
        {
            if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                Dispatcher.Invoke(UpdateStats);
                return;
            }
            lblSent.Content = _ping.GetSent();
            lblRecv.Content = _ping.GetSent() - _ping.GetLost();
            lblLost.Content = _ping.GetLost();
            lblSucc.Content = string.Format("{0:0.00}%", _ping.GetSuccessPercentage());
            lblMin.Content = _ping.GetMinLatency();
            lblMax.Content = _ping.GetMaxLatency();
            lblAvg.Content = string.Format("{0:0.00}",_ping.GetAvgLatency());
        }

        private void _ping_OnPing(object sender, SendPing.OnPingEventArgs e)
        {
            var data = new PingData() { PingSent = e.PingSent, Latency = e.Latency, Success = e.Success };
            if (chartPingData.ChartValues.Count == 20)
                chartPingData.ChartValues.RemoveAt(0);
            chartPingData.ChartValues.Add(data);
            var isChecked = chkLogData.Dispatcher.Invoke(() => chkLogData.IsChecked);
            if (isChecked.HasValue && isChecked.Value)
            {
                LogData(data, e.Target);
            }
            UpdateStats();
        }

        private void LogData(PingData data, IPAddress target)
        {
            if (!data.Success)
                Serilog.Log.Error($"{data.PingSent.ToShortDateString()} {data.PingSent.ToLongTimeString()}, { target.ToString() }, Failed, 0");
            else
                Serilog.Log.Information($"{data.PingSent.ToShortDateString()} {data.PingSent.ToLongTimeString()}, {target.ToString()}, Success, {data.Latency}");
        }

        private void btnGetAllResults_Click(object sender, RoutedEventArgs e)
        {
            if (_ping != null)
            {
                var data = _ping.GetAllResultsInCsv();
                ResultsWindow winRes = new ResultsWindow(data);
                winRes.ShowDialog();
            }
        }
    }
}
