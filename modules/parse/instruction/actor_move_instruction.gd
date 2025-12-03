class_name DramaActorMoveInstruction extends DramaInstruction
var speaker : String
var from_position : String
var to_position : String

var speaker_position_track : int
func _bind_argument():
	speaker = argument[0]
	from_position = argument[1]
	to_position = argument[2]
	pass

func _add_track_if_not_exist(timeline : Animation):
	var speaker_position_path = "SubViewport/Actor/%s:global_position" % speaker

	
	# if found, speaker_position_track remain unset which is 0
	
	var _t_speaker_position_track = get_track(timeline, speaker_position_path)
	if _t_speaker_position_track == -1:
		speaker_position_track = timeline.add_track(Animation.TYPE_VALUE)
		timeline.value_track_set_update_mode(speaker_position_track, Animation.UpdateMode.UPDATE_CONTINUOUS)
		timeline.track_set_path(speaker_position_track, speaker_position_path)
	else:
		speaker_position_track = _t_speaker_position_track
		
func _insert_starting_keyframe(context : AnimationPlayer, timeline : Animation, cursor : float):
	## below code is wrong (starting position is not original position)
	## probably anochor and pivot setup problem
	## but for moving to target position from current
	## maybe discard starting keyframe is reasonable
	## at least it works
	
	## above comment is bullshit
	## this starting frame is needed
	## it works by pinning property value to break the continouse change from last clip
	
	## last clip		current clip
	##		ending		start		ending	==> correct
	##		ending		(removed)	ending	==> wrong, animation will keep going from keyframe of previous clip to current
	
	## the real reason is that 
	## i bake keyframe before play animation
	## so query dynamic ojbect's property will only get initial value(because animation not player, property remain still)
	## that why starting frame located at left side which is intial position of my character
	
	## so i have to maintain a set of mirrored property for pre render process
	## and set and get only during pre render process
	
	## the whole archtechture become very in-direct
	## that
	##		1. i have my own render game loop
	## 		2. i have to set value with keyframe
	## 		3. i have to maintain my own property for dynamic object controll by animation
	
	
	#var original_position = context.get_node("../Actor/%s" % speaker).global_position
	#print("%s %s" % [context.get_node("../Actor/%s" % speaker).name, original_position])
	
	var original_position = context.get_node("../Position/%s" % from_position).global_position
	timeline.track_insert_key(speaker_position_track, cursor, original_position)
	pass

func _insert_ending_keyframe(context : AnimationPlayer, timeline : Animation, cursor : float):
	var destination_position = context.get_node("../Position/%s" % to_position).global_position
	timeline.track_insert_key(speaker_position_track, cursor, destination_position)
	pass

func _get_duration() -> float:
	return 0.6
