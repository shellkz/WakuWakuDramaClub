# This script is designed to be attached to any Node in your Godot scene.
# It captures an image from a specified SubViewport and saves it to a file.

extends Control

@export var anim_player : AnimationPlayer

@export var script_editor : CodeEdit
@export var timeline_viewport : SubViewport
signal completed

# clean up after rendering
# export to test

# parsing
#	replace dictionary with solid object

# timelining
#	too top down control should be more oo

# rendering
#	capture frame(take most time)
#		consider using streaming instead of static image file
#	compose video

var n : int = 0
var animation_library
func _ready() -> void:
	animation_library = AnimationLibrary.new()
	anim_player.add_animation_library("custom", animation_library)
	

	#for line in script_editor.text.split("\n"):
		#var tokens : Array[Token] = parser.Tokenize(line)
		#print(tokens)
	
	
func _on_render_finished(result : int):
	print("render finished")
	pass

func render():
	n += 1
	
	
	
	var parser : DramaScriptParser = DramaScriptParser.new()
	
	var instructions : Array[DramaInstruction] = parser.parse_script(script_editor.text.split("\n"))

	
	# bake instruction into timeline(track, keyframe)
	
	var timeline : Animation = Animation.new()
	animation_library.add_animation("%s" % n, timeline)
	
	var dialogue_panel_visible = timeline.add_track(Animation.TYPE_VALUE)
	timeline.track_set_path(dialogue_panel_visible, "SubViewport/DialoguePanel:visible")
	timeline.value_track_set_update_mode(dialogue_panel_visible, Animation.UPDATE_DISCRETE)
	timeline.track_insert_key(dialogue_panel_visible, 0.0, false)
	
	var cursor : float = 0.0
	for instruction in instructions:
		#print(instruction.argument)
		instruction._add_track_if_not_exist(timeline)
		instruction._insert_starting_keyframe(anim_player, timeline, cursor)
		cursor += instruction._get_duration()
		instruction._insert_ending_keyframe(anim_player, timeline, cursor)

		pass
	timeline.length = cursor + 0.1

	#ResourceSaver.save(timeline,"%s.res" % n)
	anim_player.seek(0.0, true)
	anim_player.play("custom/%s" % n)
	
	
	FFmpegService.DeleteFilesInDirectory(ProjectSettings.globalize_path("user://captures"), "*.png")
	
#
	### render video

	await capture_frame()
	

	var input_directory_path = ProjectSettings.globalize_path("user://captures")
	var output_file_path = "%s/output.mp4" % [ProjectSettings.globalize_path("user://captures")]
	var frame_rate = 10
	var image_file_pattern = "frame_%d.png"
	
	FFmpegService.CreateVideoFromImageSequence(input_directory_path, output_file_path, frame_rate, image_file_pattern, _on_render_finished)

	
func capture_frame():
	anim_player.seek(0.0, true)
	anim_player.play("custom/%s" % n)
	anim_player.pause()


	# push 	1/30 second until animation finished
	var cursor : float = 0.0
	var frame : int = 1
	while cursor <= anim_player.current_animation_length:
		#print(cursor)
		#print(anim_player.current_animation_length)
		#print(frame)
		
		print("frame %s processed" % [frame])
		print("current time %s" % [cursor])
		
		capture_viewport_image(frame)
		anim_player.seek(cursor, true)
		
		frame += 1
		cursor = (frame - 1) / 60.0
		
		await get_tree().process_frame
	
	# use FFMpeg to render video from imege sequce
	#FFmpegService.CreateVideoFromImageSequence(input_path, output_path)
	
	
	print("capture frmae finished")	

func capture_viewport_image(i : int) -> void:
	# Get the image from the viewport's texture
	var image: Image = timeline_viewport.get_texture().get_image()


	# Define the directory where the captured image will be saved.
	# "user://" points to the user data directory for your project.
	var capture_dir = "user://captures/"
	
	# Attempt to open the directory. If it doesn't exist, create it.
	var dir_access = DirAccess.open(capture_dir)
	if dir_access == null:
		# If DirAccess.open returns null, the directory doesn't exist.
		# Try to create it recursively.
		var error = DirAccess.make_dir_absolute(capture_dir)
		if error != OK:
			# Print an error message if directory creation fails.
			print("Error creating directory '%s': %s" % [capture_dir, error])
			return

	# Define the full file path for the captured image.
	# We'll use a simple name like "captured_viewport.png" for this example.
	var file_path = capture_dir + "frame_%s.png" % i
	
	# Save the image as a PNG file.
	var save_error = image.save_png(file_path)

	#if save_error == OK:
		#print("Successfully captured viewport to: %s" % file_path)
	#else:
		#print("Error saving image to %s: %s" % [file_path, save_error])

func _on_render_button_pressed() -> void:
	render()
