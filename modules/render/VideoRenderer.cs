using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Pipes;
using FFMpegCore.Enums;
using System.Linq;
using FFMpegCore.Exceptions;
using FFMpegCore.Arguments;
namespace WakuWakuDramaClub.Render;

[GlobalClass]
public partial class VideoRenderer : Node
{
    [Export] public SubViewport SubViewportToRecord;
    [Export] public AnimationPlayer AnimationPlayer;

    [Export] public float TargetFPS = 30.0f;
    [Export] public string OutputFileName = "exported_animation.mp4";

    private int _videoWidth;
    private int _videoHeight;
    private ConcurrentQueue<IVideoFrame> _frameQueue = new ConcurrentQueue<IVideoFrame>();
    private CancellationTokenSource _ffmpegCts;
    private TaskCompletionSource<bool> _ffmpegTaskCompletionSource;

    public override void _Ready()
    {
        if (SubViewportToRecord == null || AnimationPlayer == null)
        {
            GD.PrintErr("SubViewportToRecord or AnimationPlayer not assigned!");
            return;
        }

        _videoWidth = (int)SubViewportToRecord.Size.X;
        _videoHeight = (int)SubViewportToRecord.Size.Y;

        //GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "res://ffmpeg-binaries/", TemporaryFilesFolder = "user://temp" });
    }

    public async Task ExportAnimationToVideo(Animation animation, List<AudioClip> audio)
    {
        _ffmpegCts = new CancellationTokenSource();
        _ffmpegTaskCompletionSource = new TaskCompletionSource<bool>();

        GD.Print($"Starting export of animation to {ProjectSettings.GlobalizePath($"user://{OutputFileName}")}");

        Task ffmpegEncodingTask = Task.Run(() => RecordVideoAsync(ProjectSettings.GlobalizePath($"user://{OutputFileName}"), _ffmpegCts.Token, audio));

        AnimationLibrary animationLibrary = new AnimationLibrary();
        animationLibrary.AddAnimation("timeline", animation);
        AnimationPlayer.AddAnimationLibrary("custom", animationLibrary);
        AnimationPlayer.Play("custom/timeline");
        AnimationPlayer.Pause();

        float animationDuration = animation.Length;
        float frameDuration = 1.0f / TargetFPS;
        int totalFrames = (int)(animationDuration * TargetFPS);

        for (int i = 0; i < totalFrames; i++)
        {
            float currentTime = i * frameDuration;
            if (currentTime > animationDuration)
            {
                currentTime = animationDuration;
            }

            AnimationPlayer.Seek(currentTime, true);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            CaptureFrameFromSubViewport();

            GD.Print($"Captured frame {i + 1}/{totalFrames} at time {currentTime:F2}s");
        }

        _ffmpegCts.Cancel();
        await _ffmpegTaskCompletionSource.Task;

        GD.Print("Animation export complete!");

        _ffmpegCts.Dispose();
        _ffmpegCts = null;
        _ffmpegTaskCompletionSource = null;
    }

    private void CaptureFrameFromSubViewport()
    {
        ViewportTexture viewportTexture = SubViewportToRecord.GetTexture();
        if (viewportTexture == null)
        {
            GD.PrintErr("SubViewportTexture is null during capture!");
            return;
        }

        Godot.Image godotImage = viewportTexture.GetImage();
        if (godotImage == null || godotImage.IsEmpty())
        {
            GD.PrintErr("Failed to get image from ViewportTexture!");
            return;
        }

        try
        {
            godotImage.Convert(Godot.Image.Format.Rgba8);
            godotImage.Decompress();
            _frameQueue.Enqueue(new GodotImageVideoFrame(godotImage));
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error processing frame for FFMpegCore: {ex.Message}");
        }
    }

    private async Task RecordVideoAsync(string outputPath, CancellationToken cancellationToken, List<AudioClip> audioClips)
    {
        if (_videoWidth == 0 || _videoHeight == 0)
        {
            GD.PrintErr("Video width or height not set for RawVideoPipeSource!");
            _ffmpegTaskCompletionSource.SetResult(false);
            return;
        }

        IEnumerable<IVideoFrame> GetVideoFrames()
        {
            while (!cancellationToken.IsCancellationRequested || _frameQueue.Count > 0)
            {
                if (_frameQueue.TryDequeue(out IVideoFrame frame))
                {
                    yield return frame;
                }
                else
                {
                    SpinWait.SpinUntil(() => _frameQueue.Count > 0 || cancellationToken.IsCancellationRequested, 50);
                }
            }
        }

        var videoPipe = new RawVideoPipeSource(GetVideoFrames())
        {
            FrameRate = TargetFPS,
        };

        try
        {
            if (audioClips == null || audioClips.Count == 0)
            {
                await FFMpegArguments
                    .FromPipeInput(videoPipe, options => options
                        .WithFramerate((int)TargetFPS)
                    )
                    .OutputToFile(outputPath, true, options => options
                        .WithVideoCodec("libx264")
                        .WithFramerate((int)TargetFPS)
                        .WithConstantRateFactor(23)
                        .WithFastStart())
                    .ProcessAsynchronously();
            }
            else
            {
                // Create a list of all pipe sources (video + all audio clips)
                var audioPipes = new List<IPipeSource>();


                foreach (var audio in audioClips)
                {
                    (uint sampleRate, uint channels) = GetWavMetadata(audio.Data);
                    RawAudioPipeSource pipe = new RawAudioPipeSource(GetAudioSamples(audio));
                    pipe.SampleRate = sampleRate;
                    pipe.Channels = channels;
                    //GD.Print($"Sample rate{sampleRate}");
                    audioPipes.Add(pipe);
                }

                // Building the FFmpeg command

                var ffmpegArguments = FFMpegArguments.FromPipeInput(videoPipe);

                foreach (var pipe in audioPipes)
                {
                    ffmpegArguments.AddPipeInput(pipe);
                }



                // Create a complex filter for audio mixing with starting times
                // We'll need a way to pad the audio streams. The `adelay` filter is perfect for this.
                // It delays the start of a stream, and FFMpeg will pad the beginning with silence.
                // For this to work, we need to apply `adelay` to each audio input stream before mixing.
                var audioFilterComplex = new List<string>();
                var audioMaps = new List<string>();
                int audioIndex = 1; // Start at 1 because video is at index 0

                foreach (var audio in audioClips)
                {
                    // Delay the audio stream by its starting time in milliseconds
                    long delayInMs = (long)(audio.StartingTime * 1000);
                    // Use a unique label for each audio stream
                    string streamLabel = $"a{audioIndex}";
                    // Build the filter string: `[stream_index:audio]adelay=delay_in_ms:all=1[label]`
                    audioFilterComplex.Add($"[{audioIndex}:a]adelay={delayInMs}|{delayInMs}[{streamLabel}]");
                    audioMaps.Add($"[{streamLabel}]");
                    audioIndex++;
                }

                // Mix all delayed audio streams
                string amixFilter = string.Join("", audioMaps) + $"amix=inputs={audioClips.Count}:duration=longest";
                audioFilterComplex.Add(amixFilter);

                // Combine the complex filters into a single string
                string filterComplexString = string.Join(";", audioFilterComplex);

                await ffmpegArguments.OutputToFile(outputPath, true, options => options
                    .WithVideoCodec("libx264")
                    .WithFramerate((int)TargetFPS)
                    .WithConstantRateFactor(23)
                    .WithFastStart()
                    .WithArgument(new InputComplexFilterArgument(filterComplexString))).ProcessAsynchronously();
            
                
            }

            GD.Print($"FFMpeg process finished for {outputPath}");
        }
        catch (OperationCanceledException)
        {
            GD.Print("FFmpeg process was cancelled (normal exit after all frames processed).");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"FFMpegCore Error during recording: {ex.Message}");
            if (ex is FFMpegException ffmpegEx)
            {
                GD.PrintErr($"FFmpeg Error Output: {ffmpegEx.Message}");
            }
        }
        finally
        {
            _ffmpegTaskCompletionSource.SetResult(true);
        }
    }

