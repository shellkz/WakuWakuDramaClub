using Godot;
using System;
using System.IO;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using NAudio.Wave;
using System.Runtime.Versioning;

public partial class SystemSpeechTTS : Node
{
	// 	Microsoft Hanhan Desktop
	//  Microsoft Hanhan
	//  Microsoft Yating
	//  Microsoft Zhiwei
		// const string ssmlContent = @"<speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis"" xml:lang=""zh-TW"">
		// 	<prosody volume=""loud"" pitch=""low"" rate=""120%"">
		// 		你好 尤里西斯正在坐船回暗黑大陸
		// 	</prosody>
		// 	<prosody volume=""loud"" pitch=""x-high"" rate=""120%"">
		// 		你好 尤里西斯正在坐船回暗黑大陸
		// 	</prosody>
		// </speak>";
	public void ListVoices()
	{
		if (!OperatingSystem.IsWindows())
			throw new PlatformNotSupportedException("SystemSpeechTTS is only supported on Windows.");

		ListVoicesWindows();
	}

	[SupportedOSPlatform("windows")]
	private void ListVoicesWindows()
	{

        SpeechSynthesizer synth = null;
        
        try
        {
            // 1. Initialize the Synthesizer
            synth = new SpeechSynthesizer();

            // 2. Select a Voice (Crucial for correct language)
            // You need a voice installed on your system that supports Traditional Chinese (zh-TW or zh-HK).
            // A common fallback is a generic Chinese/Mandarin voice (zh-CN).
            // This loop attempts to find the best available Chinese voice.
            GD.Print("Searching for a suitable Chinese voice...");
            
  
            foreach (var voice in synth.GetInstalledVoices())
            {
                var info = voice.VoiceInfo;
                // Look for Chinese/Mandarin locales (zh-TW is Traditional Chinese, zh-CN is Simplified)
                if (info.Culture.Name.StartsWith("zh"))
                {
                    synth.SelectVoice(info.Name);
                    GD.Print($" Voice: {info.Name} ({info.Culture.Name})");

                }
            }

          
        }
        catch (PlatformNotSupportedException)
        {
            GD.Print("Error: System.Speech is typically only supported on Windows.");
        }
        catch (Exception ex)
        {
            GD.Print($"An error occurred: {ex.Message}");
        }
        finally
        {
            synth?.Dispose();
        }
	}
	public void TTS(string textToSpeak, string voice,string filename)
	{
		if (!OperatingSystem.IsWindows())
			throw new PlatformNotSupportedException("SystemSpeechTTS is only supported on Windows.");

		TTSWindows(textToSpeak, voice, filename);
	}

	[SupportedOSPlatform("windows")]
	private void TTSWindows(string textToSpeak, string voice,string filename)
	{
		// The file name for the output WAV file.
		string userDataBaseDir = OS.GetUserDataDir();
        string outputFileName = Path.Combine(userDataBaseDir, filename);

	
        SpeechSynthesizer synth = null;
        
        try
        {
            // 1. Initialize the Synthesizer
            synth = new SpeechSynthesizer();

           
			synth.SelectVoice(voice);
	
            // 3. Set the Output
            // Ensure the file is not already open or read-only.
            synth.SetOutputToWaveFile(outputFileName);
			
            // 4. Speak the Text and Generate the WAV File
            GD.Print($"Generating WAV file: \"{outputFileName}\" saying \"{textToSpeak}\"...");

			synth.SpeakSsml(textToSpeak);
            // 5. Clean up output (Close the file handle)
            synth.SetOutputToNull();

            // 6. Confirmation
            string fullPath = Path.GetFullPath(outputFileName);
            GD.Print("---");
            GD.Print("WAV file generation complete.");
            GD.Print($"File location: {fullPath}");
            GD.Print($"File size: {new FileInfo(outputFileName).Length} bytes");
        }
        catch (FileNotFoundException)
        {
            GD.Print($"Error: Could not find file {outputFileName}.");
        }
        catch (PlatformNotSupportedException)
        {
            GD.Print("Error: System.Speech is typically only supported on Windows.");
        }
        catch (Exception ex)
        {
            GD.Print($"An error occurred: {ex.Message}");
        }
        finally
        {
            synth?.Dispose();
        }
	}

	public (byte[], TimeSpan) SynthesizeSpeechToBytes(string textToSpeak, string voiceName)
	{
		if (!OperatingSystem.IsWindows())
			throw new PlatformNotSupportedException("SystemSpeechTTS is only supported on Windows.");

		return SynthesizeSpeechToBytesWindows(textToSpeak, voiceName);
	}

	[SupportedOSPlatform("windows")]
	private (byte[], TimeSpan) SynthesizeSpeechToBytesWindows(string textToSpeak, string voiceName)
	{
		// 1. Create a MemoryStream to hold the audio data in memory.
		using (MemoryStream audioStream = new MemoryStream())
		{
			// 2. Initialize the SpeechSynthesizer.
			using (SpeechSynthesizer synth = new SpeechSynthesizer())
			{
				// Optional: Select a specific voice.
				try
				{
					synth.SelectVoice(voiceName);
				}
				catch
				{
					// Fall back to default if the specified voice isn't available.
					Console.WriteLine($"Voice '{voiceName}' not found. Using default.");
				}

				// 3. Set the output destination to the MemoryStream.
				// The audio data will be written to this stream as a standard WAV file format.
				synth.SetOutputToWaveStream(audioStream);

				// 4. Synthesize the text.
				synth.Speak(textToSpeak);
				
				
				audioStream.Seek(0, SeekOrigin.Begin);

				TimeSpan duration;
				
				// 2. Use WaveFileReader to read the stream content (which is a WAV file)
				using (var waveReader = new WaveFileReader(audioStream))
				{
					// 3. Get the duration using the TotalTime property
					duration = waveReader.TotalTime;
				}
					// 5. Convert the MemoryStream content to a byte array.
				// The ToArray() method creates a copy of the stream's contents, 
				// which includes the full WAV header and audio data.
				return (audioStream.ToArray(), duration);
			}
		}
		
	}

}
