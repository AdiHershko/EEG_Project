using EEG_Project.Services;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;

namespace EEG_Project
{
    public class MainWindowViewModel : BindableBase
    {
        private const int NUM_HZ = 128;
        RecordingsService _recordingsService;
        public MainWindowViewModel()
        {
            _recordingsService = new RecordingsService();
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

        private int[,] _recordingMatrix;
        public int[,] RecordingMatrix
        {
            get => _recordingMatrix;
            set => SetProperty(ref _recordingMatrix, value);
        }

        private PlotModel _model = new PlotModel();
        public PlotModel Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        private int _numOfSegments = 1;
        public int NumOfSegments
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
                _graphRange = new int[NumOfSegments];
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
            get => _uploadRecordingCommand ?? new DelegateCommand(() =>
            {
                var matrix = _recordingsService.ReadRecordingFile(SelectedRecordingPath);
                RecordingMatrix = Transpose(matrix);
                var temp = new PlotModel();
                Model.Title = "Recording";
                for (int i = 0; i < RecordingMatrix.GetLength(0); i++)
                {
                    var series = new LineSeries() { Title = "Channel " + i };
                    for (int j = (RecordingMatrix.GetLength(1) / NumOfSegments) * SegmentStart; j <= (RecordingMatrix.GetLength(1) / NumOfSegments) * (SegmentStart + 1); j += NUM_HZ)
                    {
                        series.Points.Add(new DataPoint(j / NUM_HZ, RecordingMatrix[i, j]));
                    }
                    temp.Series.Add(series);
                }
                Model = temp;
            });
        }
        private DelegateCommand<object> _graphRangeSelectionChangedCommand;
        public DelegateCommand<object> GraphRangeSelectionChangedCommand
        {
            get => _graphRangeSelectionChangedCommand ?? new DelegateCommand<object>((index) =>
            {
                SegmentStart = (int)index;
            });
        }
        #endregion

        private int[,] Transpose(int[,] matrix)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);

            int[,] result = new int[h, w];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    result[j, i] = matrix[i, j];
                }
            }

            return result;
        }
    }

}

