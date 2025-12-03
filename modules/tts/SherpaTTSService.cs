using Godot;
using SherpaOnnx;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

public partial class SherpaTTSService : Node
{
    OfflineTts tts;

    public override async void _Ready()
    {





        GD.Print("Initializing TTS Service...");
        await InitializeAsync();
        GD.Print("Initialize TTS Service successfully.");

        //string text = "株式會社萬代南宮宣布，預定2025年秋季在日本推出的新作智慧型手機遊戲現已開放事前預約。";
        //float speed = 1.1f;
        //int sid = 3;
        //await TTSAsync(text, speed, sid);


    }   



    public Task InitializeAsync()
    {
        return Task.Run(() => Initialize());
 
    }
    public void Initialize()
    {
        //string modelPathString = ProjectSettings.GlobalizePath($"user://tts/models/kokoro-multi-lang-v1_1");
        //var config = new OfflineTtsConfig();
        //config.Model.Kokoro.Model = Path.Combine(modelPathString, "model.onnx");
        //config.Model.Kokoro.Voices = Path.Combine(modelPathString, "voices.bin");
        //config.Model.Kokoro.Tokens = Path.Combine(modelPathString, "tokens.txt");
        //config.Model.Kokoro.DataDir = Path.Combine(modelPathString, "espeak-ng-data");
        //config.Model.Kokoro.DictDir = Path.Combine(modelPathString, "dict");
        //config.Model.Kokoro.Lexicon = Path.Combine(modelPathString, "lexicon-us-en.txt") + "," + Path.Combine(modelPathString, "lexicon-gb-en.txt") + "," + Path.Combine(modelPathString, "lexicon-zh.txt");


        string modelPathString = ProjectSettings.GlobalizePath($"user://tts/models/vits-melo-tts-zh_en");
        var config = new OfflineTtsConfig();
        config.Model.Vits.Model = Path.Combine(modelPathString, "model.onnx");
        //config.Model.Vits.Voices = Path.Combine(modelPathString, "voices.bin");
        config.Model.Vits.Tokens = Path.Combine(modelPathString, "tokens.txt");
        //config.Model.Vits.DataDir = Path.Combine(modelPathString, "espeak-ng-data");
        config.Model.Vits.DictDir = Path.Combine(modelPathString, "dict");
        config.Model.Vits.Lexicon = Path.Combine(modelPathString, "lexicon.txt");


        //string modelPathString = ProjectSettings.GlobalizePath($"user://tts/models/Breeze2-VITS-onnx");
        //var config = new OfflineTtsConfig();
        //config.Model.Vits.Model = Path.Combine(modelPathString, "breeze2-vits.onnx");
        ////config.Model.Vits.Voices = Path.Combine(modelPathString, "voices.bin");
        //config.Model.Vits.Tokens = Path.Combine(modelPathString, "tokens.txt");
        ////config.Model.Vits.DataDir = Path.Combine(modelPathString, "espeak-ng-data");
        //config.Model.Vits.DictDir = Path.Combine(modelPathString, "dict");
        //config.Model.Vits.Lexicon = Path.Combine(modelPathString, "lexicon.txt");


        config.Model.NumThreads = 8;
        config.Model.Debug = 1;
        config.Model.Provider = "cpu";

        tts = new OfflineTts(config);
    }

    public Task<(byte[], TimeSpan)> TTSAsync(string text, float speed, int sid)
    {
         return Task.Run(() => TTS(text, speed, sid));
    }

