using Koboldcs.Configuration;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Storage;

namespace Koboldcs
{
    public partial class CUI_InstallerSetupPage : ContentPage
    {
        public static bool isExtracting = false;
        private string RequestedDirectory = string.Empty;
        private const string LatestReleaseApiUrl = "https://api.github.com/repos/comfyanonymous/ComfyUI/releases/latest";
        private string downloadPath => Path.Combine(RequestedDirectory, "ComfyUI.zip");
        private string downloadDirectory => RequestedDirectory;
        private string sevenZipExecutable => Path.Combine(downloadDirectory, "7zr.exe");
        private const string SevenZipDownloadUrl = "https://www.7-zip.org/a/7zr.exe";

        // Download Controls
        private CancellationTokenSource cancellationTokenSource;
        private bool isPaused = false;


        // Shared download state for all parts
        private long totalBytesDownloaded = 0;
        private long totalFileSize = 0;
        private readonly object progressLock = new object();
        private Stopwatch downloadStopwatch = new Stopwatch();


        private async Task<bool> CheckForExistingArchive()
        {
            if (File.Exists(downloadPath))
            {
                await DisplayAlert("Setup", "ComfyUI is already installed, unpacking it...", "OK");
                DownloadState.Text = $"Extracting archive...";
                await Extract7zFile(downloadPath);
                File.Delete(downloadPath);
                File.Delete(sevenZipExecutable);
                DownloadState.Text = $"-";
                return true;
            }
            return false;
        }

