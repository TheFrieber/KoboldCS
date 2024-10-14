using Koboldcs.Configuration;
using Koboldcs.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Koboldcs.DataServices
{
    public class ConfigDataService
    {
        private string _configFilePath;

        public ConfigDataService(string configFilePath)
        {
            _configFilePath = configFilePath;
        }

        // Method to save the Config to a file
        public void SaveConfig()
        {
            // Create an instance of ConfigSer and populate it with the current static Config data
            ConfigSer configData = new ConfigSer();

            // Serialize the ConfigSer instance to a JSON string
            string jsonString = JsonSerializer.Serialize(configData, new JsonSerializerOptions { WriteIndented = true });

            // Write the JSON string to a file
            File.WriteAllText(Path.GetDirectoryName(Environment.ProcessPath) + @"\" + _configFilePath, jsonString);
        }

        // Method to load the Config from a file
        public void LoadConfig()
        {
            if (!File.Exists(Path.GetDirectoryName(Environment.ProcessPath) + @"\" + _configFilePath))
            {
                SLogger.Log(SLogger.LogType.Warn, "No latest config file. Using defaults.");
                return;
            }

            // Read the JSON string from the file
            string jsonString = File.ReadAllText(Path.GetDirectoryName(Environment.ProcessPath) + @"\" + _configFilePath);

            // Deserialize the JSON string to a ConfigSer instance
            ConfigSer configData = JsonSerializer.Deserialize<ConfigSer>(jsonString);

            // Populate the static Config properties with the data from the ConfigSer instance
            Config.ModelPath = configData.ModelPath;
            Config.Username = configData.Username;
            Config.AIName = configData.AIName;
            Config.ImageSource = configData.ImageSource;
            Config.LayersToOffload = configData.LayersToOffload;
            Config.UseMmap = configData.UseMmap;
            Config.useMlock = configData.useMlock;
            Config.UseFlashAttention = configData.UseFlashAttention;
            Config.PrecisionType = configData.PrecisionType;
            Config.NoKVOffload = configData.NoKVOffload;
            Config.ThreadsToUse = configData.ThreadsToUse;
            Config.BatchSize = configData.BatchSize;
            Config.RopeFrequencyScale = configData.RopeFrequencyScale;
            Config.SystemTag = configData.SystemTag;
            Config.SystemPrompt = configData.SystemPrompt;
            Config.UserTag = configData.UserTag;
            Config.AITag = configData.AITag;
            Config.SelectedPreset = configData.SelectedPreset;
            Config.SelectedUsageMode = configData.SelectedUsageMode;
            Config.ContextSize = configData.ContextSize;
            Config.MaxOutput = configData.MaxOutput;
            Config.TopK = configData.TopK;
            Config.TopA = configData.TopA;
            Config.Typical = configData.Typical;
            Config.MinP = configData.MinP;
            Config.Temperature = configData.Temperature;
            Config.RepPen = configData.RepPen;
            Config.TopP = configData.TopP;
            Config.Tfs = configData.Tfs;
            Config.PresencePenalty = configData.PresencePenalty;
            Config.MirostatType = configData.MirostatType;
            Config.MirostatEta = configData.MirostatEta;
            Config.MirostatTau = configData.MirostatTau;
            Config.AddNewLineMemory = configData.AddNewLineMemory;
            Config.MemoryText = configData.MemoryText;
            Config.StopSequences = configData.StopSequences;
        }
    }
}
