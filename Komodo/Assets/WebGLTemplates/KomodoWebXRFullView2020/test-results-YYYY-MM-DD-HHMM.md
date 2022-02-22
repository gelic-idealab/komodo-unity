# WebXR Client Test 

_YYYY-MM-DD-HHMM_

_APP_NAME_

_vX_Y_Z-RC_NUM-[NON_DEV]_


# VR Tests

- Person      _NAME_
- Device      _VR_HEADSET_
- VR Type     _STANDALONE_OR_LINK_
- Browser     _VR_BROWSER_

## Client Manager 

### Syncs Poses
- Head _RESULT_
- Left Hand _RESULT_
- Right Hand _RESULT_
- Hand animation state _RESULT_
- Performs Well _RESULT_
- Client Count Limit >= _NUMBER_
- Latency <= _SECONDS_

## Interactions and Tools

### VR menu 
- Menu loads correctly on every entry _RESULT_
- Only shows spectator content initially _RESULT_
- Only one panel shows at a time _RESULT_
  - Only current tab is highlighted _RESULT_
- On enter VR, shows Create panel _RESULT_
- On enter VR, shows height calibration _RESULT_
- Activate / Deactivate _RESULT_
- Cursor respects menu handedness _RESULT_
- Switch Panels _RESULT_
- Hover Cursor _RESULT_
- Left / Right switch hands works well _RESULT_
- Settings panel hides instructor menu button _RESULT_
- Instructor menu gets shown when going from VR to Spectator _RESULT_

### Spectator menu
- (See spectator section below)

### Can teleport
- with left hand _RESULT_
- with right hand _RESULT_
- ground = purple, confirm _RESULT_
- air = red, no cylinder, cancel _RESULT_
- snap turns with left and right hands _RESULT_
- snap turn teleportation offset is correct _RESULT_
- snap turn offset is correct _RESULT_
- teleport on non-level surfaces _RESULT_

### Can use draw tool
- draw tool does not open immediately _RESULT_
- hover color _RESULT_
- select color _RESULT_
- compare color _RESULT_
- with left hand _RESULT_
- with right hand _RESULT_
- with both hands _RESULT_
- while teleporting _RESULT_
- reset stroke location on teleport _RESULT_
- draw drag while teleporting _RESULT_
- strokes have correct color and z depth re:models, avatars _RESULT_
- strokes have correct z depth re:each other _RESULT_
- strokes have correct z depth while drawing _RESULT_
- strokes have correct z depth after drawing is done _RESULT_
- move own drawings _RESULT_
- rotate own drawings _RESULT_
- scale own drawings _RESULT_
- move others' drawings _RESULT_
- rotate others' drawings _RESULT_
- undo own drawings _RESULT_

### erase
- with left hand _RESULT_
- with right hand _RESULT_
- erase others' stuff _RESULT_

### undo/redo
- undo draw _RESULT_
- undo own drawings _RESULT_
- undo erase _RESULT_

### Can use settings 
- auto-initialize height _RESULT_
- height calibration _RESULT_
- height calibration info appears in the beginning _RESULT_
- height calibration info goes away after success _RESULT_
- height adjustment _RESULT_
- height offset preserve after teleport _RESULT_

### Starting position
- goes to starting location _RESULT_
- height restores _RESULT_

### Can use people
- client list _RESULT_
- client names above head _RESULT_

## Models

### Model Interactions
- Can Grab model _RESULT_
- Grab model pack _RESULT_
- Grab synchronizes _RESULT_
- Grabbing locks for others _RESULT_
- Can Scale model _RESULT_
- Scale model pack _RESULT_
- Scale synchronizes _RESULT_
- Scaling locks for others _RESULT_
- Can Show / Hide _RESULT_
- Synced show / hide _RESULT_
- Synced show / hide for model packs _RESULT_
- Can lock model _RESULT_
- Can lock model pack _RESULT_
- Grab lock vs UI lock _RESULT_

## Connection
- Avatars disappear upon leaving or disconnecting _RESULT_

