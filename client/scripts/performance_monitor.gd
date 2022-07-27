class_name PerformanceMonitor
extends Label

func _process(delta):
	var fps: float = Performance.get_monitor(Performance.TIME_FPS)
	var vsync: bool = OS.is_vsync_enabled()
	var triangles: float = Performance.get_monitor(Performance.RENDER_VERTICES_IN_FRAME) / 3

	text = "%d fps (vsync: %s)\n%d tris" % [ fps, vsync, triangles ]

	if Input.is_action_just_released("toggle_debug_info"):
		get_parent().visible = !get_parent().visible

	if Input.is_action_just_released("toggle_fullscreen"):
		OS.window_fullscreen = !OS.window_fullscreen

	if Input.is_action_just_released("toggle_vsync"):
		OS.set_use_vsync(!vsync)
