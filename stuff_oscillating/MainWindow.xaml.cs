using System;
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

namespace stuff_oscillating
{
    
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {

        static bool IsFirst = true;

        public MainWindow()
        {
            InitializeComponent();
            System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            string decimalSeparator = ci.NumberFormat.NumberDecimalSeparator;
            CoolingCoefficientTB.Text = CoolingCoefficientTB.Text.Replace(',', decimalSeparator[0]);
            if (IsFirst)
            {
                IsFirst = false;
                this.Closed += OnMainWindowClosed;
            }
            else
                this.IsCloseButtonEnabled = false;
            DataContext = this;
            Series.Add(new LineSeries()
            {
                Title = "X",
                Values = new ChartValues<double>(),
                LineSmoothness = 0,
                PointGeometry = null,
                Fill = new SolidColorBrush(),

            });
            Model.ModelTick += OnModelTick;
            Model.Start(new Model.ModelParameters
            {
                InitialX = 1
            });
            //UpdateData(null);
        }

        private void OnMainWindowClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private delegate void UpdateDataDelegate(Model.ModelStatus result);

        void UpdateData(Model.ModelStatus result)
        {
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
            double a = result.Time;
            //Series.Clear();
            var series = Series.ElementAt(0);
            Series.Clear();
            series.Values.Add(result.X);
            Series.Add(series);
            Labels.Add(result.Time.ToString());
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

        public List<string> Labels { get; set; } = new List<string>();

        public SeriesCollection Series { get; set; } = new SeriesCollection();

        public SeriesCollection ErrorSeries { get; set; } = new SeriesCollection();

        public ObservableCollection<DataPoint> Data { get; set; } = new ObservableCollection<DataPoint>();

        //public static readonly Dictionary<Model.Methods, string> MethodNames = new Dictionary<Model.Methods, string>()
        //{
        //    {Model.Methods.Analytical, "Аналитический" }, {Model.Methods.Euler, "Эйлера" }, {Model.Methods.MEuler, "Мод. Эйлера" }, {Model.Methods.RK4, "Рунге-Кутты" }
        //};

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ListBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(ListBox, (DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item == null) return;
            var series = (LineSeries)item.Content;
            series.Visibility = series.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void ErrorListBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(ErrorListBox, (DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item == null) return;
            var series = (LineSeries)item.Content;
            series.Visibility = series.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void DoubleTBPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            string decimalSeparator = ci.NumberFormat.CurrencyDecimalSeparator;
            if (decimalSeparator == ".")
            {
                decimalSeparator = "\\" + decimalSeparator;
            }
            var textBox = sender as TextBox;
            var pos = textBox.CaretIndex;
            e.Handled = !Regex.IsMatch(textBox.Text.Substring(0, pos) + e.Text + textBox.Text.Substring(pos), @"^[-+]?[0-9]*" + decimalSeparator + @"?[0-9]*$");
        }

        private void TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                //UpdatePlot();
            }
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
                    csv.WriteField(item.AnalyticalSolutionVal);
                    csv.WriteField(item.EulerSolutionVal);
                    csv.WriteField(item.EulerErrorVal);
                    csv.WriteField(item.MEulerSolutionVal);
                    csv.WriteField(item.MEulerErrorVal);
                    csv.WriteField(item.RK4SolutionVal);
                    csv.WriteField(item.RK4ErrorVal);
                    csv.NextRecord();
                }
                file.Close();
                file.Dispose();
            }
                //File.WriteAllText(saveFileDialog.FileName, txtEditor.Text);
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
        public double AnalyticalSolutionVal { get; set; }
        public double EulerSolutionVal { get; set; }
        public double EulerErrorVal { get; set; }
        public double MEulerSolutionVal { get; set; }
        public double MEulerErrorVal { get; set; }
        public double RK4SolutionVal { get; set; }
        public double RK4ErrorVal { get; set; }
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