using CenterSpace.NMath.Core;
using EEG_Project.Services;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EEG_Project
{
    public class MainWindowViewModel : BindableBase
    {
        private const int NUM_HZ = 127;
        private RecordingsService _recordingsService;
        private HttpService _httpService;
        public MainWindowViewModel()
        {
            _recordingsService = new RecordingsService();
            _httpService = new HttpService();
        }


        #region properties
        private string _selectedRecordingPath;
        public string SelectedRecordingPath
        {
            get => _selectedRecordingPath;
            set => SetProperty(ref _selectedRecordingPath, value);
        }

        private bool _isUploadButtonEnabled;
        public bool IsUploadButtonEnabled
        {
            get => _isUploadButtonEnabled;
            set => SetProperty(ref _isUploadButtonEnabled, value);
        }
        private int _numHz = NUM_HZ;
        public int NumHz
        {
            get => _numHz;
            set => SetProperty(ref _numHz, value);
        }

        private int _secondsForWelch = 4;
        public int SecondsForWelch
        {
            get => _secondsForWelch;
            set => SetProperty(ref _secondsForWelch, value);
        }
        private double[,] _recordingMatrix;
        public double[,] RecordingMatrix
        {
            get => _recordingMatrix;
            set => SetProperty(ref _recordingMatrix, value);
        }

        private PlotModel _ratDataModel = new PlotModel();
        public PlotModel RawDataModel
        {
            get => _ratDataModel;
            set => SetProperty(ref _ratDataModel, value);
        }


        private PlotModel _welchModel = new PlotModel();
        public PlotModel WelchModel
        {
            get => _welchModel;
            set => SetProperty(ref _welchModel, value);
        }

        private PlotModel _wavesModel = new PlotModel();
        public PlotModel WavesModel
        {
            get => _wavesModel;
            set => SetProperty(ref _wavesModel, value);
        }

        private int _numOfSegments = 1;
        public int SecondsPerSegment
        {
            get => _numOfSegments;
            set
            {
                SetProperty(ref _numOfSegments, value);
                RaisePropertyChanged(nameof(GraphRange));
            }
        }

        private int[] _graphRange;
        public int[] GraphRange
        {
            get
            {
                _graphRange = new int[_totalColums / NumHz / SecondsPerSegment];
                for (int i = 0; i < _graphRange.Length; i++)
                    _graphRange[i] = i + 1;
                return _graphRange;
            }
        }

        private int _segmentStart;
        public int SegmentStart
        {
            get => _segmentStart;
            set => SetProperty(ref _segmentStart, value);
        }
        #endregion

        private int _totalColums;


        #region commands
        private DelegateCommand _browseCommand;
        public DelegateCommand BrowseCommand
        {
            get => _browseCommand ?? new DelegateCommand(() =>
            {
                OpenFileDialog o = new OpenFileDialog();
                if (o.ShowDialog() == true)
                {
                    SelectedRecordingPath = o.FileName;
                    IsUploadButtonEnabled = true;
                }
            });
        }

        private DelegateCommand _uploadRecordingCommand;
        public DelegateCommand UploadRecordingCommand
        {
            get => _uploadRecordingCommand ?? new DelegateCommand(async () =>
            {
                RecordingMatrix = await _recordingsService.ReadRecordingFile(SelectedRecordingPath);
                BuildRecordingGraphs();
            });
        }

        private void BuildRecordingGraphs()
        {
            BuildRawRecordingGraph();
        }

        private void BuildWaveHistogramGraph()
        {
            string[] names = new string[] { "Delta", "Theta", "Alpha", "Beta", "Gamma" };
            var model = new PlotModel();
            var series = new PieSeries() { Title="Wave Disturbution", InsideLabelColor = OxyColors.White };
            for (int i = 0; i < wavesArray.Length; i++)
            {
                series.Slices.Add(new PieSlice(names[i], wavesArray[i]));
            }
            model.Series.Add(series);
            WavesModel = model;
        }

        private async Task BuildWelchGraph()
        {
            (double[] freqs, double[] psd) = await _httpService.Welch(RecordingMatrix, SecondsForWelch, NumHz);
            var psdTempModel = new PlotModel();
            psdTempModel.Title = "Welch";
            var psdSeries = new LineSeries() { Title = "PSD" };
            for (int i = 0; i < psd.Length; i++)
            {
                if (freqs[i] < 4) wavesArray[0] += psd[i];
                else if (freqs[i] >= 4 && freqs[i] <= 7) wavesArray[1] += psd[i];
                else if (freqs[i] >= 8 && freqs[i] <= 15) wavesArray[2] += psd[i];
                else if (freqs[i] >= 16 && freqs[i] <= 31) wavesArray[3] += psd[i];
                else if (freqs[i] >= 32) wavesArray[4] += psd[i];
                psdSeries.Points.Add(new DataPoint(freqs[i], psd[i]));
            }
            psdTempModel.Series.Add(psdSeries);
            WelchModel = psdTempModel;
        }

        private void BuildRawRecordingGraph()
        {
            var temp = new PlotModel();
            RawDataModel.Title = "Recording";
            _totalColums = RecordingMatrix.GetLength(1);
            for (int i = 0; i < RecordingMatrix.GetLength(0); i++)
            {
                var series = new LineSeries() { Title = "Channel " + i };
                for (int j = SegmentStart * SecondsPerSegment * NumHz; j < ((SegmentStart + 1) * SecondsPerSegment) * NumHz; j++)
                {
                    series.Points.Add(new DataPoint(j, RecordingMatrix[i, j]));
                }
                temp.Series.Add(series);
            }
            RawDataModel = temp;
        }

        private DelegateCommand<object> _graphRangeSelectionChangedCommand;
        public DelegateCommand<object> GraphRangeSelectionChangedCommand
        {
            get => _graphRangeSelectionChangedCommand ?? new DelegateCommand<object>((index) =>
            {
                SegmentStart = (int)index;
            });
        }

        private DelegateCommand _detectWavesCommand;
        public DelegateCommand DetectWavesCommand =>
            _detectWavesCommand ??= new DelegateCommand(async () =>
            {
                await BuildWelchGraph();
                BuildWaveHistogramGraph();
            });
        #endregion


        private double[] wavesArray = new double[5]; //delta,theta,alpha,beta,gamma
      
    }

}

