class_name DramaInstruction
var argument : Array = []
var option : Dictionary = {}

func _init(raw_instruction : DramaRawInstruction) -> void:
	self.argument = raw_instruction.argument
	self.option = raw_instruction.option
	_bind_argument()
	
func _bind_argument():
	pass

func _add_track_if_not_exist(timeline : Animation):
	pass

func _insert_starting_keyframe(context : AnimationPlayer, timeline : Animation, cursor : float):
	pass
func _insert_ending_keyframe(context : AnimationPlayer, timeline : Animation, cursor : float):
	pass

func _get_duration() -> float:
	return 0.0



func has_track(animation: Animation, property_path: NodePath) -> bool:
	if animation == null:
		return false

	for i in range(animation.get_track_count()):

			
			var current_track_path: NodePath = animation.track_get_path(i)
			
			# NodePath comparison is efficient and handles sub-properties correctly.
			if current_track_path == property_path:
				return true
	return false

func get_track(animation: Animation, property_path: NodePath) -> int:
	if animation == null:
		return -1

	for i in range(animation.get_track_count()):

			
			var current_track_path: NodePath = animation.track_get_path(i)
			
			# NodePath comparison is efficient and handles sub-properties correctly.
			if current_track_path == property_path:
				return i
	return -1	
