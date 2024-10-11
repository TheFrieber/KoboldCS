
using LLama.Common;
using LLama.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koboldcs.Configuration
{

    public static class States
    {
        public static MainPage Main { get; set; }
    }

    public static class Config
    {
        //Main
        public static string ModelPath { get; set; }
        public static string Username { get; set; }
        public static string AIName { get; set; }
        public static string ImageSource { get; set; }
        public static bool AddNewLineMemory { get; set; }
        public static string MemoryText { get; set; }
        public static List<string> StopSequences { get; set; } = new List<string>();
        public static Dictionary<string, float> LogitBiasCollection { get; set; } = new Dictionary<string, float>();

        //Advanced Settings
        public static int LayersToOffload {  get; set; }
        public static bool UseMmap { get; set; }
        public static bool useMlock { get; set; }
        public static bool UseFlashAttention { get; set; }
        public static GGMLType PrecisionType { get; set; }
        public static bool NoKVOffload { get; set; }
        public static int? ThreadsToUse { get; set; }
        public static int BatchSize { get; set; }
        public static float RopeFrequencyScale { get; set; }

        // Media Settings
        public static bool useMClutching { get; set; }
        public static int cuiPort { get; set; }

        // Instruct Mode stuff
        public static string SystemTag { get; set; }
        public static string SystemPrompt { get; set; }
        public static string UserTag { get; set; }
        public static string AITag { get; set; }
        public static string SelectedPreset { get; set; }
        public static int SelectedUsageMode { get; set; }

        //Int CFG
        public static int ContextSize { get; set; }
        public static int MaxOutput { get; set;}
        public static int TopK { get; set; }
        public static int TopA { get; set;}

        //Float CFG
        public static float Typical {  get; set; }
        public static float MinP { get; set; }
        public static float Temperature { get; set; }
        public static float RepPen { get; set; }
        public static float TopP { get; set; }
        public static float Tfs { get; set; }
        public static float PresencePenalty { get; set; }

        //Mirostat CFG
        public static MirostatType MirostatType { get; set; }
        public static float MirostatEta { get; set; }
        public static float MirostatTau { get; set; }
    }


    public class ConfigSer
    {
        //Main
        public string ModelPath { get => Config.ModelPath; set => Config.ModelPath = value; }
        public string Username { get => Config.Username; set => Config.Username = value; }
        public string AIName { get => Config.AIName; set => Config.AIName = value; }
        public string ImageSource { get => Config.ImageSource; set => Config.ImageSource = value; }
        public bool AddNewLineMemory { get => Config.AddNewLineMemory; set => Config.AddNewLineMemory = value; }
        public string MemoryText { get => Config.MemoryText; set => Config.MemoryText = value; }
        public List<string> StopSequences { get => Config.StopSequences; set => Config.StopSequences = value; }
        public Dictionary<string, float> LogitBiasCollection { get => Config.LogitBiasCollection; set => Config.LogitBiasCollection = value; }

        //Advanced Settings
        public int LayersToOffload { get => Config.LayersToOffload; set => Config.LayersToOffload = value; }
        public bool UseMmap { get => Config.UseMmap; set => Config.UseMmap = value; }
        public bool useMlock { get => Config.useMlock; set => Config.useMlock = value; }
        public bool UseFlashAttention { get => Config.UseFlashAttention; set => Config.UseFlashAttention = value; }
        public GGMLType PrecisionType { get => Config.PrecisionType; set => Config.PrecisionType = value; }
        public bool NoKVOffload { get => Config.NoKVOffload; set => Config.NoKVOffload = value; }
        public int? ThreadsToUse { get => Config.ThreadsToUse; set => Config.ThreadsToUse = value; }
        public int BatchSize { get => Config.BatchSize; set => Config.BatchSize = value; }
        public float RopeFrequencyScale { get => Config.RopeFrequencyScale; set => Config.RopeFrequencyScale = value; }

        // Media Settings
        public bool useMClutching { get => Config.useMClutching; set => Config.useMClutching = value; }
        public int cuiPort { get => Config.cuiPort; set => Config.cuiPort = value; }

        // Instruct Mode stuff
        public string SystemTag { get => Config.SystemTag; set => Config.SystemTag = value; }
        public string SystemPrompt { get => Config.SystemPrompt; set => Config.SystemPrompt = value; }
        public string UserTag { get => Config.UserTag; set => Config.UserTag = value; }
        public string AITag { get => Config.AITag; set => Config.AITag = value; }
        public string SelectedPreset { get => Config.SelectedPreset; set => Config.SelectedPreset = value; }
        public int SelectedUsageMode { get => Config.SelectedUsageMode; set => Config.SelectedUsageMode = value; }

        //Int CFG
        public int ContextSize { get => Config.ContextSize; set => Config.ContextSize = value; }
        public int MaxOutput { get => Config.MaxOutput; set => Config.MaxOutput = value; }
        public int TopK { get => Config.TopK; set => Config.TopK = value; }
        public int TopA { get => Config.TopA; set => Config.TopA = value; }

        //Float CFG
        public float Typical { get => Config.Typical; set => Config.Typical = value; }
        public float MinP { get => Config.MinP; set => Config.MinP = value; }
        public float Temperature { get => Config.Temperature; set => Config.Temperature = value; }
        public float RepPen { get => Config.RepPen; set => Config.RepPen = value; }
        public float TopP { get => Config.TopP; set => Config.TopP = value; }
        public float Tfs { get => Config.Tfs; set => Config.Tfs = value; }
        public float PresencePenalty { get => Config.PresencePenalty; set => Config.PresencePenalty = value; }

        //Mirostat CFG
        public MirostatType MirostatType { get => Config.MirostatType; set => Config.MirostatType = value; }
        public float MirostatEta { get => Config.MirostatEta; set => Config.MirostatEta = value; }
        public float MirostatTau { get => Config.MirostatTau; set => Config.MirostatTau = value; }
    }
}
