using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using Koboldcs.Configuration;
using System.Diagnostics;
using Koboldcs.Models;
using Microsoft.Maui.Controls.Shapes;

namespace Koboldcs.Funcs
{
    public class FormattedTextBlock : StackLayout
    {
        private ImageInformationPanel _imageInformationPanel;

        public static readonly BindableProperty TextProperty =
                BindableProperty.Create(nameof(Text), typeof(string), typeof(FormattedTextBlock), propertyChanged: OnTextChanged);

        public static readonly BindableProperty ImageByteArraysProperty =
            BindableProperty.Create(
                nameof(ImageByteArrays),
                typeof(List<byte[]>),
                typeof(FormattedTextBlock),
                propertyChanged: OnImageByteArraysChanged);

        public List<byte[]> ImageByteArrays
        {
            get => (List<byte[]>)GetValue(ImageByteArraysProperty);
            set => SetValue(ImageByteArraysProperty, value);
        }

        private static void OnImageByteArraysChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (FormattedTextBlock)bindable;
            control.UpdateContent(); // Handle updating the images when the byte array changes
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }


        private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (FormattedTextBlock)bindable;
            control.UpdateContent();
        }

        private void UpdateContent()
        {
            Children.Clear();

            UpdateText(Text);



            if (ImageByteArrays != null && ImageByteArrays.Count > 0)
            {

                var horizontalStack = new HorizontalStackLayout
                {
                    Spacing = 10, // Adjust spacing between images if needed
                    Padding = 5,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                };

                foreach (var byteArray in ImageByteArrays)
                {
                    var imageSource = ImageSource.FromStream(() => new MemoryStream(byteArray));

                    var image = new Image
                    {
                        Source = imageSource,
                        HeightRequest = 100, // Adjust height and width as needed
                        WidthRequest = 100,
                        Aspect = Aspect.AspectFill
                    };

                    var outerFrame = new Border
                    {
                        Content = image,
                        Stroke = new SolidColorBrush(Colors.PaleVioletRed),
                        BackgroundColor = Color.FromRgb(65,65,65),
                        StrokeThickness = 1,
                        Padding = 5
                    };

                    image.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() => ShowImageInfo(byteArray))
                    });

                    horizontalStack.Children.Add(outerFrame);
                }

                var scrollView = new ScrollView
                {
                    Orientation = ScrollOrientation.Horizontal,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
                    BackgroundColor = Colors.DimGray,
                    Content = horizontalStack
                };

                var frame = new Frame
                {
                    Content = scrollView,
                    CornerRadius = 5, // Slight corner radius for the frame
                    HasShadow = false, // No shadow for a flat look
                    BackgroundColor = Colors.Orchid,
                    Padding = new Thickness(0) // No padding inside the frame
                };

                Children.Add(frame);


            }
        }

        private void UpdateImageInformationPanel()
        {
            //This doesn't work because setting a new one and then checking if it equals always results a null.
            //But may keep it if someone wants to open multiple windows, this may be a feature? I'll let it for now.
            _imageInformationPanel = new ImageInformationPanel();
            if (Children.FirstOrDefault(m => m.Equals(_imageInformationPanel)) == null)
            {
                Children.Add(_imageInformationPanel);

            }
            else
            {
                Children.Remove(Children.First(m => m.Equals(_imageInformationPanel)));
                Children.Add(_imageInformationPanel);
            }
        } 

        private void ShowImageInfo(byte[] imageBytes)
        {
            // Assuming the BindingContext is set to a Message instance
            if (BindingContext is Message message)
            {
                // Set ImageInfo in the Message model
                message.ImageInfo = $"Image Size: {imageBytes.Length / 1024} KB";

                // Update the ImageInformationPanel to show the updated info
                message.CurrentSelectedImageIndex = message.Images.FindIndex(m => m.Equals(imageBytes));
                UpdateImageInformationPanel();
                _imageInformationPanel.ImageInfo = message.ImageInfo;


                // Show the panel
                _imageInformationPanel.IsVisible = true;
            }
        }

        private void UpdateText(string newText)
        {
            if (string.IsNullOrWhiteSpace(newText))
            {
                Children.Clear();
                return;
            }

            // Clear the children initially
            Children.Clear();

            var parts = Regex.Split(newText, @"(\*[^*]+\*|\""[^\""]+\""|```[^`]+```)", RegexOptions.Singleline);

            // To accumulate normal text and preserve spaces
            FormattedString currentFormattedString = new FormattedString();
            bool previousWasCodeBlock = false;

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                if (part.StartsWith("*") && part.EndsWith("*") && part.Length > 2)
                {
                    // Italicized text
                    AddTextInSpans(currentFormattedString, part.Substring(1, part.Length - 2), FontAttributes.Italic, Colors.OrangeRed);
                }
                else if (part.StartsWith("\"") && part.EndsWith("\"") && part.Length > 2)
                {
                    // Quoted text
                    AddTextInSpans(currentFormattedString, part.Substring(1, part.Length - 2), FontAttributes.None, Colors.Cyan);
                }
                else if (part.StartsWith("```") && part.EndsWith("```"))
                {
                    // If we have accumulated text, create a label for it
                    if (currentFormattedString.Spans.Any())
                    {
                        var label = CreateFormattedLabel(currentFormattedString);
                        Children.Add(label);
                        currentFormattedString = new FormattedString(); // Reset for next segment
                    }

                    // Code block
                    if (part.Length >= 6) // Ensure the part is long enough
                    {
                        var codeText = part.Substring(3, part.Length - 6);
                        var frame = CreateCodeBlockFrame(codeText);
                        Children.Add(frame);
                        previousWasCodeBlock = true;
                    }
                }
                else
                {
                    // Normal text, handle whitespace carefully
                    if (previousWasCodeBlock)
                    {
                        // Add a space before normal text if it follows a code block
                        AddTextInSpans(currentFormattedString, " ", FontAttributes.None, Colors.Black);
                        previousWasCodeBlock = false;
                    }
                    AddTextInSpans(currentFormattedString, part, FontAttributes.None, Colors.Black);
                }
            }

            // Add remaining text in a new label
            if (currentFormattedString.Spans.Any())
            {
                var label = CreateFormattedLabel(currentFormattedString);
                Children.Add(label);
            }
        }

        // CreateFormattedLabel method to create labels with FormattedString
        private Label CreateFormattedLabel(FormattedString formattedString)
        {
            return new Label
            {
                FormattedText = formattedString,
                BackgroundColor = Colors.Transparent,
                Margin = new Thickness(0) // Ensure there's no margin adding unwanted space
            };
        }

        private Frame CreateCodeBlockFrame(string codeText)
        {
            // Extract the first word after ``` as the language
            var firstNewlineIndex = codeText.IndexOf('\n');
            var language = firstNewlineIndex > -1 ? codeText.Substring(0, firstNewlineIndex).Trim() : string.Empty;
            var codeContent = firstNewlineIndex > -1 ? codeText.Substring(firstNewlineIndex + 1) : codeText;

            // Create a label for the language name (top bar)
            var languageLabel = new Label
            {
                Text = language,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Colors.Gray,
                TextColor = Colors.Cyan,
                Padding = new Thickness(5, 2),
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Center
            };

            // Create the code block editor
            var codeEditor = new Editor
            {
                Text = codeContent,
                FontFamily = "Courier New", // Monospaced font
                TextColor = Colors.Red,
                BackgroundColor = Color.FromRgb(45, 45, 45),
                IsReadOnly = true,
                IsTextPredictionEnabled = false,
                IsSpellCheckEnabled = false,
                Margin = new Thickness(0) // No margin, handled by Frame padding
            };

            // Create a grid layout for the top bar and editor
            var grid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto }, // For the top bar
                    new RowDefinition { Height = GridLength.Star }  // For the editor
                },
                Padding = new Thickness(0,5)
            };

            // Add the label and editor to the grid
            grid.Add(languageLabel, 0, 0);
            grid.Add(codeEditor, 0, 1);

            // Return the frame containing the grid layout
            return new Frame
            {
                Content = grid,
                BorderColor = Colors.Gray, // Optional: color for the frame border
                CornerRadius = 5, // Slight corner radius for the frame
                HasShadow = false, // No shadow for a flat look
                BackgroundColor = Color.FromRgb(45, 45, 45), // Background color of the frame
                Padding = new Thickness(0) // No padding inside the frame
            };
        }


        // Method to add text in spans
        private void AddTextInSpans(FormattedString formattedString, string text, FontAttributes attributes, Color color)
        {
            const int maxSpanLength = 1000; // Define a max length for each span
            int length = text.Length;

            for (int i = 0; i < length; i += maxSpanLength)
            {
                int remainingLength = length - i;
                int spanLength = Math.Min(maxSpanLength, remainingLength);

                if (spanLength <= 0) break;

                string spanText = text.Substring(i, spanLength);

                formattedString.Spans.Add(new Span
                {
                    Text = spanText,
                    FontAttributes = attributes,
                    TextColor = color
                });
            }
        }

    }
}
