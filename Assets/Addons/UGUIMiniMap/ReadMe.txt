Thanks for purchase UGUI MiniMap addon for MFPS 2.0
Version 2.4.2

For documentation and tutorials go to (in Unity Editor Toolbar) MFPS -> Tutorials -> MiniMap
or see the online documentation (DEPRECATED): http://lovattostudio.com/documentations/ugui-minimap/

Get Started:

-After import the package in your project you only need add an layer to get ready.
in the LayerMask List (Edit -> Project Settings -> Tags And Layers -> Layers -> *) add a new layer in the field number 10 called 'MiniMap'.
that's, check the full documentation for details.

MFPS INTEGRATION:

- for integrate in MFPS simple go to MFPS -> Addons -> MiniMap -> Enable
- Wait until script compilation finish and then go to MFPS -> Addons -> MiniMap -> Setup Players
- Add one of the MiniMap prefabs (Addons -> UGUIMiniMap -> Content -> Prefabs) in the room scene (see the documentation for more details)
- Ready!

Any problem or question, feel free to contact us:

Contact Form: http://www.lovattostudio.com/en/support/
Forum: http://lovattostudio.com/forum/index.php

If you have a problem or bug, please contact us before leave a bad review, we respond in no time.

Change Log:

2.4.2
Fix: Enemies icon shows in the minimap even when they aren't firing after the second spawn.

2.4.1
Fix: Minimap rotation jump effect when continuing rotating.

2.4.0
Compatibility with MFPS 1.9

2.3.3
Compatibility with MFPS 1.8

2.3.2
Fix: MiniMap 3D prefab was broken.
Improve: Hide the minimap when player dies and show again until player respawn.

2.3
Improve: Overall performance.
Add: Custom update frame rate you can set a custom frame rate to update the minimap icons, useful for mobile platforms.
Fix: Circular minimap compass was showing the coordinates inverted.

2.2.1
Fix: Player setup integration set Player Selector players as bots.

2.2 (03/06/2020)
Improve: Integrated with Kill Streaks addon.
Improve: Added option to display zoom buttons only on full screen map.
Fix: Teammate bots icons were not showing for local client at start.
Improve: Now include the all the player selector prefabs when the addon is enabled.

2.1.4
Improve: Now you can turn off the option to show enemy's in minimap when those shoot.

2.1.3
Fix: isEnterInGame doesn't exist error in MFPS 1.6
Fix: Compass doesn't work

2.1
-Improve: Update the documentation and integrate with Unity Editor (Windows -> MiniMap -> Documentation)
-Improve: Screen shot tool system.
-Improve: Editor inspector scripts.
-Improve: Now can add the player icon in bl_MiniMap -> Render Settings -> Player Icon, instead search the Image Component for add it.
-Improve: You not longer need to assign the Item prefab for each bl_MiniMapItem script that you set up, this will take automatically.
-Improve: Integration with MFPS
-Improve: Automatically integration now include the bots prefabs.

2.0
-New: Now can get an world position from the minimap.
-Add: World Point Markers, player click one point on the mini map and a marker will appear on the world map, it will disappear when player yet close to it.
-Improve: Clean code and improve performance.
-Improve: Add custom inspector for bl_MiniMap, now is easy to find and understand the settings.