class_name DramaActorEntranceInstruction extends DramaInstruction
var speaker : String
var position : String

var speaker_visible_track : int
var speaker_position_track : int
var speaker_modulate_track : int
func _bind_argument():
	speaker = argument[0]
	
	position = argument[1] if argument.size() > 1 else "中"
	pass

func _add_track_if_not_exist(timeline : Animation):
	var speaker_visible_path = "SubViewport/Actor/%s:visible" % speaker
	var speaker_position_path = "SubViewport/Actor/%s:global_position" % speaker
	var speaker_modulate_path = "SubViewport/Actor/%s:modulate" % speaker
	# if found, speaker_visible_track remain unset which is 0
	
	var _t_speaker_visible_track = get_track(timeline, speaker_visible_path)
	if _t_speaker_visible_track == -1:
		speaker_visible_track = timeline.add_track(Animation.TYPE_VALUE)
		timeline.value_track_set_update_mode(speaker_visible_track, Animation.UpdateMode.UPDATE_DISCRETE)
		timeline.track_set_path(speaker_visible_track, speaker_visible_path)
	else:
		speaker_visible_track = _t_speaker_visible_track
		

	var _t_speaker_position_track = get_track(timeline, speaker_position_path)
	if _t_speaker_position_track == -1:
		speaker_position_track = timeline.add_track(Animation.TYPE_VALUE)
		timeline.value_track_set_update_mode(speaker_position_track, Animation.UpdateMode.UPDATE_CONTINUOUS)
		timeline.track_set_path(speaker_position_track, speaker_position_path)
	else:
		speaker_position_track = _t_speaker_position_track
		
	var _t_speaker_modulate_track = get_track(timeline, speaker_modulate_path)
	if _t_speaker_modulate_track == -1:
		speaker_modulate_track = timeline.add_track(Animation.TYPE_VALUE)
		timeline.value_track_set_update_mode(speaker_modulate_track, Animation.UpdateMode.UPDATE_CONTINUOUS)
		timeline.track_set_path(speaker_modulate_track, speaker_modulate_path)
	else:
		speaker_modulate_track = _t_speaker_modulate_track
		
func _insert_starting_keyframe(context : AnimationPlayer, timeline : Animation, cursor : float):
	#print("%s %s" % [speaker, speech])
	timeline.track_insert_key(speaker_visible_track, cursor, true)
	
	
	var destination_position = context.get_node("../Position/%s" % position).position
	timeline.track_insert_key(speaker_position_track, cursor, destination_position)
	
	timeline.track_insert_key(speaker_modulate_track, cursor, Color.BLACK)
	pass
	
func _insert_ending_keyframe(context : AnimationPlayer, timeline : Animation, cursor : float):
	
	timeline.track_insert_key(speaker_modulate_track, cursor, Color.WHITE)
	pass

func _get_duration() -> float:
	return 0.4
