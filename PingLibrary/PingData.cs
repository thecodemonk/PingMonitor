using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingLibrary
{
    public class PingData
    {
        private DateTime _PingSent;
        public DateTime PingSent {
            get { return _PingSent;  }
            set {
                _PingSent = value;
                OnPropertyChanged("PingSent");
            }
        }

        private long _Latency;
        public long Latency {
            get { return _Latency; }
            set {
                _Latency = value;
                OnPropertyChanged("Latency");
            }
        }

        private bool _Success;
        public bool Success {
            get { return _Success; }
            set {
                _Success = value;
                OnPropertyChanged("Success");
            }
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
