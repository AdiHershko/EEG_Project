using EEG_Project.Services;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static EEG_Project.Commons.WaveTypes;

namespace EEG_Project
{
    public class TrainModelViewModel : BindableBase, IDialogAware
    {
        private readonly IHttpService _httpService;
        private readonly IRecordingsService _recordingsService;


        public TrainModelViewModel(IHttpService httpService, IRecordingsService recordingsService)
        {
            _httpService = httpService;
            _recordingsService = recordingsService;
        }

        #region properties
        private string _adhdFolderPath = @"convert_mat_to_csv_adhd";
        public string AdhdFolderPath
        {
            get => _adhdFolderPath;
            set => SetProperty(ref _adhdFolderPath, value);
        }

        private string _controlFolderPath = @"convert_mat_to_csv_control";
        public string ControlFolderPath
        {
            get => _controlFolderPath;
            set => SetProperty(ref _controlFolderPath, value);
        }

        private string _selectedClassificationFilePath;
        public string SelectedClassificationFilePath
        {
            get => _selectedClassificationFilePath;
            set => SetProperty(ref _selectedClassificationFilePath, value);
        }

        private int _numHz = 127;
        public int NumHz
        {
            get => _numHz;
            set => SetProperty(ref _numHz, value);
        }

        private int _numberOfParts = 15;
        public int NumberOfParts
        {
            get => _numberOfParts;
            set => SetProperty(ref _numberOfParts, value);
        }

        private int _secondsForWelch = 4;
        public int SecondsForWelch
        {
            get => _secondsForWelch;
            set => SetProperty(ref _secondsForWelch, value);
        }

        private int _selectedTrainingChannel = 0;
        public int SelectedTrainingChannel
        {
            get => _selectedTrainingChannel;
            set => SetProperty(ref _selectedTrainingChannel, value);
        }

        private string _isAdhd = "";
        public string IsAdhd
        {
            get => _isAdhd;
            set => SetProperty(ref _isAdhd, value);
        }

        private WaveType _selectedTrainingWaveType = WaveType.Beta;
        public WaveType SelectedTrainingWaveType
        {
            get => _selectedTrainingWaveType;
            set => SetProperty(ref _selectedTrainingWaveType, value);
        }

        public IReadOnlyList<WaveType> WaveTypes { get; } = EnumsNET.Enums.GetValues<WaveType>();
        #endregion

        #region commands
        private DelegateCommand _browseAdhdCommand;
        public DelegateCommand BrowseAdhdCommand =>
            _browseAdhdCommand ??= new DelegateCommand(() =>
            {
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                var res = (bool)dialog.ShowDialog();
                if(res)
                {
                    AdhdFolderPath = dialog.SelectedPath;
                }
            });
        private DelegateCommand _browseControlCommand;

        public DelegateCommand BrowseControlCommand =>
            _browseControlCommand ??= new DelegateCommand(() =>
            {
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                var res = (bool)dialog.ShowDialog();
                if (res)
                {
                    ControlFolderPath = dialog.SelectedPath;
                }
            });


        private DelegateCommand _processAdhdCommand;
        public DelegateCommand ProcessAdhdCommand =>
            _processAdhdCommand ??= new DelegateCommand(async () =>
            {
                var files = Directory.GetFiles(AdhdFolderPath);
                int fileCounter = 0;
                foreach (var file in files)
                {
                    using (var sr = new StreamReader(file))
                    {
                        var recordingMatrix = await _recordingsService.ReadRecordingFile(file);
                        List<double[]> data = new List<double[]>();
                        for (int j = 0; j < NumberOfParts; j++)
                        {
                            var l = new double[5];
                            var row = GetRow(recordingMatrix, SelectedTrainingChannel);
                            (double[] freqs, double[] psd) = await _httpService.Welch(row.Skip(j * row.Length / NumberOfParts).Take(row.Length / NumberOfParts).ToArray(), SecondsForWelch, NumHz);
                            for (int k = 0; k < psd.Length; k++)
                            {
                                if (freqs[k] < 4) l[(int)WaveType.Delta] += psd[k];
                                else if (freqs[k] >= 4 && freqs[k] <= 7) l[(int)WaveType.Theta] += psd[k];
                                else if (freqs[k] >= 8 && freqs[k] <= 15) l[(int)WaveType.Alpha] += psd[k];
                                else if (freqs[k] >= 16 && freqs[k] <= 31) l[(int)WaveType.Beta] += psd[k];
                                else if (freqs[k] >= 32) l[(int)WaveType.Gamma] += psd[k];
                            }
                            data.Add(l);
                        }
                        using (StreamWriter sw = new StreamWriter(@$"data\adhd\{fileCounter++}.csv"))
                        {
                            for (int i = 0; i < NumberOfParts; i++)
                            {
                                sw.Write($"{data[i][(int)SelectedTrainingWaveType]},");
                            }
                            sw.Write("1"); //1 for adhd label
                        }
                    }
                }
                using (StreamWriter sw = new StreamWriter(@$"data\adhd\all\allAdhd.csv"))
                {
                    foreach (var file in Directory.GetFiles(@"data\adhd"))
                    {
                        sw.WriteLine(new StreamReader(file).ReadToEnd());
                    }
                }
            });


