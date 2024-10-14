using Koboldcs.Configuration;
using Koboldcs.Logger;
using Koboldcs.Models;
using LLama.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Koboldcs.DataServices
{
    public class ChatDataService
    {
        private string _configFilePath;

        // Provides a reference to the singleton instance of ChatConfig.
        // The instance itself can be serialized, unlike static classes, which cannot. DI might have been a better choice for flexibility, but yeah...
        private ChatConfig _ch => ChatConfig.Instance;

        public void SaveConfig(ObservableCollection<Message> messages)
        {
            // Put the Stuff KoboldCS uses in it's main Config into this one to make it shareable with KoboldCPP
            // SUPPORT FOR KOBOLDCPP
            switch (Config.SelectedUsageMode)
            {
                case 0: // Chat Mode
                    _ch.RootConfig.savedsettings.opmode = "3";
                    break;
                case 1: // Instruct Mode
                    _ch.RootConfig.savedsettings.opmode = "0";
                    break;
                case 2: // Story Mode
                    _ch.RootConfig.savedsettings.opmode = "1";
                    break;
                case 3: // Adventure Mode
                    _ch.RootConfig.savedsettings.opmode = "2";
                    break;
                default:
                    break;
            }
            _ch.RootConfig.savedsettings.chatname = Config.Username;
            _ch.RootConfig.savedsettings.chatopponent = Config.AIName;
            _ch.RootConfig.savedsettings.instruct_starttag = Config.UserTag;
            _ch.RootConfig.savedsettings.instruct_endtag = Config.AITag;
            _ch.RootConfig.savedsettings.instruct_systag = Config.SystemTag;
            _ch.RootConfig.savedsettings.instruct_sysprompt = Config.SystemPrompt;

            _ch.RootConfig.savedsettings.max_context_length = Config.ContextSize;
            _ch.RootConfig.savedsettings.max_length = Config.MaxOutput;
            _ch.RootConfig.savedsettings.rep_pen = Config.RepPen;
            _ch.RootConfig.savedsettings.temperature = Config.Temperature;
            _ch.RootConfig.savedsettings.top_p = Config.TopP;
            _ch.RootConfig.savedsettings.top_a = Config.TopA;
            _ch.RootConfig.savedsettings.top_k = Config.TopK;
            _ch.RootConfig.savedsettings.tfs_s = Config.Tfs;
            _ch.RootConfig.savedsettings.typ_s = Config.Typical;
            _ch.RootConfig.savedsettings.min_p = Config.MinP;
            _ch.RootConfig.savedsettings.presence_penalty = Config.PresencePenalty;
            _ch.RootConfig.savedsettings.miro_type = (int)Config.MirostatType;
            _ch.RootConfig.savedsettings.miro_tau = Config.MirostatTau;
            _ch.RootConfig.savedsettings.miro_eta = Config.MirostatEta;

            _ch.RootConfig.memory = Config.MemoryText;


            // _ch.RootConfig.savedaestheticsettings.AI_portrait = Config.ImageSource; TODO

            if (messages != null && messages.Count != 0)
            {
                switch (Config.SelectedUsageMode)
                {
                    case 0: // Chat Mode
                        foreach (var item in messages)
                        {
                            string selectedName = item.IsUser == true ? Config.Username : Config.AIName;
                            if (item == messages.FirstOrDefault())
                            {
                                _ch.RootConfig.prompt = selectedName + item.Text;
                                continue;
                            }
                            _ch.RootConfig.actions.Add(selectedName + item.Text);
                        }
                        break;
                    case 1: // Instruct Mode
                        _ch.RootConfig.savedsettings.opmode = "0";
                        break;
                    case 2: // Story Mode
                        _ch.RootConfig.savedsettings.opmode = "1";
                        break;
                    case 3: // Adventure Mode
                        _ch.RootConfig.savedsettings.opmode = "2";
                        break;
                    default:
                        break;
                }
            }


            // Serialize the ConfigSer instance to a JSON string
            string jsonString = JsonSerializer.Serialize(_ch.RootConfig, new JsonSerializerOptions { WriteIndented = true });

            // Write the JSON string to a file
            File.WriteAllText(Path.GetDirectoryName(Environment.ProcessPath) + @"\" + _configFilePath, jsonString);
        }












        // Method to load the Config from a file
        public void LoadConfig(ChatHistory chatHistory)
        {
            if (!File.Exists(Path.GetDirectoryName(Environment.ProcessPath) + @"\" + _configFilePath))
            {
                SLogger.Log(SLogger.LogType.Warn, "No latest config file. Using defaults.");
                return;
            }

            // Read the JSON string from the file
            string jsonString = File.ReadAllText(Path.GetDirectoryName(Environment.ProcessPath) + @"\" + _configFilePath);

            // Deserialize the JSON string to a ChatConfig instance
            ChatConfig.Root configData = JsonSerializer.Deserialize<ChatConfig.Root>(jsonString);

            _ch.RootConfig = configData;

            Config.Username = _ch.RootConfig.savedsettings.chatname;
            Config.AIName = _ch.RootConfig.savedsettings.chatopponent;
            Config.UserTag = _ch.RootConfig.savedsettings.instruct_starttag;
            Config.AITag = _ch.RootConfig.savedsettings.instruct_endtag;
            Config.SystemTag = _ch.RootConfig.savedsettings.instruct_systag;
            Config.SystemPrompt = _ch.RootConfig.savedsettings.instruct_sysprompt;

            Config.ContextSize = _ch.RootConfig.savedsettings.max_context_length;
            Config.MaxOutput = _ch.RootConfig.savedsettings.max_length;
            Config.RepPen = (float)_ch.RootConfig.savedsettings.rep_pen;
            Config.Temperature = (float)_ch.RootConfig.savedsettings.temperature;
            Config.TopP = (float)_ch.RootConfig.savedsettings.top_p;
            Config.TopA = _ch.RootConfig.savedsettings.top_a;
            Config.TopK = _ch.RootConfig.savedsettings.top_k;
            Config.Tfs = _ch.RootConfig.savedsettings.tfs_s;
            Config.Typical = _ch.RootConfig.savedsettings.typ_s;
            Config.MinP = _ch.RootConfig.savedsettings.min_p;
            Config.PresencePenalty = _ch.RootConfig.savedsettings.presence_penalty;
            Config.MirostatType = (MirostatType)_ch.RootConfig.savedsettings.miro_type;
            Config.MirostatTau = _ch.RootConfig.savedsettings.miro_tau;
            Config.MirostatEta = (float)_ch.RootConfig.savedsettings.miro_eta;

            Config.MemoryText = _ch.RootConfig.memory;  // Shitty, as MemoryText won't be updated in MainPageViewModel..
                                                        // I can't ref to that because it's a property.

            switch (Config.SelectedUsageMode)
            {
                case 0: // Chat Mode
                    foreach (var item in configData.actions)
                    {
                       // string purified = item.Replace(configData,"");
                    }
                    break;
                case 1: // Instruct Mode
                    _ch.RootConfig.savedsettings.opmode = "0";
                    break;
                case 2: // Story Mode
                    _ch.RootConfig.savedsettings.opmode = "1";
                    break;
                case 3: // Adventure Mode
                    _ch.RootConfig.savedsettings.opmode = "2";
                    break;
                default:
                    break;
            }

        }
    }
}
