using Godot;
using System;

public static class AnimationExtensions
{
    /// <summary>
    /// Updates the value and time of a specific keyframe on a given animation track.
    /// This method extends the Animation class.
    /// </summary>
    /// <param name="animation">The Animation instance.</param>
    /// <param name="trackPath">The NodePath to the property being animated.</param>
    /// <param name="trackType">The type of the animation track.</param>
    /// <param name="keyIndex">The index of the keyframe to update.</param>
    /// <param name="value">The new value for the keyframe.</param>
    /// <param name="time">The new time for the keyframe.</param>
    /// <returns>True if the keyframe was successfully updated, false otherwise.</returns>
    public static bool UpdateTrackKey(this Animation animation, NodePath trackPath, Animation.TrackType trackType, int keyIndex, Variant value, double time)
    {
        bool valueUpdated = UpdateTrackKeyValue(animation, trackPath, trackType, keyIndex, value);
        bool timeUpdated = UpdateTrackKeyTime(animation, trackPath, trackType, keyIndex, time);
        return valueUpdated & timeUpdated;
    }
    /// <summary>
    /// Updates the value and time of a specific keyframe on a given animation track.
    /// This method extends the Animation class.
    /// </summary>
    /// <param name="animation">The Animation instance.</param>
    /// <param name="trackPath">The NodePath to the property being animated.</param>
    /// <param name="trackType">The type of the animation track.</param>
    /// <param name="keyIndex">The index of the keyframe to update.</param>
    /// <param name="value">The new value for the keyframe.</param>
    /// <param name="time">The new time for the keyframe.</param>
    /// <returns>True if the keyframe was successfully updated, false otherwise.</returns>
    public static bool UpdateTrackKeyValue(this Animation animation, NodePath trackPath, Animation.TrackType trackType, int keyIndex, Variant value)
    {
        if (animation == null)
        {
            GD.PrintErr("Provided animation object is null.");
            return false;
        }

        // Find the track by iterating through the animation's tracks.
        int trackCount = animation.GetTrackCount();
        for (int i = 0; i < trackCount; i++)
        {
            // Check if the track path and type match.
            if (animation.TrackGetPath(i) == trackPath && animation.TrackGetType(i) == trackType)
            {
                // Check if the key index is valid for this track.
                if (keyIndex < 0 || keyIndex >= animation.TrackGetKeyCount(i))
                {
                    GD.PrintErr($"Key index {keyIndex} is out of bounds for track '{trackPath}'.");
                    return false;
                }

                // Update the keyframe's value and time.
                animation.TrackSetKeyValue(i, keyIndex, value);
                //GD.Print($"Successfully updated keyframe at index {keyIndex} on track '{trackPath}'.");

                return true;
            }
        }

        GD.PrintErr($"Track with path '{trackPath}' and type '{trackType}' not found in the animation.");
        return false;
    }
    /// <summary>
    /// Updates the value and time of a specific keyframe on a given animation track.
    /// This method extends the Animation class.
    /// </summary>
    /// <param name="animation">The Animation instance.</param>
    /// <param name="trackPath">The NodePath to the property being animated.</param>
    /// <param name="trackType">The type of the animation track.</param>
    /// <param name="keyIndex">The index of the keyframe to update.</param>
    /// <param name="value">The new value for the keyframe.</param>
    /// <param name="time">The new time for the keyframe.</param>
    /// <returns>True if the keyframe was successfully updated, false otherwise.</returns>
    public static bool UpdateTrackKeyTime(this Animation animation, NodePath trackPath, Animation.TrackType trackType, int keyIndex, double time)
    {
        if (animation == null)
        {
            GD.PrintErr("Provided animation object is null.");
            return false;
        }

        // Find the track by iterating through the animation's tracks.
        int trackCount = animation.GetTrackCount();
        for (int i = 0; i < trackCount; i++)
        {
            // Check if the track path and type match.
            if (animation.TrackGetPath(i) == trackPath && animation.TrackGetType(i) == trackType)
            {
                // Check if the key index is valid for this track.
                if (keyIndex < 0 || keyIndex >= animation.TrackGetKeyCount(i))
                {
                    GD.PrintErr($"Key index {keyIndex} is out of bounds for track '{trackPath}'.");
                    return false;
                }

                // Update the keyframe's value and time.
                animation.TrackSetKeyTime(i, keyIndex, time);
                //GD.Print($"Successfully updated keyframe at index {keyIndex} on track '{trackPath}'.");

                return true;
            }
        }

        GD.PrintErr($"Track with path '{trackPath}' and type '{trackType}' not found in the animation.");
        return false;
    }


