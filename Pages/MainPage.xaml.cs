using Koboldcs.ComfyUI_Integration;
using Koboldcs.Configuration;
using Koboldcs.DataServices;
using Koboldcs.MessageCenter;
using Koboldcs.Models;
using Microsoft.Maui.Platform;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;


namespace Koboldcs
{

    public partial class MainPage : ContentPage
    {

        private bool isBarVisible = false;

        private LinearGradientBrush gradientBrush;
        private Border brd;
        private ContentView animation = new ContentView();
        private bool isAdvChanged = false;

        public MainPage()
        {
            if (States.Main != null) return;
            InitializeComponent();
            Config.ModelPath = "";
            BindingContext = new MainPageViewModel();

            MessagingCenter.Subscribe<MainPageViewModel, ScrollToBottomMessage>(this, "ScrollToBottom", (sender, message) =>
            {
                ScrollToBottom(message.Animate);
            });

            MessagingCenter.Subscribe<MainPageViewModel, RequestLogitModifierEdit>(this, "RequestLogitModifierEdit", (sender, message) =>
            {
                RequestEdit(message.LogitBiasEntry);
            });

            MessagingCenter.Subscribe<MainPageViewModel, DisplayAlertMessage>(this, "DisplayAlert", async (sender, message) =>
            {
                bool answer = await DisplayAlertVM(message.Title, message.Message, message.Button1, message.Button2);

                MessagingCenter.Send(this, "AlertResponse", new AlertResponseMessage { IsConfirmed = answer });

                if (answer)
                {
                    await Task.Delay(500);
                    CTXSlider.Value = Config.ContextSize / 1024;
                    PathEntry.Text = Config.ModelPath;
                    MaxTokensToGenEntry.Text = Config.MaxOutput.ToString();
                    TempEntry.Text = Config.Temperature.ToString(CultureInfo.InvariantCulture); // We want to use dots and not commas when converting floats to strings
                    RepPenEntry.Text = Config.RepPen.ToString(CultureInfo.InvariantCulture);
                    TopPEntry.Text = Config.TopP.ToString(CultureInfo.InvariantCulture);
                    TopKEntry.Text = Config.TopK.ToString(CultureInfo.InvariantCulture);
                    TypicalEntry.Text = Config.Typical.ToString(CultureInfo.InvariantCulture);
                    TFSEntry.Text = Config.Tfs.ToString(CultureInfo.InvariantCulture);
                    MinPEntry.Text = Config.MinP.ToString(CultureInfo.InvariantCulture);
                    PrPenEntry.Text = Config.PresencePenalty.ToString(CultureInfo.InvariantCulture);
                    ETAEntry.Text = Config.MirostatEta.ToString(CultureInfo.InvariantCulture);
                    TAUEntry.Text = Config.MirostatTau.ToString(CultureInfo.InvariantCulture);
                    LayersToOffloadEntry.Text = Config.LayersToOffload.ToString(CultureInfo.InvariantCulture);
                    RopeFreqScaleEntry.Text = Config.RopeFrequencyScale.ToString(CultureInfo.InvariantCulture);
                    UsemmapCheckBox.IsChecked = Config.UseMmap;
                    UsemlockCheckBox.IsChecked = Config.useMlock;
                    ThreadsEntry.Text = Config.ThreadsToUse.ToString();
                    UseFACheckBox.IsChecked = Config.UseFlashAttention;
                    NoKvOffloadCheckBox.IsChecked = Config.NoKVOffload;
                    BatchsizeEntry.Text = Config.BatchSize.ToString(CultureInfo.InvariantCulture);
                    UseMClutchingBox.IsChecked = Config.useMClutching;
                    cuiPortTextbox.Text = Config.cuiPort.ToString();
                    isAdvChanged = false;
                }


            });



        }


        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();

        protected override async void OnAppearing()
        {
            if (States.Main != null) return;

            base.OnAppearing();
            StartGradientRotation();
            StartFadeAnimation();
            PathEntry.Text = Config.ModelPath;
            CTXSlider.Value = 4;
            LogitModifierPanel.OnPromptClosed += LogitModifierPanel_OnPromptClosed;
            (BindingContext as MainPageViewModel)?.OnAppearing();

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ConfigDataService ds = new ConfigDataService("last.json");
            ds.SaveConfig();
        }

