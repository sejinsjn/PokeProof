using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tesseract;

namespace PokeSqueeze.Services
{
    public sealed class SummaryExtractionException : Exception
    {
        public SummaryExtractionException(string message)
            : base(message)
        {
        }

        public SummaryExtractionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public static class SummaryExtractor
    {
        private const int SampleDurationSeconds = 5;
        private const int SampleFrameCount = 5;

        public static string ReadLastScreenText(string videoPath)
        {
            string assemblyDirectory = Path.GetDirectoryName(typeof(SummaryExtractor).Assembly.Location);
            string dataPath = Path.Combine(assemblyDirectory, "tessdata");
            string tempDirectory = Path.Combine(Path.GetTempPath(), "PokeProof", Guid.NewGuid().ToString("N"));

            foreach (string language in new[] { "eng", "deu", "jpn", "kor" })
            {
                if (!File.Exists(Path.Combine(dataPath, language + ".traineddata")))
                {
                    throw new SummaryExtractionException("The OCR language data is missing: " + language);
                }
            }

            try
            {
                Directory.CreateDirectory(tempDirectory);
                FFmpegInitializer.InitializeFFmpeg();
                string ffmpegPath = Path.Combine(Path.GetTempPath(), "FFmpeg", "ffmpeg.exe");
                string framePattern = Path.Combine(tempDirectory, "frame-%02d.png");
                string arguments = "-hide_banner -loglevel error -nostdin -y -sseof -"
                    + SampleDurationSeconds
                    + " -i "
                    + QuoteArgument(videoPath)
                    + " -vf \"fps=1,crop=w='if(gt(ih,iw),floor(iw*0.92/2)*2,iw)':h='if(gt(ih,iw),floor(ih*0.32/2)*2,ih)':x='if(gt(ih,iw),floor(iw*0.04/2)*2,0)':y='if(gt(ih,iw),floor(ih*0.34/2)*2,0)',scale=iw*2:ih*2:flags=lanczos,format=gray,eq=contrast=1.25\" -frames:v "
                    + SampleFrameCount
                    + " "
                    + QuoteArgument(framePattern);

                ProcessStartInfo startInfo = new ProcessStartInfo(ffmpegPath, arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        throw new SummaryExtractionException("FFmpeg could not be started.");
                    }

                    string errorOutput = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new SummaryExtractionException(
                            "FFmpeg could not extract frames from the video. " + errorOutput.Trim());
                    }
                }

                string[] framePaths = Directory.GetFiles(tempDirectory, "frame-*.png")
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray();
                if (framePaths.Length == 0)
                {
                    throw new SummaryExtractionException("No frames could be extracted from the end of the video.");
                }

                return ReadBestFrameText(dataPath, framePaths);
            }
            catch (TesseractException exception)
            {
                throw new SummaryExtractionException("The OCR engine could not read the video frame.", exception);
            }
            catch (Win32Exception exception)
            {
                throw new SummaryExtractionException("The OCR or video processing tool could not be started.", exception);
            }
            catch (IOException exception)
            {
                throw new SummaryExtractionException("The video frame could not be read or saved.", exception);
            }
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

        private static string ReadBestFrameText(string dataPath, string[] framePaths)
        {
            string bestText = string.Empty;
            float bestConfidence = -1;

            using (TesseractEngine engine = new TesseractEngine(
                dataPath,
                "eng+deu+jpn+kor",
                EngineMode.LstmOnly))
            {
                foreach (string framePath in framePaths)
                {
                    using (Pix image = Pix.LoadFromFile(framePath))
                    using (Page page = engine.Process(image, PageSegMode.SparseText))
                    {
                        string text = page.GetText();
                        float confidence = page.GetMeanConfidence();
                        if (!string.IsNullOrWhiteSpace(text) && confidence > bestConfidence)
                        {
                            bestText = text.Trim();
                            bestConfidence = confidence;
                        }
                    }
                }
            }

            return bestText;
        }

        private static string QuoteArgument(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }
    }
}
