App: Core
Build
Based on (<branch> <commit>)

Initials
Date
Session Names

Device / Browser

General
  Renders Correctly	
		
Client Manager
  Syncs Poses
    Head
    Left Hand
    Right Hand
		Hand animation state
	Performs Well
    Client Count Limit
		Latency
		
Interactions and Tools
  Can use menu
    Activate / Deactivate
    Control Buttons
    Hover Cursor
		Menu loads correctly on every entry
		Left / Right spawn works well
		Handedness / cursor
	Can teleport
    with one hand
    with either hand
		snap turns
		offset is correct
	Can use draw tool
    with one hand
    while teleporting
		reset stroke location on teleport
		draw drag while teleporting
		with either hand
		strokes have correct color and z depth re:models, avatars
		strokes have correct z depth re:each other
		grab and scale
		erase
		erase others' stuff
		undo/redo
		undo draw, undo erase
	Can use settings
    height adjustment
		height offset preserve after teleport
	Can re-center	goes to world center
		height restores
	Can use people
    client list
		client names above head
		
Run-Time Import
  Loads	Performant
	Loads Passed Values
    Scale
    IsWholeObject
	Can Grab
    Grab synchronizes
		Grabbing locks for others
	Can Scale
    Scale synchronizes
		Scaling locks for others
	Can Show / Hide
  Synced show / hide
	Can Lock
  Synced single-object lock
		Synced multi-object lock
		Grab lock vs UI lock
	Performs Well	Texture Limit
		Vertex Limit
		Count Limit
		
		
Scenes
  Loads	Performant
	Switches Performant
	Performs Well	Light Limit
		Texture Limit
		Vertex Limit
		Count Limit
	Catches Up State Upon (Re-)Entry
    Scenes
		Avatar poses
		Asset poses, visiblity, locks, scales
		Drawing poses, color
		Menu pose, visibility
	Can use keyboard controls
    rotate (`Q` `E` `2` `3` or Left Mouse Drag)
    strafe (`W` `A` `S` `D`)
    pan (Middle Mouse Drag)
    hyperspeed scroll (Mouse Scroll)
		
