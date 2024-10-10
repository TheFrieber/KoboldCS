
using Koboldcs.Configuration;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Koboldcs.Logger.SLogger;

namespace Koboldcs.ComfyUI_Integration
{

    public class comfyUI
    {
        private static comfyUI _INST;
        public static comfyUI Instance => _INST ??= new comfyUI();



        private const string API_URL = "http://127.0.0.1:";
        private string api_URL => API_URL + Config.cuiPort;

        private bool _isGenerating = false;
        private bool _isActive = false;
        private Image _image;


        public async Task<bool> SearchInstance()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(api_URL + "/launch-browser").ConfigureAwait(false);


                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    return false;


                }
            }
            catch (Exception ex)
            {
                Log(LogType.Error, "Occoured when trying to search for a ComfyUI instance: " + ex);
                return false;
            }
        }

        public async Task<byte[]> RequestGeneration(string prompt = "")
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(180);
                    string url = "";

                    if(string.IsNullOrEmpty(prompt))
                        url = $"{api_URL}/run-script";
                    else
                        url = $"{api_URL}/run-script?input_string={Uri.EscapeDataString(prompt)}";

                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);


                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // Read the image data from the response as a byte array
                        byte[] imageData = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                        return imageData;
                    }

                    return null;


                }
            }
            catch (Exception ex)
            {
                Log(LogType.Error, "Occoured when trying to generate image with a ComfyUI instance: " + ex.GetBaseException());
                return null;
            }
        }
    }
}
