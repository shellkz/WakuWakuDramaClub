class_name DramaDialogueInstruction extends DramaInstruction
#對話 ["蓮實", "請進"]
var speaker : String
var speech : String
var expression : String

var dialogue_panel_visible_track : int
var speaker_label_text_track : int
var speech_label_text_track : int
var speaker_expression_track : int
func _bind_argument():
	speaker = argument[0]
	speech = argument[1]
	
	if option.has("表情"):
		expression = option["表情"]
	pass

func _add_track_if_not_exist(timeline : Animation):
	var speaker_label_text_path = "SubViewport/DialoguePanel/SpeakerLabel:text"
	var speech_label_text_path = "SubViewport/DialoguePanel/SpeechLabel:text"
	var dialogue_panel_visible_path = "SubViewport/DialoguePanel:visible"
	var speaker_expression_path = "SubViewport/Actor/%s/Pivot/ExpressionAnimationPlayer" % speaker
	
	
	
	# if found, speaker_label_text_track remain unset which is 0
	
	var _t_speaker_label_text_track = get_track(timeline, speaker_label_text_path)
	if _t_speaker_label_text_track == -1:
		speaker_label_text_track = timeline.add_track(Animation.TYPE_VALUE)
		timeline.value_track_set_update_mode(speaker_label_text_track, Animation.UpdateMode.UPDATE_DISCRETE)
		timeline.track_set_path(speaker_label_text_track, speaker_label_text_path)
		
	else:
		speaker_label_text_track = _t_speaker_label_text_track
		
	var _t_speech_label_text_track = get_track(timeline, speech_label_text_path)
	if _t_speech_label_text_track == -1:
		speech_label_text_track = timeline.add_track(Animation.TYPE_VALUE)
		timeline.value_track_set_update_mode(speech_label_text_track, Animation.UpdateMode.UPDATE_DISCRETE)
		timeline.track_set_path(speech_label_text_track, speech_label_text_path)
	else:
		speech_label_text_track = _t_speech_label_text_track

	var _t_dialogue_panel_visible_track = get_track(timeline, dialogue_panel_visible_path)
	if _t_dialogue_panel_visible_track == -1:
		dialogue_panel_visible_track = timeline.add_track(Animation.TYPE_VALUE)
		timeline.value_track_set_update_mode(dialogue_panel_visible_track, Animation.UpdateMode.UPDATE_DISCRETE)
		timeline.track_set_path(dialogue_panel_visible_track, dialogue_panel_visible_path)
	else:
		dialogue_panel_visible_track = _t_dialogue_panel_visible_track
		
	
	var _t_speaker_expression_track = get_track(timeline, speaker_expression_path)
	if _t_speaker_expression_track == -1:
		speaker_expression_track = timeline.add_track(Animation.TYPE_ANIMATION)
		#timeline.value_track_set_update_mode(speaker_expression_track, Animation.UpdateMode.UPDATE_CONTINUOUS)
		timeline.track_set_path(speaker_expression_track, speaker_expression_path)
	else:
		speaker_expression_track = _t_speaker_expression_track
	
	#print("%s %s " % [speaker_label_text_track, speech_label_text_track])
func _insert_starting_keyframe(context : AnimationPlayer, timeline : Animation, cursor : float):

	#dialogue panel visible on
	timeline.track_insert_key(dialogue_panel_visible_track, cursor, true)
	timeline.track_insert_key(speaker_label_text_track, cursor, speaker)
	timeline.track_insert_key(speech_label_text_track, cursor, speech)
	timeline.animation_track_insert_key(speaker_expression_track, cursor, expression)
	
	pass
func _insert_ending_keyframe(context : AnimationPlayer, timeline : Animation, cursor : float):
	timeline.track_insert_key(dialogue_panel_visible_track, cursor, false)
	timeline.track_insert_key(speaker_label_text_track, cursor, "")
	timeline.track_insert_key(speech_label_text_track, cursor, "")
	pass

func _get_duration() -> float:
	return speech.length() / 4.0
