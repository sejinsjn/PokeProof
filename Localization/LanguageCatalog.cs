using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PokeSqueeze.Localization
{
    public sealed class LanguageCatalog : INotifyPropertyChanged
    {
        private readonly Dictionary<string, string> german = new Dictionary<string, string>
        {
            ["appTitle"] = "PokeProof – Tauschbelege",
            ["appSubtitle"] = "Videos für Tauschbelege vorbereiten",
            ["languageLabel"] = "Sprache",
            ["settings"] = "Einstellungen",
            ["output"] = "Ausgabe",
            ["createFolder"] = "Für jedes Video einen Ordner erstellen",
            ["createFolderRequiredTooltip"] = "Wird für separate Textdateien pro Video benötigt.",
            ["tradeHistory"] = "Tauschverlauf als Textdatei speichern",
            ["tradeHistoryTooltip"] = "Dieser Text wird als TradeHistory.txt im Ausgabeordner gespeichert.",
            ["extractSummary"] = "Pokémon-Summary als Summary.txt auslesen",
            ["extractSummaryTooltip"] = "Liest den Text aus den letzten Sekunden des Videos lokal aus und speichert ihn getrennt.",
            ["renameVideos"] = "Videos umbenennen",
            ["startNumber"] = "Startnummer",
            ["fileExtension"] = "Dateiendung",
            ["ternaryNote"] = "Nummern werden im Ternärsystem fortlaufend vergeben.",
            ["compressVideos"] = "Video komprimieren (Ton wird entfernt)",
            ["compression"] = "Komprimierung",
            ["compressionDescription"] = "AMD: HEVC, höchste Qualitätsstufe. NVIDIA: HEVC NVENC.",
            ["selectedFilesNone"] = "Noch keine Dateien ausgewählt",
            ["selectedFileOne"] = "1 Datei ausgewählt",
            ["selectedFilesMany"] = "{0} Dateien ausgewählt",
            ["chooseVideos"] = "Videos auswählen…",
            ["queue"] = "Warteschlange",
            ["queueSummary"] = "{0} von {1} Videos bearbeitet",
            ["addToQueue"] = "Hinzufügen",
            ["startProcessing"] = "Verarbeitung starten",
            ["remove"] = "Entfernen",
            ["queueEmpty"] = "Noch keine Videos in der Warteschlange. Wähle links Videos aus und füge sie hier hinzu.",
            ["queueInitial"] = "Wähle Videos aus, um zu beginnen.",
            ["queueAdded"] = "Videos hinzugefügt. Du kannst weitere auswählen oder die Verarbeitung starten.",
            ["queueInvalidNumber"] = "Die Startnummer muss eine gültige Zahl sein.",
            ["queueStarted"] = "Verarbeitung gestartet: {0} Video(s).",
            ["queueCurrentVideo"] = "Video {0} von {1}: {2}",
            ["queueFinished"] = "Fertig: {0} verarbeitet, {1} übersprungen.",
            ["queueFinishedErrors"] = "Abgeschlossen: {0} verarbeitet, {1} übersprungen, {2} mit Fehlern.",
            ["missingSource"] = "Die Quelldatei wurde nicht gefunden.",
            ["statusWaiting"] = "Wartend",
            ["statusPreparing"] = "Wird vorbereitet",
            ["statusSkipped"] = "Übersprungen (Zieldatei existiert)",
            ["statusCopying"] = "Wird kopiert",
            ["statusAnalysing"] = "Video wird analysiert",
            ["statusCompressing"] = "Video wird komprimiert",
            ["statusExtractingSummary"] = "Pokémon-Summary wird ausgelesen",
            ["statusDoneSummaryWarning"] = "Video fertig; Summary-Auslesung prüfen",
            ["statusSkippedSummaryExtracted"] = "Video bereits vorhanden; Summary ergänzt",
            ["statusDone"] = "Fertig",
            ["statusError"] = "Fehler",
            ["summaryNoText"] = "Kein lesbarer Text erkannt. Bitte das Video prüfen.",
            ["summaryExtractionFailed"] = "Summary konnte nicht ausgelesen werden: {0}",
            ["fileFilter"] = "Videodateien|*.mp4;*.mkv;*.mov;*.avi;*.webm;*.m4v|Alle Dateien|*.*"
        };

        private readonly Dictionary<string, string> english = new Dictionary<string, string>
        {
            ["appTitle"] = "PokeProof – Trade Proofs",
            ["appSubtitle"] = "Prepare videos for trade proofs",
            ["languageLabel"] = "Language",
            ["settings"] = "Settings",
            ["output"] = "Output",
            ["createFolder"] = "Create a folder for each video",
            ["createFolderRequiredTooltip"] = "Required to save separate text files for each video.",
            ["tradeHistory"] = "Save trade history as a text file",
            ["tradeHistoryTooltip"] = "This text is saved as TradeHistory.txt in the output folder.",
            ["extractSummary"] = "Read Pokémon summary to Summary.txt",
            ["extractSummaryTooltip"] = "Reads text from the final seconds of the video locally and saves it separately.",
            ["renameVideos"] = "Rename videos",
            ["startNumber"] = "Starting number",
            ["fileExtension"] = "File extension",
            ["ternaryNote"] = "Numbers are assigned sequentially in base 3.",
            ["compressVideos"] = "Compress video (audio will be removed)",
            ["compression"] = "Encoder",
            ["compressionDescription"] = "AMD: HEVC, highest quality. NVIDIA: HEVC NVENC.",
            ["selectedFilesNone"] = "No files selected",
            ["selectedFileOne"] = "1 file selected",
            ["selectedFilesMany"] = "{0} files selected",
            ["chooseVideos"] = "Choose videos…",
            ["queue"] = "Queue",
            ["queueSummary"] = "{0} of {1} videos processed",
            ["addToQueue"] = "Add to queue",
            ["startProcessing"] = "Start processing",
            ["remove"] = "Remove",
            ["queueEmpty"] = "No videos in the queue yet. Choose videos on the left and add them here.",
            ["queueInitial"] = "Choose videos to get started.",
            ["queueAdded"] = "Videos added. You can choose more or start processing.",
            ["queueInvalidNumber"] = "The starting number must be a valid integer.",
            ["queueStarted"] = "Processing started: {0} video(s).",
            ["queueCurrentVideo"] = "Video {0} of {1}: {2}",
            ["queueFinished"] = "Done: {0} processed, {1} skipped.",
            ["queueFinishedErrors"] = "Finished: {0} processed, {1} skipped, {2} failed.",
            ["missingSource"] = "The source file could not be found.",
            ["statusWaiting"] = "Waiting",
            ["statusPreparing"] = "Preparing",
            ["statusSkipped"] = "Skipped (output already exists)",
            ["statusCopying"] = "Copying",
            ["statusAnalysing"] = "Analyzing video",
            ["statusCompressing"] = "Compressing video",
            ["statusExtractingSummary"] = "Reading Pokémon summary",
            ["statusDoneSummaryWarning"] = "Video done; check summary extraction",
            ["statusSkippedSummaryExtracted"] = "Video already existed; summary added",
            ["statusDone"] = "Done",
            ["statusError"] = "Error",
            ["summaryNoText"] = "No readable text was recognized. Check the video.",
            ["summaryExtractionFailed"] = "Could not read the summary: {0}",
            ["fileFilter"] = "Video files|*.mp4;*.mkv;*.mov;*.avi;*.webm;*.m4v|All files|*.*"
        };

        private string language = "Deutsch";

        public event PropertyChangedEventHandler PropertyChanged;

        public string Language
        {
            get => language;
            set
            {
                if (value != "Deutsch" && value != "English")
                {
                    throw new ArgumentException("Unsupported language.", nameof(value));
                }

                if (language == value)
                {
                    return;
                }

                language = value;
                OnPropertyChanged();
                OnPropertyChanged("Item[]");
            }
        }

        public string this[string key] => (Language == "English" ? english : german)[key];

        public string Format(string key, params object[] arguments)
        {
            CultureInfo culture = Language == "English"
                ? CultureInfo.GetCultureInfo("en-US")
                : CultureInfo.GetCultureInfo("de-DE");
            return string.Format(culture, this[key], arguments);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