    // Assumes the provided byte array is a valid WAV file
    private (uint sampleRate, uint channels) GetWavMetadata(byte[] wavData)
    {
        // A simple WAV header parser
        // Bytes 22-23: Number of Channels
        // Bytes 24-27: Sample Rate
        if (wavData.Length < 44) // Standard WAV header size is 44 bytes
        {
            GD.PrintErr("Invalid WAV file data: Header is too short.");
            return (0, 0);
        }

        uint channels = BitConverter.ToUInt16(wavData, 22);
        uint sampleRate = BitConverter.ToUInt32(wavData, 24);

        return (sampleRate, channels);
    }

    private IEnumerator<IAudioSample> GetAudioSamples(AudioClip audioClip)
    {
        // WAV header is 44 bytes. We only want the raw PCM data.
        const int wavHeaderSize = 44;
        var audioData = new byte[audioClip.Data.Length - wavHeaderSize];
        Array.Copy(audioClip.Data, wavHeaderSize, audioData, 0, audioData.Length);

        using (var stream = new MemoryStream(audioData))
        {
            var buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                yield return new ByteAudioSample(buffer.Take(bytesRead).ToArray());
            }
        }
    }

    public sealed class InputComplexFilterArgument : IArgument
    {
        public string Text { get; }
        public InputComplexFilterArgument(string value) => Text = $"-filter_complex \"{value}\"";
    }
    public class GodotImageVideoFrame : IVideoFrame
    {
        private Godot.Image _godotImage;
        private bool _isDisposed = false;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public string Format { get; private set; }

        public GodotImageVideoFrame(Godot.Image godotImage)
        {
            _godotImage = godotImage ?? throw new ArgumentNullException(nameof(godotImage));
            godotImage.Convert(Godot.Image.Format.Rgba8);
            godotImage.Decompress();

            Width = godotImage.GetWidth();
            Height = godotImage.GetHeight();
            Format = "rgba";
        }

        public void Serialize(Stream pipe)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(GodotImageVideoFrame));
            }

            byte[] pixelData = _godotImage.GetData().ToArray();
            pipe.Write(pixelData, 0, pixelData.Length);
            Dispose();
        }

        public async Task SerializeAsync(Stream pipe, CancellationToken token)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(GodotImageVideoFrame));
            }

            token.ThrowIfCancellationRequested();

            byte[] pixelData = _godotImage.GetData().ToArray();
            await pipe.WriteAsync(pixelData, 0, pixelData.Length, token);
            Dispose();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _godotImage.Dispose();
                _godotImage = null;
                _isDisposed = true;
            }
        }
    }

    public class AudioClip
    {
        public string Format;
        public byte[] Data;
        public double StartingTime;
    }

    public class ByteAudioSample : IAudioSample
    {
        private readonly byte[] _data;

        public ByteAudioSample(byte[] data)
        {
            _data = data;
        }

        public void Serialize(Stream stream)
        {
            stream.Write(_data, 0, _data.Length);
        }

        public async Task SerializeAsync(Stream stream, CancellationToken token)
        {
            await stream.WriteAsync(_data, 0, _data.Length, token);
        }
    }
}