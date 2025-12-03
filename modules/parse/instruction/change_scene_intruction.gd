class_name DramaChangeSceneInstruction extends DramaInstruction
var background : String

var background_texture_track : int
func _bind_argument():
	var path : String = ""
	for arg in argument:
		path += ("%s " % arg)
	path = path.trim_suffix(" ") 
	background = path
	pass

func _add_track_if_not_exist(timeline : Animation):
	var background_texture_path = "SubViewport/Background:texture"
	
	# if found, background_texture_track remain unset which is 0
	
	var _t_background_texture_track = get_track(timeline, background_texture_path)
	if _t_background_texture_track == -1:
		background_texture_track = timeline.add_track(Animation.TYPE_VALUE)
		timeline.value_track_set_update_mode(background_texture_track, Animation.UpdateMode.UPDATE_DISCRETE)
		timeline.track_set_path(background_texture_track, background_texture_path)
	else:
		background_texture_track = _t_background_texture_track
		
func _insert_starting_keyframe(context : AnimationPlayer, timeline : Animation, cursor : float):
	#print("%s %s" % [speaker, speech])
	
	timeline.track_insert_key(background_texture_track, cursor, load("res://assets/backgrounds/%s.jpg" % background))
	
	pass
