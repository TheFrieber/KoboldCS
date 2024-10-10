using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Koboldcs.Models
{
    public partial class Message : ObservableObject
    {
        private string text;
        private bool isUser;
        private string profileImage;
        private string title;
        private bool isEditing;
        private List<string> retryLog = new List<string>();
        private bool isLastMessage;
        private List<byte[]> imageSources = new List<byte[]>();
        private string imageInfo;
        private int currentselectedImageIndex;
        private ImageSource currentselectedImage;



        public string Text
        {
            get => text;
            set
            {
                if (SetProperty(ref text, value))
                {

                }
            }
        }

        public List<string> RetryLog
        {
            get => retryLog;
            set
            {
                if (SetProperty(ref retryLog, value))
                {
                    OnPropertyChanged(nameof(CurrentRetryText));
                }
            }
        }

        public bool IsUser
        {
            get => isUser;
            set => SetProperty(ref isUser, value);
        }

        public string ProfileImage
        {
            get => profileImage;
            set => SetProperty(ref profileImage, value);
        }

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public bool IsEditing
        {
            get => isEditing;
            set => SetProperty(ref isEditing, value);
        }

        public string CurrentRetryText
        {
            get
            {
                if (RetryLog != null && RetryLog.Any())
                {
                    // Get the index of the current text in the retry log
                    int index = RetryLog.IndexOf(text);

                    // Return the index as A/B format
                    return $"{index + 1}/{RetryLog.Count}";
                }
                return "1/1"; // Default when there are no retries
            }
        }

        public bool IsLastMessage
        {
            get => isLastMessage;
            set => SetProperty(ref isLastMessage, (value &= !IsUser)); // Only set to true, when the message is also not from user
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        public void UpdateRetryLog()
        {
            // Check if Text exists in RetryLog
            int index = RetryLog.IndexOf(text);

            if (index != -1)
            {
                // Text exists; replace the old entry
                RetryLog[index] = text;
            }
            else
            {
                // Text does not exist; add a new entry
                RetryLog.Add(text);
            }
            OnPropertyChanged(nameof(CurrentRetryText));
        }

        public void UpdateCurrentRetryText()
        {
            OnPropertyChanged(nameof(CurrentRetryText));
        }

        public List<byte[]> Images
        {
            get => imageSources;
            set => SetProperty(ref imageSources, value);
        }

        public string ImageInfo
        {
            get => imageInfo;
            set => SetProperty(ref imageInfo, value);
        }

        public ImageSource CurrentSelectedImage
        {
            get => ImageSource.FromStream(() => new MemoryStream(Images[currentselectedImageIndex]));
            set => SetProperty(ref currentselectedImage, value);
        }

        public int CurrentSelectedImageIndex
        {
            get => currentselectedImageIndex;
            set => SetProperty(ref currentselectedImageIndex, value);
        }
    }



    public class LogitBiasEntry
    {
        public string Token { get; set; }
        public float Bias { get; set; }

        public LogitBiasEntry(string token, float bias)
        {
            Token = token;
            Bias = bias;
        }
    }

}
