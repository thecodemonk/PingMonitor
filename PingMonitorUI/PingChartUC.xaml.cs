using LiveCharts;
using LiveCharts.Configurations;
using PingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Interaction logic for PingChartUC.xaml
    /// </summary>
    public partial class PingChartUC : UserControl, INotifyPropertyChanged
    {
        private double _axisMax;
        private double _axisMin;
        private SolidColorBrush OkBrush;
        private SolidColorBrush FailBrush;
        private DateTime LaunchTime;

        public PingChartUC()
        {
            InitializeComponent();

            ChartValues = new ChartValues<PingData>();

            var mapper = Mappers.Xy<PingData>().X(model => model.PingSent.Ticks).Y(model => model.Latency).Fill(model => model.Success ? OkBrush : FailBrush).Stroke(model => Brushes.Transparent);
            OkBrush = new SolidColorBrush(Color.FromRgb(254, 192, 7));
            FailBrush = new SolidColorBrush(Color.FromRgb(238, 83, 80));

            Charting.For<PingData>(mapper);
            DateTimeFormatter = value => new DateTime((long)value).ToString("hh:mm:ss");
            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            AxisUnit = TimeSpan.TicksPerSecond;
            LaunchTime = DateTime.Now;
            SetAxisLimits();

            DataContext = this;
        }

        public ChartValues<PingLibrary.PingData> ChartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }

        private void SetAxisLimits()
        {
            var first = ChartValues.OrderBy(item => item.PingSent).FirstOrDefault();
            var last = ChartValues.OrderByDescending(item => item.PingSent).FirstOrDefault();

            if (first != null && last != null)
            {
                if (last.PingSent.Subtract(first.PingSent).TotalSeconds > 30)
                {
                    first = ChartValues.Where(item => last.PingSent.Subtract(item.PingSent).TotalSeconds <= 30).OrderBy(item => item.PingSent).First();
                }
                AxisMin = first.PingSent.Ticks;
                AxisMax = last.PingSent.Ticks;
            }
            else
            {
                AxisMax = LaunchTime.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
                AxisMin = LaunchTime.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
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

        private void CartesianChart_UpdaterTick(object sender)
        {
            SetAxisLimits();
        }
    }
}