    public (byte[], TimeSpan) TTS(string text, float speed, int sid)
    {
   

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var audio = tts.Generate(text, speed, sid);
        stopwatch.Stop();
        GD.Print($"Time elapsed for TTS: {stopwatch.Elapsed}");



        stopwatch.Restart();
        string path = ProjectSettings.GlobalizePath($"user://temp.wav");
        audio.SaveToWaveFile(path);
        

        IncreaseVolume(path, ProjectSettings.GlobalizePath($"user://temp_o.wav"), 6.0f);


        TimeSpan duration;
        int sampleRate;
        int channels;
        int bitsPerSample;
        int averageBytesPerSecond;
        long byteLength;
        using (var reader = new WaveFileReader(ProjectSettings.GlobalizePath($"user://temp_o.wav")))
        {

            duration = reader.TotalTime;
            sampleRate = reader.WaveFormat.SampleRate;
            channels = reader.WaveFormat.Channels;
            bitsPerSample = reader.WaveFormat.BitsPerSample;
            averageBytesPerSecond = reader.WaveFormat.AverageBytesPerSecond;
            byteLength = reader.Length;
            ////Console.WriteLine($"File Path: {filePath}");
            //Console.WriteLine($"-----------------------------------");
            //Console.WriteLine($"Total Time: {reader.TotalTime}"); // Duration of the audio
            //Console.WriteLine($"Sample Rate: {reader.WaveFormat.SampleRate} Hz");
            //Console.WriteLine($"Channels: {reader.WaveFormat.Channels}");
            //Console.WriteLine($"Bits Per Sample: {reader.WaveFormat.BitsPerSample} bits");
            //Console.WriteLine($"Average Bytes Per Second: {reader.WaveFormat.AverageBytesPerSecond}");
            //Console.WriteLine($"File Length: {reader.Length} bytes");

        }

        byte[] result = File.ReadAllBytes(ProjectSettings.GlobalizePath($"user://temp_o.wav"));
        stopwatch.Stop();

        GD.Print($"Time elapsed for convert wave: {stopwatch.Elapsed}");
        return (result, duration);

    }

    public void IncreaseVolume(string inputFile, string outputFile, float volumeFactor)
    {
        using (var reader = new WaveFileReader(inputFile))
        {
            var sampleProvider = reader.ToSampleProvider();
            var volumeProvider = new VolumeSampleProvider(sampleProvider) { Volume = volumeFactor };
            var wave16 = new SampleToWaveProvider16(volumeProvider);
            WaveFileWriter.CreateWaveFile(outputFile, wave16);

        }

    }
    //public byte[] ManipulateWave(byte[] inputWaveBytes, float volume, int newSampleRate)
    //{
    //    // A MemoryStream to hold the final output data
    //    using (var outputStream = new MemoryStream())
    //    {
    //        // Use a MemoryStream to read the input byte array
    //        using (var inputStream = new MemoryStream(inputWaveBytes))
    //        {
    //            // Use WaveFileReader to read the WAV data from the stream
    //            using (var reader = new WaveFileReader(inputStream))
    //            {
    //                // Pitch manipulation: Resample the audio to a new rate
    //                var newWaveFormat = new WaveFormat(newSampleRate, reader.WaveFormat.BitsPerSample, reader.WaveFormat.Channels);
    //                using (var resampler = new WaveFormatConversionStream(newWaveFormat, reader))
    //                {
    //                    // Volume manipulation: Wrap the resampler in a WaveChannel32
    //                    using (var volumeStream = new WaveChannel32(resampler))
    //                    {
    //                        volumeStream.Volume = volume;

    //                        // Write the final stream to the output MemoryStream
    //                        using (var writer = new WaveFileWriter(outputStream, volumeStream.WaveFormat))
    //                        {
    //                            // Create a buffer to read and write chunks of audio data
    //                            byte[] buffer = new byte[volumeStream.WaveFormat.AverageBytesPerSecond];
    //                            int bytesRead;

    //                            // Read from the volumeStream and write to the writer
    //                            while ((bytesRead = volumeStream.Read(buffer, 0, buffer.Length)) > 0)
    //                            {
    //                                writer.Write(buffer, 0, bytesRead);
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        // Return the manipulated audio data as a new byte[]
    //        return outputStream.ToArray();
    //    }
    //}


    //// Super slow 
    //public byte[] ConvertAudioToWavBytes(OfflineTtsGeneratedAudio audio)
    //{
    //    // Find the peak amplitude for normalization
    //    float maxAbsValue = audio.Samples.Max(s => Math.Abs(s));
    //    float normalizationFactor = maxAbsValue > 0 ? 1.0f / maxAbsValue : 1.0f;

    //    // Convert float samples to 16-bit PCM shorts with normalization
    //    short[] pcmSamples = new short[audio.Samples.Length];
    //    for (int i = 0; i < audio.Samples.Length; i++)
    //    {
    //        float normalizedSample = audio.Samples[i] * normalizationFactor;
    //        pcmSamples[i] = (short)(Math.Max(-1.0f, Math.Min(1.0f, normalizedSample)) * short.MaxValue);
    //    }

    //    // Use a MemoryStream to hold the WAV file in memory
    //    using (var memoryStream = new MemoryStream())
    //    {
    //        // Define the audio format (16-bit PCM, mono)
    //        var waveFormat = new WaveFormat((int)audio.SampleRate, 16, 1);

