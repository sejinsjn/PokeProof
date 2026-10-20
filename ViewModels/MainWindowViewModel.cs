using BarusuProofOrganizer.Models;
using FFMpegCore.Enums;
using FFMpegCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MVVMCore;

namespace PokeSqueeze.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public CommandBase OpenFilesCommand { get; set; } = null;
        public CommandBase AddToQueueCommand { get; set; } = null;
        public CommandBase StartProcessCommand { get; set; } = null;
        public CommandBase DeleteItemCommand { get; set; } = null;
        public List<string> Files { get; set; } = new List<string>();
        public List<VideoProof> Queue { get; set; } = new List<VideoProof>();
        public List<VideoProof> QueueCopy { get; set; } = new List<VideoProof>();
        public Boolean CreateFolder { get; set; } = false;

        private bool addTradeHistory = false;
        public Boolean AddTradeHistory
        {
            get => addTradeHistory;
            set
            {
                addTradeHistory = value;
                if (addTradeHistory) CreateFolder = true;
                OnPropertyChanged(nameof(CreateFolder));
                OnPropertyChanged(nameof(AddTradeHistory));
            }
        }

        private Boolean renameVideos;
        public Boolean RenameVideos
        {
            get { return renameVideos; }
            set
            {
                renameVideos = value;
                OnPropertyChanged(nameof(RenameVideos));
            }
        }
        public Boolean CompressVideos { get; set; } = false;
        public string BeginningNumber { get; set; } = "12";
        public string TernaryNumberStart { get; set; } = "0";
        public string FileEnding { get; set; } = ".mp4";
        public string TradeHistory { get; set; } = "";
        public int TernaryNumberTracking { get; set; } = 0;

        private List<string> graphicsCardList = new List<string>() { "AMD", "Nvidia"};

        public List<string> GraphicsCardList
        {
            get => graphicsCardList;
            set
            {
                graphicsCardList = value;
            }
        }

        private string graphicsCard = "AMD"; 
        public string GraphicsCard
        {
            get => graphicsCard;
            set
            {
                graphicsCard = value;
            }
        }

        private VideoProof selectedVideoProof;

        public VideoProof SelectedVideoProof
        {
            get { return selectedVideoProof; }
            set
            {
                selectedVideoProof = value;
                OnPropertyChanged(nameof(SelectedVideoProof));
            }
        }
        private Thread thread { get; set; }

        public override void Initialize(object data)
        {
            OpenFilesCommand = new CommandBase(ExecuteOpenFilesCommand);
            AddToQueueCommand = new CommandBase(ExecuteAddToQueueCommand);
            StartProcessCommand = new CommandBase(ExecuteStartProcessCommand);
            DeleteItemCommand = new CommandBase(ExecuteDeleteItemCommand);
        }

        public void ExecuteOpenFilesCommand(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files(*.*) | *.*";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
                Files = openFileDialog.FileNames.ToList();
        }

        public void ExecuteAddToQueueCommand(object obj)
        {
            if (Files != null && Files.Count > 0)
            {
                lock (Queue)
                {
                    TernaryNumberTracking = Int32.Parse(TernaryNumberStart);

                    foreach (string file in Files)
                    {
                        string directoryOutput = Path.GetDirectoryName(file) + "\\Output";

                        if (File.Exists(file))
                        {
                            string fileName = Path.GetFileName(file); 
                            string muteFilePath = Path.Combine(directoryOutput, fileName);
                            string tradeHistory = null;
                            bool compressVideo = false;

                            if (RenameVideos) fileName = CreateFileName(BeginningNumber, ToTernary(TernaryNumberTracking), FileEnding);
                            if (CreateFolder) directoryOutput = Path.Combine(directoryOutput, Path.GetFileNameWithoutExtension(fileName));
                            if (AddTradeHistory) tradeHistory = TradeHistory;
                            if (CompressVideos) compressVideo = true;

                            VideoProof videoProof = new VideoProof(file, muteFilePath, directoryOutput + "\\" + fileName, tradeHistory, compressVideo);

                            Queue.Add(videoProof);
                            TernaryNumberTracking++;
                        }
                    }

                    QueueCopy = new List<VideoProof>(Queue);
                    Files.Clear();
                }
            }

            OnPropertyChanged(nameof(QueueCopy));
        }

        public void ExecuteStartProcessCommand(object obj)
        {
            if (thread == null || !thread.IsAlive)
            {
                thread = new Thread(StartProcess);
                thread.Start();
            }
        }

        public void StartProcess()
        {
            if (Queue != null && Queue.Count > 0)
            {
                lock (Queue)
                {
                    QueueCopy = new List<VideoProof>(Queue);
                    OnPropertyChanged(nameof(QueueCopy));
                }
                foreach (VideoProof videoProof in QueueCopy)
                {
                    if (File.Exists(videoProof.OldFilePath) && !File.Exists(videoProof.NewFilePath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(videoProof.OldFilePath) + "\\Output");
                        Directory.CreateDirectory(Path.GetDirectoryName(videoProof.NewFilePath));

                        if (videoProof.TradeHistory != null && CreateFolder)
                            using (StreamWriter sw = File.CreateText(videoProof.NewFilePath.Replace(Path.GetFileName(videoProof.NewFilePath), "") + "TradeHistory.txt"))
                            {
                                sw.Write(videoProof.TradeHistory); 
                            }

                        if (videoProof.Compress)
                        {
                            FFmpegInitializer.InitializeFFmpeg();
                            if (GraphicsCard.Contains("Nvidia")) 
                            {
                                FFMpeg.Mute(videoProof.OldFilePath, videoProof.MuteFilePath);
                                FFMpegArguments
                                    .FromFileInput(videoProof.MuteFilePath)
                                    .OutputToFile(videoProof.NewFilePath, false, options => options
                                        .WithCustomArgument("-c:v hevc_nvenc")       // GPU Encoding
                                        .WithCustomArgument("-preset p7")            // HQ Preset
                                        .WithCustomArgument("-cq 22")                // Qualitätsfaktor (niedriger = besser)
                                        .WithCustomArgument("-b:v 0")                // Bitrate ignored in CQ mode
                                        .WithCustomArgument("-maxrate 2M")           // Optional: deckelt die Bitrate
                                        .WithCustomArgument("-bufsize 4M")           // Puffergröße
                                        .WithCustomArgument("-profile:v main")       // Kompatibilität
                                        .WithVideoFilters(filterOptions => filterOptions
                                            .Scale(VideoSize.FullHd))
                                        .WithAudioCodec("aac")                       // Komprimiertes Audio
                                        .WithAudioBitrate(128000)                    // 128 kbps AAC
                                        .WithFastStart())
                                    .ProcessSynchronously();

                                File.Delete(videoProof.MuteFilePath);
                                Queue.Remove(videoProof);
                            }
                            else
                            {
                                FFMpegArguments
                                    .FromFileInput(videoProof.OldFilePath)
                                    .OutputToFile(videoProof.NewFilePath, false, options => options
                                        .WithCustomArgument("-c:v hevc_amf")
                                        .WithCustomArgument("-usage high_quality")
                                        .WithCustomArgument("-quality quality")
                                        .WithCustomArgument("-rc qvbr")
                                        .WithCustomArgument("-qvbr_quality_level 51")
                                        .WithCustomArgument("-fps_mode passthrough")
                                        .WithCustomArgument("-an")
                                        .WithFastStart())
                                    .ProcessSynchronously();

                                Queue.Remove(videoProof);
                            }

                            
                        }
                        else
                        {
                            Queue.Remove(videoProof);
                            File.Copy(videoProof.OldFilePath, videoProof.NewFilePath);
                        }

                    }

                    lock (Queue)
                    {
                        QueueCopy = new List<VideoProof>(Queue);
                        OnPropertyChanged(nameof(QueueCopy));
                    }
                }
            }
        }

        public void ExecuteDeleteItemCommand(object obj)
        {
            if (obj is VideoProof videoProof)
            {
                Queue.Remove(videoProof);

                lock (Queue)
                {
                    QueueCopy = new List<VideoProof>(Queue);
                    OnPropertyChanged(nameof(QueueCopy));
                }
            }
        }

        public static String ToTernary(int value)
        {
            if (value == 0)
                return "0";

            StringBuilder Sb = new StringBuilder();
            Boolean signed = false;

            if (value < 0)
            {
                signed = true;
                value = -value;
            }

            while (value > 0)
            {
                Sb.Insert(0, value % 3);
                value /= 3;
            }

            if (signed)
                Sb.Insert(0, '-');

            return Sb.ToString();
        }

        public static String CreateFileName(string beginning, string ternaryNumber, string fileEnding)
        {
            StringBuilder Sb = new StringBuilder();

            Sb.Append(beginning);

            if (beginning.Length == 1) Sb.Append("0");

            switch (ternaryNumber.Length)
            {
                case 1:
                    Sb.Append("000"); break;
                case 2:
                    Sb.Append("00"); break;
                case 3:
                    Sb.Append("0"); break;
            }

            Sb.Append(ternaryNumber);

            Sb.Append(fileEnding);

            return Sb.ToString();
        }

        public override void OnBeforeClose()
        {

        }
    }

}
