﻿using System;
using System.Collections.Generic;
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
using MahApps.Metro.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;
using CsvHelper;
using SciChart.Charting.Model.DataSeries;
using System.Timers;
using SciChart.Core.Extensions;
using SciChart.Core.Framework;

namespace stuff_oscillating
{ 
    
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        static bool IsFirst = true;
        XyDataSeries<double, double> XDataSeries = new XyDataSeries<double, double>() { FifoCapacity = 500, SeriesName = "X" };
        XyDataSeries<double, double> SpeedDataSeries = new XyDataSeries<double, double>() { FifoCapacity = 500, SeriesName = "Speed" };
        XyDataSeries<double, double> EnergyDataSeries = new XyDataSeries<double, double>() { FifoCapacity = 500, SeriesName = "Energy" };
        XyDataSeries<double, double> PhaseDataSeries = new XyDataSeries<double, double>() { FifoCapacity = 500, SeriesName = "Phase", AcceptsUnsortedData=true };
        IUpdateSuspender chartSuspender = null;
        IUpdateSuspender phaseSuspender = null;
        Rectangle rectangle = new Rectangle()
        {
            Fill = new SolidColorBrush(Colors.Teal)
        };
        Line spring = new Line()
        {
            Stroke = new SolidColorBrush(Colors.Black),
            X1 = 0,
            X2 = 0,
            Y1 = 0,
            Y2 = 0
        };
        Line xLine = new Line()
        {
            Stroke = new SolidColorBrush(Colors.Black),
            StrokeThickness = 4,
            X1 = 200,
            X2 = 1180,
            Y1 = 0,
            Y2 = 0
        };
        Line yLine = new Line()
        {
            Stroke = new SolidColorBrush(Colors.Black),
            StrokeThickness = 4,
            X1 = 0,
            X2 = 0,
            Y1 = 100,
            Y2 = 0
        };
        TextBlock X1textBlock = new TextBlock()
        {
            Foreground = new SolidColorBrush(Colors.Black),
            FontSize = 14
        };
        TextBlock X2textBlock = new TextBlock()
        {
            Foreground = new SolidColorBrush(Colors.Black),
            FontSize = 14
        };
        double min = Double.PositiveInfinity;
        double max = Double.NegativeInfinity;

        public MainWindow()
        {
            InitializeComponent();
            if (IsFirst)
            {
                IsFirst = false;
                Closed += OnMainWindowClosed;
            }
            else
                IsCloseButtonEnabled = false;
            xSeries.DataSeries = XDataSeries;
            speedSeries.DataSeries = SpeedDataSeries;
            energySeries.DataSeries = EnergyDataSeries;
            phaseSeries.DataSeries = PhaseDataSeries;
            animCanvas.Children.Add(xLine);
            animCanvas.Children.Add(yLine);
            animCanvas.Children.Add(rectangle);
            animCanvas.Children.Add(spring);
            animCanvas.Children.Add(X1textBlock);
            animCanvas.Children.Add(X2textBlock);
            Canvas.SetLeft(spring, 0);
            Canvas.SetTop(spring, 360);
            Canvas.SetLeft(X2textBlock, 1080);
            Canvas.SetLeft(X1textBlock, 200);
            DataContext = this;
            Model.ModelTick += OnModelTick;
        }

        private void OnMainWindowClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private delegate void UpdateDataDelegate(Model.ModelStatus result);