        private async Task SetDownloadDirectory(bool isAlternative = false)
        {
            if(isAlternative)
                await DisplayAlert("Setup", "Please select the root directory of the ComfyUI folder (ComfyUI_windows_portable).", "OK");
            else
                await DisplayAlert("Setup", "Please select a download directory. Recommended minimum free space on disk is 20GB", "OK");

            try
            {
                // Cross-platform folder picker using CommunityToolkit.Maui
                var folderPicker = FolderPicker.Default;
                var result = await folderPicker.PickAsync(default);

                if (result.Folder != null)
                {
                    RequestedDirectory = result.Folder.Path;
                }
                else
                {
                    // User canceled the operation
                    await DisplayAlert("Cancelled", "No folder selected.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        public CUI_InstallerSetupPage()
        {
            InitializeComponent();
        }

        public async Task MiddleMan_DownloadLatestRelease(string downloadUrl, string downloadPath)
        {
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await Task.Run(async () => await DownloadLatestRelease(downloadUrl, downloadPath, cancellationToken: cancellationTokenSource.Token));
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
            totalBytesDownloaded = 0;
            downloadStopwatch.Reset();
            totalFileSize = 0;
        }

        public async Task MiddleMan_DownloadExtension()
        {
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await Task.Run(async () => await DownloadExtension());
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
            totalBytesDownloaded = 0;
            downloadStopwatch.Reset();
            totalFileSize = 0;
        }

        private async void OnInstallComfyUIWithExtensionClicked(object sender, EventArgs e)
        {
            DwControlBar.IsVisible = false;
            DwInfoBar.IsVisible = true;
            await SetDownloadDirectory();

            DisplayAlert("Setup", "Installing ComfyUI with ExtendedAPIHook extension...", "OK");

            try
            {
                // Step 1: Fetch the latest release info from GitHub
                var downloadUrl = await GetLatestReleaseDownloadUrl();

                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    // Step 2: Download the release asset with progress tracking

                    if (await CheckForExistingArchive())
                    {
                        downloadStopwatch.Start();
                        DownloadState.Text = $"Installing Extension...";
                        Task.Run(async () => DownloadExtension());
                        return;
                    }
                    downloadStopwatch.Start();
                    await Task.Run(async () => await MiddleMan_DownloadLatestRelease(downloadUrl, downloadPath));
                    DownloadState.Text = $"Installing Extension...";
                    await Task.Run(async () => await MiddleMan_DownloadExtension());
                }
                else
                {
                    await DisplayAlert("Error", "Could not fetch the download URL.", "OK");
                    DownloadState.Text = $"-";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                DownloadState.Text = $"-";
            }
            DwControlBar.IsVisible = true;
            DwInfoBar.IsVisible = false;
        }

        private async void OnInstallComfyUIOnlyClicked(object sender, EventArgs e)
        {
            DwControlBar.IsVisible = false;
            DwInfoBar.IsVisible = true;
            await SetDownloadDirectory();


            if (await CheckForExistingArchive())
            {
                return;
            }

            DisplayAlert("Setup", "Installing ComfyUI only...", "OK");

            try
            {
                // Step 1: Fetch the latest release info from GitHub
                var downloadUrl = await GetLatestReleaseDownloadUrl();

                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    // Step 2: Download the release asset with progress tracking
                    downloadStopwatch.Start();
                    await Task.Run(async () => await MiddleMan_DownloadLatestRelease(downloadUrl, downloadPath));
                    DownloadState.Text = $"-";
                }
                else
                {
                    await DisplayAlert("Error", "Could not fetch the download URL.", "OK");
                    DownloadState.Text = $"-";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                DownloadState.Text = $"-";
            }
            DwControlBar.IsVisible = true;
            DwInfoBar.IsVisible = false;
        }

        private async void OnInstallExtensionOnlyClicked(object sender, EventArgs e)
        {
            DwControlBar.IsVisible = false;
            DwInfoBar.IsVisible = true;
            await SetDownloadDirectory(true);
            RequestedDirectory = Path.GetDirectoryName(RequestedDirectory);

            DisplayAlert("Setup", "Installing ExtendedAPIHook to an existing ComfyUI instance...", "OK");
            try
            {
                // Step 1: Fetch the latest release info from GitHub
                var downloadUrl = await GetLatestReleaseDownloadUrl();

                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    // Step 2: Download the release asset with progress tracking

                    DownloadState.Text = $"Installing Extension...";
                    await Task.Run(async () => await MiddleMan_DownloadExtension());

                }
                else
                {
                    await DisplayAlert("Error", "Could not fetch the download URL.", "OK");
                    DownloadState.Text = $"-";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                DownloadState.Text = $"-";
            }
            DwControlBar.IsVisible = true;
            DwInfoBar.IsVisible = false;
        }

        private void OnCancelDownloadClicked(object sender, EventArgs e)
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel(); // Cancel the download
                DownloadState.Text = "Download Canceled";
            }
        }

        private void OnPauseDownloadClicked(object sender, EventArgs e)
        {
            isPaused = !isPaused; // Toggle pause state

            if (isPaused)
            {
                DownloadState.Text = "Download Paused";
                ((Button)sender).Text = "Resume Download";
            }
            else
            {
                DownloadState.Text = "Downloading...";
                ((Button)sender).Text = "Pause Download";
            }
        }

        private async void OnCancelSetupClicked(object sender, EventArgs e)
        {
            DisplayAlert("Setup", "Setup cancelled.", "OK");
            Application.Current.MainPage = States.Main; // Replace this with navigation to your main page if needed
        }


        private async void StartThrobberAnimation()
        {
            while (isExtracting)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DownloadProgressBar.ProgressTo(1.0, 2500, Easing.BounceIn);
                    await DownloadProgressBar.ProgressTo(0.0, 2500, Easing.BounceOut);
                });
            }
        }

