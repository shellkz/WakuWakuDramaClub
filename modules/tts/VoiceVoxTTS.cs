using Godot;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using HttpClient = System.Net.Http.HttpClient;

/// <summary>
/// A Godot C# autoload service for interacting with a local VOICEVOX server.
/// This service connects on startup and provides a method to synthesize speech.
/// </summary>
public partial class TTSService : Node
{
    // The base URL for the local VOICEVOX engine API.
    // Make sure the engine is running on this address and port.
    private const string VoicevoxApiUrl = "http://127.0.0.1:50021";

    // The speaker ID for Zunda-mon's "Normal" voice.
    // This value can be found by querying the /speakers endpoint of the VOICEVOX API.
    private const int ZundamonSpeakerId = 3;

    private HttpClient _httpClient = new HttpClient();
    private bool _isConnected = false;

    // A signal to notify other parts of the application about the connection status.
    [Signal]
    public delegate void ConnectionStatusChangedEventHandler(bool isConnected);

    //public override void _Ready()
    //{
    //    ConnectionStatusChanged += OnConnectionStatusChanged;

    //    // It's good practice to initialize the HttpClient once per application lifecycle.
    //    _httpClient = new HttpClient();

    //    // Attempt to connect to the VOICEVOX server on startup.
    //    AttemptConnection();
    //}

    private  void OnConnectionStatusChanged(bool isConnected)
    {
        //if (isConnected)
        //{
        //    AudioStreamWav stream = await TTS("大好きなのだ");
        //}
    }

    /// <summary>
    /// Attempts to connect to the VOICEVOX server by checking its version.
    /// This method can be called on startup or as a retry mechanism.
    /// </summary>
    public async void AttemptConnection()
    {
        GD.Print("TTSService: Attempting to connect to VOICEVOX server...");
        try
        {
            // The /version endpoint is a simple way to check if the server is alive.
            var response = await _httpClient.GetAsync($"{VoicevoxApiUrl}/version");
            response.EnsureSuccessStatusCode();

            var version = await response.Content.ReadAsStringAsync();
            GD.Print($"TTSService: Successfully connected to VOICEVOX Engine v{version}!");

            // Set the connection status and emit the signal.
            _isConnected = true;
            EmitSignal(SignalName.ConnectionStatusChanged, true);
        }
        catch (HttpRequestException e)
        {
            GD.PrintErr($"TTSService: Failed to connect to VOICEVOX server. Please ensure the engine is running. Error: {e.Message}");

            // Set the connection status to false and emit the signal.
            _isConnected = false;
            EmitSignal(SignalName.ConnectionStatusChanged, false);
        }
    }

    /// <summary>
    /// A retry method to attempt connection to the VOICEVOX server again.
    /// </summary>
    public void RetryConnection()
    {
        if (_isConnected)
        {
            GD.Print("TTSService: Already connected. No need to retry.");
            return;
        }
        AttemptConnection();
    }

    /// <summary>
    /// Synthesizes speech from text using the VOICEVOX API with Zunda-mon's voice.
    /// </summary>
    /// <param name="speech">The text to be converted to speech.</param>
    /// <returns>A Godot.WAVAudioStream containing the synthesized audio, or null if an error occurs.</returns>
    public async Task<byte[]> TTS(string speech)
    {
        if (!_isConnected)
        {
            GD.PrintErr("TTSService: Not connected to VOICEVOX server. Cannot synthesize speech.");
            return null;
        }

        try
        {
            // Step 1: Create the audio query
            // This endpoint generates a JSON object describing the audio, including phoneme and accent data.
            string audioQueryUrl = $"{VoicevoxApiUrl}/audio_query?text={Uri.EscapeDataString(speech)}&speaker={ZundamonSpeakerId}";
            var audioQueryResponse = await _httpClient.PostAsync(audioQueryUrl, null);
            audioQueryResponse.EnsureSuccessStatusCode();

            string audioQueryJson = await audioQueryResponse.Content.ReadAsStringAsync();

            // Step 2: Synthesize the audio
            // The /synthesis endpoint takes the audio query JSON and a speaker ID to generate the WAV file.
            string synthesisUrl = $"{VoicevoxApiUrl}/synthesis?speaker={ZundamonSpeakerId}";
            var synthesisContent = new StringContent(audioQueryJson, Encoding.UTF8, "application/json");
            var synthesisResponse = await _httpClient.PostAsync(synthesisUrl, synthesisContent);
            synthesisResponse.EnsureSuccessStatusCode();

            // Step 3: Get the raw WAV bytes
            byte[] wavData = await synthesisResponse.Content.ReadAsByteArrayAsync();

            // Step 4: Convert the byte array to a Godot WAVAudioStream


            return wavData;
        }
        catch (HttpRequestException e)
        {
            GD.PrintErr($"TTSService: Error during TTS synthesis. Error: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            GD.PrintErr($"TTSService: An unexpected error occurred during TTS synthesis. Error: {e.Message}");
            return null;
        }
    }
}