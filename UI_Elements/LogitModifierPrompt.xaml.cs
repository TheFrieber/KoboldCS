using Koboldcs.MessageCenter;
using Koboldcs.Models;

namespace Koboldcs.UI_Elements;

public partial class LogitModifierPrompt : ContentView
{
    public event EventHandler<(string token, float bias, bool isEdit)> OnPromptClosed;


    private bool isEditRequest = false;


    public string Token { get; set; }
    public float Bias { get; set; }


    public LogitModifierPrompt()
    {
        InitializeComponent();
    }

    public void ChangeFocusToTokenEntry()
    {
        TokenEntry.Focus();
    }

    public void SetEditValues(LogitBiasEntry CurrentLogitModifier)
    {
        Token = CurrentLogitModifier.Token;
        Bias = CurrentLogitModifier.Bias * 100f;
        TokenEntry.Text = Token;
        BiasSlider.Value = Bias;
        isEditRequest = true;
    }

    private void OnBiasSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        // Update the BiasPercentageLabel when the Slider value changes
        BiasPercentageLabel.Text = $"{e.NewValue:F0}%";
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        if (!isEditRequest)
        {
            Token = TokenEntry.Text;

            // Use the Slider value for Bias (convert from 0-100% to 0-1 range if needed)
            Bias = (float)BiasSlider.Value / 100.0f; // Or just use BiasSlider.Value for 0-100%

            var existingEntry = ((MainPageViewModel)BindingContext).LogitBiasCollection.FirstOrDefault(n => n.Token == Token);

            if (existingEntry != null) // Added another check here to prevent closing and erasing Input when true
            {
                await Application.Current.MainPage.DisplayAlert("Logit Manager", "That Logit Bias already exists.", "OK");
                return;
            }

            // Close the prompt and return the values
            OnPromptClosed?.Invoke(this, (Token, Bias, isEditRequest));
            this.IsVisible = false; // Hide the ContentView
            TokenEntry.Text = "";
            BiasSlider.Value = 50f;
        }
        else
        {

            // Use the Slider value for Bias (convert from 0-100% to 0-1 range if needed)
            Bias = (float)BiasSlider.Value / 100.0f; // Or just use BiasSlider.Value for 0-100%

            var existingEntry = ((MainPageViewModel)BindingContext).LogitBiasCollection.FirstOrDefault(n => n.Token == TokenEntry.Text);

            if (existingEntry != null && existingEntry.Token != Token) // Added another check here to prevent closing and erasing Input when true
            {
                await Application.Current.MainPage.DisplayAlert("Logit Manager", "That Logit Bias already exists.", "OK");
                return;
            }

            Token = TokenEntry.Text;

            // Close the prompt and return the values
            OnPromptClosed?.Invoke(this, (Token, Bias, isEditRequest));
            isEditRequest = false;
            this.IsVisible = false; // Hide the ContentView
            TokenEntry.Text = "";
            BiasSlider.Value = 50f;

        }

    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await this.FadeTo(0, 250, Easing.CubicOut);
        // Close the prompt without returning any values
        OnPromptClosed?.Invoke(this, (null, 0, false));
        if(isEditRequest)
            isEditRequest = false;
        this.IsVisible = false;
        TokenEntry.Text = "";
        BiasSlider.Value = 50f;
    }
}