### Catches Up State Upon (Re-)Entry
- Auto connect _RESULT_
- Turn off network connection and turn back on _RESULT_
- Get Joined to Session _RESULT_
- `[!]` when disconnected from server _RESULT_
- `[!]` when disconnected from room (session) _RESULT_
- Client disappears from others' worlds _RESULT_
- Client starts moving again on auto-reconnect _RESULT_
- Avatar names _RESULT_
- Avatar people _RESULT_
- Avatar poses _RESULT_
- Model poses _RESULT_
- Model pack poses _RESULT_
- visibility for models _RESULT_
- visibility for model packs _RESULT_
- lock for models _RESULT_
- lock for model packs _RESULT_
- scale for models _RESULT_
- scale for model packs _RESULT_
- “You just joined” own _RESULT_
- “Someone just joined” others _RESULT_
- “You just left” own _RESULT_
- “Someone just left” others _RESULT_
- Close Connection and Rejoin simulated offline _RESULT_
- Drawing poses, color _RESULT_

# Spectator Tests

- Person      NAME
- Device      PC_MAC_LINUX
- Browser     DESKTOP_BROWSER

## Testing WebXR Client

### General 
- Renders Correctly _RESULT_

### Sync poses
- Head _RESULT_
- Can Show / Hide _RESULT_
- Synced show / hide _RESULT_
- Can Lock _RESULT_
- Synced single-object lock _RESULT_
- Synced multi-object lock _RESULT_
- Lock/Unlock Show/Hide syncs on user click only _RESULT_

### Network connection
- Display session name _RESULT_
- Display runtime app and build _RESULT_
- Display server name _RESULT_
- Display session number _RESULT_
- Connect to sync name space _RESULT_
- Display Sync ID _RESULT_
- Display ping/pong _RESULT_
- Display client names _RESULT_
- No extra clients _RESULT_

### Reconnection
- Close connection and rejoin _RESULT_
- Leave and rejoin _RESULT_

### Spectator-only menu
- Height calibration and up/down are hidden for spectator mode _RESULT_
- Create panel is hidden for spectator mode _RESULT_
- Settings panel shows instructor menu button _RESULT_
- Closing settings panel closes instructor menu _RESULT_
- Pressing back closes instructor menu _RESULT_
- Capture button is available _RESULT_
- Capture button sends start_recording event _RESULT_
- Capture button can change between Start and Stop _RESULT_
- Capture button sends end_recording event _RESULT_

### Can use keyboard controls 
- rotate (`Q` `E` `2` `3` or Left Mouse Drag) _RESULT_
- strafe (`W` `A` `S` `D`) _RESULT_
- hyperspeed scroll (Mouse Scroll) _RESULT_

### Can use mouse controls
- click-drag to rotate view _RESULT_
- pan (Middle Mouse Drag) _RESULT_
- right-click (no dragging yet) to teleport _RESULT_

**Capture**
- capture button starts capture _RESULT_
- capture button stops capture _RESULT_
### Capture file
- Capture file includes events for pressing settings, people, interact, create tabs _RESULT_
- Capture file includes events for calling out the VR menu _RESULT_

### Capture output
- Capture file is valid _RESULT_
- Interactions are included (type "interaction") _RESULT_
- Sync Transforms (Poses, type "position") are included _RESULT_
- Sync Drawings (type "draw") are included _RESULT_
- All Drawing Messages are well-formed _RESULT_
- Stroke ID is well-formed _RESULT_
___
# Issues

## Links to Issues
- LINK_TO_ISSUE_WITH_NAME_OF_ISSUE

## Bugs
- STEPS_TO_REPRODUCE
  - EXPECTED_RESULT
  - ACTUAL_RESULT

## Workarounds
- BUG
  - WORKAROUND
___

# How to use this file

## Filling out test results

* Replace `_RESULT_` with any of the valid results from the template below
* Take out the period in the middle of the word (it's just there to make sure Find and Replace doesn't edit the template)

## Writing bugs

* Temporarily write bugs in the bulleted list
* Transfer bugs to issues on the appropriate repo
* Link to the issue in the section "Links to Issues"

## Writing workarounds

* Explain the bug
* Explain how to work around it

## Transferring new and deleted tests to the template

* Use a text editor with a diff or compare function to easily copy the new tests (or delete tests) without copying the test results.
* Keep the test name to one line, and write details on how to execute the test in the Komodo Manual

## Template

```
# Template
## [Section]
### [Subsection]
\[Test\] [RESULT]

where RESULT = …
- P.ASS 
- F.AIL / NETF.AIL
- D.EFER / NETD.EFER
- W.ORKAROUND / NETW.ORKAROUND
- N./T 
```
