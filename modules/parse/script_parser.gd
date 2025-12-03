class_name DramaScriptParser
var supported_instruction = ["場景", "等待", "效果音", "等待"]

var factory : Dictionary[String, Callable] = {
	"對話" : func(raw_instruction : DramaRawInstruction):
		return DramaDialogueInstruction.new(raw_instruction),
		
	"登場" : func(raw_instruction : DramaRawInstruction):
		return DramaActorEntranceInstruction.new(raw_instruction),
	
	"場景" : func(raw_instruction : DramaRawInstruction):
		return DramaChangeSceneInstruction.new(raw_instruction),
		
	"等待" : func(raw_instruction : DramaRawInstruction):
		return DramaWaitInstruction.new(raw_instruction),
		
	"移動" : func(raw_instruction : DramaRawInstruction):
		return DramaActorMoveInstruction.new(raw_instruction),
}
func parse_script(script : Array[String]) -> Array[DramaInstruction]:
	var result : Array[DramaInstruction] = []
	
	for line in script:
		var tokens = tokenize_line(line)
		var raw_instruction : DramaRawInstruction = DramaRawInstruction.new()
		
		var options = tokens.filter(func(token): return token["type"] == "option")
		
		for option in options:
			var key = option["value"].split("=")[0]
			var value = option["value"].split("=")[1]
			raw_instruction.option[key] = value	
		
		tokens = tokens.filter(func(token): return token["type"] != "option")
		
		
	
		
		if supported_instruction.has(tokens[0]["value"]):
			raw_instruction.type = tokens[0]["value"]
			tokens.erase(tokens[0])
			raw_instruction.argument = tokens.map(func(token): return token["value"])
		
		else:
			if tokens[1]["type"] == "dialogue":
				raw_instruction.type = "對話"
				raw_instruction.argument = tokens.map(func(token): return token["value"])
				
			else:
				raw_instruction.type = tokens[1]["value"]
				tokens.erase(tokens[1])
				raw_instruction.argument = tokens.map(func(token): return token["value"])
				pass
		
		
		if factory.has(raw_instruction.type):
			result.append(factory[raw_instruction.type].call(raw_instruction))
		else:
			print("[Instruction] Unknown instruction %s" % raw_instruction.type)
	
	
	
	return result
func tokenize_line(line: String) -> Array:
	var tokens: Array = []
	var current_token: String = ""
	var in_wrapper: bool = false
	var wrapper_start_char: String = "" # Stores the opening wrapper character ('「' or '（')

	# Define the wrapper character pairs
	var wrapper_pairs: Dictionary = {
		"「": "」",
		"（": "）"
	}

	for i in range(line.length()):
		var char: String = line[i]

		if in_wrapper:
			# If we are inside a wrapper, check for the closing character
			var expected_closing_char: String = wrapper_pairs[wrapper_start_char]
			if char == expected_closing_char:
				# Found the closing wrapper, add the accumulated token with specific type
				var token_type: String
				if wrapper_start_char == "「":
					token_type = "dialogue"
				elif wrapper_start_char == "（":
					token_type = "option"
				else:
					token_type = "wrapped" # Fallback, though should be covered by above

				tokens.append({"value": current_token, "type": token_type})
				current_token = ""
				in_wrapper = false
				wrapper_start_char = "" # Reset wrapper start char
			else:
				# Still inside the wrapper, append character to current token
				current_token += char
		else:
			# Not inside a wrapper, check for opening wrappers or whitespace
			if wrapper_pairs.has(char):
				# Found an opening wrapper character
				if not current_token.is_empty():
					# Add any accumulated token before the wrapper starts as a normal type
					tokens.append({"value": current_token, "type": "normal"})
					current_token = ""
				in_wrapper = true
				wrapper_start_char = char
			# In Godot 4, `is_space()` is not a method of String.
			# We can check against common whitespace characters.
			elif char == " " or char == "\t" or char == "\n" or char == "\r":
				# Found a whitespace character outside a wrapper
				if not current_token.is_empty():
					# Add the accumulated token if it's not empty as a normal type
					tokens.append({"value": current_token, "type": "normal"})
					current_token = ""
			else:
				# Regular character, append to current token
				current_token += char

	# After the loop, add any remaining characters in current_token as a normal type
	if not current_token.is_empty():
		tokens.append({"value": current_token, "type": "normal"})

	return tokens
