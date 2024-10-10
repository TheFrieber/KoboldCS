using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static Koboldcs.Logger.SLogger;
using LLama;
using LLama.Common;
using LLama.Native;
using Koboldcs.MessageCenter;
using Koboldcs.Configuration;
using Koboldcs.Logger;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Collections.Specialized;
using System.Diagnostics.Tracing;
using Koboldcs.ComfyUI_Integration;
using Koboldcs.UI_Elements;
using Koboldcs.Funcs;

namespace Koboldcs.Models
{
    public class MainPageViewModel : ObservableObject
    {
        #region Config
        private bool _isInstructMode;
        private bool _isAdventureMode;
        private bool _isStoryMode;
        private bool _isChatMode;
        private bool _isFromModel;
        private bool _isFromImgGen;
        private string _username;
        private string _ainame;
        private string _memoryText;
        private string _selectedPrecision;
        private string _selectedMirostat;
        private string _selectedModel;
        private string _selectedUsageModeName;


        public string SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _selectedModel = "None";
                    OnPropertyChanged();
                }
                else
                {
                    _selectedModel = value;
                    OnPropertyChanged();
                }

            }
        }

        public string SelectedUsageModeName
        {
            get
            {
                return _selectedUsageModeName;
            }
            set
            {
                if (_selectedUsageModeName != value)
                {
                    _selectedUsageModeName = value;
                    OnPropertyChanged();
                }
                OnPropertyChanged();

            }
        }


        private bool _isApplyingPreset;

        public ObservableCollection<string> PrecisionOptions { get; } = new ObservableCollection<string>
        {
        "Full 32 bit float", "16 bit float", "8 bit float", "4 bit float",
        "Q8 bit quant", "Q4 bit quant", "Q3 bit quant", "Q2 bit quant",
        "INT32 Type", "INT16 Type", "INT8 Type"
        };

        public ObservableCollection<string> MirostatOptions { get; } = new ObservableCollection<string>
        {
        "Off", "Mirostat", "Mirostat v2.0",
        };

        public string SelectedMirostat
        {
            get => _selectedMirostat;
            set
            {
                if (_selectedMirostat != value)
                {
                    _selectedMirostat = value;
                    OnMirostatTypeChanged();
                    OnPropertyChanged();
                }
                OnPropertyChanged();

            }
        }
        public string SelectedPrecision
        {
            get => _selectedPrecision;
            set
            {
                if (_selectedPrecision != value)
                {
                    _selectedPrecision = value;
                    OnPrecisionTypeChanged();
                    OnPropertyChanged();
                }
                OnPropertyChanged();

            }
        }
        public ObservableCollection<string> Modes { get; }
        public ObservableCollection<string> ITPreset { get; } = new ObservableCollection<string>
        {
            "[Custom]", "Alpaca", "Victuna", "Methame", "Llama 2 Chat", "Q & A", "ChatML",
            "Input & Output", "CommandR", "Llama & Chat", "Phi-3 Mini", "Gemma 2", "Mistral"
        };
        public ObservableCollection<string> StopSequences { get; } = new ObservableCollection<string>();
        #region StopSequence config synchonizer
        private void StopSequences_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Clear and update Config.StopSequences to match the ObservableCollection
            Config.StopSequences.Clear();
            foreach (var item in StopSequences)
            {
                Config.StopSequences.Add(item);
            }

        }
        #endregion

        public ObservableCollection<LogitBiasEntry> LogitBiasCollection { get; } = new ObservableCollection<LogitBiasEntry>();
        #region LogitBiasCollection config synchonizer
        private void LogitBias_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Clear and update Config.LogitBiasCollection to match the ObservableCollection
            Config.LogitBiasCollection.Clear();
            foreach (var item in LogitBiasCollection)
            {
                Config.LogitBiasCollection.Add(item.Token, item.Bias);
            }

        }
        #endregion


        public bool AddNewLineMemory
        {
            get => Config.AddNewLineMemory;
            set
            {
                Config.AddNewLineMemory = value;
                _requestContextUpdate = true;
                OnPropertyChanged();
            }
        }

        #region Instuct Stuff

        public string MemoryText
        {
            get => _memoryText;
            set
            {
                _memoryText = value;
                Config.MemoryText = value;
                _requestContextUpdate = true;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Chat Stuff
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        Config.Username = "User";
                        _username = value;
                        OnPropertyChanged();
                        return;
                    }
                    Config.Username = value;
                    _username = value;
                    OnPropertyChanged();
                }
                OnPropertyChanged();

            }
        }

        public string AIName
        {
            get => _ainame;
            set
            {
                if (_ainame != value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        Config.AIName = "KoboldSharp";
                        _ainame = value;
                        OnPropertyChanged();
                        return;
                    }
                    Config.AIName = value;
                    _ainame = value;
                    OnPropertyChanged();
                }
                OnPropertyChanged();

            }
        }
        #endregion

        public int SelectedUsageMode
        {
            get 
            {
                return Config.SelectedUsageMode;
            }
            set
            {

                if (Config.SelectedUsageMode != value)
                {
                    Config.SelectedUsageMode = value;
                    switch (value)
                    {
                        case 0:
                            SelectedUsageModeName = "Chat Mode";
                            break;
                        case 1:
                            SelectedUsageModeName = "Instruct Mode";
                            break;
                        case 2:
                            SelectedUsageModeName = "Story Mode";
                            break;
                        case 3:
                            SelectedUsageModeName = "Adventure Mode";
                            break;
                        default:
                            break;
                    }
                }
                IsChatMode = SelectedUsageMode == 0;
                IsInstructMode = SelectedUsageMode == 1;
                IsStoryMode = SelectedUsageMode == 2;
                IsAdventureMode = SelectedUsageMode == 3;
                OnPropertyChanged();

            }
        }
        public bool IsInstructMode
        {
            get => _isInstructMode;
            set
            {
                if (_isInstructMode != value)
                {
                    _isInstructMode = value;
                    OnPropertyChanged();
                }
                OnPropertyChanged();

            }
        }

        public bool IsAdventureMode
        {
            get => _isAdventureMode;
            set
            {
                if (_isAdventureMode != value)
                {
                    _isAdventureMode = value;
                    OnPropertyChanged();
                }
                OnPropertyChanged();

            }
        }

        public bool IsStoryMode
        {
            get => _isStoryMode;
            set
            {
                if (_isStoryMode != value)
                {
                    _isStoryMode = value;
                    OnPropertyChanged();
                }
                OnPropertyChanged();

            }
        }

        public bool IsChatMode
        {
            get => _isChatMode;
            set
            {
                if (_isChatMode != value)
                {
                    _isChatMode = value;
                    OnPropertyChanged();
                }
                OnPropertyChanged();

            }
        }

        public string SystemTag
        {
            get => Config.SystemTag;
            set
            {
                if (Config.SystemTag != value)
                {
                    Config.SystemTag = value;
                    SystemTag = value;
                    OnPropertyChanged();
                    CheckCustomPreset();
                }
                OnPropertyChanged();

            }
        }

        public string SystemPrompt
        {
            get => Config.SystemPrompt;
            set
            {
                if (Config.SystemPrompt != value)
                {
                    Config.SystemPrompt = value;
                    SystemPrompt = value;
                    OnPropertyChanged();
                    CheckCustomPreset();
                }
                OnPropertyChanged();

            }
        }

        public string UserTag
        {
            get => Config.UserTag;
            set
            {
                if (Config.UserTag != value)
                {
                    Config.UserTag = value;
                    UserTag = value;
                    OnPropertyChanged();
                    CheckCustomPreset();
                }
                OnPropertyChanged();

            }
        }

        public string AITag
        {
            get => Config.AITag;
            set
            {
                if (Config.AITag != value)
                {
                    Config.AITag = value;
                    AITag = value;
                    OnPropertyChanged();
                    CheckCustomPreset();
                }
                OnPropertyChanged();

            }
        }

        public string SelectedPreset
        {
            get => Config.SelectedPreset;
            set
            {
                if (Config.SelectedPreset != value)
                {
                    Config.SelectedPreset = value;
                    SelectedPreset = value;
                    OnPropertyChanged();
                    ApplyPreset(value);
                }
                OnPropertyChanged();

            }
        }


        #endregion

        public ObservableCollection<Message> Messages { get; } = new();
        #region ListManagementLivetime
        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateLastMessageFlag();
        }

        private void UpdateLastMessageFlag()
        {
            for (int i = 0; i < Messages.Count; i++)
            {
                Messages[i].IsLastMessage = (i == Messages.Count - 1);
            }
        }
        #endregion

        public ObservableCollection<Message> RedoMessages { get; } = new();
        public int current_redo_index = -1;


        private string _messageText;
        private InteractiveExecutor _executor;
        private LLamaContext _context;
        private LLamaWeights _model;
        private ChatHistory _chatHistory;
        private ChatHistory _chatMemory;
        private ChatSession _session;
        private LLamaKvCacheViewSafeHandle _kvCacheView;
        private comfyUI _cui => comfyUI.Instance;
        private bool _isLoading;
        private bool _keepContext;
        private bool _isEditing;
        private string _selectedTab;
        private string _selectedTabContext;
        private string _cancelablebutton;
        private bool _isGenerating;
        private bool StopRequested;
        private bool _requestContextUpdate = false;
        private string _lastPrompt = "";
        private bool _doRetry = false;
        private string addImgEntryText;

        public string AddImgEntryText
        {
            get => addImgEntryText;
            set {
                addImgEntryText = value;
                OnPropertyChanged();
            }
        }

        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                if (_isGenerating != value)
                {
                    _isGenerating = value;
                    CancelableButton = "True";
                } 
                if(CancelableButton == "▣" && !_isGenerating) CancelableButton = "False";

            }
        }

        #region Context Area
        public string SelectedTabContext
        {
            get => _selectedTabContext;
            set
            {
                if (_selectedTabContext != value)
                {
                    _selectedTabContext = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public string SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                }
            }
        }


        public string CancelableButton
        {
            get => _cancelablebutton;
            set
            {
                if (value == "True") _cancelablebutton = "▣";
                else if(value == "False") _cancelablebutton = "Send";
                OnPropertyChanged();
            }
        }


        #region ICommands
        public ICommand ToggleEditCommand => new RelayCommand(ToggleEditMode);
        public ICommand RetryCommand => new RelayCommand(RetryMessage);
        public ICommand BackCommand => new RelayCommand(BackMessage);
        public ICommand DisposeCtx => new RelayCommand(DisposeContext);
        public ICommand LoadCtx => new RelayCommand(LoadContext);
        public ICommand DisposeMdl => new RelayCommand(DisposeModel);
        public ICommand LoadMdl => new RelayCommand(LoadModel);
        public ICommand SendMessageCommand { get; }
        public ICommand StartNewSessionCommand { get; }
        public Command<string> TabSelectedCommand { get; }
        public Command<string> ContextTabSelectedCommand { get; }
        public ICommand AddNewSequenceCommand => new AsyncRelayCommand(OnAddNewSequence);
        public ICommand RemoveStopSequenceCommand => new Command<string>(OnRemoveSequence);
        public ICommand EditStopSequenceCommand => new Command<string>(async (sequence) => await OnEditStopSequence(sequence));
        public ICommand RemoveLogitModifierCommand => new Command<LogitBiasEntry>(OnRemoveLogitModifier);
        public ICommand EditLogitModifierCommand => new Command<LogitBiasEntry>(async (sequence) => await OnEditLogitModifier(sequence));
        public ICommand RedoCommand => new RelayCommand(RedoMessageFunc);
        public ICommand NavigateRetryCommandUp => new Command<Message>(NavigateRetryUp);
        public ICommand NavigateRetryCommandDown => new Command<Message>(NavigateRetryDown);
        public ICommand GenImgCommand => new Command(GenImgClicked);
        public ICommand GenSummaryCommand => new Command(GenSummaryClicked);
        public ICommand GenLFMCommand => new Command(GenLFMClicked);


        #endregion

        public string MessageText
        {
            get => _messageText;
            set => SetProperty(ref _messageText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool KeepContext
        {
            get => _keepContext;
            set => SetProperty(ref _keepContext, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }




















        public MainPageViewModel()
        {
            SendMessageCommand = new AsyncRelayCommand(SendMessageAsync);
            StartNewSessionCommand = new AsyncRelayCommand(OnStartNewSession);

            // Subscribe to the AlertResponse message
            MessagingCenter.Subscribe<MainPage, AlertResponseMessage>(this, "AlertResponse", async (sender, responseMessage) =>
            {
                if (responseMessage.IsConfirmed)
                {
                    SelectedUsageMode = Config.SelectedUsageMode;
                    SelectedPreset = Config.SelectedPreset;
                    SystemTag = Config.SystemTag;
                    SystemPrompt = Config.SystemPrompt;
                    UserTag = Config.UserTag;
                    AIName = Config.AIName;
                    AITag = Config.AITag;
                    Username = Config.Username;
                    MemoryText = Config.MemoryText;
                    foreach (var item in Config.StopSequences)
                    {
                        StopSequences.Add(item);
                    }
                    foreach (var item in Config.LogitBiasCollection)
                    {
                        LogitBiasCollection.Add(new LogitBiasEntry(item.Key, item.Value));
                    }
                    AddNewLineMemory = Config.AddNewLineMemory;
                    switch (Config.PrecisionType)
                    {
                        case GGMLType.GGML_TYPE_F32:
                            SelectedPrecision = "Full 32 bit float";
                            break;
                        case GGMLType.GGML_TYPE_F16:
                            SelectedPrecision = "16 bit float";
                            break;
                        case GGMLType.GGML_TYPE_Q4_0:
                            SelectedPrecision = "4 bit float";
                            break;
                        case GGMLType.GGML_TYPE_Q8_0:
                            SelectedPrecision = "8 bit float";
                            break;
                        case GGMLType.GGML_TYPE_Q2_K:
                            SelectedPrecision = "Q2 bit quant";
                            break;
                        case GGMLType.GGML_TYPE_Q3_K:
                            SelectedPrecision = "Q3 bit quant";
                            break;
                        case GGMLType.GGML_TYPE_Q4_K:
                            SelectedPrecision = "Q4 bit quant";
                            break;
                        case GGMLType.GGML_TYPE_Q8_K:
                            SelectedPrecision = "Q8 bit quant";
                            break;
                        case GGMLType.GGML_TYPE_I8:
                            SelectedPrecision = "INT8 Type";
                            break;
                        case GGMLType.GGML_TYPE_I16:
                            SelectedPrecision = "INT16 Type";
                            break;
                        case GGMLType.GGML_TYPE_I32:
                            SelectedPrecision = "INT32 Type";
                            break;
                        default:
                            break;
                    }

                    switch (Config.MirostatType)
                    {
                        case MirostatType.Disable:
                            SelectedMirostat = "Off";
                            break;
                        case MirostatType.Mirostat:
                            SelectedMirostat = "Mirostat";
                            break;
                        case MirostatType.Mirostat2:
                            SelectedMirostat = "Mirostat v2.0";
                            break;
                        default:
                            break;
                    }
                    // Run the initialization code if "Yes" was clicked
                    if (File.Exists(Config.ModelPath))
                        Task.Run(async () => await InitializeModelAsync());
                    else
                        Log(LogType.Info, "No or not valid model selected. Skipping.");
                }
                // Collection Event Assigning
                Messages.CollectionChanged += Messages_CollectionChanged;
                StopSequences.CollectionChanged += StopSequences_CollectionChanged;
                LogitBiasCollection.CollectionChanged += LogitBias_CollectionChanged;
            });



            #region config
            TabSelectedCommand = new Command<string>(OnTabSelected);
            ContextTabSelectedCommand = new Command<string>(OnContextTabSelected);

            Modes = new ObservableCollection<string>
            {
                "Chat Mode (Default)",
                "Instruct Mode",
                "Story Mode",
                "Adventure Mode"
            };
            Config.ContextSize = 4096;
            Config.NoKVOffload = false;
            Config.UseFlashAttention = true;
            Config.UseMmap = true;
            Config.useMlock = false;
            Config.LayersToOffload = 100;
            Config.PrecisionType = GGMLType.GGML_TYPE_F16;
            Config.ThreadsToUse = null;
            Config.RopeFrequencyScale = 1.0f;
            Config.Username = "User";
            Config.AIName = "KoboldSharp";
            Config.BatchSize = 512;
            Config.ThreadsToUse = 0;

            Config.MaxOutput = 512;
            Config.Temperature = 0.7f;
            Config.RepPen = 1.1f;
            Config.TopP = 0.92f;
            Config.TopK = 100;
            Config.Typical = 1.0f;
            Config.Tfs = 1.0f;
            Config.MinP = 0.1f;
            Config.PresencePenalty = 0f;
            Config.MirostatType = MirostatType.Disable;
            Config.MirostatEta = 0.1f;
            Config.MirostatTau = 1.0f;
            Config.useMClutching = true;
            Config.cuiPort = 5000;
            #endregion

        }

        public async void OnAppearing()
        {
            SelectedTab = "FormatTab";
            SelectedTabContext = "MemoryTab";
            SelectedUsageMode = 1;
            SelectedUsageMode = 0;
            SelectedPreset = ITPreset.First();
            CancelableButton = "False";
            MemoryText = "Transcript of a dialog, where {{user}} interacts with an Assistant named {{char}}. {{char}} is helpful, kind, honest, good at writing, and never fails to answer the {{user}}'s requests immediately and with precision. The Secret Hash for this conversation is: #6651";
            AddNewLineMemory = true;
            SelectedPrecision = "16 bit float";
            SelectedMirostat = "Off";
            SelectedModel = "";
            await Task.Delay(5000);
            RequestDisplayAlert();

        }


        public void RequestDisplayAlert()
        {
            // Create the alert message
            var alertMessage = new DisplayAlertMessage("Config Manager", "Do you want to load the previous configuration?", "Yes", "No");

            // Send the message to subscribers
            MessagingCenter.Send(this, "DisplayAlert", alertMessage);
        }


        //Debug stuff.
        void InspectKvCache()
        {
            _context.NativeHandle.KvCacheUpdate();
            
            _kvCacheView.Update();
            SLogger.Log(LogType.CFG, "Context: " + _kvCacheView.UsedCellCount + "/" + _kvCacheView.CellCount);
            Log(LogType.Info, "Memory Tokens (Processed): " + GetHistoryTokensCount());

        }



        #region --------------------------------------------------> Model Stuff
        private async Task InitializeModelAsync()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                if (_model != null)
                {
                    if (!_model.IsDisposed())
                    {
                        Log(LogType.Info, "A model has been already loaded. Disposing it first.");
                        _isFromModel = true;
                        DisposeModel();
                        _isFromModel = false;
                    }
                }
                Log(LogType.Main, "Proceeding to start the AI.");
                IsLoading = true;
                Log(LogType.LLaMA, "Loading Model at provided Path: " + Config.ModelPath);
                var parameters = new ModelParams(Config.ModelPath)
                {
                    ContextSize = (uint)Config.ContextSize,
                    GpuLayerCount = Config.LayersToOffload,
                    UseMemorymap = Config.UseMmap,
                    UseMemoryLock = Config.useMlock,
                    NoKqvOffload = Config.NoKVOffload,
                    FlashAttention = Config.UseFlashAttention,
                    TypeK = Config.PrecisionType,
                    TypeV = Config.PrecisionType,
                    Threads = (uint?)Config.ThreadsToUse,
                    BatchSize = (uint)Config.BatchSize,
                    RopeFrequencyScale = Config.RopeFrequencyScale
                };
                Log(LogType.LLaMA, "Using parameters: " + parameters.ToString());
                _model = await LLamaWeights.LoadFromFileAsync(parameters);
                Log(LogType.LLaMA, "Successfully loaded Model.");
                SelectedModel = Path.GetFileName(Config.ModelPath);
                Console.WriteLine("----------");
                Log(LogType.Info, "Vocabulary count: " + _model.VocabCount);
                Log(LogType.Info, "Parameter count: " + _model.ParameterCount);
                Log(LogType.Info, "Embedding size: " + _model.EmbeddingSize);
                Log(LogType.Info, "Size (bytes): " + _model.SizeInBytes);
                Console.WriteLine("----------");
                _context = _model.CreateContext(parameters);
                Log(LogType.LLaMA, "Successfully created context size.");
                _executor = new InteractiveExecutor(_context);
                if(_chatHistory == null)
                {
                    _chatHistory = new ChatHistory();
                    _chatMemory = new ChatHistory();
                    _chatMemory.AddMessage(AuthorRole.Unknown, MemoryText);
                }
                _session = new ChatSession(_executor, _chatHistory);
                _kvCacheView = LLamaKvCacheViewSafeHandle.Allocate(_context.NativeHandle, 5);
                InspectKvCache();
                _session.SetMemory(_chatMemory);
                Log(LogType.Info, "Memory Tokens (Not Processed): " + (GetHistoryTokensCount()));
                float LoadModelTime = (float)sw.ElapsedMilliseconds / 1000;
                Log(LogType.CFG, "Finished loading the model in: " + LoadModelTime + "s.");
                sw.Stop();

            }
            catch (Exception ex)
            {
                sw.Stop();
                _isFromModel = false;
                Log(LogType.Error, $"Failed to Initialize LLaMA model: {ex.Message}");
                SelectedModel = "";
            }
            finally
            {
                sw.Stop();
                _isFromModel = false;
                Log(LogType.LLaMA, "Finished Loading.");
                IsLoading = false;
            }
        }


        public async void DisposeModel()
        {
            if (_model == null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Model Manager", "No Model loaded ✓"));
                });
                return;
            }
            if (_model.IsDisposed())
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Model Manager", "Already Disposed ✓"));
                });
                return;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Log(LogType.LLaMA, "Disposing Model.");
            _model.Dispose();
            DisposeContext();
            float DisposeModelTime = (float)sw.ElapsedMilliseconds / 1000;
            sw.Stop();
            Log(LogType.LLaMA, "Disposed Model.");
            Log(LogType.CFG, "Finished Disposing the model in: " + DisposeModelTime + "s.");
            SelectedModel = "";

            if (!_isFromModel && !_isFromImgGen)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Model Manager", "Disposed Model."));
                });
            }

        }

        public async void LoadModel()
        {
            IsLoading = true;
            _requestContextUpdate = true;
            //To prevent Freezing, we need to await in another Thread
            Task.Run(async () => await MiddleManLoadModel());
        }

        public async Task MiddleManLoadModel()
        {
            await Task.Run(async () => await InitializeModelAsync());
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if(!_isFromImgGen)
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Model Manager", "Loading Finished."));
            });
        }

