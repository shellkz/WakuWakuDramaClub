using Godot;
using System;
using FFMpegCore; // Ensure this namespace is imported
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace WakuWakuDramaClub.Render;

public partial class FFmpegService : Node // You might make this an Autoload Singleton
{
    [Signal]
    public delegate void CompletedEventHandler(string result);
  
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
       
        // This will ensure the binaries are extracted when the game starts
        EnsureFFmpegBinariesExist();

    }

    // Main function to ensure FFmpeg and FFprobe binaries are extracted and available.
    // This function checks the current operating system to determine the correct source
    // path for the binaries and extracts them to 'user://binaries/ffmpeg/' if they
    // do not already exist.

    public void EnsureFFmpegBinariesExist()
    {
        string osName = OS.GetName();
        string osTypeDir = ""; // This will hold "windows", "linux", "osx", etc.

        // Determine the OS-specific directory name for the binaries
        switch (osName)
        {
            case "Windows":
                osTypeDir = "windows";
                break;
            case "X11": // Common name for Linux in Godot
                osTypeDir = "linux";
                break;
            case "OSX":
                osTypeDir = "osx";
                break;
            // Add other OS types if you have binaries for them (e.g., "Android", "iOS")
            default:
                GD.PrintErr($"Unsupported OS ({osName}) for FFmpeg extraction. Binaries might not be available.");
                return;
        }

        // Define the base source directory within the PCK file
        string sourceBaseDir = $"res://binaries/ffmpeg/{osTypeDir}/";
        // Define the target destination directory in the user:// directory
        string destinationBaseDir = "user://binaries/ffmpeg/";

        // Ensure the destination directory exists recursively
        using var dirAccess = DirAccess.Open("user://"); // Open the user:// directory
        if (dirAccess == null)
        {
            GD.PrintErr("Failed to open user:// directory to create subdirectories.");
            return;
        }

        // Check if the destination directory already exists
        if (!dirAccess.DirExists(destinationBaseDir))
        {
            // If it doesn't exist, create it recursively
            Error error = dirAccess.MakeDirRecursive(destinationBaseDir);
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to create directory {destinationBaseDir}. Error: {error}");
                dirAccess.Dispose(); // Dispose DirAccess on error
                return;
            }
            else
            {
                GD.Print($"Created directory: {destinationBaseDir}");
            }
        }
        dirAccess.Dispose(); // Dispose DirAccess after use

        // Construct full source and destination paths for ffmpeg.exe
        string ffmpegSourcePath = sourceBaseDir + "ffmpeg.exe";
        string ffmpegDestPath = destinationBaseDir + "ffmpeg.exe";

        // Construct full source and destination paths for ffprobe.exe
        string ffprobeSourcePath = sourceBaseDir + "ffprobe.exe";
        string ffprobeDestPath = destinationBaseDir + "ffprobe.exe";

        // Attempt to copy ffmpeg.exe if it doesn't exist
        bool ffmpegExtracted = CopyFileIfNotExist(ffmpegSourcePath, ffmpegDestPath);
        // Attempt to copy ffprobe.exe if it doesn't exist
        bool ffprobeExtracted = CopyFileIfNotExist(ffprobeSourcePath, ffprobeDestPath);

        if (ffmpegExtracted && ffprobeExtracted)
        {
            GD.Print("FFmpeg and FFprobe extraction process completed successfully.");
            // Provide the globalized paths which can be used with OS.Execute()
            string globalizedFFmpegPath = ProjectSettings.GlobalizePath(ffmpegDestPath);
            string globalizedFFprobePath = ProjectSettings.GlobalizePath(ffprobeDestPath);

            GD.Print($"FFmpeg will be accessible at: {globalizedFFmpegPath}");
            GD.Print($"FFprobe will be accessible at: {globalizedFFprobePath}");



            FFMpegCore.GlobalFFOptions.Configure(new FFMpegCore.FFOptions
            {
                BinaryFolder = globalizedFFmpegPath.GetBaseDir() // The directory where ffmpeg.exe and ffprobe.exe are located
            });
            // Then you can use FFmpegCore operations.
            // For example:
            // var mediaInfo = await FFmpeg.Get=FFmpeg.GetMediaInfoAsync("path/to/your/video.mp4");
            // GD.Print($"Video duration: {mediaInfo.Duration}");
        }
        else
        {
            GD.PrintErr("One or more FFmpeg/FFprobe files failed to extract or were not found.");
        }
    }

    // Helper function to copy a file from a source path to a specific destination path.
    // This function will only copy the file if it does not already exist at the destination.
    //
    // @param sourcePath string: The full path to the source file (e.g., "res://path/to/file.exe")
    // @param destinationPath string: The full path where the file should be copied (e.g., "user://path/to/target.exe")
    // @return bool: True if the file exists at the destination (either pre-existing or successfully copied), false otherwise.
    private bool CopyFileIfNotExist(string sourcePath, string destinationPath)
    {
        // Check if the file already exists at the destination
        if (Godot.FileAccess.FileExists(destinationPath))
        {
            GD.Print($"File already exists at {destinationPath}. Skipping copy.");
            return true; // Consider it successful if it already exists
        }

        // Open the source file for reading
        using var file = Godot.FileAccess.Open(sourcePath, Godot.FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"Failed to open source file: {sourcePath}. Error: {Godot.FileAccess.GetOpenError()}");
            return false;
        }

        // Read the entire content of the source file into a byte array
        byte[] data = file.GetBuffer((long)file.GetLength());
        file.Close(); // Close the source file immediately after reading

        // Open (or create) the destination file for writing
        using var newFile = Godot.FileAccess.Open(destinationPath, Godot.FileAccess.ModeFlags.Write);
        if (newFile == null)
        {
            GD.PrintErr($"Failed to create destination file: {destinationPath}. Error: {Godot.FileAccess.GetOpenError()}");
            return false;
        }

        // Write the byte array data to the new file
        newFile.StoreBuffer(data);
        newFile.Close(); // Close the new file immediately after writing

        // Verify if the file was successfully created at the destination
        if (Godot.FileAccess.FileExists(destinationPath))
        {
            GD.Print($"Successfully copied {sourcePath} to {destinationPath}");
            return true;
        }
        else
        {
            GD.PrintErr($"Failed to verify copy of {sourcePath} to {destinationPath}");
            return false;
        }
    }
   
    public void GetVideoDuration(string videoPath, Callable onCompleted)
    {
        GetVideoDurationAsync(videoPath, onCompleted);
    }
    //  GetVideoDuration test code
    //    string exampleVideoPath = "C:/Users/zechi/Videos/hie.mkv"; // Example Windows path

    //    // It's good practice to wait a moment or use a signal if EnsureFFmpegBinariesExist
    //    // were asynchronous and its completion was critical before calling FFmpegCore.
    //    // For simplicity here, we assume it completes quickly enough in _Ready.
    //    // If you were to call this immediately after EnsureFFmpegBinariesExist()
    //    // and that function was async, you'd await it first.

    //    // You might want to delay this call or make it dependent on a successful
    //    // FFmpegCore configuration if you're not absolutely sure the binaries
    //    // are ready right after EnsureFFmpegBinariesExist completes.
    //    // For a robust solution, consider a callback or a state variable.

    //    // For demonstration, we'll call it directly.
    //    TimeSpan duration = await GetVideoDuration(exampleVideoPath);
    //        if (duration != TimeSpan.Zero)
    //        {
    //            GD.Print($"The duration of the example video is: {duration.TotalSeconds} seconds.");
    //        }
    //        else
    //{
    //    GD.PrintErr("Could not get duration for the example video.");
    //}
    public async Task<TimeSpan> GetVideoDurationAsync(string videoPath, Callable onCompleted)
    {
        if (!System.IO.File.Exists(videoPath))
        {
            GD.PrintErr($"Video file not found at: {videoPath}");
            onCompleted.Call(-1.0);
            return TimeSpan.Zero;
            
        }

        try
        {
            // FFProbe.AnalyseAsync requires the global FFmpegCore configuration to be set.
            var mediaInfo = await FFMpegCore.FFProbe.AnalyseAsync(videoPath);
            GD.Print($"Video '{videoPath}' duration: {mediaInfo.Duration}");
            onCompleted.Call(mediaInfo.Duration.TotalSeconds);
            return mediaInfo.Duration;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error getting video duration for '{videoPath}': {e.Message}");
            onCompleted.Call(-1.0);
            return TimeSpan.Zero;
        }
    }


    public void CreateVideoFromImageSequence(
         string inputDirectoryPath,
         string outputFilePath,
         int frameRate,
         string imageFilePattern, // Changed to a typical FFmpeg sequence pattern
         Callable onCompleted)
    {

        CreateVideoFromImageSequenceAsync(inputDirectoryPath, outputFilePath, frameRate, imageFilePattern, onCompleted);
    }

    public async Task<bool> CreateVideoFromImageSequenceAsync(
         string inputDirectoryPath,
         string outputFilePath,
         int frameRate,
         string imageFilePattern, // Changed to a typical FFmpeg sequence pattern
         Callable onCompleted)
  
    {
        // 1. Validate input directory
        if (!Directory.Exists(inputDirectoryPath))
        {
            GD.PushError($"Error: Input directory not found at '{inputDirectoryPath}'");
            onCompleted.Call(-1);
            return false;
        }

        // 2. Construct the full input image sequence path for FFmpeg
        // FFmpeg uses a pattern like "path/to/frames/frame_%03d.png" for numbered sequences.
        // %03d means a 3-digit padded number (e.g., 001, 002). Adjust %0xd based on your numbering.
        string fullImageSequenceInputPath = Path.Combine(inputDirectoryPath, imageFilePattern).Replace("\\", "/");
        
        // 3. Perform a quick check to see if any images matching the pattern exist.
        // This is a heuristic check, FFmpeg will ultimately confirm if the sequence is valid.
        // We'll use a more general pattern for Directory.EnumerateFiles to find any potential images.
        var commonImageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        var imageFiles = Directory.EnumerateFiles(inputDirectoryPath)
                                  .Where(file => commonImageExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                                  .ToList();


        if (!imageFiles.Any())
        {
            GD.PushError($"Error: No image files found in '{inputDirectoryPath}' that could form a sequence.");
            onCompleted.Call(-1);
            return false;
        }
   

        // 4. Ensure the output directory exists
        var outputDirectory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
        {
            try
            {
                Directory.CreateDirectory(outputDirectory);
                GD.Print($"Created output directory: '{outputDirectory}'");
            }
            catch (Exception dirEx)
            {
                GD.PushError($"Error creating output directory '{outputDirectory}': {dirEx.Message}");
                onCompleted.Call(-1);
                return false;
            }
        }

        GD.Print($"Starting video creation from image sequence: '{fullImageSequenceInputPath}' at {frameRate} fps...");
        GD.Print($"Output will be saved to: '{outputFilePath}'");

        try
        {
            // --- IMPORTANT: Ensure FFmpeg binaries are available ---
            // FFmpegCore requires ffmpeg.exe and ffprobe.exe to be accessible.
            // They should ideally be in your application's executable directory.
            // If not, you can configure their path explicitly like this (uncomment and adjust):
            // GlobalFFmpegOptions.Configure(new FFmpegOptions { BinaryFolder = @"C:\path\to\ffmpeg\bin" });

            // Execute FFmpeg command asynchronously using FromFileInput
            var success = await FFMpegArguments
                .FromFileInput(fullImageSequenceInputPath, false, options => options
                    .WithFramerate(frameRate) // Set the input framerate for the image sequence
                )
                .OutputToFile(outputFilePath, overwrite: true, options => options
                    .WithVideoCodec("libx264")      // Use H.264 video codec for MP4
                    .WithConstantRateFactor(23)     // Quality setting (lower is higher quality, larger file size)
                    .WithSpeedPreset(FFMpegCore.Enums.Speed.SuperFast) // Encoding speed (faster is lower quality/larger file)
                    .ForcePixelFormat("yuv420p")    // Ensure compatibility with most players
                )
                .ProcessAsynchronously();

            if (success)
            {
                GD.Print($"Successfully created video: '{outputFilePath}'");
                onCompleted.Call(1);
                return true;
            }
            else
            {
                GD.PushError($"Video creation failed for '{outputFilePath}'");
                onCompleted.Call(-1);
                return false;
            }
            
        }
        catch (Exception ex)
        {
            GD.PushError($"An error occurred during video creation: {ex.Message}");
            GD.PushError(ex.StackTrace); // Printing stack trace for debugging
            onCompleted.Call(-1);
            return false;
        }
    }

    void hi(string who)
    {
        GD.Print($"Hi, {who}");
    }

    public void GetDuck(string videoPath, Callable onCompleted)
    {
        
        GetDuckAsync(videoPath, onCompleted);
    }
    public async Task GetDuckAsync(string videoPath, Callable onCompleted)
    {
        await Task.Delay(1000);
        onCompleted.Call("result");
    }


    public bool DeleteFilesInDirectory(string directoryPath, string filePattern)
    {
        // Check if the directory exists
        if (!Directory.Exists(directoryPath))
        {
            GD.PrintErr($"Error: Directory not found at path: {directoryPath}");
            return false;
        }

        GD.Print($"Attempting to delete files in: {directoryPath} with pattern: {filePattern}");
        bool success = true;

        try
        {
            // Get all files matching the pattern in the specified directory.
            // Directory.GetFiles natively supports wildcard patterns (* and ?).
            string[] filesToDelete = Directory.GetFiles(directoryPath, filePattern);

            if (filesToDelete.Length == 0)
            {
                GD.Print($"No files found matching pattern '{filePattern}' in '{directoryPath}'.");
                return true; // No files to delete, so it's a success
            }

            GD.Print($"Found {filesToDelete.Length} files to delete.");

            // Iterate through the found files and delete each one
            foreach (string filePath in filesToDelete)
            {
                try
                {
                    File.Delete(filePath);
                    GD.Print($"Successfully deleted: {filePath}");
                }
                catch (UnauthorizedAccessException)
                {
                    GD.PrintErr($"Error: Access denied when trying to delete: {filePath}. Check permissions.");
                    success = false;
                }
                catch (IOException e)
                {
                    GD.PrintErr($"Error: IO exception when deleting {filePath}: {e.Message}");
                    success = false;
                }
                catch (Exception e)
                {
                    GD.PrintErr($"An unexpected error occurred while deleting {filePath}: {e.Message}");
                    success = false;
                }
            }
        }
        catch (DirectoryNotFoundException)
        {
            GD.PrintErr($"Error: The directory '{directoryPath}' was not found during file enumeration.");
            success = false;
        }
        catch (UnauthorizedAccessException)
        {
            GD.PrintErr($"Error: Access denied when enumerating files in '{directoryPath}'. Check directory permissions.");
            success = false;
        }
        catch (Exception e)
        {
            GD.PrintErr($"An unexpected error occurred during file enumeration in '{directoryPath}': {e.Message}");
            success = false;
        }

        return success;
    }

}