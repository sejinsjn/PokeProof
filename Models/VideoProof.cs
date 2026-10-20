using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace BarusuProofOrganizer.Models
{
    public class VideoProof : INotifyPropertyChanged
    {
        private double progress;
        private string status = "Wartend";
        private string statusKey = "statusWaiting";
        private string errorMessage = string.Empty;

        public string OldFilePath { get; set; }
        public string MuteFilePath { get; set; }
        public string NewFilePath { get; set; }
        public string TradeHistory { get; set; }
        public bool Compress { get; set; }
        public string GraphicsCard { get; set; }
        public bool ExtractSummary { get; set; }

        public string SourceFileName => Path.GetFileName(OldFilePath);

        public double Progress
        {
            get => progress;
            set
            {
                if (Math.Abs(progress - value) < 0.1)
                {
                    return;
                }

                progress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string ProgressText => $"{Progress:0}%";

        public string Status
        {
            get => status;
            private set
            {
                if (status == value)
                {
                    return;
                }

                status = value;
                OnPropertyChanged();
            }
        }

        public string StatusKey => statusKey;

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (errorMessage == value)
                {
                    return;
                }

                errorMessage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public VideoProof()
        {
        }

        public VideoProof(string oldFilePath, string muteFilePath, string newFilePath, string tradeHistory, bool compress)
        {
            OldFilePath = oldFilePath;
            MuteFilePath = muteFilePath;
            NewFilePath = newFilePath;
            TradeHistory = tradeHistory;
            Compress = compress;
        }

        public void SetStatus(string key, string text)
        {
            statusKey = key;
            OnPropertyChanged(nameof(StatusKey));
            Status = text;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
