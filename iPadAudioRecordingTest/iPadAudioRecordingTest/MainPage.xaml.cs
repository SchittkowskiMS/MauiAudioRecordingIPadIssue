using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using Plugin.Maui.Audio;
using System.IO;
using System.Threading;

namespace iPadAudioRecordingTest
{
    public partial class MainPage : ContentPage
    {
        private IAudioRecorder? audioRecorder;
        private FileAudioSource? recordingResult;

        private IAudioPlayer? audioPlayer;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await MainThread.InvokeOnMainThreadAsync(async () => await Permissions.RequestAsync<Permissions.StorageWrite>().ConfigureAwait(false));
        }

        private async void btRecordingStart_Clicked(object sender, EventArgs e)
        {
            if(audioRecorder == null)
            {
                audioRecorder = AudioManager.Current.CreateRecorder();
            }
            if(!audioRecorder.CanRecordAudio)
            {
                await Alert("Can't record audio");
                return;
            }
            await audioRecorder.StartAsync();
        }

        private async void btRecordingStop_Clicked(object sender, EventArgs e)
        {
            if(audioRecorder == null)
            {
                await Alert("Recorder == null");
                return;
            }
            if(!audioRecorder.IsRecording) await Alert("Recorder is not recording");

            IAudioSource audioSource = await audioRecorder.StopAsync();
            if(audioSource == null)
            {
                await Alert("audioSource == null");
                return;
            }
            recordingResult = audioSource as FileAudioSource;
            if (recordingResult == null) await Alert("recordingResult == null");

        }
        private async void btReplayStart_Clicked(object sender, EventArgs e)
        {
            if(recordingResult == null)
            {
                await Alert("recordingResult == null, cant start replay");
                return;
            }
            if(audioPlayer != null)
            {
                audioPlayer.Dispose();
                audioPlayer = null;
            }
            audioPlayer = AudioManager.Current.CreatePlayer(recordingResult.GetFilePath());
            audioPlayer.PlaybackEnded += async (s, a) =>
            {
                await Alert("Playback ended");
            };
            audioPlayer.Play();
        }

        private async void btReplayStop_Clicked(object sender, EventArgs e)
        {
            if (audioPlayer == null)
            {
                await Alert("audioPlayer == null");
                return;
            }
            if(!audioPlayer.IsPlaying) await Alert("audioPlayer is not playing");
            audioPlayer.Stop();
            await Alert("audioPlayer stopped");
        }

        private async Task Alert(string message)
        {
            await DisplayAlert("Alert", message, "OK");
        }

        private async void btRecordingSave_Clicked(object sender, EventArgs e)
        {
            if(recordingResult == null)
            {
                await Alert("recordingResult == null");
                return;
            }
            var fileNameFinal = string.Format("audiofromtestpage_{0}.mp3", DateTime.UtcNow.ToString("yyMMddHHmmss"));
            var fileSaverResult = await FileSaver.Default.SaveAsync(fileNameFinal, recordingResult.GetAudioStream(), CancellationToken.None);
            if (fileSaverResult.IsSuccessful)
            {
                await Alert($"The file was saved successfully to location: {fileSaverResult.FilePath}");
            }
            else
            {
                await Alert($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}");
            }
        }
    }
}
