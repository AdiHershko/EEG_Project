using CenterSpace.NMath.Core;
using EEG_Project.Services;
using EnumsNET;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static EEG_Project.Commons.WaveTypes;

namespace EEG_Project
{
    public class MainWindowViewModel : BindableBase
    {
        private IRecordingsService _recordingsService;
        private IHttpService _httpService;
        private IDialogService _dialogService;

        public MainWindowViewModel(IDialogService dialogService, IRecordingsService recordingsService, IHttpService httpService)
        {
            _dialogService = dialogService;
            _recordingsService = recordingsService;
            _httpService = httpService;

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

        private bool _isUploaded;
        public bool IsUploaded
        {
            get => _isUploaded;
            set => SetProperty(ref _isUploaded, value);
        }


        private int _numHz = 127;
        public int NumHz
        {
            get => _numHz;
            set => SetProperty(ref _numHz, value);
        }

        private int _numberOfChannels;
        public int NumberOfChannels
        {
            get => _numberOfChannels;
            set => SetProperty(ref _numberOfChannels, value);
        }

        private int _recordingLength;
        public int RecordingLength
        {
            get => _recordingLength;
            set => SetProperty(ref _recordingLength, value);
        }

        private int _channel = 0;
        public int Channel
        {
            get => _channel;
            set => SetProperty(ref _channel, value);
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

        private int _numberOfParts;
        public int NumberOfParts
        {
            get => _numberOfParts;
            set => SetProperty(ref _numberOfParts, value);

        }

        public IReadOnlyList<WaveType> WaveTypes { get; } = Enums.GetValues<WaveType>();

        private WaveType _selectedWaveType = WaveType.Delta;
        public WaveType SelectedWaveType
        {
            get => _selectedWaveType;
            set => SetProperty(ref _selectedWaveType, value);
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

        private PlotModel _partialWavesModel = new PlotModel();
        public PlotModel PartialWavesModel
        {
            get => _partialWavesModel;
            set => SetProperty(ref _partialWavesModel, value);
        }





        #endregion


        #region commands
        private DelegateCommand _browseCommand;
        public DelegateCommand BrowseCommand =>
            _browseCommand ??= new DelegateCommand(() =>
            {
                OpenFileDialog o = new OpenFileDialog();
                if (o.ShowDialog() == true)
                {
                    SelectedRecordingPath = o.FileName;
                    IsUploadButtonEnabled = true;
                }
            });

        private DelegateCommand _uploadRecordingCommand;
        public DelegateCommand UploadRecordingCommand =>
            _uploadRecordingCommand ??= new DelegateCommand(async () =>
            {
                RecordingMatrix = await _recordingsService.ReadRecordingFile(SelectedRecordingPath);
                NumberOfChannels = RecordingMatrix.GetLength(0);
                RecordingLength = RecordingMatrix.GetLength(1) / NumHz;
                IsUploaded = true;
            });

        private DelegateCommand _openRawDataDialogCommand;
        public DelegateCommand OpenRawDataDialogCommand =>
            _openRawDataDialogCommand ??= new DelegateCommand(() =>
             {
                 var parameters = new DialogParameters
                 {
                     {"record", RecordingMatrix },
                     {"hz", NumHz },
                     {"channels", NumberOfChannels }
                 };
                 _dialogService.ShowDialog(nameof(RawDataView), parameters, r => { });
             });

        private DelegateCommand _openTrainModelDialogCommand;
        public DelegateCommand OpenTrainModelDialogCommand =>
            _openTrainModelDialogCommand ??= new DelegateCommand(() =>
            {
                _dialogService.ShowDialog(nameof(TrainModelView));
            });

        private DelegateCommand<object> _waveSelectionChangedCommand;
        public DelegateCommand<object> WaveSelectionChangedCommand =>
            _waveSelectionChangedCommand ??= new DelegateCommand<object>(wave =>
            {
                SelectedWaveType = (WaveType)wave;
                BuildPartialWaveGraph();
            });

        private DelegateCommand _detectWavesCommand;
        public DelegateCommand DetectWavesCommand =>
            _detectWavesCommand ??= new DelegateCommand(async () =>
            {
                await BuildWelchGraph();
                BuildWaveHistogramGraph();
            });

        private DelegateCommand _divideAndWelchCommand;
        public DelegateCommand DivideAndWelchCommand =>
            _divideAndWelchCommand ??= new DelegateCommand(async () =>
            {
                wavesArrayList.Clear();
                for (int i = 0; i < NumberOfParts; i++)
                {
                    var row = GetRow(RecordingMatrix, Channel);
                    (double[] freqs, double[] psd) = await Welch(row.Skip(i * row.Length / NumberOfParts).Take(row.Length / NumberOfParts).ToArray(), SecondsForWelch, NumHz);
                    wavesArrayList.Add(new double[5]);
                    for (int j = 0; j < psd.Length; j++)
                    {
                        if (freqs[j] < 4) wavesArrayList[i][0] += psd[j];
                        else if (freqs[j] >= 4 && freqs[j] <= 7) wavesArrayList[i][1] += psd[j];
                        else if (freqs[j] >= 8 && freqs[j] <= 15) wavesArrayList[i][2] += psd[j];
                        else if (freqs[j] >= 16 && freqs[j] <= 31) wavesArrayList[i][3] += psd[j];
                        else if (freqs[j] >= 32) wavesArrayList[i][4] += psd[j];
                    }
                }
                BuildPartialWaveGraph();
            });
        private List<double[]> wavesArrayList = new List<double[]>();

        #endregion


        public double[] GetRow(double[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

        private void BuildPartialWaveGraph()
        {
            var model = new PlotModel();
            var series = new LineSeries() { Title = "Wave partial disturbution", Color = waveColorDictionary[SelectedWaveType]};
            for (int i = 0; i < wavesArrayList.Count; i++)
            {
                series.Points.Add(new DataPoint(i, wavesArrayList[i][(int)SelectedWaveType]));
            }
            model.Series.Add(series);
            PartialWavesModel = model;
        }
        private void BuildWaveHistogramGraph()
        {
            WavesModel = new PlotModel();
            string[] names = new string[] { "Delta", "Theta", "Alpha", "Beta", "Gamma" };
            var model = new PlotModel();
            var series = new PieSeries() { Title = "Wave Disturbution", InsideLabelColor = OxyColors.White };
            for (int i = 0; i < wavesArray.Length; i++)
            {
                series.Slices.Add(new PieSlice(names[i], wavesArray[i]));
            }
            model.Series.Add(series);
            WavesModel = model;
        }

        private async Task BuildWelchGraph()
        {
            (double[] freqs, double[] psd) = await Welch(RecordingMatrix, Channel, SecondsForWelch, NumHz);
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

        private async Task<(double[], double[])> Welch(double[,] recordingMatrix, int channel, int secondsForWelch, int numHz)
        {
            return await _httpService.Welch(recordingMatrix, channel, secondsForWelch, numHz);
        }

        private async Task<(double[], double[])> Welch(double[] array, int secondsForWelch, int numHz)
        {
            return await _httpService.Welch(array, secondsForWelch, numHz);
        }



        private double[] wavesArray = new double[5]; //delta,theta,alpha,beta,gamma

        private Dictionary<WaveType, OxyColor> waveColorDictionary = new Dictionary<WaveType, OxyColor>()
        {
            {WaveType.Delta, OxyColor.FromRgb(0,255,0)},
            {WaveType.Gamma, OxyColor.FromRgb(255,0,0)},
            {WaveType.Beta, OxyColor.FromRgb(0,0,255)},
            {WaveType.Theta, OxyColor.FromRgb(209,195,0)},
            {WaveType.Alpha, OxyColor.FromRgb(181,26,4)}
        };

    }

}

