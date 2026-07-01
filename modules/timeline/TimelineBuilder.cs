using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using WakuWakuDramaClub.Scripting.Instructions;
using WakuWakuDramaClub.Render;
using WakuWakuDramaClub.Project;
namespace WakuWakuDramaClub.Timline;
public partial class TimelineBuilder : Node
{
    // TimelineViewport
    [Export]
    public Stage TimelineViewport { get; set; }

 

    public async Task<Timeline> BuildTimeline(List<Instruction> instructions)
    {
        

        Animation timeline = new Animation();
        List<VideoRenderer.AudioClip> audios = new List<VideoRenderer.AudioClip>();
        double cursor = 0.0;



        foreach (Instruction instruction in instructions)
        {


            AnimationPack animationPack = await instruction.BakeAsAnimation(TimelineViewport);
        
            
            animationPack.ConvertTo(TimelineViewport.AnimationPlayer); 
       

        
            // [Time Advancing and insert keyframe]
            foreach (AnimationClip clip in animationPack.Clips)
            {
               
                timeline.InsertAnimation(clip.Animation, cursor, true, ProjectDefaults.FrameDelta);
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