    /// <summary>
    /// Converts the track paths of an animation to match a new AnimationPlayer's context.
    /// This method performs the conversion in-place by modifying the provided animation.
    /// </summary>
    /// <param name="animation">The animation to convert. This object will be modified.</param>
    /// <param name="sourcePlayer">The AnimationPlayer that currently owns the animation and its original track paths.</param>
    /// <param name="targetPlayer">The AnimationPlayer that will own the new animation and its new track paths.</param>
    public static void ConvertTo(this Animation animation, AnimationPlayer sourcePlayer, AnimationPlayer targetPlayer)
    {
        // Get the root nodes for both the source and target AnimationPlayers
        Node sourceRoot = sourcePlayer.GetNode(sourcePlayer.RootNode);
        Node targetRoot = targetPlayer.GetNode(targetPlayer.RootNode);

        // Safety checks for the root nodes
        if (sourceRoot == null || targetRoot == null)
        {
            GD.PrintErr("One of the AnimationPlayer's root nodes could not be found. Cannot convert animation.");
            return;
        }

        // Iterate through each track of the animation and convert its path
        for (int i = 0; i < animation.GetTrackCount(); i++)
        {
            NodePath originalPath = animation.TrackGetPath(i);

            NodePath nodePathOnly = originalPath.GetConcatenatedNames();
            string propertyPath = originalPath.GetConcatenatedSubNames();

            Node animatedNode = sourceRoot.GetNodeOrNull(nodePathOnly);

            if (animatedNode != null)
            {
                NodePath newPathToNode = targetRoot.GetPathTo(animatedNode);

                string newCompletePathString = newPathToNode.ToString();
                if (!string.IsNullOrEmpty(propertyPath))
                {
                    newCompletePathString += ":" + propertyPath;
                }

                animation.TrackSetPath(i, new NodePath(newCompletePathString));
            }
            else
            {
                GD.PrintErr($"Could not find animated node for path: '{originalPath}'. This track will be skipped.");
            }
        }
    }


    /// <summary>
    /// Inserts an animation into another at a specified time.
    /// This method performs the insertion in-place by modifying the provided animation.
    /// </summary>
    /// <param name="animation">The base animation to insert into. This object will be modified.</param>
    /// <param name="inserted">The animation to be inserted.</param>
    /// <param name="from">The time in the base animation where the insertion should begin.</param>
    /// <param name="overwrite">If true, will overwrite a key if one already exists at the new time.</param>
    public static void InsertAnimation(this Animation animation, Animation inserted, double from, bool overwrite = true)
    {
        // Determine the new length of the combined animation.
        // It's either the length of the original animation or the length of the inserted
        // animation plus the start time, whichever is longer.
        double insertedEnd = from + inserted.Length;
        double newLength = Math.Max(animation.Length, insertedEnd);
        animation.Length = (float)newLength;

        // Iterate through each track of the animation to be inserted
        for (int i = 0; i < inserted.GetTrackCount(); i++)
        {
            Animation.TrackType trackType = inserted.TrackGetType(i);
            NodePath trackPath = inserted.TrackGetPath(i);
            Animation.UpdateMode updateMode = inserted.ValueTrackGetUpdateMode(i);

            // Find an existing track in the base animation with the same path and type, or create a new one.
            int targetTrackIndex = animation.FindTrack(trackPath, trackType);
            if (targetTrackIndex == -1)
            {
                targetTrackIndex = animation.AddTrack(trackType);
                animation.TrackSetPath(targetTrackIndex, trackPath);
                animation.ValueTrackSetUpdateMode(targetTrackIndex, updateMode);
            }

            // Copy all keyframes from the inserted animation to the base animation,
            // offsetting their time by the cursor time.
            for (int k = 0; k < inserted.TrackGetKeyCount(i); k++)
            {
                
                double keyTime = inserted.TrackGetKeyTime(i, k);
                Variant keyValue = inserted.TrackGetKeyValue(i, k);
                double newKeyTime = keyTime + from;
               

                // Corrected logic: Check for an existing keyframe using TrackFindKey and overwrite it if specified.
                int existingKeyIndex = animation.TrackFindKey(targetTrackIndex, newKeyTime, Animation.FindMode.Approx);
                if (overwrite && existingKeyIndex != -1)
                {
                    animation.TrackSetKeyValue(targetTrackIndex, existingKeyIndex, keyValue);
                }
                else
                {
                    animation.TrackInsertKey(targetTrackIndex, newKeyTime, keyValue);
                }
            }
        }
    }
}