        private DelegateCommand _processControlCommand;
        public DelegateCommand ProcessControlCommand =>
            _processControlCommand ??= new DelegateCommand(async () =>
            {
                var files = Directory.GetFiles(ControlFolderPath);
                int fileCounter = 0;
                foreach (var file in files)
                {
                    using (var sr = new StreamReader(file))
                    {
                        var recordingMatrix = await _recordingsService.ReadRecordingFile(file);
                        List<double[]> data = new List<double[]>();
                        for (int j = 0; j < NumberOfParts; j++)
                        {
                            var l = new double[5];
                            var row = GetRow(recordingMatrix, SelectedTrainingChannel);
                            (double[] freqs, double[] psd) = await _httpService.Welch(row.Skip(j * row.Length / NumberOfParts).Take(row.Length / NumberOfParts).ToArray(), SecondsForWelch, NumHz);
                            for (int k = 0; k < psd.Length; k++)
                            {
                                if (freqs[k] < 4) l[(int)WaveType.Delta] += psd[k];
                                else if (freqs[k] >= 4 && freqs[k] <= 7) l[(int)WaveType.Theta] += psd[k];
                                else if (freqs[k] >= 8 && freqs[k] <= 15) l[(int)WaveType.Alpha] += psd[k];
                                else if (freqs[k] >= 16 && freqs[k] <= 31) l[(int)WaveType.Beta] += psd[k];
                                else if (freqs[k] >= 32) l[(int)WaveType.Gamma] += psd[k];
                            }
                            data.Add(l);
                        }
                        using (StreamWriter sw = new StreamWriter(@$"data\control\{fileCounter++}.csv"))
                        {
                            for (int i = 0; i < NumberOfParts; i++)
                            {

                                sw.Write($"{data[i][(int)SelectedTrainingWaveType]},");
                            }
                            sw.Write("0"); //0 for non-adhd label
                        }
                    }
                }
                using (StreamWriter sw = new StreamWriter(@$"data\control\all\allControl.csv"))
                {
                    foreach (var file in Directory.GetFiles(@"data\control"))
                    {
                        sw.WriteLine(new StreamReader(file).ReadToEnd());
                    }
                }
            });


        private DelegateCommand _trainCommand;
        public DelegateCommand TrainCommand =>
            _trainCommand ??= new DelegateCommand(() =>
            {
                _httpService.Train(NumberOfParts);
            });

        private DelegateCommand _browseClassificationFileCommand;
        public DelegateCommand BrowseClassificationFileCommand =>
            _browseClassificationFileCommand ??= new DelegateCommand(() =>
            {

                OpenFileDialog o = new OpenFileDialog();
                if (o.ShowDialog() == true)
                {
                    SelectedClassificationFilePath = o.FileName;
                    //IsUploadButtonEnabled = true;
                }
            });

        private DelegateCommand _classifyCommand;
        public DelegateCommand ClassifyCommand =>
            _classifyCommand ??= new DelegateCommand(async () =>
            {
                IsAdhd = "";
                List<double> data = new List<double>();
                using (var sr = new StreamReader(SelectedClassificationFilePath))
                {
                    var recordingMatrix = await _recordingsService.ReadRecordingFile(SelectedClassificationFilePath);
                    var row = GetRow(recordingMatrix, SelectedTrainingChannel);
                    for (int j = 0; j < NumberOfParts; j++)
                    {
                        var l = new double[5];
                        (double[] freqs, double[] psd) = await _httpService.Welch(row.Skip(j * row.Length / NumberOfParts).Take(row.Length / NumberOfParts).ToArray(), SecondsForWelch, NumHz);
                        for (int k = 0; k < psd.Length; k++)
                        {
                            if (freqs[k] < 4) l[0] += psd[k];
                            else if (freqs[k] >= 4 && freqs[k] <= 7) l[1] += psd[k];
                            else if (freqs[k] >= 8 && freqs[k] <= 15) l[2] += psd[k];
                            else if (freqs[k] >= 16 && freqs[k] <= 31) l[3] += psd[k];
                            else if (freqs[k] >= 32) l[4] += psd[k];
                        }
                        data.Add(l[(int)SelectedTrainingWaveType]);
                    }
                }
                IsAdhd = await _httpService.Predict(data.ToArray());
            });

        public string Title => "Train";


        #endregion

        public double[] GetRow(double[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }


        #region IDialogAware
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {

        }

        public event Action<IDialogResult> RequestClose;

        #endregion
    }
}
