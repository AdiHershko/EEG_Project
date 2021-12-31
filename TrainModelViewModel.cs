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
        private string _adhdFolderPath = @"C:\Users\warnn\Desktop\חישה\convert_mat_to_csv_adhd";
        public string AdhdFolderPath
        {
            get => _adhdFolderPath;
            set => SetProperty(ref _adhdFolderPath, value);
        }

        private string _controlFolderPath = @"C:\Users\warnn\Desktop\חישה\convert_mat_to_csv_control";
        public string ControlFolderPath
        {
            get => _controlFolderPath;
            set => SetProperty(ref _controlFolderPath, value);
        }

        private int _numHz = 127;
        public int NumHz
        {
            get => _numHz;
            set => SetProperty(ref _numHz, value);
        }

        private int _numberOfParts = 13;
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


        #endregion

        #region commands
        private DelegateCommand _browseAdhdCommand;
        public DelegateCommand BrowseAdhdCommand =>
            _browseAdhdCommand ??= new DelegateCommand(() =>
            {
               
            });
        private DelegateCommand _browseControlCommand;

        public DelegateCommand BrowseControlCommand =>
            _browseControlCommand ??= new DelegateCommand(() =>
            {
               
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
                        List<List<double[]>> data = new List<List<double[]>>();
                        //for (int i = 0; i < NumberOfChannels; i++)
                        //{
                            var l = new List<double[]>();
                            for (int j = 0; j < NumberOfParts; j++)
                            {
                                var row = GetRow(recordingMatrix, 5 /*selected channel*/);
                                (double[] freqs, double[] psd) = await _httpService.Welch(row.Skip(j * row.Length / NumberOfParts).Take(row.Length / NumberOfParts).ToArray(), SecondsForWelch, NumHz);
                                l.Add(new double[5]);
                                for (int k = 0; k < psd.Length; k++)
                                {
                                    if (freqs[k] < 4) l[j][0] += psd[k];
                                    else if (freqs[k] >= 4 && freqs[k] <= 7) l[j][1] += psd[k];
                                    else if (freqs[k] >= 8 && freqs[k] <= 15) l[j][2] += psd[k];
                                    else if (freqs[k] >= 16 && freqs[k] <= 31) l[j][3] += psd[k];
                                    else if (freqs[k] >= 32) l[j][4] += psd[k];
                                }
                            //}
                            data.Add(l);
                        }
                        using (StreamWriter sw = new StreamWriter(@$"C:\Users\warnn\Desktop\data\adhd\{fileCounter++}.csv"))
                        {
                            for (int i = 0; i < l.Count; i++)
                            {
                                //for (int j = 0; j < data[i].Count; j++)
                                //{
                                //for (int k = 0; k < data[i][j].Length; k++)
                                //{
                                //sw.Write($"{data[i][j][3]},");
                                sw.Write($"{l[i][3]},");

                                //}
                                //}
                                //sw.WriteLine();
                            }
                        }
                    }
                }
                using (StreamWriter sw = new StreamWriter(@$"C:\Users\warnn\Desktop\data\adhd\all\all.csv"))
                {
                    foreach (var file in Directory.GetFiles(@"C:\Users\warnn\Desktop\data\adhd"))
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
                        List<List<double[]>> data = new List<List<double[]>>();
                        //for (int i = 0; i < NumberOfChannels; i++)
                        //{
                        var l = new List<double[]>();
                        for (int j = 0; j < NumberOfParts; j++)
                        {
                            var row = GetRow(recordingMatrix, 5 /*selected channel*/);
                            (double[] freqs, double[] psd) = await _httpService.Welch(row.Skip(j * row.Length / NumberOfParts).Take(row.Length / NumberOfParts).ToArray(), SecondsForWelch, NumHz);
                            l.Add(new double[5]);
                            for (int k = 0; k < psd.Length; k++)
                            {
                                if (freqs[k] < 4) l[j][0] += psd[k];
                                else if (freqs[k] >= 4 && freqs[k] <= 7) l[j][1] += psd[k];
                                else if (freqs[k] >= 8 && freqs[k] <= 15) l[j][2] += psd[k];
                                else if (freqs[k] >= 16 && freqs[k] <= 31) l[j][3] += psd[k];
                                else if (freqs[k] >= 32) l[j][4] += psd[k];
                            }
                            //}
                            data.Add(l);
                        }
                        using (StreamWriter sw = new StreamWriter(@$"C:\Users\warnn\Desktop\data\control\{fileCounter++}.csv"))
                        {
                            for (int i = 0; i < l.Count; i++)
                            {
                                //for (int j = 0; j < data[i].Count; j++)
                                //{
                                    //for (int k = 0; k < data[i][j].Length; k++)
                                   // {
                                        sw.Write($"{l[i][3]},");
                                    //}
                                //}
                                //sw.WriteLine();
                            }
                        }
                    }
                }
                using (StreamWriter sw = new StreamWriter(@$"C:\Users\warnn\Desktop\data\control\all\all.csv"))
                {
                    foreach (var file in Directory.GetFiles(@"C:\Users\warnn\Desktop\data\control"))
                    {
                        sw.WriteLine(new StreamReader(file).ReadToEnd());
                    }
                }
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