        private void StartGradientRotation()
        {
            var animation = new Animation();
            const double fullCircle = 2 * Math.PI;

            // Define the animation
            animation.Add(0, 1, new Animation(v =>
            {
                double angle = v * fullCircle;
                double x = Math.Cos(angle);
                double y = Math.Sin(angle);

                // Update the gradient brush's StartPoint and EndPoint
                GradientBrush.StartPoint = new Microsoft.Maui.Graphics.Point(-x * 0.5 + 0.5, -y * 0.5 + 0.5);
                GradientBrush.EndPoint = new Microsoft.Maui.Graphics.Point(x * 0.5 + 0.5, y * 0.5 + 0.5);
            }));

            // Set the duration and start the animation
            animation.Commit(this, "GradientRotation", length: 10000, repeat: () => true);
        }

        private async void StartFadeAnimation()
        {
            if (LoadingLabel == null) return;

            while (true)
            {
                if (BindingContext is MainPageViewModel viewModel && viewModel.IsLoading)
                {
                    // Fade in
                    await LoadingLabel.FadeTo(1, 500); // 500 ms fade in
                                                       // Pause for a short time
                    await Task.Delay(180); // Pause for 500 ms
                                           // Fade out
                    await LoadingLabel.FadeTo(0, 500); // 500 ms fade out
                                                       // Pause for a short time
                    await Task.Delay(180); // Pause for 500 ms
                }
                else
                {
                    // Ensure label is fully transparent when not loading
                    LoadingLabel.Opacity = 0;
                    break;
                }
            }
        }

        #region Menu bar/Panel Stuff

        bool isMenuMoving = false;
        private async void OnMenuTapped(object sender, EventArgs e)
        {
            if (!SideMenu.IsVisible && !isMenuMoving)
            {
                isMenuMoving = true;

                Overlay.IsVisible = true;
                Overlay.FadeTo(0.8, 200, Easing.CubicInOut);

                SideMenu.IsVisible = true;
                await SideMenu.TranslateTo(0, 0, 200, Easing.CubicInOut);

                isMenuMoving = false;

            }
            else if(!isMenuMoving)
            {
                isMenuMoving = true;

                await SideMenu.TranslateTo(-SideMenu.Width, 0, 200, Easing.CubicInOut);
                SideMenu.IsVisible = false;

                if (!AnyPanelVisible())
                {
                    await Overlay.FadeTo(0, 250, Easing.CubicInOut);
                    Overlay.IsVisible = false;
                }

                isMenuMoving = false;
            }
        }
        private async void TogglePanel(ContentView panel)
        {
            if(panel.IsVisible)
            {
                await panel.FadeTo(0, 250, Easing.CubicInOut);
                panel.IsVisible = false;

                if(!AnyPanelVisible() && !SideMenu.IsVisible)
                {
                    await Overlay.FadeTo(0, 250, Easing.CubicInOut);
                    Overlay.IsVisible = false;

                }
                return;
            }

            if (!Overlay.IsVisible)
            {
                Overlay.IsVisible = true;
                await Overlay.FadeTo(0.8, 250, Easing.CubicInOut);
            }

            // Hide all panels
            NewSessionPanel.IsVisible = false;
            ScenariosPanel.IsVisible = false;
            SaveLoadPanel.IsVisible = false;
            SettingsPanel.IsVisible = false;
            ContextPanel.IsVisible = false;
            AddImgPanel.IsVisible = false;

            // Show the selected panel with a fade-in animation
            panel.Opacity = 0;
            panel.IsVisible = true;
            await Task.Delay(10);
            await panel.FadeTo(1, 250, Easing.CubicInOut);
        }

        private void NewSession_Clicked(object sender, EventArgs e)
        {
            TogglePanel(NewSessionPanel);
        }

        private void Scenarios_Clicked(object sender, EventArgs e)
        {
            TogglePanel(ScenariosPanel);
        }

        private void SaveLoad_Clicked(object sender, EventArgs e)
        {
            TogglePanel(SaveLoadPanel);
        }

        private async void Settings_Clicked(object sender, EventArgs e)
        {
            TogglePanel(SettingsPanel);
        }


        private bool AnyPanelVisible()
        {
            return NewSessionPanel.IsVisible || ScenariosPanel.IsVisible || SaveLoadPanel.IsVisible || SettingsPanel.IsVisible || ContextPanel.IsVisible || AddImgPanel.IsVisible;
        }
        #endregion

        #region NewSessionPanel Stuff

