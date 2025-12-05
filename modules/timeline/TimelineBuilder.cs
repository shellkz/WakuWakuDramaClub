using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using WakuWakuDramaClub.Parse;
using WakuWakuDramaClub.Render;
namespace WakuWakuDramaClub.Timline;
public partial class TimelineBuilder : Node
{
    // TimelineViewport
    [Export]
    public TimelineViewport TimelineViewport { get; set; }

 

    public async Task<Timeline> BuildTimeline(List<Instruction> instructions)
    {
        

        Animation timeline = new Animation();
        List<VideoRenderer.AudioClip> audios = new List<VideoRenderer.AudioClip>();
        double cursor = 0.0;



        foreach (Instruction instruction in instructions)
        {


            AnimationPack animationPack = await instruction.BakeAsAnimation(TimelineViewport);
        
       
            animationPack.ConvertTo(TimelineViewport.AnimationPlayer); 
       
            //// [auto sync state]
            //  foreach clip in pack
            //      foreach track
            //          track.context_node.set(track.context_property, track.last key value)
        
            // [Time Advancing and insert keyframe]
            foreach (AnimationClip clip in animationPack.Clips)
            {
               
                timeline.InsertAnimation(clip.Animation, cursor, true);
            }

            foreach (VideoRenderer.AudioClip audio in animationPack.Audios)
            {

                audio.StartingTime += cursor;
                audios.Add(audio);
            }

    
            cursor += animationPack.GetDuration();
        }


   


        return  new Timeline(timeline, audios); ;
    }
   



}
