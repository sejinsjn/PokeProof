using BarusuProofOrganizer.Models;
using FFMpegCore;
using Microsoft.Win32;
using MVVMCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PokeSqueeze.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public CommandBase OpenFilesCommand { get; private set; }
        public CommandBase AddToQueueCommand { get; private set; }
        public CommandBase StartProcessCommand { get; private set; }
        public CommandBase DeleteItemCommand { get; private set; }
        public ObservableCollection<VideoProof> Queue { get; } = new ObservableCollection<VideoProof>();

        private List<string> files = new List<string>();
        private bool createFolder;
        private bool addTradeHistory;
        private bool renameVideos;
        private bool compressVideos;
        private string beginningNumber = "12";
        private string ternaryNumberStart = "0";
        private string fileEnding = ".mp4";
        private string tradeHistory = string.Empty;
        private string graphicsCard = "AMD";
        private bool isProcessing;
        private double queueProgress;
        private string queueStatus = "Wähle Videos aus, um zu beginnen.";

        public string SelectedFilesText => files.Count == 0
            ? "Noch keine Dateien ausgewählt"
            : files.Count == 1 ? "1 Datei ausgewählt" : $"{files.Count} Dateien ausgewählt";

        public int QueueCount => Queue.Count;

        public string QueueSummary
        {
            get
            {
                int finishedCount = Queue.Count(item => IsFinished(item.Status));
                return $"{finishedCount} von {Queue.Count} Videos bearbeitet";
            }
        }

        public double QueueProgress
        {
            get => queueProgress;
            private set
            {
                if (Math.Abs(queueProgress - value) < 0.1)
                {
                    return;
                }

                queueProgress = value;
                OnPropertyChanged();
            }
        }

        public string QueueStatus
        {
            get => queueStatus;
            private set
            {
                if (queueStatus == value)
                {
                    return;
                }

                queueStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsProcessing
        {
            get => isProcessing;
            private set
            {
                if (isProcessing == value)
                {
                    return;
                }

                isProcessing = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool CreateFolder
        {
            get => createFolder;
            set
            {
                if (createFolder == value)
                {
                    return;
                }

                createFolder = value;
                OnPropertyChanged();
            }
        }

        public bool AddTradeHistory
        {
            get => addTradeHistory;
            set
            {
                if (addTradeHistory == value)
                {
                    return;
                }

                addTradeHistory = value;
                if (addTradeHistory)
                {
                    CreateFolder = true;
                }

                OnPropertyChanged();
            }
        }

        public bool RenameVideos
        {
            get => renameVideos;
            set
            {
                if (renameVideos == value)
                {
                    return;
                }

                renameVideos = value;
                OnPropertyChanged();
            }
        }

        public bool CompressVideos
        {
            get => compressVideos;
            set
            {
                if (compressVideos == value)
                {
                    return;
                }

                compressVideos = value;
                OnPropertyChanged();
            }
        }

        public string BeginningNumber
        {
            get => beginningNumber;
            set
            {
                if (beginningNumber == value)
                {
                    return;
                }

                beginningNumber = value;
                OnPropertyChanged();
            }
        }

        public string TernaryNumberStart
        {
            get => ternaryNumberStart;
            set
            {
                if (ternaryNumberStart == value)
                {
                    return;
                }

                ternaryNumberStart = value;
                OnPropertyChanged();
            }
        }

        public string FileEnding
        {
            get => fileEnding;
            set
            {
                if (fileEnding == value)
                {
                    return;
                }

                fileEnding = value;
                OnPropertyChanged();
            }
        }

        public string TradeHistory
        {
            get => tradeHistory;
            set
            {
                if (tradeHistory == value)
                {
                    return;
                }

                tradeHistory = value;
                OnPropertyChanged();
            }
        }

        public List<string> GraphicsCardList { get; } = new List<string> { "AMD", "Nvidia" };

        public string GraphicsCard
        {
            get => graphicsCard;
            set
            {
                if (graphicsCard == value)
                {
                    return;
                }

                graphicsCard = value;
                OnPropertyChanged();
            }
        }

        public override void Initialize(object data)
        {
            OpenFilesCommand = new CommandBase(ExecuteOpenFilesCommand, _ => !IsProcessing);
            AddToQueueCommand = new CommandBase(ExecuteAddToQueueCommand, _ => !IsProcessing && files.Count > 0);
            StartProcessCommand = new CommandBase(
                ExecuteStartProcessCommand,
                _ => !IsProcessing && Queue.Any(item => item.Status == "Wartend" || item.Status == "Fehler"));
            DeleteItemCommand = new CommandBase(ExecuteDeleteItemCommand, _ => !IsProcessing);
        }

        public void ExecuteOpenFilesCommand(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Videodateien|*.mp4;*.mkv;*.mov;*.avi;*.webm;*.m4v|Alle Dateien|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                files = openFileDialog.FileNames.Where(File.Exists).ToList();
                OnPropertyChanged(nameof(SelectedFilesText));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public void ExecuteAddToQueueCommand(object obj)
        {
            if (files.Count == 0)
            {
                return;
            }

            if (!int.TryParse(TernaryNumberStart, out int ternaryNumber))
            {
                QueueStatus = "Die Startnummer muss eine gültige Zahl sein.";
                return;
            }

            foreach (string file in files)
            {
                string outputDirectory = Path.Combine(Path.GetDirectoryName(file), "Output");
                string fileName = Path.GetFileName(file);

                if (RenameVideos)
                {
                    fileName = CreateFileName(BeginningNumber, ToTernary(ternaryNumber), FileEnding);
                }

                if (CreateFolder)
                {
                    outputDirectory = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(fileName));
                }

                string newFilePath = Path.Combine(outputDirectory, fileName);
                VideoProof videoProof = new VideoProof(
                    file,
                    Path.Combine(outputDirectory, Path.GetFileName(file)),
                    newFilePath,
                    AddTradeHistory ? TradeHistory : null,
                    CompressVideos)
                {
                    GraphicsCard = GraphicsCard
                };

                Queue.Add(videoProof);
                ternaryNumber++;
            }

            files.Clear();
            OnPropertyChanged(nameof(SelectedFilesText));
            UpdateQueueSummary();
            QueueStatus = "Videos hinzugefügt. Du kannst weitere auswählen oder die Verarbeitung starten.";
            CommandManager.InvalidateRequerySuggested();
        }

        public void ExecuteStartProcessCommand(object obj)
        {
            if (IsProcessing)
            {
                return;
            }

            List<VideoProof> pendingVideos = Queue
                .Where(item => item.Status == "Wartend" || item.Status == "Fehler")
                .ToList();

            if (pendingVideos.Count == 0)
            {
                return;
            }

            foreach (VideoProof video in pendingVideos)
            {
                video.Status = "Wartend";
                video.Progress = 0;
                video.ErrorMessage = string.Empty;
            }

            IsProcessing = true;
            QueueStatus = $"Verarbeitung gestartet: {pendingVideos.Count} Video(s).";
            UpdateQueueSummary();
            Task.Run(() => ProcessQueue(pendingVideos));
        }

        private void ProcessQueue(IList<VideoProof> videos)
        {
            int successfulCount = 0;
            int skippedCount = 0;
            int failedCount = 0;

            for (int index = 0; index < videos.Count; index++)
            {
                VideoProof video = videos[index];
                UpdateOnUi(() =>
                {
                    video.Status = "Wird vorbereitet";
                    QueueStatus = $"Video {index + 1} von {videos.Count}: {video.SourceFileName}";
                });

                bool outputExisted = File.Exists(video.NewFilePath);

                try
                {
                    if (!File.Exists(video.OldFilePath))
                    {
                        throw new FileNotFoundException("Die Quelldatei wurde nicht gefunden.", video.OldFilePath);
                    }

                    if (outputExisted)
                    {
                        skippedCount++;
                        UpdateOnUi(() =>
                        {
                            video.Status = "Übersprungen (Zieldatei existiert)";
                            video.Progress = 100;
                            UpdateQueueSummary();
                        });
                        continue;
                    }

                    string outputDirectory = Path.GetDirectoryName(video.NewFilePath);
                    Directory.CreateDirectory(outputDirectory);

                    if (video.TradeHistory != null)
                    {
                        File.WriteAllText(Path.Combine(outputDirectory, "TradeHistory.txt"), video.TradeHistory);
                    }

                    if (video.Compress)
                    {
                        ProcessCompressedVideo(video);
                    }
                    else
                    {
                        UpdateOnUi(() => video.Status = "Wird kopiert");
                        CopyFileWithProgress(video);
                        UpdateOnUi(() => video.Progress = 100);
                    }

                    successfulCount++;
                    UpdateOnUi(() => video.Status = "Fertig");
                }
                catch (Exception exception)
                {
                    string errorMessage = exception.Message;
                    if (!outputExisted && File.Exists(video.NewFilePath))
                    {
                        try
                        {
                            File.Delete(video.NewFilePath);
                        }
                        catch (IOException cleanupException)
                        {
                            errorMessage += $" Unvollständige Ausgabedatei konnte nicht entfernt werden: {cleanupException.Message}";
                        }
                        catch (UnauthorizedAccessException cleanupException)
                        {
                            errorMessage += $" Unvollständige Ausgabedatei konnte nicht entfernt werden: {cleanupException.Message}";
                        }
                    }

                    failedCount++;
                    UpdateOnUi(() =>
                    {
                        video.Status = "Fehler";
                        video.ErrorMessage = errorMessage;
                    });
                }

                UpdateOnUi(UpdateQueueSummary);
            }

            UpdateOnUi(() =>
            {
                IsProcessing = false;
                QueueStatus = failedCount == 0
                    ? $"Fertig: {successfulCount} verarbeitet, {skippedCount} übersprungen."
                    : $"Abgeschlossen: {successfulCount} verarbeitet, {skippedCount} übersprungen, {failedCount} mit Fehlern.";
                UpdateQueueSummary();
            });
        }

        private void ProcessCompressedVideo(VideoProof video)
        {
            UpdateOnUi(() => video.Status = "Analysiere Video");
            FFmpegInitializer.InitializeFFmpeg();
            TimeSpan duration = FFProbe.Analyse(video.OldFilePath).Duration;

            FFMpegArgumentProcessor processor = FFMpegArguments
                .FromFileInput(video.OldFilePath)
                .OutputToFile(video.NewFilePath, false, options =>
                {
                    if (string.Equals(video.GraphicsCard, "Nvidia", StringComparison.OrdinalIgnoreCase))
                    {
                        options
                            .WithCustomArgument("-c:v hevc_nvenc")
                            .WithCustomArgument("-preset p7")
                            .WithCustomArgument("-cq 22")
                            .WithCustomArgument("-b:v 0")
                            .WithCustomArgument("-maxrate 2M")
                            .WithCustomArgument("-bufsize 4M")
                            .WithCustomArgument("-profile:v main")
                            .WithCustomArgument("-an");
                    }
                    else
                    {
                        options
                            .WithCustomArgument("-c:v hevc_amf")
                            .WithCustomArgument("-usage high_quality")
                            .WithCustomArgument("-quality quality")
                            .WithCustomArgument("-rc qvbr")
                            .WithCustomArgument("-qvbr_quality_level 51")
                            .WithCustomArgument("-fps_mode passthrough")
                            .WithCustomArgument("-an");
                    }

                    options.WithFastStart();
                });

            UpdateOnUi(() => video.Status = "Komprimiert");
            if (duration > TimeSpan.Zero)
            {
                processor.NotifyOnProgress(
                    percentage => UpdateOnUi(() =>
                    {
                        video.Progress = Math.Max(0, Math.Min(100, percentage));
                        UpdateQueueSummary();
                    }),
                    duration);
            }

            processor.ProcessSynchronously();
            UpdateOnUi(() =>
            {
                video.Progress = 100;
                UpdateQueueSummary();
            });
        }

        private void CopyFileWithProgress(VideoProof video)
        {
            const int bufferSize = 1024 * 1024;
            byte[] buffer = new byte[bufferSize];
            long totalBytes = new FileInfo(video.OldFilePath).Length;
            long copiedBytes = 0;

            using (FileStream source = new FileStream(video.OldFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream destination = new FileStream(video.NewFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destination.Write(buffer, 0, bytesRead);
                    copiedBytes += bytesRead;
                    double percentage = totalBytes == 0 ? 100 : copiedBytes * 100d / totalBytes;
                    UpdateOnUi(() =>
                    {
                        video.Progress = percentage;
                        UpdateQueueSummary();
                    });
                }
            }
        }

        public void ExecuteDeleteItemCommand(object obj)
        {
            if (IsProcessing || !(obj is VideoProof videoProof))
            {
                return;
            }

            Queue.Remove(videoProof);
            UpdateQueueSummary();
            if (Queue.Count == 0)
            {
                QueueStatus = "Wähle Videos aus, um zu beginnen.";
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void UpdateQueueSummary()
        {
            OnPropertyChanged(nameof(QueueCount));
            OnPropertyChanged(nameof(QueueSummary));

            if (Queue.Count == 0)
            {
                QueueProgress = 0;
                return;
            }

            QueueProgress = Queue.Average(item =>
                IsFinished(item.Status) ? 100 : Math.Max(0, Math.Min(100, item.Progress)));
        }

        private void UpdateOnUi(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        private static bool IsFinished(string status)
        {
            return status == "Fertig"
                || status == "Fehler"
                || status == "Übersprungen (Zieldatei existiert)";
        }

        public static string ToTernary(int value)
        {
            if (value == 0)
            {
                return "0";
            }

            StringBuilder result = new StringBuilder();
            bool signed = value < 0;
            value = Math.Abs(value);

            while (value > 0)
            {
                result.Insert(0, value % 3);
                value /= 3;
            }

            if (signed)
            {
                result.Insert(0, '-');
            }

            return result.ToString();
        }

        public static string CreateFileName(string beginning, string ternaryNumber, string fileEnding)
        {
            StringBuilder result = new StringBuilder();
            result.Append(beginning);

            if (beginning.Length == 1)
            {
                result.Append("0");
            }

            result.Append(ternaryNumber.PadLeft(4, '0'));
            result.Append(fileEnding);
            return result.ToString();
        }

        public override void OnBeforeClose()
        {
        }
    }
}
