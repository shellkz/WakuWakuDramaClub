using Godot;
using System;

using WakuWakuDramaClub.Timline;
public partial class DialoguePanel : Panel
{
    [Export]
    public AnimationPlayer Player { get; set; }

   

    public AnimationClip ShowDialogue(string speaker, string speech, TimeSpan voiceDuration)
    {


        Animation animation = (Animation)Player.GetAnimation("show_dialogue").Duplicate();
        

        double duration = voiceDuration.TotalSeconds + 0.1;
        //GD.Print($"dialogue duration {speech} {duration}");
        animation.Length = (float)duration;


        animation.UpdateTrackKeyTime(".:visible", Animation.TrackType.Value, 1, duration);


        animation.UpdateTrackKeyValue("SpeakerLabel:text", Animation.TrackType.Value, 0, speaker);
        animation.UpdateTrackKeyTime("SpeakerLabel:visible", Animation.TrackType.Value, 1, duration);


        animation.UpdateTrackKeyValue("SpeechLabel:text", Animation.TrackType.Value, 0, speech);
        animation.UpdateTrackKeyTime("SpeechLabel:visible", Animation.TrackType.Value, 1, duration);

        
        // Discrete Confirmed
        //int test = animation.FindTrack(".:visible", Animation.TrackType.Value);
        //GD.Print($"Update Mode {animation.ValueTrackGetUpdateMode(test)}");
        
        return new AnimationClip(Player, animation);
    }

   

    // Constant values for normal reading speeds.
    // A common average for English is 225 WPM.
    private const double AlphabeticTextWpm = 225.0;
    // A common average for East Asian languages is 400 characters per minute.
    private const double CJKTextCpm = 400.0;


    /// <summary>
    /// Calculates the estimated time in seconds it takes to read a given string.
    /// The method automatically detects if the text is alphabetic or East Asian
    /// and uses the appropriate average reading speed (WPM or CPM). This updated
    /// version can also handle mixed-language strings.
    /// </summary>
    /// <param name="text">The input string to be evaluated.</param>
    /// <returns>The estimated reading time in seconds. Returns 0 if the input string is null or empty.</returns>
    public double CalculateReadingTime(string text)
    {

        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }
        return text.Length / 5.0;



        //double totalReadingTimeInSeconds = 0;

        //// Seperate into Alphabetic or CJK blocks
        //// 我是Dark Flame Master => 我是 + Dark Flame Master

        //var regex = new Regex(@"(\p{IsCJKUnifiedIdeographs}+|\w+)", RegexOptions.Compiled);
        //var matches = regex.Matches(text);

        //foreach (Match match in matches)
        //{
        //    string segment = match.Value;

        //    // For CJK blocks, use character per minute to evaluate
        //    bool isCJKBlock = Regex.IsMatch(segment, @"\p{IsCJKUnifiedIdeographs}");
        //    if (isCJKBlock)
        //    {
        //        int totalCharacters = segment.Length;
        //        double readingTimeInMinutes = totalCharacters / CJKTextCpm;
        //        totalReadingTimeInSeconds += readingTimeInMinutes * 60;
        //    }
        //    // For Alphabetic blocks, use word per minute to evaluate
        //    else
        //    {
        //        string[] words = segment.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //        int totalWords = words.Length;
        //        double readingTimeInMinutes = totalWords / AlphabeticTextWpm;
        //        totalReadingTimeInSeconds += readingTimeInMinutes * 60;
        //    }
        //}

        //return totalReadingTimeInSeconds;
    }
}