        void UpdateData(Model.ModelStatus result)
        {
            Data.Add(new DataPoint()
            {
                PointNumber = Data.IsEmpty() ? 1 : Data.Last().PointNumber + 1,
                TimePoint = result.Time,
                X = result.X,
                V = result.Velocity,
                E = result.Energy,
            });
            using (sciChartSurface.SuspendUpdates())
            {
                XDataSeries.Append(result.Time, result.X);
                SpeedDataSeries.Append(result.Time, result.Velocity);
                EnergyDataSeries.Append(result.Time, result.Energy);
            }
            using (sciPhaseChartSurface.SuspendUpdates())
            {
                PhaseDataSeries.Append(result.X, result.Velocity);
            }
            min = Math.Min(min, result.X);
            max = Math.Max(max, result.X);
            if (animTab.IsSelected)
            {
                double k = min != 0 && max != 0
                    ? 800 / (Math.Abs(min) + Math.Abs(max))
                    : 800;
                k = Math.Min(k, 800);
                k = Math.Max(k, 20);
                rectangle.Width = k / 4;
                rectangle.Height = k / 4;
                spring.X2 = 200 + (result.X - min) * k;
                spring.StrokeThickness = 1 + (80 * Math.Cos((spring.X2 - 200) * Math.PI / 1600)) * k / 800;
                spring.Stroke = new SolidColorBrush(new Color()
                {
                    R = result.X < 0 ? (byte)(Math.Cos((result.X - min) * Math.PI / 2 / Math.Abs(min)) * 255) : (byte)0,
                    G = (byte)(Math.Abs(Math.Sin((result.X - min) * Math.PI / (Math.Abs(min) + Math.Abs(max)))) * 255),
                    B = result.X > 0 ? (byte)(Math.Cos((max - result.X) * Math.PI / 2 / Math.Abs(max)) * 255) : (byte)0,
                    A = 255
                });
                Canvas.SetTop(rectangle, 360 - rectangle.Height / 2);
                Canvas.SetLeft(rectangle, spring.X2);
                Canvas.SetLeft(yLine, 200 - min * k + rectangle.Width / 2);
                double bottom = 360 + rectangle.Height / 2;
                Canvas.SetTop(xLine, bottom);
                Canvas.SetTop(X1textBlock, bottom + 10);
                Canvas.SetTop(X2textBlock, bottom + 10);
                yLine.Y2 = bottom + 20;
                yLine.Y1 = bottom - 20;
                xLine.X2 = 200 + (max - min) * k + rectangle.Width;
                X1textBlock.Text = min.ToString("N2");
                X2textBlock.Text = max.ToString("N2");
                Canvas.SetLeft(X2textBlock, Math.Min(xLine.X2, 1200));
            }
            //double time = stopwatch.Elapsed.TotalMilliseconds;
            //List<double> values = new List<double>() { 1 };
            //List<double> v = new List<double>() { 0 };
            //for (int i = 1; i < 250; ++i)
            //{
            //    double a = -0.1 * values.Last() - 0.1 * v.Last();
            //    double dt = 0.1;
            //    v.Add(v.Last() + a * dt);
            //    values.Add(values.Last() + v.Last() * dt);
            //}
            //Series.Clear();
            //Series.Add(new LineSeries()
            //{
            //    Title = "X",
            //    Values = new ChartValues<double>(values),
            //    LineSmoothness = 0,
            //    PointGeometry = null,
            //    Fill = new SolidColorBrush(),
            //});
            //double a = result.Time;
            //Series.Clear();
            //var series = Series.ElementAt(0);
            //Series.Clear();
            //series.Values.Add(result.X);
            //Series.Add(series);
            //Labels.Add(result.Time.ToString());
            //ErrorSeries.Clear();
            //foreach (var it in result.ApproximationData)
            //{
            //    Series.Add(new LineSeries
            //    {
            //        Title = MethodNames[it.Key],
            //        Values = new ChartValues<double>(it.Value.Values),
            //        LineSmoothness = 0,
            //        PointGeometry = null,
            //        Fill = new SolidColorBrush(),
            //    });
            //    if (it.Value.Error == null)
            //        continue;
            //    ErrorSeries.Add(new LineSeries()
            //    {
            //        Title = MethodNames[it.Key],
            //        Values = new ChartValues<double>(it.Value.Error),
            //        LineSmoothness = 0,
            //        PointGeometry = null,
            //        Fill = new SolidColorBrush(),
            //    });
            //};
            //Labels.Clear();
            //Labels.AddRange(result.ArgumentValues.ConvertAll(new Converter<double, string>((double x) => { return x.ToString(); })));
            //Data.Clear();
            //using (var n = (from i in Enumerable.Range(0, result.ArgumentValues.Count()) select i).GetEnumerator())
            //using (var anlv = result.ApproximationData[Model.Methods.Analytical].Values.GetEnumerator())
            //using (var eulv = result.ApproximationData[Model.Methods.Euler].Values.GetEnumerator())
            //using (var eule = result.ApproximationData[Model.Methods.Euler].Error.GetEnumerator())
            //using (var meulv = result.ApproximationData[Model.Methods.MEuler].Values.GetEnumerator())
            //using (var meule = result.ApproximationData[Model.Methods.MEuler].Error.GetEnumerator())
            //using (var rk4v = result.ApproximationData[Model.Methods.RK4].Values.GetEnumerator())
            //using (var rk4e = result.ApproximationData[Model.Methods.RK4].Error.GetEnumerator())
            //using (var time = result.ArgumentValues.GetEnumerator())
            //{
            //    while (n.MoveNext() && anlv.MoveNext() && eulv.MoveNext() && eule.MoveNext() && meulv.MoveNext() && meule.MoveNext() && rk4v.MoveNext() && rk4e.MoveNext() && time.MoveNext())
            //    {
            //        Data.Add(new DataPoint
            //        {
            //            PointNumber = n.Current,
            //            TimePoint = time.Current,
            //            AnalyticalSolutionVal = anlv.Current,
            //            EulerSolutionVal = eulv.Current,
            //            EulerErrorVal = eule.Current,
            //            MEulerSolutionVal = meulv.Current,
            //            MEulerErrorVal = meule.Current,
            //            RK4SolutionVal = rk4v.Current,
            //            RK4ErrorVal = rk4e.Current,
            //        });
            //    }
            //}
            //EulerDeviation = result.ApproximationData[Model.Methods.Euler].StandardDeviation;
            //EulerError = result.ApproximationData[Model.Methods.Euler].Error.Last();
            //MEulerDeviation = result.ApproximationData[Model.Methods.MEuler].StandardDeviation;
            //MEulerError = result.ApproximationData[Model.Methods.MEuler].Error.Last();
            //RK4Deviation = result.ApproximationData[Model.Methods.RK4].StandardDeviation;
            //RK4Error = result.ApproximationData[Model.Methods.RK4].Error.Last();
        }

