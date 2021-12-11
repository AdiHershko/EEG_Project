using OxyPlot;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EEG_Project
{
    public class RawDataViewModel : BindableBase, IDialogAware
    {

        #region IDialogAware
        public string Title => "Raw data";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            RecordingMatrix = parameters.GetValue<double[,]>("record");
            NumHz = parameters.GetValue<int>("hz");
            var numChannels = parameters.GetValue<int>("channels");
            Channels = new int[numChannels];
            for (int i = 0; i < numChannels; i++)
                Channels[i] = i;
            BuildRawRecordingGraph();
        }
        #endregion

        #region Properties
        private PlotModel _ratDataModel = new PlotModel();
        public PlotModel RawDataModel
        {
            get => _ratDataModel;
            set => SetProperty(ref _ratDataModel, value);
        }
        private int _totalColums;


        private double[,] _recordingMatrix;
        public double[,] RecordingMatrix
        {
            get => _recordingMatrix;
            set => SetProperty(ref _recordingMatrix, value);
        }
        private int _secondsPerSegment = 1;
        public int SecondsPerSegment
        {
            get => _secondsPerSegment;
            set
            {
                SetProperty(ref _secondsPerSegment, value);
                RaisePropertyChanged(nameof(GraphRange));
            }
        }
        private int _segmentStart;
        public int SegmentStart
        {
            get => _segmentStart;
            set => SetProperty(ref _segmentStart, value);
        }

        private int[] _channels;
        public int[] Channels
        {
            get => _channels;
            set => SetProperty(ref _channels, value);
        }

        private int _selectedChannel;
        public int SelectedChannel
        {
            get => _selectedChannel;
            set => SetProperty(ref _selectedChannel, value);
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
        private int _numHz;
        public int NumHz
        {
            get => _numHz;
            set => SetProperty(ref _numHz, value);
        }
        #endregion

        #region Commands

        private DelegateCommand<object> _graphRangeSelectionChangedCommand;
        public DelegateCommand<object> GraphRangeSelectionChangedCommand
        {
            get => _graphRangeSelectionChangedCommand ?? new DelegateCommand<object>((index) =>
            {
                SegmentStart = (int)index;
            });
        }
        private DelegateCommand _buildGraphCommand;
        public DelegateCommand BuildGraphCommand
        {
            get => _buildGraphCommand ?? new DelegateCommand(() =>
            {
                BuildRawRecordingGraph();
            });
        }
        #endregion

        #region private methods
        private void BuildRawRecordingGraph()
        {
            var temp = new PlotModel();
            RawDataModel.Title = "Recording";
            _totalColums = RecordingMatrix.GetLength(1);
            var series = new LineSeries() { Title = "Channel " + SelectedChannel };
            for (int j = SegmentStart * SecondsPerSegment * NumHz; j < ((SegmentStart + 1) * SecondsPerSegment) * NumHz; j++)
            {
                series.Points.Add(new DataPoint(j, RecordingMatrix[SelectedChannel, j]));
            }
            temp.Series.Add(series);
            RawDataModel = temp;
            RaisePropertyChanged(nameof(RawDataModel));
        }
        #endregion
    }
}