        #region Installer Logic
        private async Task<string> GetLatestReleaseDownloadUrl()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; GrandUI)");

                var response = await client.GetStringAsync(LatestReleaseApiUrl);
                var jsonDoc = JsonDocument.Parse(response);

                // Extract the first "browser_download_url" from the release JSON
                var downloadUrl = jsonDoc
                    .RootElement
                    .GetProperty("assets")
                    .EnumerateArray()
                    .First()
                    .GetProperty("browser_download_url")
                    .GetString();

                return downloadUrl;
            }
        }

        private async Task DownloadLatestRelease(string url, string filePath, CancellationToken cancellationToken, int numberOfParts = 12) // DownloadFileInParallel
        {
            try
            {
                numberOfParts = Environment.ProcessorCount;
                using (var httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                    response.EnsureSuccessStatusCode();

                    totalFileSize = response.Content.Headers.ContentLength ?? -1L;
                    if (totalFileSize <= 0)
                        throw new Exception("Unable to determine file size.");

                    double downloadSizeGB = totalFileSize / (1024.0 * 1024 * 1024);
                    MainThread.BeginInvokeOnMainThread(() => DownloadSize.Text = $"{downloadSizeGB:F2}");
                    MainThread.BeginInvokeOnMainThread(() => DownloadState.Text = $"Downloading parts...");

                    long partSize = totalFileSize / numberOfParts;
                    var tasks = new List<Task>();

                    for (int i = 0; i < numberOfParts; i++)
                    {
                        long start = i * partSize;
                        long end = (i == numberOfParts - 1) ? totalFileSize - 1 : (start + partSize - 1);
                        tasks.Add(DownloadFilePart(url, filePath, start, end, i, cancellationToken));
                    }
                    Timer timer = new Timer(UpdateUI, null, 0, 500);

                    await Task.WhenAll(tasks);

                    // Optionally combine the parts into a single file if needed.
                    MainThread.BeginInvokeOnMainThread(() => DownloadState.Text = $"Combining parts...");
                    await CombineParts(filePath, numberOfParts);
                    // Extract and then delete the zip file
                    MainThread.BeginInvokeOnMainThread(() => 
                    {
                        DownloadState.Text = $"Extracting archive...";
                    });
                    await Extract7zFile(downloadPath);
                    File.Delete(downloadPath);
                    File.Delete(sevenZipExecutable);
                    downloadStopwatch.Stop();
                    MainThread.BeginInvokeOnMainThread(() => DownloadState.Text = $"-");
                    MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        DisplayAlert("Success", "ComfyUI Download completed.", "OK");

                    });
                }
            }
            catch (OperationCanceledException)
            {

                MainThread.BeginInvokeOnMainThread(() => DisplayAlert("Canceled", "The download was canceled.", "OK"));

            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                });
            }

        }

        private async Task DownloadExtension()
        {
            try
            {
                isExtracting = true;
                Task.Run(StartThrobberAnimation);

                using (var httpClient = new HttpClient())
                {

                    // Install Python dependencies
                    MainThread.BeginInvokeOnMainThread(() => DownloadState.Text = $"Installing Python dependencies...");
                    await InstallPythonDependencies();

                    // Download and place the ExtendedAPIHook folder
                    MainThread.BeginInvokeOnMainThread(() => DownloadState.Text = $"Downloading ExtendedAPIHook...");
                    await DownloadAndPlaceExtendedAPIHook();

                    MainThread.BeginInvokeOnMainThread(() => DownloadState.Text = $"-");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Success", "Setup completed successfully.", "OK");
                    });
                }
                isExtracting = false;
            }
            catch (Exception ex)
            {
                isExtracting = false;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                });

            }
        }

        private async Task InstallPythonDependencies()
        {
            string pythonEmbeddedPath = Path.Combine(downloadDirectory, "ComfyUI_windows_portable", "python_embeded");
            string pythonExecutable = Path.Combine(pythonEmbeddedPath, "python.exe");

            if (!File.Exists(pythonExecutable))
            {
                throw new Exception("Python executable not found in the embedded Python folder.");
            }

            var dependencies = new List<string> { "selenium", "webdriver_manager", "flask", "flasgger" };
            foreach (var dependency in dependencies)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonExecutable,
                    Arguments = $"-m pip install {dependency}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string errorOutput = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Failed to install {dependency}: {errorOutput}");
                    }
                }
            }
        }

        private async Task DownloadAndPlaceExtendedAPIHook()
        {
            string targetDirectory = Path.Combine(downloadDirectory, "ComfyUI_windows_portable", "ComfyUI", "custom_nodes");
            Directory.CreateDirectory(targetDirectory);

            string gitUrl = "https://github.com/TheFrieber/ExtendedAPIHook/archive/refs/heads/main.zip";
            string tempZipPath = Path.Combine(downloadDirectory, "ExtendedAPIHook.zip");

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(gitUrl);
                response.EnsureSuccessStatusCode();

                using (var fileStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }

            // Extract the downloaded zip into the custom_nodes directory
            string customNodesPath = Path.Combine(downloadDirectory, "ComfyUI_windows_portable", "ComfyUI", "custom_nodes");
            await ExtractZipFile(tempZipPath, customNodesPath);

            // Move the extracted folder to the custom_nodes directory
            string extractedFolderPath = Path.Combine(customNodesPath, "ExtendedAPIHook-main", "ExtendedAPIHook");
            string destinationPath = Path.Combine(targetDirectory, "ExtendedAPIHook");
            string tempFolderPath = Path.Combine(customNodesPath, "ExtendedAPIHook-main");


            if (Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            Directory.Move(extractedFolderPath, destinationPath);
            File.Delete(tempZipPath);
            Directory.Delete(tempFolderPath, true);
        }

        private async Task ExtractZipFile(string zipFilePath, string extractPath)
        {
            try
            {
                // Ensure the extraction directory exists
                Directory.CreateDirectory(extractPath);

                // Use ZipArchive to extract the files
                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Open))
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(extractPath, overwriteFiles: true);
                }

                await Task.CompletedTask; // Ensure this remains asynchronous if needed
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during extraction
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Error", $"An error occurred while extracting the archive: {ex.Message}", "OK");
                });
            }
        }

        double progress;
        double elapsedSeconds;
        double downloadSpeedMBps;
        double downloadedSizeGB;

        private async Task DownloadFilePart(string url, string filePath, long start, long end, int partNumber, CancellationToken cancellationToken)
        {
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Range = new RangeHeaderValue(start, end);

                using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var partFilePath = $"{filePath}.part{partNumber}";

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(partFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        bool wasPaused = false;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            while (isPaused)
                            {
                                wasPaused = true;
                                MainThread.BeginInvokeOnMainThread(() => DownloadState.Text = $"Paused download.");
                                await Task.Delay(1500); // Wait while paused
                            }
                            if (wasPaused)
                            {
                                MainThread.BeginInvokeOnMainThread(() => DownloadState.Text = $"Downloading parts...");
                                wasPaused = false;
                            }

                            await fileStream.WriteAsync(buffer, 0, bytesRead);

                            // Update the shared totalBytesDownloaded and UI safely
                            lock (progressLock)
                            {
                                totalBytesDownloaded += bytesRead;
                                progress = (double)totalBytesDownloaded / totalFileSize;

                                elapsedSeconds = downloadStopwatch.Elapsed.TotalSeconds;
                                downloadSpeedMBps = (totalBytesDownloaded / (1024.0 * 1024)) / elapsedSeconds;
                                downloadedSizeGB = totalBytesDownloaded / (1024.0 * 1024 * 1024);
                            }

                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                }
            }
        }

        private async Task Extract7zFile(string archiveFilePath)
        {
            try
            {
                isExtracting = true;
                Task.Run(StartThrobberAnimation);

                await Ensure7zrExistsAsync();

                if (!File.Exists(sevenZipExecutable))
                {
                    MainThread.BeginInvokeOnMainThread(() => DisplayAlert("Error", "7zr.exe not found in the download directory.", "OK"));
                    isExtracting = false;
                    return;
                }

                // Prepare the extraction command: 7zr x archiveFilePath -oextractPath
                string command = $"{sevenZipExecutable} x {archiveFilePath} -o{Path.GetDirectoryName(archiveFilePath)}";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    UseShellExecute = true,  // Allows launching the Command Prompt with output visible
                    CreateNoWindow = false,   // Ensures the Command Prompt window is visible
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {

                    }
                    else
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () => await DisplayAlert("Error", $"Extraction failed. Exit Code: {process.ExitCode}", "OK"));
                    }
                }
                isExtracting = false;

            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () => await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK"));
                isExtracting = false;

            }
            finally
            {
                isExtracting = false;
            }
        }


        private async Task CombineParts(string filePath, int numberOfParts)
        {
            using (var outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                for (int i = 0; i < numberOfParts; i++)
                {
                    var partFilePath = $"{filePath}.part{i}";
                    using (var inputStream = new FileStream(partFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 8192, true))
                    {
                        await inputStream.CopyToAsync(outputStream);
                    }
                    File.Delete(partFilePath); // Delete the part file after merging
                }
            }
        }

        private void UpdateUI(object state)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DownloadedSize.Text = $"{downloadedSizeGB:F2}";
                DownloadSpeed.Text = $"{downloadSpeedMBps:F2}";
                DownloadProgressBar.Progress = progress;
                ProgressLabel.Text = $"{(int)(progress * 100)}%";
            });
        }

        private async Task Ensure7zrExistsAsync()
        {
            if (!File.Exists(sevenZipExecutable))
            {
                await Download7zrExecutable();
            }
        }

        private async Task Download7zrExecutable()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(SevenZipDownloadUrl);
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(sevenZipExecutable, FileMode.Create, FileAccess.Write))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() => DisplayAlert("Error", $"Failed to download 7zr.exe: {ex.Message}", "OK"));
            }
        }
        #endregion
    }
}