        void OnModelTick(object sender, Model.ModelStatus result)
        {
            Dispatcher.Invoke(new UpdateDataDelegate(UpdateData), result);
        }

        private double _eulerDeviation;
        public double EulerDeviation {
            get { return _eulerDeviation; }
            set {
                _eulerDeviation = value;
                OnPropertyChanged("EulerDeviation");
            }
        }

        private double _eulerError;
        public double EulerError {
            get { return _eulerError; }
            set {
                _eulerError = value;
                OnPropertyChanged("EulerError");
            }
        }

        private double _meulerDeviation;
        public double MEulerDeviation {
            get { return _meulerDeviation; }
            set {
                _meulerDeviation = value;
                OnPropertyChanged("MEulerDeviation");
            }
        }

        private double _meulerError;
        public double MEulerError {
            get { return _meulerError; }
            set {
                _meulerError = value;
                OnPropertyChanged("MEulerError");
            }
        }

        private double _rK4Deviation;
        public double RK4Deviation {
            get { return _rK4Deviation; }
            set {
                _rK4Deviation = value;
                OnPropertyChanged("RK4Deviation");
            }
        }

        private double _rK4Error;
        public double RK4Error {
            get { return _rK4Error; }
            set {
                _rK4Error = value;
                OnPropertyChanged("RK4Error");
            }
        }

