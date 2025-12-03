class_name DramaWaitInstruction extends DramaInstruction

var duration : float

func _bind_argument():
	duration = float(argument[0])
	pass
	

func _get_duration() -> float:
	print("wait %s" % duration)
	return duration
