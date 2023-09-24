@tool
extends TextureRect

func update_aspect_ratio():
	(material as ShaderMaterial).set_shader_parameter("iResolution", size)