    //        // Create a WaveFileWriter that writes to the MemoryStream
    //        using (var waveWriter = new WaveFileWriter(memoryStream, waveFormat))
    //        {
    //            // Write all the PCM samples to the file
    //            waveWriter.WriteSamples(pcmSamples, 0, pcmSamples.Length);
    //            waveWriter.Flush();
    //        }


    //        // Return the byte array of the complete WAV file
    //        return memoryStream.ToArray();
    //    }
    //}
    //public byte[] ConvertAudioToWavBytes(OfflineTtsGeneratedAudio audio)
    //{
    //    //  float[] Sample   -1 ~ 1
    //    //  but sample value may not make full use of sample range
    //    //  for example, max sample value is 0.6
    //    //  we can multiple every sample by 1.0 / 0.6 to expand sample to -1 ~ 1 which increase volume without distort waveshape
    //    //  then finally we trun float[] into short[](16 bits integar) which match wav file format 



    //    // The audio samples are 32-bit floats from -1.0 to 1.0.
    //    // However, the actual peak might be lower than 1.0.
    //    // To increase volume without clipping, we normalize the samples
    //    // so the peak is exactly 1.0.

    //    // 1. Find the peak amplitude (maximum absolute value).
    //    float maxAbsValue = 0.0f;
    //    foreach (var sample in audio.Samples)
    //    {
    //        float absSample = Math.Abs(sample);
    //        if (absSample > maxAbsValue)
    //        {
    //            maxAbsValue = absSample;
    //        }
    //    }

    //    // 2. Calculate the normalization factor.
    //    // If maxAbsValue is 0, the audio is silent, so we don't scale.
    //    float normalizationFactor = maxAbsValue > 0 ? 1.0f / maxAbsValue : 1.0f;


    //    // The audio samples are 32-bit floats. We need to convert them to 16-bit integers
    //    // (short) which is a common format for WAV files.
    //    short[] pcmSamples = new short[audio.Samples.Length];
    //    for (int i = 0; i < audio.Samples.Length; i++)
    //    {
    //        float normalizedSample = audio.Samples[i] * normalizationFactor;

    //        // Clamp the float value to the range [-1.0, 1.0] and scale it to the short range
    //        pcmSamples[i] = (short)(Math.Max(-1.0f, Math.Min(1.0f, audio.Samples[i])) * short.MaxValue);
    //    }

    //    // Use a MemoryStream to build the WAV file byte-by-byte in memory.
    //    using (MemoryStream memoryStream = new MemoryStream())
    //    using (BinaryWriter writer = new BinaryWriter(memoryStream, Encoding.UTF8))
    //    {
    //        // --- WAV Header Construction ---

    //        // The WAV format consists of chunks. The first is the "RIFF" chunk.
    //        writer.Write(Encoding.UTF8.GetBytes("RIFF"));

    //        // This is a placeholder for the file size, which will be updated later.
    //        writer.Write(0);

    //        writer.Write(Encoding.UTF8.GetBytes("WAVE"));

    //        // The "fmt " sub-chunk describes the format of the audio data.
    //        writer.Write(Encoding.UTF8.GetBytes("fmt "));
    //        writer.Write(16); // Sub-chunk size (16 for PCM).
    //        writer.Write((short)1); // Audio format (1 = PCM).
    //        writer.Write((short)1); // Number of channels (1 = mono).
    //        writer.Write((int)audio.SampleRate); // Sample rate.
    //        writer.Write((int)audio.SampleRate * 2); // Byte rate (sample rate * channels * bits per sample / 8).
    //        writer.Write((short)2); // Block alignment (channels * bits per sample / 8).
    //        writer.Write((short)16); // Bits per sample.

    //        // The "data" sub-chunk contains the actual audio samples.
    //        writer.Write(Encoding.UTF8.GetBytes("data"));

    //        // This is a placeholder for the audio data size, to be updated later.
    //        writer.Write(0);

    //        // Write the 16-bit PCM samples to the stream.
    //        for (int i = 0; i < pcmSamples.Length; i++)
    //        {
    //            writer.Write(pcmSamples[i]);
    //        }

    //        // --- Update Placeholder Values ---

    //        // Go back to the beginning of the stream to update the file size.
    //        writer.Seek(4, SeekOrigin.Begin);
    //        writer.Write((int)(memoryStream.Length - 8)); // File size minus the 'RIFF' and size fields.

    //        // Go back to the data chunk size placeholder and update it.
    //        writer.Seek(40, SeekOrigin.Begin);
    //        writer.Write((int)(memoryStream.Length - 44)); // Data size.

    //        return memoryStream.ToArray();
    //    }
    //}
}