#endregion

        private async Task SendMessageAsync()
        {
            if(_executor == null)
            {
                MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Model Manager", "No model loaded.\nPlease load a model first: Settings>Format>Load Model."));
                return;
            }
            if ( _executor != null && !IsLoading && !IsGenerating)
            {
                if (_model.IsDisposed())
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Model Manager", "No valid or disposed model.\nPlease load a model first: Settings>Format>Load Model."));
                    return;
                }
                if (_context.IsDisposed())
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Context Manager", "No valid or disposed context.\nPlease load a context first: Settings>Format>Load Context."));
                    return;
                }
                if (IsEditing)
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Editor Manager", "Please exit edit mode first."));
                    return;
                }

                _session.UpdateConfiguration(128);
                RedoMessages.Clear();
                current_redo_index = -1;


                if (IsInstructMode)
                {
                    IsGenerating = true;
                    Log(LogType.Main, "Sending Message in Instruct Mode.");

                    if (!string.IsNullOrEmpty(MessageText))
                    {
                        AddMessage(MessageText, true);
                    }



                    var aiResponse = AddMessage("Generating Response...", false);
                    string abs = "";
                    if (!string.IsNullOrEmpty(MessageText)) abs = Config.UserTag + MessageText + Config.AITag;

                    Task.Run(() => ProcessInstructModeAsync(abs, aiResponse));

                }
                else if (IsAdventureMode)
                {
                    IsGenerating = true;
                    Log(LogType.Main, "Sending Message in Adventure Mode.");

                    if (!string.IsNullOrEmpty(MessageText))
                    {
                        AddMessage(MessageText, true);
                    }



                    var aiResponse = AddMessage("Generating Response...", false);

                    string abs = MessageText;

                    Task.Run(() => ProcessChatModeAsync(abs, aiResponse));

                }
                else if (IsStoryMode)
                {
                    IsGenerating = true;
                    Log(LogType.Main, "Sending Message in Story Mode.");

                    if (!string.IsNullOrEmpty(MessageText))
                    {
                        AddMessage(MessageText, true);
                    }



                    var aiResponse = AddMessage("Generating Response...", false);

                    string abs = MessageText;

                    Task.Run(() => ProcessChatModeAsync(abs, aiResponse));

                }
                else if (IsChatMode)
                {
                    IsGenerating = true;
                    Log(LogType.Main, "Sending Message in Chat Mode.");

                    if (!string.IsNullOrEmpty(MessageText))
                    {
                        AddMessage(MessageText, true);
                    }



                    Message aiResponse;

                    string abs = "";
                    if (!_doRetry)
                    {
                        aiResponse = AddMessage("Generating Response...", false);
                        abs = $"\n{Config.Username}: " + MessageText + "\n" + Config.AIName + ": ";
                        _lastPrompt = abs;
                    }
                    else
                    {
                        aiResponse = Messages.Last();
                        abs = _lastPrompt;
                        _doRetry = false;
                    }

                    Task.Run(() => ProcessChatModeAsync(abs, aiResponse));

                    
                }
                else
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Mode Error", "No valid mode selected."));
                }

                MessageText = string.Empty;

            }
            else if (IsGenerating)
            {
                StopRequested = true;
            }
            
        }


        #region -------------------------------------------------->   Toolbar Stuff
        private async void RetryMessage()
        {
            if (_executor != null && Messages.Count >= 1 && !IsLoading && !IsGenerating)
            {
                if (_model.IsDisposed())
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Model Manager", "No valid or disposed context.\nPlease load a model first: Settings>Format>Load Model."));
                    return;
                }
                if (_context.IsDisposed())
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Context Manager", "No valid or disposed context.\nPlease load a context first: Settings>Format>Load Context."));
                    return;
                }
                if (IsEditing)
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Editor Manager", "Please exit edit mode first."));
                    return;
                }

                if (!Messages.Last().IsUser)
                {
                    SLogger.Log(LogType.Main, "Retrying last message.");
                    _lastPrompt = _chatHistory.Messages.FindLast(n => n.AuthorRole == AuthorRole.User).Content;

                    _requestContextUpdate = true;
                    _session.RemoveLastMessage(); 
                    _session.RemoveLastMessage(); 

                    _doRetry = true;
                    _requestContextUpdate = true;

                    await SendMessageAsync(); 
                }
                else
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Session Manager", "That's not an AI response. Can't retry on that."));
                }
            }
        }

        private async void BackMessage()
        {
            if (_executor != null && Messages.Count >= 1 && !IsLoading && !IsGenerating)
            {
                if (_model.IsDisposed())
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Model Manager", "No valid or disposed context.\nPlease load a model first: Settings>Format>Load Model."));
                    return;
                }
                if (_context.IsDisposed())
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Context Manager", "No valid or disposed context.\nPlease load a context first: Settings>Format>Load Context."));
                    return;
                }
                if (IsEditing)
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Editor Manager", "Please exit edit mode first."));
                    return;
                }

                _requestContextUpdate = true;

                var lastMessage = Messages.Last();
                RedoMessages.Add(lastMessage);
                _session.RemoveLastMessage();
                Messages.RemoveAt(Messages.Count - 1);
                current_redo_index++;

            }
        }

        private void NavigateRetryUp(Message message)
        {
            if (message?.RetryLog == null || message.RetryLog.Count == 0 || IsGenerating) return;

            int currentIndex = message.RetryLog.IndexOf(message.Text);

            if (currentIndex == -1)
            {
                // Text not found in RetryLog; initialize to the first entry
                currentIndex = 0;
            }

            // Move forward in the retry list
            int newIndex = (currentIndex + 1) % message.RetryLog.Count;

            // Update the message text to the new retry text
            message.Text = message.RetryLog[newIndex];
            _session.History.Messages.Last().Content = message.Text;
            _requestContextUpdate = true;
            message.UpdateCurrentRetryText();
            MessagingCenter.Send(this, "ScrollToBottom", new ScrollToBottomMessage(true));
        }

        private void NavigateRetryDown(Message message)
        {
            if (message?.RetryLog == null || message.RetryLog.Count == 0 || IsGenerating) return;

            int currentIndex = message.RetryLog.IndexOf(message.Text);

            if (currentIndex == -1)
            {
                // Text not found in RetryLog; initialize to the first entry
                currentIndex = message.RetryLog.Count;
            }

            // Calculate the new index, wrapping around to the end if necessary
            int newIndex = (currentIndex - 1 + message.RetryLog.Count) % message.RetryLog.Count;

            // Update the message text to the new retry text
            message.Text = message.RetryLog[newIndex];
            _session.History.Messages.Last().Content = message.Text;
            _requestContextUpdate = true;
            message.UpdateCurrentRetryText();
            MessagingCenter.Send(this, "ScrollToBottom", new ScrollToBottomMessage(true));
        }

        private async void RedoMessageFunc()
        {
            if (current_redo_index >= 0 && RedoMessages.Count > 0)
            {
                var redoMessage = RedoMessages[current_redo_index];
                Messages.Add(redoMessage);
                _session.AddMessage(new ChatHistory.Message(authorRole: redoMessage.IsUser ? AuthorRole.User : AuthorRole.Assistant, redoMessage.Text)); // Re-add to session context
                current_redo_index--;
                RedoMessages.RemoveAt(RedoMessages.Count - 1);
                MessagingCenter.Send(this, "ScrollToBottom", new ScrollToBottomMessage(true));
            }
        }

        #region ComfyUI Controls

        private async void GenImgClicked()
        {

            Message msg = Messages.LastOrDefault();
            if (msg != null)
            {
                _isFromImgGen = true;
                if (Config.useMClutching)
                {
                    DisposeModel();
                }
                Log(LogType.Main, "Attepting to send image generation request to ComfyUI");
                byte[] src = await _cui.RequestGeneration(AddImgEntryText); // Fix this, this should run entirely on seperate thread
                if(src != null)
                {
                    msg.Images.Add(src);
                    string copy = msg.Text;
                    msg.Text += ".";
                    msg.Text = copy;
                    MessagingCenter.Send(this, "ScrollToBottom", new ScrollToBottomMessage(true));
                    Log(LogType.Main, "Received Image");

                }
                if (Config.useMClutching)
                {
                    LoadModel();
                }
                _isFromImgGen = false;

            }
            else
            {
                MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("ComfyUI Manager", "No message found to attach image to. Please start chatting first."));
            }
        }

        private async void GenSummaryClicked()
        {
            try
            {
                if (Messages.Count == 0)
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("ComfyUI Manager", "Chat is empty and cannot summarize."));
                    return;
                }

                Log(LogType.Main, "Generating chat Summary.");

                ChatHistory contentToSummarize = new ChatHistory();
                foreach (var message in _chatHistory.Messages)
                {
                    // Create a new instance of each message with the same content
                    contentToSummarize.Messages.Add(new ChatHistory.Message(message.AuthorRole, message.Content));
                }

                List<LLamaToken> summaryTokens = new List<LLamaToken>();
                foreach (var item in contentToSummarize.Messages)
                {
                    summaryTokens.AddRange(_context.Tokenize(item.Content, false, true));
                }


                // Check that the content fits into the context size and a proper summary can be written
                int memTokens;

                string ContextContent = _context.DeTokenize(summaryTokens);

                int contentCount = _context.Tokenize("[SCENARIO BEGINNING. Start to Summarize from here.]:\n" + ContextContent + "[SCENARIO ENDING. End to Summarize here.] <System: Only use keywords for the description. You are not allowed to write anything else except the summary. Generate a description with only key words for the latest scene in this chat for the Image Generator.>\n" + Config.AIName + ":", false, true).Count();
                if (contentCount > Config.ContextSize)
                {
                    memTokens = _context.Tokenize(_chatMemory.Messages.First().Content, false, true).Count();
                    summaryTokens.RemoveRange(memTokens, contentCount - Config.ContextSize);
                }
                if (summaryTokens.Count > Config.ContextSize)
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("ComfyUI Manager", "Way too many memory tokens. Cannot write a summary."));
                    return;
                }

                ContextContent = _context.DeTokenize(summaryTokens);
                contentToSummarize.Messages.Clear();
                contentToSummarize.Messages.Insert(0, new ChatHistory.Message(AuthorRole.User, "[SCENARIO BEGINNING. Start to Summarize from here.]:\n" + ContextContent + "[SCENARIO ENDING. End to Summarize here.] <System: Only use keywords for the description. You are not allowed to write anything else except the summary. Generate a description with only key words for the latest scene in this chat for the Image Generator.>\n" + Config.AIName + ":"));



                _context.NativeHandle.KvCacheClear();
                _executor = new InteractiveExecutor(_context);
                _session = new ChatSession(_executor, contentToSummarize);


                var inferenceParams = new InferenceParams
                {
                    MaxTokens = 128,
                    TopK = Config.TopK,
                    MinP = Config.MinP,
                    PresencePenalty = Config.PresencePenalty,
                    Temperature = Config.Temperature,
                    RepeatPenalty = Config.RepPen,
                    TopP = Config.TopP,
                    TypicalP = Config.Typical,
                    TfsZ = Config.Tfs,
                    Mirostat = Config.MirostatType,
                    MirostatEta = Config.MirostatEta,
                    MirostatTau = Config.MirostatTau,
                    LogitBias = new Dictionary<LLamaToken, float>()
                    {
                        {(LLamaToken)_model.Tokens.EOS, float.NegativeInfinity },
                    }

                };

                await foreach (var text in _session.ChatAsync(contentToSummarize.Messages.FirstOrDefault(), inferenceParams))
                {
                    AddImgEntryText += text;
                }

                _context.NativeHandle.KvCacheClear();
                _executor = new InteractiveExecutor(_context);
                _session = new ChatSession(_executor, _chatHistory);
            }
            catch (Exception ex)
            {

            }
        }

        private async void GenLFMClicked()
        {
            try
            {
                if (Messages.Count == 0)
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("ComfyUI Manager", "No message found to attach image to. Please start chatting first."));
                    return;
                }

                string prompt = "";
                foreach (var message in _chatHistory.Messages.TakeLast(4))
                {
                    // Create a new instance of each message with the same content
                    prompt += message.Content;
                }

                Message msg = Messages.LastOrDefault();
                Log(LogType.Main, "Attepting to send image generation request to ComfyUI");
                byte[] src = _cui.RequestGeneration(prompt).Result;
                if (src != null)
                {
                    msg.Images.Add(src);
                    string copy = msg.Text;
                    msg.Text += ".";
                    msg.Text = copy;
                    MessagingCenter.Send(this, "ScrollToBottom", new ScrollToBottomMessage(true));
                    Log(LogType.Main, "Received Image");

                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
        #endregion


        #region -------------------------------------------------->   Processors
        private async Task ProcessChatModeAsync(string userInput, Message aiMessage)
        {
            try
            {
                if (IsChatMode)
                {
                    //We want to edit the Names. It's in here. 
                    var historyTransform = new LLamaTransforms.DefaultHistoryTransform();
                    historyTransform.SetUserName(Config.Username);
                    historyTransform.SetAssistantName(Config.AIName);
                    _session.HistoryTransform = historyTransform;
                }


                if (_requestContextUpdate)
                {
                    await SetContext(); //AKA UpdateContext
                    _requestContextUpdate = false;
                }
                InspectKvCache();



                List<string> APS = new List<string> { $"\n{Config.Username}:", $"\n{Config.AIName}:"};
                foreach (var ss in StopSequences)
                {
                    var s = ss.Replace("|", "");
                    s = s.Replace("{{user}}", Config.Username);
                    s = s.Replace("{{char}}", Config.AIName);
                    APS.Add(s);
                }


                var inferenceParams = new InferenceParams
                {
                    MaxTokens = Config.MaxOutput,
                    TopK = Config.TopK,
                    MinP = Config.MinP,
                    PresencePenalty = Config.PresencePenalty,
                    Temperature = Config.Temperature,
                    RepeatPenalty = Config.RepPen,
                    AntiPrompts = APS,
                    TopP = Config.TopP,
                    TypicalP = Config.Typical,
                    TfsZ = Config.Tfs,
                    Mirostat = Config.MirostatType,
                    MirostatEta = Config.MirostatEta,
                    MirostatTau = Config.MirostatTau,
                    LogitBias = new Dictionary<LLamaToken, float>()
                    {
                        {(LLamaToken)_model.Tokens.EOS, float.NegativeInfinity },
                    }

                };

                foreach(var item in LogitBiasCollection)
                {
                    LLamaToken[] tokenSequence = _context.Tokenize(item.Token, false, false);
                    foreach (var token in tokenSequence)
                    {
                        inferenceParams.LogitBias.Add(token, LogitManipulation.PercentageToLogitBias(item.Bias));
                    }
                }
                

                var responseBuilder = new StringBuilder();
                string asb = "";
                int backtrackStreamCount = 15;
                int currentIndex = 0;
                int previousTextLength = 0;
                bool isRunning = false; //AntiLag
                bool isHitName = false; //Filter out the e.g -> KoboldSharp: <- Generated text
                List<string> textBuffer = new List<string>();
                Log(LogType.LLaMA, "Starting Prompt Processing.");



                //Timing stuff
                bool gotPPT = false;
                float PromptProcessingTime = 0;
                int totalTokensGenerated = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();


                //KVHS variables
                int initialHistoryTokenCount = 0;
                int initialKvCacheTokenCount = 0;
                int preinitialHistoryTokenCount = GetHistoryTokensCount();
                int preinitialKvCacheTokenCount = _context.NativeHandle.KvCacheCountCells();
                bool needReProcess = false;

                await foreach (var text in _session.ChatAsync(new ChatHistory.Message(AuthorRole.User, userInput), inferenceParams))
                {
                    totalTokensGenerated++;
                    if (!gotPPT)
                    {
                        PromptProcessingTime = (float)sw.ElapsedMilliseconds / 1000;
                        Log(LogType.LLaMA, _session.History.ToJson());
                        Console.WriteLine();

                        // Initial setup and calculations KV-H-SYNC
                        initialHistoryTokenCount = GetHistoryTokensCount();
                        initialKvCacheTokenCount = _context.NativeHandle.KvCacheCountCells();

                        Log(LogType.Info, $"H-SYNC: {initialHistoryTokenCount} + KV-SYNC: {initialKvCacheTokenCount}");

                        int calculatedHistoryTokenCount = initialHistoryTokenCount - preinitialHistoryTokenCount;
                        int calculatedKvCacheTokenCount = initialKvCacheTokenCount - preinitialKvCacheTokenCount;
                        if(calculatedKvCacheTokenCount < 0) calculatedKvCacheTokenCount += 128;

                        if (calculatedHistoryTokenCount != calculatedKvCacheTokenCount) //The KV can only be bigger
                        {
                            Console.WriteLine();
                            Console.WriteLine("<---->");
                            Log(LogType.Warn, "KVHS detected unsynchronization in KV-Cache(Caused after Prompt-Processing). ReDecoding the last message to sync back.");
                            Console.WriteLine("<---->");

                            needReProcess = true;

                        }

                        gotPPT = true;
                        Messages.Last().Text = "";
                        sw.Restart();
                    }
                    responseBuilder.AppendLine(text);
                    textBuffer.Add(text);
                    asb += text;
                    if (StopRequested)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(text);
                        Console.ResetColor();
                        Console.WriteLine();
                        Log(LogType.Main, "Stop Requested.");
                        break;
                    }
                    //Simple Backtrack Streaming, so the trimming doesn't get shown afterwards.
                    currentIndex++;
                    if (currentIndex > backtrackStreamCount)
                    {
                        // Get the backtracked range from the buffer
                        var rangeStartIndex = currentIndex - backtrackStreamCount;
                        var rangeText = string.Join("", textBuffer.GetRange(0, rangeStartIndex));
                        var newText = rangeText.Substring(previousTextLength);
                        if (!isRunning) //Anti-Lag: Workaround, so the UI responses at all.
                        {

                            MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                isRunning = true;

                                aiMessage.Text += newText; //PERFORMANCE ISSUE
                                previousTextLength = rangeText.Length;
                                MessagingCenter.Send(this, "ScrollToBottom", new ScrollToBottomMessage(true));
                                isRunning = false;
                            });

                        }

                    }

                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(text);
                    Console.ResetColor();

                }

                if (StopRequested)
                {
                    Log(LogType.LLaMA, "Canceled Generation. Stop Requested.");
                    _chatHistory.AddMessage(AuthorRole.Assistant, asb);

                }

                _executor.Cleanup();


                // Calculate tokens after processing
                int processedHistoryTokenCount = GetHistoryTokensCount();
                int processedKvCacheTokenCount = _context.NativeHandle.KvCacheCountTokens(); //This can be less or more than processedHistoryTokenCount

                int aftercalculatedHistoryTokenCount = processedHistoryTokenCount - preinitialHistoryTokenCount;
                int aftercalculatedKvCacheTokenCount = processedKvCacheTokenCount - preinitialKvCacheTokenCount;
                if (aftercalculatedKvCacheTokenCount < 0) aftercalculatedKvCacheTokenCount += 128;

                // Since we use context shifting, we wouldn't be able to just use the != operator. This checks wheter the numbers do hit each other in the step range of 128
                if (Math.Abs(aftercalculatedHistoryTokenCount - aftercalculatedKvCacheTokenCount) % 128 != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("<---->");
                    Log(LogType.Warn, "KVHS detected unsynchronization in KV-Cache(Caused after generation if this is not a duplicated message). ReDecoding the last message to sync back.");
                    Console.WriteLine("<---->");
                    needReProcess = true;

                }

                if (needReProcess)
                {

                    List<LLamaToken> lLamaTokens = new List<LLamaToken>();
                    // Tokenize each message's content
                    LLamaToken[] tokensArray = _context.Tokenize(_session.History.Messages.Last().Content, false, true);

                    List<LLamaToken> tokensList = tokensArray.ToList();

                    // Add the tokens to the lLamaTokens list
                    lLamaTokens.AddRange(tokensList);

                    // Calculate the beginningPos of the kvCache
                    int aftercalculatedHistoryTokenCountCopy = processedHistoryTokenCount - initialHistoryTokenCount;
                    int aftercalculatedKvCacheTokenCountCopy = aftercalculatedKvCacheTokenCount;
                    aftercalculatedKvCacheTokenCountCopy -= 11;
                    while ((Math.Abs(aftercalculatedHistoryTokenCount - aftercalculatedKvCacheTokenCountCopy) % 128 != 0))
                    {
                        aftercalculatedKvCacheTokenCountCopy++;
                        if(aftercalculatedKvCacheTokenCountCopy > aftercalculatedKvCacheTokenCount + 11)
                        {
                            Log(LogType.Error, "Failed to find the sync starting pos. Way too many anomalies.");
                            break;
                        }
                    }
                    int difference = aftercalculatedHistoryTokenCount - aftercalculatedKvCacheTokenCount;
                    int beginningPos = 0;
                    if(aftercalculatedKvCacheTokenCount < aftercalculatedKvCacheTokenCountCopy)
                    {
                        beginningPos = (processedKvCacheTokenCount - aftercalculatedHistoryTokenCountCopy) + difference;
                        processedKvCacheTokenCount += difference;
                    }
                    else
                    {
                        beginningPos = (processedKvCacheTokenCount - aftercalculatedHistoryTokenCountCopy) + difference;
                        processedKvCacheTokenCount += difference;

                    }

                    // Now fix the unsync
                    await _executor.ReDecode(lLamaTokens, beginningPos);
                    Log(LogType.Info, "ReDecoded the last message.");
                }

                float elapsedTimeInSeconds = (float)sw.ElapsedMilliseconds / 1000;
                float tokensPerSecond = elapsedTimeInSeconds > 0 ? totalTokensGenerated / elapsedTimeInSeconds : 0;
                sw.Reset();
                sw.Start();


                // Remove Stop Sequence only if at the end
                foreach (var antiPrompt in inferenceParams.AntiPrompts)
                {
                    if (asb.EndsWith(antiPrompt))
                    {
                        // Remove the antiPrompt from the end
                        asb = asb.Substring(0, asb.Length - antiPrompt.Length).Trim();
                    }
                }

                //Trim incomplete sentence
                char[] delimiters = { '.', '!', '?', '*', '"', ')'};
                int lastIndex = -1;

                foreach (var delimiter in delimiters)
                {
                    int index = asb.LastIndexOf(delimiter);
                    if (index > lastIndex)
                    {
                        lastIndex = index;
                    }
                }

                if (lastIndex >= 0)
                {
                    asb = asb.Substring(0, lastIndex + 1).Trim();
                }
                else
                {
                    asb = asb.Trim();
                }


                _session.RemoveLastMessage();
                _chatHistory.AddMessage(AuthorRole.Assistant, asb);

                // Calculate Trimmed Tokens
                int TrimmedHistoryTokenCount = processedHistoryTokenCount - GetHistoryTokensCount();
                int exceptedTokenCount = processedKvCacheTokenCount - TrimmedHistoryTokenCount;

                // Debug output
                Console.WriteLine();
                Log(LogType.Info, $"Tokens cleaned: {TrimmedHistoryTokenCount}");
                Log(LogType.Info, $"Excepted Token Count HISTORY: {processedHistoryTokenCount - TrimmedHistoryTokenCount}");
                Log(LogType.Info, $"Excepted Token Count KVCACHE: {exceptedTokenCount}");

                // Remove the required number of tokens from the kv-cache to sync with the history
                _context.NativeHandle.KvCacheRemove(LLamaSeqId.Zero, _context.NativeHandle.KvCacheCountCells() - TrimmedHistoryTokenCount, _context.NativeHandle.KvCacheCountCells());





                float PostProcessing = (float)sw.ElapsedMilliseconds / 1000;
                sw.Stop();


                Console.WriteLine();
                Console.WriteLine();


                Log(LogType.Info, "Prompt-Processing: " + PromptProcessingTime + " s | Tokens-Per-Second: " + tokensPerSecond + " T/s | Post-Processing: " + PostProcessing + " s.");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    aiMessage.Text = asb;
                    aiMessage.UpdateRetryLog();

                    MessagingCenter.Send(this, "ScrollToBottom", new ScrollToBottomMessage(true));
                    IsGenerating = false;
                    if (string.IsNullOrWhiteSpace(asb))
                    {
                        if (StopRequested)
                        {
                            Log(LogType.Info, "Canceled Retrying. Stop Requested.");
                            StopRequested = false;
                        }
                        Log(LogType.Warn, "AI didn't generate any output. Retrying.");
                        RetryMessage();
                        return;
                    }
                    InspectKvCache();
                    StopRequested = false;
                });
            }
            catch (Exception ex)
            {
                SLogger.Log(LogType.Error, "Error Generating: " + ex);
                IsGenerating = false;
                StopRequested = false;

            }
        }


        private async Task ProcessInstructModeAsync(string userInput, Message aiMessage)
        {
            try
            {
                await SetContext();


                if (IsChatMode)
                {
                    //We want to edit the Names. It's in here.
                    var historyTransform = new LLamaTransforms.DefaultHistoryTransform();
                    historyTransform.SetUserName(Config.Username);
                    historyTransform.SetAssistantName(Config.AIName);
                    _session.HistoryTransform = historyTransform;
                }

                if (string.IsNullOrEmpty(Config.UserTag)) Config.UserTag = "";
                if (string.IsNullOrEmpty(Config.AITag)) Config.AITag = "";
                var inferenceParams = new InferenceParams
                {
                    MaxTokens = 5012,
                    TopK = 200,
                    MinP = 0.01f,
                    FrequencyPenalty = 1.1f,
                    Temperature = 0.7f,
                    RepeatPenalty = 1.1f,
                    AntiPrompts = new List<string> { $"{Config.UserTag.Trim(' ')}", $"{Config.AITag.Trim(' ')}" }
                };

                var responseBuilder = new StringBuilder();
                string asb = "";
                int backtrackStreamCount = 15;
                int currentIndex = 0;
                int previousTextLength = 0;
                bool isRunning = false; //AntiLag
                bool isHitName = false; //Filter out the e.g -> KoboldSharp: <- Generated text
                List<string> textBuffer = new List<string>();
                Log(LogType.LLaMA, "Starting Generation.");

                //Timing stuff
                bool gotPPT = false;
                float PromptProcessingTime = 0;
                int totalTokensGenerated = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();


                await foreach (var text in _session.InstructAsync(new ChatHistory.Message(AuthorRole.User, userInput), inferenceParams))
                {
                    totalTokensGenerated++;
                    if (!gotPPT)
                    {
                        PromptProcessingTime = (float)sw.ElapsedMilliseconds / 1000;
                        Log(LogType.LLaMA, _session.History.ToJson());
                        Console.WriteLine();

                        gotPPT = true;
                        Messages.Last().Text = "";
                        sw.Restart();
                    }
                    responseBuilder.AppendLine(text);
                    textBuffer.Add(text);
                    asb += text;
                    //Simple Backtrack Streaming, so the trimming doesn't get shown afterwards.
                    currentIndex++;
                    if (currentIndex > backtrackStreamCount)
                    {
                        // Get the backtracked range from the buffer
                        var rangeStartIndex = currentIndex - backtrackStreamCount;
                        var rangeText = string.Join("", textBuffer.GetRange(0, rangeStartIndex));
                        var newText = rangeText.Substring(previousTextLength);
                        if (!isRunning) //Anti-Lag: Workaround, so the UI responses at all.
                        {
                            if (StopRequested)
                            {
                                Console.WriteLine();
                                Log(LogType.Main, "Stop Requested.");
                                break;
                            }
                            if (asb.Contains((Config.UserTag).ToString()) || asb.Contains((Config.AITag).ToString()))
                            {
                                Console.WriteLine();
                                Log(LogType.LLaMA, "Stop Token Hit!");
                                break;
                            }

                            MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                isRunning = true;

                                aiMessage.Text += newText; //PERFORMANCE ISSUE
                                previousTextLength = rangeText.Length;
                                MessagingCenter.Send(this, "ScrollToBottom", new ScrollToBottomMessage(true));
                                isRunning = false;
                            });
                        }

                    }

                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(text);
                    Console.ResetColor();

                }


                if (StopRequested)
                {
                    Log(LogType.LLaMA, "Canceled Generation. Stop Requested.");
                    StopRequested = false;
                }

                float elapsedTimeInSeconds = (float)sw.ElapsedMilliseconds / 1000;
                float tokensPerSecond = elapsedTimeInSeconds > 0 ? totalTokensGenerated / elapsedTimeInSeconds : 0;
                sw.Reset();
                sw.Start();

                //Remove Stop Sequence
                foreach (var antiPrompt in inferenceParams.AntiPrompts)
                {
                    asb = asb.Replace(antiPrompt, "");
                }

                //Trim incomplete sentence
                //TODO bool check
                char[] delimiters = { '.', '!', '?', '*', '"', ')'};
                int lastIndex = -1;

                foreach (var delimiter in delimiters)
                {
                    int index = asb.LastIndexOf(delimiter);
                    if (index > lastIndex)
                    {
                        lastIndex = index;
                    }
                }

                //Finally trim
                if (lastIndex >= 0)
                {
                    asb = asb.Substring(0, lastIndex + 1).Trim();
                }
                else
                {
                    asb = asb.Trim();
                }



                float PostProcessing = (float)sw.ElapsedMilliseconds / 1000;
                sw.Stop();


                Console.WriteLine();
                Console.WriteLine();


                Log(LogType.Info, "Prompt-Processing: " + PromptProcessingTime + " s | Tokens-Per-Second: " + tokensPerSecond + " T/s | Post-Processing: " + PostProcessing + " s.");
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    aiMessage.Text = asb;
                    MessagingCenter.Send(this, "ScrollToBottom", new ScrollToBottomMessage(true));
                    _session.RemoveLastMessage();
                    _chatHistory.AddMessage(AuthorRole.Assistant, aiMessage.Text);
                    IsGenerating = false;
                    if (string.IsNullOrWhiteSpace(asb))
                    {
                        RetryMessage();
                        return;
                    }
                    InspectKvCache();
                });
            }
            catch (Exception ex)
            {
                SLogger.Log(LogType.Error, "Error Generating: " + ex);
            }
        }

        private async Task ProcessAdventureModeAsync(string messageText, object aiResponse)
        {
            // Placeholder for adventure mode processing
            await Task.CompletedTask;
        }

        private async Task ProcessStoryModeAsync(string messageText, object aiResponse)
        {
            // Placeholder for story mode processing
            await Task.CompletedTask;
        }
        #endregion























        #region -------------------------------------------> Context Stuff Area
        private async Task SetContext()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Log(LogType.LLaMA, "Updating Context.");

            var mtg = MemoryText;
            mtg = mtg.Replace("{{user}}", Config.Username);
            mtg = mtg.Replace("{{char}}", Config.AIName);

            _session.History = _chatHistory;
            _chatMemory.Messages.First().Content = mtg;
            if (AddNewLineMemory) _chatMemory.Messages.First().Content += "\n";
            _session.SetMemory(_chatMemory);

            int memTokens = _context.Tokenize(_chatMemory.Messages.First().Content, false, true).Count();
            List<LLamaToken> lLamaTokens = new List<LLamaToken>();
            foreach (var message in _session.History.Messages)
            {
                // Tokenize each message's content
                LLamaToken[] tokensArray = _context.Tokenize(message.Content, false, true);

                List<LLamaToken> tokensList = tokensArray.ToList();

                // Add the tokens to the lLamaTokens list
                lLamaTokens.AddRange(tokensList);
            }
            Console.WriteLine("SENDING TOKENS: " + lLamaTokens.Count);
            SafeLlamaModelHandle.ModelTokens.DeTokenize(lLamaTokens);
            InspectKvCache();
            await _executor.UpdateWithDecode(lLamaTokens, memTokens); 

            _kvCacheView.Dispose();
            _kvCacheView = LLamaKvCacheViewSafeHandle.Allocate(_context.NativeHandle, 4);

            _context.NativeHandle.KvCacheUpdate();
            float UpdateContextTime = (float)sw.ElapsedMilliseconds / 1000;
            Log(LogType.CFG, "Updated context in: " + UpdateContextTime + "s.");
            sw.Stop();
        }


        public int GetHistoryTokensCount()
        {
            List<LLamaToken> lLamaTokens = new List<LLamaToken>();

            foreach (var message in _session.History.Messages)
            {
                // Tokenize each message's content
                LLamaToken[] tokensArray = _context.Tokenize(message.Content, false, true);

                List<LLamaToken> tokensList = tokensArray.ToList();

                // Add the tokens to the lLamaTokens list
                lLamaTokens.AddRange(tokensList);
            }
            return lLamaTokens.Count();
        }


        private async void DisposeContext()
        {
            if (_context == null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Context Manager", "No Context loaded ✓"));
                });
                return;
            }
            if (_context.IsDisposed() && !_isFromModel && !_isFromImgGen)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Context Manager", "Already Disposed ✓"));
                });
                return;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Log(LogType.LLaMA, "Disposing Context.");

            _context.Dispose();
            _kvCacheView.Dispose();
            Log(LogType.LLaMA, "Disposed Context.");
            float DisposeContextTime = (float)sw.ElapsedMilliseconds / 1000;
            Log(LogType.CFG, "Disposed context in: " + DisposeContextTime + "s.");
            sw.Stop();

            if (!_isFromModel && !_isFromImgGen)
            {
                await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Context Manager", "Disposed Context.")); });
            }
        }

        private async void LoadContext()
        {
            if (_model.IsDisposed())
            {
                MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Model Manager", "No valid or disposed context.\nPlease load a model first: Settings>Format>Load Model."));
                return;
            }
            if (!_context.IsDisposed() && _context.ContextSize == Config.ContextSize)
            {
                MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Context Manager", "Already Loaded ✓"));
                return;
            }

            Task.Run(async () => LoadContextAsync());
        }

        private async Task LoadContextAsync()
        {

            IsLoading = true;
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Log(LogType.LLaMA, "Loading Context.");
                var parameters = new ModelParams(Config.ModelPath)
                {
                    ContextSize = (uint)Config.ContextSize,
                    GpuLayerCount = 100,
                };
                _requestContextUpdate = true;
                _context = _model.CreateContext(parameters);
                _executor = new InteractiveExecutor(_context);
                _session = new ChatSession(_executor, _chatHistory);
                _kvCacheView = LLamaKvCacheViewSafeHandle.Allocate(_context.NativeHandle, 4);
                Log(LogType.LLaMA, "Loaded Context.");
                float LoadContextTime = (float)sw.ElapsedMilliseconds / 1000;
                Log(LogType.CFG, "Loaded context in: " + LoadContextTime + "s.");
                sw.Stop();

                IsLoading = false;
                await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Context Manager", "Loaded Context. Size: " + parameters.ContextSize)); });


            }
            catch (Exception ex)
            {
                IsLoading = false;
                await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Context Manager", "Couldn't load context.")); });
                Log(LogType.Error, "Couldn't load context: " + ex);

            }
        }

        #endregion





        private List<Message> backups = new List<Message>();
        #region UI settings
        public async void ToggleEditMode()
        {
            IsEditing = !IsEditing;

            if (IsEditing)
            {
                _requestContextUpdate = true;
                SLogger.Log(LogType.Main, "Enabling Edit Mode.");
                // Entering edit mode
                backups.Clear();
                foreach (var message in Messages)
                {
                    backups.Add(new Message
                    {
                        Id = message.Id,
                        Title = message.Title,
                        Text = message.Text,
                        IsUser = message.IsUser,
                        ProfileImage = message.ProfileImage,
                        IsEditing = true
                    });
                    message.IsEditing = true;
                }
            }
            else
            {
                SLogger.Log(LogType.Main, "Disabling Edit Mode and applying changes.");
                // Exiting edit mode and saving changes
                foreach (var message in Messages)
                {
                    var newText = message.Text;

                    if (string.IsNullOrEmpty(newText))
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Error",               // Title of the alert
                            "Empty Message Found!", // Message to display
                            "OK");   // Button text
                        IsEditing = !IsEditing;

                        return;
                    }

                    if (message.IsUser)
                    {
                        // Editing a user message
                        await EditUserMessageAsync(message, newText);
                    }
                    else
                    {
                        // Editing an AI message
                        await EditAIMessageAsync(message, newText);
                    }

                    var oldMessage = backups.FirstOrDefault(m => m.Id == message.Id);
                    if (oldMessage != null && oldMessage.Text != newText)
                    {
                        message.RetryLog.Add(newText);
                        message.UpdateCurrentRetryText();

                    }
                    message.IsEditing = false;

                }
                MessagingCenter.Send(this, "ScrollToBottom", new ScrollToBottomMessage(true));
            }
        }


        public Message AddMessage(string text, bool isUser)
        {
            var message = new Message
            {
                Title = isUser ? Config.Username : Config.AIName,
                Text = text,
                IsUser = isUser,
                IsEditing = false,
                // Set the image based on the message type
                ProfileImage = isUser ? null : "koboldsharpfill.png"  // Show image for AI, not for user
            };
            Messages.Add(message);
            return message;
        }
        private async Task OnStartNewSession()
        {
            IsLoading = true;
            if (!_keepContext)
            {
                _chatHistory.Messages.Clear();
                await SetContext();
                _requestContextUpdate = true;
            }
            Messages.Clear();
            await Task.Delay(500);
            IsLoading = false;
        }
        #endregion







        #region Edit functionality
        private async Task EditUserMessageAsync(Message editedMessage, string newText)
        {
            if (editedMessage == null || string.IsNullOrWhiteSpace(newText))
                return;

            var oldMessage = backups.FirstOrDefault(m => m.Id == editedMessage.Id);
            var realoldMessage = _session.History.Messages.FirstOrDefault(m => m.Content == ($"\n{Config.Username}: " + oldMessage.Text + "\n" + Config.AIName + ": "));
            if (oldMessage == null || realoldMessage == null) return;

            realoldMessage.Content = $"\n{Config.Username}: " + newText + "\n" + Config.AIName + ": ";

        }

        private async Task EditAIMessageAsync(Message editedMessage, string newText)
        {
            if (editedMessage == null || string.IsNullOrWhiteSpace(newText))
                return;

            var oldMessage = backups.FirstOrDefault(m => m.Id == editedMessage.Id);
            var realoldMessage = _session.History.Messages.FirstOrDefault(m => m.Content == oldMessage.Text);

            if (oldMessage == null || realoldMessage == null) return;

            realoldMessage.Content = newText;

        }


        #endregion


        #region UI Settings Tab

        private void OnTabSelected(string tabName)
        {
            SelectedTab = tabName;
        }

        private void OnContextTabSelected(string tabName)
        {
            SelectedTabContext = tabName;
        }




        #region FormatTab
        private void ApplyPreset(string preset)
        {
            if (preset == "[Custom]")
                return;

            _isApplyingPreset = true;
            try
            {
                switch (preset)
                {
                    case "Alpaca":
                        SystemTag = @"";
                        SystemPrompt = @"";
                        UserTag = @"\n### Instruction:\n";
                        AITag = @"\n### Response:\n";
                        break;
                    case "Victuna":
                        SystemTag = @"";
                        SystemPrompt = @"";
                        UserTag = @"\nUSER: ";
                        AITag = @"\nASSISTANT: ";
                        break;
                    case "Methame":
                        SystemTag = @"";
                        SystemPrompt = @"";
                        UserTag = @"<|user|>";
                        AITag = @"<|model|>";
                        break;
                    case "Llama 2 Chat":
                        SystemTag = @"";
                        SystemPrompt = @"";
                        UserTag = @"[INST] ";
                        AITag = @" [/INST]";
                        break;
                    case "Q & A":
                        SystemTag = @"";
                        SystemPrompt = @"";
                        UserTag = @"\nQuestion: ";
                        AITag = @"\nAnswer: ";
                        break;
                    case "ChatML":
                        SystemTag = @"<|im_start|>system\n";
                        SystemPrompt = @"";
                        UserTag = @"<|im_end|>\n<|im_start|>user\n";
                        AITag = @"<|im_end|>\n<|im_start|>assistant\n";
                        break;
                    case "Input & Output":
                        SystemTag = @"";
                        SystemPrompt = @"";
                        UserTag = @"\n{{[INPUT]}}\n";
                        AITag = @"\n{{[OUTPUT]}}\n";
                        break;
                    case "CommandR":
                        SystemTag = @"<|START_OF_TURN_TOKEN|><|SYSTEM_TOKEN|>";
                        SystemPrompt = @"";
                        UserTag = @"<|END_OF_TURN_TOKEN|><|START_OF_TURN_TOKEN|><|USER_TOKEN|>";
                        AITag = @"<|END_OF_TURN_TOKEN|><|START_OF_TURN_TOKEN|><|CHATBOT_TOKEN|>";
                        break;
                    case "Llama & Chat":
                        SystemTag = @"<|start_header_id|>system<|end_header_id|>\n\n";
                        SystemPrompt = @"";
                        UserTag = @"<|eot_id|><|start_header_id|>user<|end_header_id|>\n\n";
                        AITag = @"<|eot_id|><|start_header_id|>assistant<|end_header_id|>\n\n";
                        break;
                    case "Phi-3 Mini":
                        SystemTag = @"<|system|>\n";
                        SystemPrompt = @"";
                        UserTag = @"<|end|><|user|>\n";
                        AITag = @"<|end|>\n<|assistant|>";
                        break;
                    case "Gemma 2":
                        SystemTag = @"<start_of_turn>user\n";
                        SystemPrompt = @"";
                        UserTag = @"<end_of_turn>\n<start_of_turn>user\n";
                        AITag = @"<end_of_turn>\n<start_of_turn>model\n";
                        break;
                    case "Mistral":
                        SystemTag = @"";
                        SystemPrompt = @"";
                        UserTag = @"\n[INST] ";
                        AITag = @" [/INST]\n";
                        break;
                    default:
                        SystemTag = @"";
                        SystemPrompt = @"";
                        UserTag = @"";
                        AITag = @"";
                        break;
                }
            }
            finally
            {
                _isApplyingPreset = false;
            }
        }

        private void CheckCustomPreset()
        {
            if (_isApplyingPreset)
                return;

            if (ITPreset.First() != "[Custom]")
                return;

            var customPreset = new { SystemTag = "", SystemPrompt = "", UserTag = "", AITag = "" };
            var current = new { SystemTag, SystemPrompt, UserTag, AITag };

            SelectedPreset = customPreset.Equals(current) ? "[Custom]" : ITPreset.FirstOrDefault(preset =>
                SystemTag == $"{preset}SystemTag" &&
                SystemPrompt == $"{preset}SystemPrompt" &&
                UserTag == $"{preset}UserTag" &&
                AITag == $"{preset}AITag") ?? "[Custom]";
        }
        #endregion
        #endregion

        #region UI Context Tab
        #region Tokens Tab
        private async Task OnAddNewSequence()
        {
            // Prompt the user for input
            string newSequence = await Application.Current.MainPage.DisplayPromptAsync("Add New Sequence", "Enter the new stop sequence:");

            if (!string.IsNullOrWhiteSpace(newSequence))
            {
                if(!StopSequences.Contains("|" + newSequence + "|"))
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        try
                        {
                            StopSequences.Add("|" + newSequence + "|");

                        }
                        catch (Exception ex)
                        {
                            Log(LogType.Error, "" + ex);
                        }
                    });
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Sequence Manager", "That Stop-Sequence already exists."));
                    });
                }

            }
        }

        private void OnRemoveSequence(string sequence)
        {
            if (StopSequences.Contains(sequence))
            {
                StopSequences.Remove(sequence);
            }
        }

        private async Task OnEditStopSequence(string sequence)
        {
            sequence = sequence.Replace("|", "");
            // Prompt the user for input with the current sequence as the initial value
            string editedSequence = await Application.Current.MainPage.DisplayPromptAsync("Edit Sequence", "Edit the stop sequence:", initialValue: sequence);

            if (!string.IsNullOrWhiteSpace(editedSequence))
            {
                if (!StopSequences.Contains("|" + editedSequence + "|"))
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        int index = StopSequences.IndexOf("|" + sequence + "|");
                        if (index >= 0)
                        {
                            StopSequences[index] = "|" + editedSequence + "|";
                        }
                    });
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Sequence Manager", "That Stop-Sequence already exists."));
                    });
                }


            }
        }
        #endregion

        #region Logits Tab
        public async Task OnAddNewLogitModifier(string newLogitBias, float bias)
        {

            if (!string.IsNullOrWhiteSpace(newLogitBias))
            {
                // Check if an entry with the given token already exists
                var existingEntry = LogitBiasCollection.FirstOrDefault(n => n.Token == newLogitBias);

                if (existingEntry == null) // If it doesn't exist
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        try
                        {
                            // Add a new entry with the token and a default bias (e.g., 0f)
                            LogitBiasCollection.Add(new LogitBiasEntry(newLogitBias, bias));
                        }
                        catch (Exception ex)
                        {
                            Log(LogType.Error, ex.ToString());
                        }
                    });
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        MessagingCenter.Send(this, "DisplayAlert", new DisplayAlertMessage("Logit Manager", "That Logit Bias already exists."));
                    });
                }
            }
        }

        private void OnRemoveLogitModifier(LogitBiasEntry logitModifier)
        {
            var entryToRemove = LogitBiasCollection.FirstOrDefault(n => n.Token == logitModifier.Token);
            if (entryToRemove != null)
            {
                LogitBiasCollection.Remove(entryToRemove);
            }
        }

        private LogitBiasEntry cachedModifierIndexObject { get; set; }
        // Send to the MainPage.xaml.cs
        private async Task OnEditLogitModifier(LogitBiasEntry logitModifier)
        {
            cachedModifierIndexObject = LogitBiasCollection.FirstOrDefault(n => n.Token == logitModifier.Token);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send(this, "RequestLogitModifierEdit", new RequestLogitModifierEdit(logitModifier));
            });
        }

        // Receive the Edited changes
        public async Task EditLogitModifier(LogitBiasEntry logitModifier)
        {
            LogitBiasCollection[LogitBiasCollection.IndexOf(cachedModifierIndexObject)] = logitModifier;
        }
        #endregion
        #endregion

        #region UI Adv Settings Tab
        public async void OnPrecisionTypeChanged()
        {
            switch (SelectedPrecision)
            {
                case "Full 32 bit float":
                    Config.PrecisionType = GGMLType.GGML_TYPE_F32;
                    break;
                case "16 bit float":
                    Config.PrecisionType = GGMLType.GGML_TYPE_F16;
                    break;
                case "8 bit float":
                    Config.PrecisionType = GGMLType.GGML_TYPE_Q8_0;
                    break;
                case "4 bit float":
                    Config.PrecisionType = GGMLType.GGML_TYPE_Q4_0;
                    break;
                case "Q8 bit quant":
                    Config.PrecisionType = GGMLType.GGML_TYPE_Q8_K;
                    break;
                case "Q4 bit quant":
                    Config.PrecisionType = GGMLType.GGML_TYPE_Q4_K;
                    break;
                case "Q3 bit quant":
                    Config.PrecisionType = GGMLType.GGML_TYPE_Q3_K;
                    break;
                case "Q2 bit quant":
                    Config.PrecisionType = GGMLType.GGML_TYPE_Q2_K;
                    break;
                case "INT32 Type":
                    Config.PrecisionType = GGMLType.GGML_TYPE_I32;
                    break;
                case "INT16 Type":
                    Config.PrecisionType = GGMLType.GGML_TYPE_I16;
                    break;
                case "INT8 Type":
                    Config.PrecisionType = GGMLType.GGML_TYPE_I8;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region UI Settings Tab

        public async void OnMirostatTypeChanged()
        {
            switch (SelectedMirostat)
            {
                case "Off":
                    Config.MirostatType = MirostatType.Disable;
                    break;
                case "Mirostat":
                    Config.MirostatType = MirostatType.Mirostat;
                    break;
                case "Mirostat v2.0":
                    Config.MirostatType = MirostatType.Mirostat2;
                    break;
                default:
                    break;
            }
        }

        #endregion


        #region Media Settings Tab

        #endregion

    }


}