        private async void OkNewSession_Clicked(object sender, EventArgs e)
        {
            OnMenuTapped(sender, e);
            TogglePanel(NewSessionPanel);
        }
        private void CancelNewSession_Clicked(object sender, EventArgs e)
        {
            TogglePanel(NewSessionPanel);
        }
        #endregion

        #region SettingsPanel Stuff
        private async void CTXSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            // Recognize the sender as a Slider object.
            Slider slider = (Slider)sender;

            var sliderCValue = (int)slider.Value * 1024;
            Config.ContextSize = sliderCValue;
            var sliderKValue = (int)sliderCValue / 1000;

            // Update label text (optional)
            CtxSizeEntry.Text = sliderCValue.ToString() + " | " + sliderKValue + "K";
        }
        private void OnXButtonClicked(object sender, EventArgs e) // GLOBALLY, also used by Save/Load Panel
        {
            Button Sender = (Button)sender;
            Element Parent = Sender.Parent;
            Parent = Parent.Parent;
            Parent = Parent.Parent;
            Parent = Parent.Parent;
            TogglePanel((ContentView)Parent);
        }
        private async void selectModelClicked(object sender, EventArgs e)
        {
            var result = await PickAndShowFile();
            if (result != null)
            {
                // Handle the selected file
                // For example, display the file path
                await DisplayAlert("File Selected", result.FullPath, "OK");
                PathEntry.Text = result.FullPath;
                Config.ModelPath = result.FullPath;

            }
        }
        public async Task<FileResult> PickAndShowFile()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a model file",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.gguf", "public.ggml" } },
                        { DevicePlatform.Android, new[] { "application/gguf", "application/ggml" } },
                        { DevicePlatform.WinUI, new[] { ".gguf", ".ggml" } },
                        { DevicePlatform.MacCatalyst, new[] { "public.gguf", "public.ggml" } }
                    })
                });

                if (result != null)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }

            return null;
        }


        private void OnTabButtonClicked(object sender, EventArgs e)
        {
            // Hide all tab contents
            FormatTabContent.IsVisible = false;
            SamplersTabContent.IsVisible = false;
            MediaTabContent.IsVisible = false;
            AdvancedTabContent.IsVisible = false;

            // Show the selected tab content
            if (sender == FormatTabButton)
            {
                FormatTabContent.IsVisible = true;
            }
            else if (sender == SamplersTabButton)
            {
                SamplersTabContent.IsVisible = true;
            }
            else if (sender == MediaTabButton)
            {
                MediaTabContent.IsVisible = true;
            }
            else if (sender == AdvancedTabButton)
            {
                AdvancedTabContent.IsVisible = true;
            }
        }
        #endregion

        private async void OnMessageEntryCompleted(object sender, EventArgs e)
        {
            if (BindingContext is MainPageViewModel viewModel)
            {
                // Trigger the SendMessageCommand
                if (viewModel.SendMessageCommand.CanExecute(null))
                {
                    if(!(BindingContext as MainPageViewModel).IsGenerating)
                        viewModel.SendMessageCommand.Execute(null);
                }
            }
        }

        bool isToolbarMoving = false;
        private async void OnToggleBarClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            if (isBarVisible && !isToolbarMoving)
            {
                isToolbarMoving = true;

                // Slide out the bar
                await AdditionalBar.TranslateTo(0, 70, 250, Easing.CubicInOut);
                AdditionalBar.IsVisible = false;
                button.Text = "˅";
                ScrollToBottom(true);

                isToolbarMoving = false;

            }
            else if (!isToolbarMoving)
            {
                isToolbarMoving = true;

                AdditionalBar.TranslationY = 70;
                AdditionalBar.IsVisible = true;
                // Slide in the bar
                await AdditionalBar.TranslateTo(0, 0, 250, Easing.CubicInOut);
                button.Text = "˄";
                ScrollToBottom(true);

                isToolbarMoving = false;

            }

            isBarVisible = !isBarVisible;
        }




        #region Overlay
        private async void OnOverlayTapped(object sender, EventArgs e)
        {
            // Implement your logic to close all tab windows
            if (isAdvChanged)
            {
                bool answer = await DisplayAlert("Config Manager", "To apply changes, you need to reload the model. Do it now?", "Yes", "No");
                if (answer)
                {
                    MessagingCenter.Send(this, "AlertResponse", new AlertResponseMessage { IsConfirmed = answer });
                }
                isAdvChanged = false;
            }
            CloseAllTabs();
        }

        private async void CloseAllTabs()
        {
            if (NewSessionPanel.IsVisible)
            {
                NewSessionPanel.FadeTo(0, 100, Easing.CubicInOut);

            }
            if (SaveLoadPanel.IsVisible)
            {
                SaveLoadPanel.FadeTo(0, 100, Easing.CubicInOut);

            }
            if (ScenariosPanel.IsVisible)
            {
                ScenariosPanel.FadeTo(0, 100, Easing.CubicInOut);

            }
            if (SettingsPanel.IsVisible)
            {
                SettingsPanel.FadeTo(0, 100, Easing.CubicInOut);
            }
            if (SideMenu.IsVisible)
            {
               SideMenu.TranslateTo(-SideMenu.Width, 0, 200, Easing.CubicInOut);

            }
            if (ContextPanel.IsVisible)
            {
                ContextPanel.FadeTo(0, 100, Easing.CubicInOut);
            }
            if (AdvMenuPanel.IsVisible)
            {
                AdvMenuPanel.FadeTo(0, 100, Easing.CubicInOut);
            }
            if (AddImgPanel.IsVisible)
            {
                AddImgPanel.FadeTo(0, 100, Easing.CubicInOut);
            }

            await Overlay.FadeTo(0, 250, Easing.CubicInOut);


            Overlay.IsVisible = false;
            NewSessionPanel.IsVisible = false;
            SaveLoadPanel.IsVisible = false;
            ScenariosPanel.IsVisible = false;
            SettingsPanel.IsVisible = false;
            SideMenu.IsVisible = false;
            AdvMenuPanel.IsVisible = false;
            ContextPanel.IsVisible = false;
            AddImgPanel.IsVisible = false;

        }
        #endregion



        #region MessageCenter Stuff
        private async void ScrollToBottom(bool animate)
        {
            if (MessagesCollectionView.ItemsSource is IList messages && messages.Count > 0)
            {
               MessagesCollectionView.ScrollTo(messages[messages.Count - 1], position: ScrollToPosition.End, animate: animate);
            }
        }

        private async Task<bool> DisplayAlertVM(string title, string message, string btn1, string btn2)
        {
            if (string.IsNullOrEmpty(btn1))
            {
                await DisplayAlert(title, message, "OK");
                return false;
            }
            else
            {
                bool answer = await DisplayAlert(title, message, btn1, btn2);
                if (answer)
                {
                    ConfigDataService ds = new ConfigDataService("last.json");
                    ds.LoadConfig();

                }
                return answer;
            }
        }
        #endregion

        private void ActivityIndicator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (LoadingLabel == null) return;
            if (!LoadingLabel.IsVisible && Throbber.IsVisible)
            {
                StartFadeAnimation();
            }
        }

        private void SendButton_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Button.Text))
            {
                var button = sender as Button;

                if (button != null)
                {
                    if (button.Text == "Send")
                    {
                        button.BackgroundColor = Color.FromRgb(0, 122, 255);
                    }
                    else
                    {
                        button.BackgroundColor = Colors.Red;
                    }
                }
            }
        }


        #region Context stuff
        private void ContextBtnClicked(object sender, EventArgs e)
        {
            TogglePanel(ContextPanel);
        }
        private void OnContextXButtonClicked(object sender, EventArgs e)
        {
            TogglePanel(ContextPanel);
        }

        private void OnContextTabButtonClicked(object sender, EventArgs e)
        {
            // Hide all tab contents
            MemoryTabContent.IsVisible = false;
            TokensTabContent.IsVisible = false;
            LogitsTabContent.IsVisible = false;


            // Show the selected tab content
            if (sender == MemoryTabButton)
            {
                MemoryTabContent.IsVisible = true;
            }
            else if (sender == TokensTabButton)
            {
                TokensTabContent.IsVisible = true;
            }
            else if(sender == LogitsTabButton)
            {
                LogitsTabContent.IsVisible = true;
            }
        }
        #endregion

        #region Add Image stuff
        private void AddImgBtnClicked(object sender, EventArgs e)
        {
            TogglePanel(AddImgPanel);
        }
        private void OnAddImgXButtonClicked(object sender, EventArgs e)
        {
            TogglePanel(AddImgPanel);
        }
        #endregion

        #region Advanced Settings stuff
        private void OnAdvSettingsClicked(object sender, EventArgs e)
        {
            TogglePanel(AdvMenuPanel);
        }
        private async void OnAdvSettingsXButtonClicked(object sender, EventArgs e)
        {
            if (isAdvChanged)
            {
                bool answer = await DisplayAlert("Config Manager", "To apply changes, you need to reload the model. Do it now?", "Yes", "No");
                if(answer)
                {
                    MessagingCenter.Send(this, "AlertResponse", new AlertResponseMessage { IsConfirmed = answer });
                }
                isAdvChanged = false;
            }
            TogglePanel(AdvMenuPanel);
            TogglePanel(SettingsPanel);

        }
        #endregion

        #region Advanced Panel Settings stuff
        private void FlashAttention_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Config.UseFlashAttention = (sender as CheckBox).IsChecked;
            isAdvChanged = true;
        }

        private void ManualMap_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Config.UseMmap = (sender as CheckBox).IsChecked;
            isAdvChanged = true;
        }

        private void MemoryLock_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Config.useMlock = (sender as CheckBox).IsChecked;
            isAdvChanged = true;
        }

        private void NoKVOffload_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Config.NoKVOffload = (sender as CheckBox).IsChecked;
            isAdvChanged = true;
        }

        private void LayersEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;
                isAdvChanged = true;

                // Check if the new text is a valid number
                if (!string.IsNullOrWhiteSpace(newText) && !int.TryParse(newText, out _))
                {
                    // If not, revert to the old text
                    entry.Text = e.OldTextValue;
                    return;
                }
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.LayersToOffload = int.Parse(entry.Text);
                    return;

                }
                if (string.IsNullOrWhiteSpace(newText))
                {
                    Config.LayersToOffload = 100;

                }
            }
        }

        private void ThreadsEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;
                isAdvChanged = true;

                // Check if the new text is a valid number
                if (!string.IsNullOrWhiteSpace(newText) && !int.TryParse(newText, out _))
                {
                    // If not, revert to the old text
                    entry.Text = e.OldTextValue;
                    return;
                }
                if (newText == "0")
                {
                    Config.ThreadsToUse = null;
                    return;

                }
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.ThreadsToUse = int.Parse(entry.Text);
                    return;

                }
                if (string.IsNullOrWhiteSpace(newText))
                {
                    Config.ThreadsToUse = null;

                }

            }
        }

        private void BatchSizeEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;
                isAdvChanged = true;

                // Check if the new text is a valid number
                if (!string.IsNullOrWhiteSpace(newText) && !int.TryParse(newText, out _))
                {
                    // If not, revert to the old text
                    entry.Text = e.OldTextValue;
                    return;
                }
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.BatchSize = int.Parse(entry.Text);
                    return;

                }
                if (string.IsNullOrWhiteSpace(newText))
                {
                    Config.BatchSize = 512;
                }

            }
        }

        private void RoPEScaleEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;
                isAdvChanged = true;

                if (!string.IsNullOrWhiteSpace(newText) && !float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.RopeFrequencyScale = float.Parse(newText, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    Config.RopeFrequencyScale = 1.0f;
                }
            }
        }
        bool isfirst = true;
        private void PrecisionPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isfirst) { isfirst = false; return; }
            isAdvChanged = true;

        }
        #endregion


        #region Settings>Sampler stuff

        private void MaxOutputEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                // Check if the new text is a valid number
                if (!string.IsNullOrWhiteSpace(newText) && !int.TryParse(newText, out _))
                {
                    // If not, revert to the old text
                    entry.Text = e.OldTextValue;
                    return;
                }
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.MaxOutput = int.Parse(entry.Text);
                    return;

                }
                if (string.IsNullOrWhiteSpace(newText))
                {
                    Config.MaxOutput = 512;
                }

            }
        }

        private void TemperatureEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                if (!string.IsNullOrWhiteSpace(newText) && !float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.Temperature = float.Parse(newText, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    Config.Temperature = 0.7f;
                }
            }
        }

        private void RepPenEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                if (!string.IsNullOrWhiteSpace(newText) && !float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.RepPen = float.Parse(newText, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    Config.RepPen = 1.1f;
                }
            }
        }

        private void TopPEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                if (!string.IsNullOrWhiteSpace(newText) && !float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.TopP = float.Parse(newText, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    Config.TopP = 0.92f;
                }
            }
        }

        private void TopKEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                // Check if the new text is a valid number
                if (!string.IsNullOrWhiteSpace(newText) && !int.TryParse(newText, out _))
                {
                    // If not, revert to the old text
                    entry.Text = e.OldTextValue;
                    return;
                }
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.TopK = int.Parse(entry.Text);
                    return;

                }
                if (string.IsNullOrWhiteSpace(newText))
                {
                    Config.TopK = 100;
                }

            }
        }

        private void EtaEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                if (!string.IsNullOrWhiteSpace(newText) && !float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.MirostatEta = float.Parse(newText, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    Config.MirostatEta = 0.1f;
                }
            }
        }

        private void TauEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                if (!string.IsNullOrWhiteSpace(newText) && !float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.MirostatTau = float.Parse(newText, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    Config.MirostatTau = 1.0f;
                }
            }
        }

        private void TypicalEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                if (!string.IsNullOrWhiteSpace(newText) && !float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.Typical = float.Parse(newText, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    Config.Typical = 1.0f;
                }
            }
        }

        private void TFSEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                if (!string.IsNullOrWhiteSpace(newText) && !float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.Tfs = float.Parse(newText, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    Config.Tfs = 1.0f;
                }
            }
        }

        private void MinPEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                if (!string.IsNullOrWhiteSpace(newText) && !float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.MinP = float.Parse(newText, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    Config.MinP = 0.1f;
                }
            }
        }

        private void PrPenEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                if (!string.IsNullOrWhiteSpace(newText) && !float.TryParse(newText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.PresencePenalty = float.Parse(newText, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    Config.PresencePenalty = 0.0f;
                }
            }
        }

        #endregion

        #region Media Tab
        private async void setupCuiClicked(object sender, EventArgs e)
        {
            States.Main = this;
            Application.Current.MainPage = new CUI_InstallerSetupPage();
        }

        private async void detectCuiClicked(object sender, EventArgs e)
        {
            cuiDetectTextbox.Text = "-";
            cuiDetectTextbox.TextColor = Colors.Yellow;
            comfyUI cui = comfyUI.Instance;
            if (cui.SearchInstance().Result)
            {
                cuiDetectTextbox.Text = "ComfyUI detected";
                cuiDetectTextbox.TextColor = Colors.Lime;
            }
            else
            {
                cuiDetectTextbox.Text = "Failed to detect ComfyUI instance";
                cuiDetectTextbox.TextColor = Colors.OrangeRed;
            }

        }

        private void UseClutching_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Config.useMClutching = (sender as CheckBox).IsChecked;
        }

        private void cuiPortEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                // Check if the new text is a valid number
                if (!string.IsNullOrWhiteSpace(newText) && !int.TryParse(newText, out _))
                {
                    // If not, revert to the old text
                    entry.Text = e.OldTextValue;
                    return;
                }
                if (newText == "0")
                {
                    Config.cuiPort = 5000;
                    return;

                }
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    Config.cuiPort = int.Parse(entry.Text);
                    return;

                }
                if (string.IsNullOrWhiteSpace(newText))
                {
                    Config.cuiPort = 5000;

                }

            }
        }
        #endregion

        private async void AddNewLogitModifierClicked(object sender, EventArgs e)
        {
            LogitModifierPanel.IsVisible = true;
            LogitModifierPanel.ChangeFocusToTokenEntry();
            await LogitModifierPanel.FadeTo(1, 250, Easing.CubicIn);
        }

        private async void LogitModifierPanel_OnPromptClosed(object sender, (string token, float bias, bool isEdit) result)
        {
            var (token, bias, isEdit) = result;

            if (!isEdit)
            {
                // Call the ViewModel method to handle the new logit modifier
                await ((MainPageViewModel)BindingContext).OnAddNewLogitModifier(token, bias);

                
            }
            else
            {
                await ((MainPageViewModel)BindingContext).EditLogitModifier(new LogitBiasEntry(token, bias));

            }
            LogitModifierPanel.IsVisible = false;

        }

        public async void RequestEdit(LogitBiasEntry CurrentLogitModifier)
        {
            LogitModifierPanel.IsVisible = true;
            await LogitModifierPanel.FadeTo(1, 250, Easing.CubicIn);
            LogitModifierPanel.ChangeFocusToTokenEntry();
            LogitModifierPanel.SetEditValues(CurrentLogitModifier);
        }

    }

}
