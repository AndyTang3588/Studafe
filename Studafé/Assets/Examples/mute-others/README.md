# Mute Others

## Description
This example shows how to mute and unmute other Players.

## How to Use This Example

Open the `mute-others` scene to first test it out in the Unity Editor, or visit [this world](https://vrchat.com/home/world/wrld_6b2ed4f1-09be-4dee-91b2-fc49e12ecb88) in the VRChat Client.
This example requires TextMeshPro, a window will show offering to "Import TMP Essentials" if you don't already have TextMeshPro in your project. Accept this offer and re-open the scene after it's done importing.

Press the button labeled "Mute Players" to mute all the other players who are in the instance with you, and change the button label to "Unmute Players". Press the button again to restore the other players' voices and the original "Mute Players" label.

## How it Works

There is a single U# program on the "PlayerMuteLogic" GameObject which contains all the logic for the demo.

### MuteOthers.cs

This program has a boolean `_isMuted` variable which is toggled whenever the `_Trigger()` function is called from the MuteButton in the scene.
After flipping the value of this variable, `OnMuteUpdated()` is called, which fetches a list of all the Players in the instance, and then changes their [VoiceDistanceFar](https://creators.vrchat.com/worlds/udon/players/player-audio#set-voice-distance-far) to effectively mute and unmute them. When this value is set to 0, it means that other player voices will travel 0 meters before they are silent. When unmuted, the value is restored to `_defaultVoiceDistance` which is 25 unless you change it in the inspector, so their voices can be heard for the default distance of 25 meters.
This function also changes the text on the button, which is referenced as `_buttonLabel`.
The messages to display for each state are easily changeable in the inspector, as `Message Muted` and `Message Unmuted`.

#### Variables
* Button Label - a reference to the label of the button to change its message, should be already set properly.
* Message Muted - the text shown on the button while other players are muted, default is "Unmute Players"
* Message Unmuted - the text shown on the button while other players are unmuted, default is "Mute Players"
* Default Voice Distance - the value that will be set for all player voice distances when they are unmuted, default is 25, which is the platform-wide default.

## Challenge

Can you update this prefab to only mute some players? One approach would be to do this based on where players are, for a jump-start on that you can check out the [PlayerJoinZones](https://ask.vrchat.com/t/player-join-zones/26642) example, which creates a collection of players based on whether they are in a Trigger area.