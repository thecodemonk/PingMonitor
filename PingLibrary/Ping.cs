using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PingLibrary
{
    public class SendPing
    {
        private IPAddress _address;
        private Timer _timer;

        public delegate void OnPingEventHandler(object sender, OnPingEventArgs e);
        public event OnPingEventHandler OnPing;

        ObservableCollection<PingData> _data;
        public ObservableCollection<PingData> Data
        {
            get { return _data; }
        }

        public SendPing(IPAddress address, int Interval = 1000)
        {
            _address = address;
            _data = new ObservableCollection<PingData>();
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = Interval;
        }

        public void StartPings()
        {
            _timer.Start();
        }

        public void StopPings()
        {
            _timer.Stop();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DoPing();
        }

        private void DoPing()
        {
            Ping pingSender = new Ping();
            
            pingSender.SendPingAsync(_address).ContinueWith(pingReply => {
                var reply = pingReply.Result;
                PingData _ping = new PingData() { PingSent = DateTime.Now };
                if (reply.Status == IPStatus.Success)
                {
                    _ping.Success = true;
                    _ping.Latency = reply.RoundtripTime;
                }
                else
                {
                    _ping.Success = false;
                }

                _data.Add(_ping);
                PingEvent(_ping.PingSent, _ping.Latency, _ping.Success, _address);
            });
        }

        public string GetSummaryResults()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Sent: {0} Recv: {1} Lost: {2}{3}", _data.Count, _data.Where(p => p.Success).Count(), _data.Where(p => p.Success == false).Count(), Environment.NewLine));
            sb.Append(string.Format("Min: {0} Max: {1} Avg: {2}{3}", _data.Where(p => p.Success).Select(p => p.Latency).Min(), _data.Where(p => p.Success).Select(p => p.Latency).Max(), _data.Where(p => p.Success).Select(p => p.Latency).Average(), Environment.NewLine));
            return sb.ToString();
        }

        public string GetAllResults()
        {
            StringBuilder sb = new StringBuilder();
            _data.OrderBy(item => item.PingSent).ToList().ForEach(ping => {
                sb.Append($"{ping.PingSent.ToShortDateString()} {ping.PingSent.ToLongTimeString()} : {_address.ToString()} {ping.Success} {ping.Latency}ms{System.Environment.NewLine}");
            });
            return sb.ToString();
        }

        public string GetAllResultsInCsv()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Date Time, Target, Success, Latency (ms) {System.Environment.NewLine}");
            _data.OrderBy(item => item.PingSent).ToList().ForEach(ping => {
                sb.Append($"{ping.PingSent.ToShortDateString()} {ping.PingSent.ToLongTimeString()},{_address.ToString()},{ping.Success},{ping.Latency}{System.Environment.NewLine}");
            });
            return sb.ToString();
        }

        public long GetSent()
        {
            return _data.Count();
        }

        public long GetLost()
        {
            return _data.Where(item => item.Success == false).Count();
        }

        public long GetMinLatency()
        {
            return _data.Any(item => item.Success) ? _data.Where(item=> item.Success).Select(item => item.Latency).Min() : 0;
        }

        public long GetMaxLatency()
        {
            return _data.Any(item => item.Success) ? _data.Where(item => item.Success).Select(item => item.Latency).Max(): 0;
        }

        public double GetAvgLatency()
        {
            return _data.Any(item => item.Success) ? _data.Where(item => item.Success).Select(item => item.Latency).Average() : 0;
        }

        public double GetSuccessPercentage()
        {
            return ((_data.Where(item => item.Success).Count() / (float)_data.Count()) * 100.0);
        }

        protected virtual void PingEvent(DateTime Sent, long Latency, bool Success, IPAddress target)
        {
            if (OnPing != null) OnPing(this, new OnPingEventArgs(Sent, Latency, Success, target));
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public class OnPingEventArgs : EventArgs
        {
            private readonly DateTime _PingSent;
            private readonly long _Latency;
            private readonly bool _Success;
            private readonly IPAddress _Target;

            public OnPingEventArgs(DateTime Sent, long Latency, bool Success, IPAddress target)
            {
                _PingSent = Sent;
                _Latency = Latency;
                _Success = Success;
                _Target = target;
            }

            public DateTime PingSent { get { return _PingSent; } }
            public long Latency { get { return _Latency; } }
            public bool Success { get { return _Success; } }
            public IPAddress Target { get { return _Target; } }
        }
    }
}