        public ObservableCollection<DataPoint> Data { get; set; } = new ObservableCollection<DataPoint>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void DoubleTBPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            string decimalSeparator = ci.NumberFormat.NumberDecimalSeparator;
            if (decimalSeparator == ".")
            {
                decimalSeparator = "\\" + decimalSeparator;
            }
            var textBox = sender as TextBox;
            var pos = textBox.CaretIndex;
            e.Handled = !Regex.IsMatch(textBox.Text.Substring(0, pos) + e.Text + textBox.Text.Substring(pos), @"^[-+]?[0-9]*" + decimalSeparator + @"?[0-9]*$");
        }

        private void TB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }

        private String PassDefaultIfEmpty(String s)
        {
            if (String.IsNullOrEmpty(s))
                return "1";
            if (s == "-" || s == "+")
                return s + "1";
            return s;
        }

        //private void UpdatePlot() => Model.BeginCalculation(new Model.Parameters()
        //{
        //    InitialTemperature = Convert.ToDouble(PassDefaultIfEmpty(StartTempTB.Text)),
        //    CoolingCoefficient = Convert.ToDouble(PassDefaultIfEmpty(CoolingCoefficientTB.Text)),
        //    EnvironmentTemperature = Convert.ToDouble(PassDefaultIfEmpty(AmbientTempTB.Text)),
        //    SegmentCount = Convert.ToInt32(PassDefaultIfEmpty(SegmentCountTB.Text)),
        //    TimeRange = Convert.ToDouble(PassDefaultIfEmpty(TimeRangeTB.Text)),
        //    Methods = new List<Model.Methods>()
        //        {
        //            Model.Methods.Analytical, Model.Methods.Euler, Model.Methods.MEuler, Model.Methods.RK4
        //        }
        //});

        private void IntTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var pos = textBox.CaretIndex;
            e.Handled = !Regex.IsMatch(textBox.Text.Substring(0, pos) + e.Text + textBox.Text.Substring(pos), @"^[-+]?[0-9]*$");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                StreamWriter file = new StreamWriter(saveFileDialog.FileName);
                var csv = new CsvWriter(file);
                foreach(var item in Data)
                {
                    csv.WriteField(item.PointNumber);
                    csv.WriteField(item.TimePoint);
                    csv.WriteField(item.X);
                    csv.WriteField(item.V);
                    csv.WriteField(item.E);
                    csv.NextRecord();
                }
                file.Close();
                file.Dispose();
            }
                //File.WriteAllText(saveFileDialog.FileName, txtEditor.Text);
        }

        private void TabablzControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlotTab.IsSelected)
            {
                if (chartSuspender != null)
                {
                    chartSuspender.Dispose();
                    chartSuspender = null;
                }
            }
            else
            {
                if (chartSuspender == null)
                    chartSuspender = sciChartSurface.SuspendUpdates();
            }
            if (PhaseTab.IsSelected)
            {
                if (phaseSuspender != null)
                {
                    phaseSuspender.Dispose();
                    phaseSuspender = null;
                }
            }
            else
            {
                if (phaseSuspender == null)
                    phaseSuspender = sciPhaseChartSurface.SuspendUpdates();
            }
        }

        private void StartBtn_OnClick(object sender, RoutedEventArgs e)
        {
            XDataSeries.Clear();
            SpeedDataSeries.Clear();
            EnergyDataSeries.Clear();
            PhaseDataSeries.Clear();
            Data.Clear();
            StartBtn.IsEnabled = false;
            StopBtn.IsEnabled = true;
            ImpulseBtn.IsEnabled = true;
            PauseButton.IsEnabled = true;
            ToggleTextBoxes(false);
            min = Double.PositiveInfinity;
            max = Double.NegativeInfinity;
            Model.Start(new Model.ModelParameters
            {
                ObjectMass = Convert.ToDouble(MassTB.Text),
                InitialX = Convert.ToDouble(InitialPositionTB.Text),
                InitialVelocity = Convert.ToDouble(InitialSpeedTB.Text),
                ForcePeriod = Convert.ToDouble(ExternalForcePeriodTB.Text),
                ForceAmplitude = Convert.ToDouble(ExternalForceAmplitudeTB.Text),
                FrictionCoeffitient = Convert.ToDouble(FrictionCoefficientTB.Text),
                RestrictionCoeffitient = Convert.ToDouble(RestrictionCoefficientTB.Text),
                UseForce = Convert.ToBoolean(ExternalForceCB.IsChecked)
            });
        }

        private void StopBtn_OnClick(object sender, RoutedEventArgs e)
        {
            StartBtn.IsEnabled = true;
            StopBtn.IsEnabled = false;
            ImpulseBtn.IsEnabled = false;
            PauseButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
            ToggleTextBoxes(true);
            Model.Stop();
        }

        private void ImpulseBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Model.Impulse = Convert.ToDouble(ImpulseTB.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            min = (double)XDataSeries.YMin;
            max = (double)XDataSeries.YMax;
        }

        private void ExternalForceAmplitudeTB_TextChanged(object sender, TextChangedEventArgs e) => Model.Parameters.ForceAmplitude = Convert.ToDouble(PassDefaultIfEmpty(ExternalForceAmplitudeTB.Text));

        private void ExternalForcePeriodTB_TextChanged(object sender, TextChangedEventArgs e) => Model.Parameters.ForcePeriod = Convert.ToDouble(PassDefaultIfEmpty(ExternalForcePeriodTB.Text));

        private void ExternalForceCB_Checked(object sender, RoutedEventArgs e) => Model.Parameters.UseForce = ExternalForceCB.IsChecked.Value;

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            PauseButton.IsEnabled = false;
            ResumeButton.IsEnabled = true;
            ImpulseBtn.IsEnabled = false;
            Model.Pause();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            ResumeButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
            ImpulseBtn.IsEnabled = true;
            Model.Resume();
        }

        private void ToggleTextBoxes(bool value)
        {
            MassTB.IsEnabled = value;
            RestrictionCoefficientTB.IsEnabled = value;
            FrictionCoefficientTB.IsEnabled = value;
            InitialSpeedTB.IsEnabled = value;
            InitialPositionTB.IsEnabled = value;
        }

    }

    public class OpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible
                ? 1d
                : .2d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class DataPoint
    {
        public int PointNumber { get; set; }
        public double TimePoint { get; set; }
        public double X { get; set; }
        public double V { get; set; }
        public double E { get; set; }
    }

    [ValueConversion(typeof(object), typeof(string))]
    public class StringFormatConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(new object[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Trace.TraceError("StringFormatConverter: does not support TwoWay or OneWayToSource bindings.");
            return DependencyProperty.UnsetValue;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string format = parameter?.ToString();
                if (String.IsNullOrEmpty(format))
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    for (int index = 0; index < values.Length; ++index)
                    {
                        builder.Append("{" + index + "}");
                    }
                    format = builder.ToString();
                }
                return String.Format(/*culture,*/ format, values);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("StringFormatConverter({0}): {1}", parameter, ex.Message);
                return DependencyProperty.UnsetValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Trace.TraceError("StringFormatConverter: does not support TwoWay or OneWayToSource bindings.");
            return null;
        }
    }

}