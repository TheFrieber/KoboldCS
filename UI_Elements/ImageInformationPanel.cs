using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koboldcs.Funcs
{
    public class ImageInformationPanel : ContentView
    {

        public ImageInformationPanel()
        {
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.7); // Semi-transparent background
            IsVisible = false; // Initially hidden
            MaximumHeightRequest = 1024;

            // Create the top bar with the title
            var topBar = new Grid
            {
                BackgroundColor = Colors.DarkSlateGray,
                HeightRequest = 50,

                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.FillAndExpand // Stretch fully horizontally
            };

            topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var titleLabel = new Label
            {
                Text = "Image Information",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(titleLabel, 1);

            var closeButton = new Button
            {
                Text = "Close",
                BackgroundColor = Colors.DarkRed,
                TextColor = Colors.OrangeRed,
                BorderColor = Colors.OrangeRed,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0,0,5,0),
                Padding = new Thickness(10, 0)
            };
            closeButton.Clicked += (s, e) => IsVisible = false;
            Grid.SetColumn(closeButton, 2);

            topBar.Children.Add(titleLabel);
            topBar.Children.Add(closeButton);

            // Image and information
            var imageInfoLabel = new Label
            {
                TextColor = Colors.White,
                FontSize = 10,
                HorizontalOptions = LayoutOptions.FillAndExpand, // Stretch fully horizontally
                VerticalOptions = LayoutOptions.Start
            };
            imageInfoLabel.SetBinding(Label.TextProperty, new Binding("ImageInfo"));

            var image = new Image
            {
                MinimumHeightRequest = 258, // Adjust height and width as needed
                MinimumWidthRequest = 258,
                Aspect = Aspect.AspectFit,
            };
            image.SetBinding(Image.SourceProperty, new Binding("CurrentSelectedImage"));

            var imageFrame = new Frame
            {
                Content = image,
                BackgroundColor = Colors.Black,
                BorderColor = Colors.OliveDrab,
                CornerRadius = 10,
                MaximumHeightRequest = 612,
                MaximumWidthRequest = 612,
                Padding = new Thickness(10),
                Margin = new Thickness(0),
                HorizontalOptions = LayoutOptions.CenterAndExpand // Stretch fully horizontally
            };

            // Combine all elements in a stack layout
            var panelContent = new StackLayout
            {
                Children = { topBar, imageFrame, imageInfoLabel },
                VerticalOptions = LayoutOptions.FillAndExpand, // Stretch fully vertically
                HorizontalOptions = LayoutOptions.FillAndExpand, // Stretch fully horizontally
                Spacing = 20,
                Padding = new Thickness(20)
            };

            // Add a border frame around the content
            var borderFrame = new Frame
            {
                Content = panelContent,
                BackgroundColor = Colors.Black,
                CornerRadius = 10,
                Padding = new Thickness(10),
                Margin = new Thickness(20),
                HorizontalOptions = LayoutOptions.FillAndExpand, // Stretch fully horizontally
                VerticalOptions = LayoutOptions.FillAndExpand // Stretch fully vertically
            };

            Content = borderFrame;
        }

        public string ImageInfo
        {
            get => (string)GetValue(ImageInfoProperty);
            set => SetValue(ImageInfoProperty, value);
        }

        public static readonly BindableProperty ImageInfoProperty =
            BindableProperty.Create(nameof(ImageInfo), typeof(string), typeof(ImageInformationPanel), string.Empty);
    }
}
