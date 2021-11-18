# WebXR Client Test YYYY-MM-DD-HHMM

# Template
## [Section]

**[Subsection]**
\[Test\] [RESULT]

RESULT = …

- PASS 
- FAIL / NETFAIL
- DEFER / NETDEFER
- WORKAROUND / NETWORKAROUND
- N/T 
- [Detail]
# Details

App   [NAME]
Build vX.Y.Z-[SEMESTER]-[RC#]-[DEV/NONDEV]

# VR

Person          NAME
Device          PC/HEADSET
Browser         BROWSER
VR / Spectator  e.g., QUEST LINK

## Client Manager 

**Syncs Poses**
Head [RESULT]
Left Hand [RESULT]
Right Hand [RESULT]
Hand animation state [RESULT]
Performs Well [RESULT]
Client Count Limit >= 2
Latency <= ~1s

## Interactions and Tools
****
**Can use menu** 
Activate / Deactivate [RESULT]
Control Buttons [RESULT]
Hover Cursor [RESULT]
Menu loads correctly on every entry [RESULT]
Left / Right spawn works well [RESULT]
Handedness / cursor [RESULT]

**Can teleport**
with left hand [RESULT]
with right hand [RESULT]
ground = purple, confirm [RESULT]
air = red, no cylinder, cancel [RESULT]
snap turns [RESULT]
left hand [RESULT]
right hand [RESULT]
offset is correct [RESULT]

**Can use draw tool**
hover color [RESULT] [2]
select color [RESULT]
compare color [RESULT] [2]
with left hand [RESULT]
with right hand [RESULT]
while teleporting [RESULT]
reset stroke location on teleport [RESULT]
draw drag while teleporting [RESULT]
strokes have correct color and z depth re:models, avatars [RESULT]
strokes have correct z depth re:each other [RESULT]
strokes have correct z depth while drawing [RESULT]
strokes have correct z depth after drawing is done [RESULT]
grab and scale draw strokes [RESULT]

**erase**
with left hand [RESULT]
with right hand [RESULT]
erase others' stuff [RESULT]

**undo/redo**
undo draw [RESULT]
undo erase [RESULT]

**Can use settings** 
auto-initialize height [RESULT]
height calibration [RESULT]
height adjustment [RESULT]
height offset preserve after teleport [RESULT]

**Starting position**
goes to world center [RESULT]
height restores [RESULT]

**Can use people**
client list [RESULT]
client names above head [RESULT]

## Models

**Run-time import**
Loads Performant NOT TESTED

**Loads Passed Values**
Scale [RESULT]
IsWholeObject (ModelPack) [RESULT]

**Model Interactions**
Can Grab [RESULT]
Grab synchronizes [RESULT]
Grabbing locks for others [RESULT]
Can Scale [RESULT]
Scale synchronizes [RESULT]
Scaling locks for others [RESULT]
Can Show / Hide [RESULT]
Synced show / hide [RESULT]
Can Lock [RESULT]
Synced single-object lock [RESULT]
Synced model pack lock [RESULT]
Grab lock vs UI lock [RESULT]

**Performs Well** 
Texture Limit [RESULT]
Vertex Limit [RESULT]
Count Limit [RESULT]

## Scene

Light Limit [RESULT]
****Texture Limit [RESULT]
Vertex Limit [RESULT]
Count Limit [RESULT]

## Connection

**Catches Up State Upon (Re-)Entry**
Auto connect [RESULT]
Get Joined to Session [RESULT]
Avatar names [RESULT]
Avatar people [RESULT]
Avatar poses [RESULT]
Model poses [RESULT]
visibility [RESULT]
locks [RESULT]
scales [RESULT]
“Someone just joined” own [RESULT] [3]
“Someone just joined” others [RESULT] [3]
“Someone just left” own [RESULT]
“Someone just left” others [RESULT]
Close Connection and Rejoin simulated offline [RESULT]
Drawing poses, color [RESULT]

# Spectator

Person          NAME
Device          PC/HEADSET
Browser         BROWSER

## Testing WebXR Client

**General** 
Renders Correctly [RESULT]

**Sync poses**
Head [RESULT]
Can Show / Hide [RESULT]
Synced show / hide [RESULT]
Can Lock [RESULT]
Synced single-object lock [RESULT]
Synced multi-object lock [RESULT]
Lock/Unlock Show/Hide syncs on user click only [RESULT]
Height adjustment [RESULT]
Up/down arrows [RESULT]

**Can use keyboard controls** 
rotate (`Q` `E` `2` `3` or Left Mouse Drag) [RESULT]
strafe (`W` `A` `S` `D`) [RESULT]
pan (Middle Mouse Drag) [RESULT]
hyperspeed scroll (Mouse Scroll) [RESULT]

## Bugs
- [example]
