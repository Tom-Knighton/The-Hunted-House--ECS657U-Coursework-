# The-Hunted-House--ECS657U-Coursework-

## Group AJ - The Hunted House (PP 1-1)
A survival/stealth horror game in which you are trying to escape a house, you were kidnapped and brought here. You must call the police and can wait for them to arrive - if you can survive that long...

**! Uses LFS !**
---

### Objectives:
- You have been kidnapped and must escape safely or kill your captors
- You can win by finding all the gate keys and using them to unlock the main gate
- You can also win by killing all the enemies, but they are difficult to kill
- You can find keys around the house, they are randomly spawned, but some will always spawn near bosses.
- You can find weapons around the house to help you.
- After calling the police, you *could* wait until they arrive, but that will take a long time, and the captors might realise what you've done...

### How to play:
- Select difficulty
- By default: use WASD to move and mouse to look
- E to interact
- Left click to attack
- Tab to open/close inventory, 1/2/3/4 to access hotbar slots
- Shift to sprint
- Ctrl to crouch
- Escape to pause
- Space to jump
- You need to escape cell, collect keys and escape, or kill enemies, or survive until police arrive

### Features:
- Three 'bosses' or enemies that will patrol, see you, attack and search for you.
- Various weapons to find and use with varying damage stats
- Two areas, the inside of the house including the basement - and an outside area with fog and stormy rain.
- Rain audio differs between inside and outside
- Footstep sounds
- House layout means you can escape the enemies if you try, but can also sneak around to avoid them.
- Sky changes with the time of day.
- 'Interactable' objects, with a highlight and prompts on screen when you are looking at them.
- Customisable keybindings
- Customisable difficulity settings
- Cut scenes:
    - Start of game
    - After calling police
    - Opening gate
- A HUD showing your health, sprint left, time until you can attack again
- Enemies have their own 3D HUDs showing their health, and have damage popups
- GameManager, AudioManager and UIManager singletons to properly manage relevant data
- Enemies have a complex state machine - with each state a separate MonoBehaviour that can be applied and registered. Enemies also have attachable MonoBehaviour scripts for vision, health etc.


### External assets:
All assets are royalty free and their licenses allow us to use and distribute them in our game for free.
|Type|Use|Link|
|:-|:-|:-|
|Audio|Footsteps| https://pixabay.com/sound-effects/footsteps-on-wood-floor-14735/ and https://www.fesliyanstudios.com/play-mp3/6994|
|Audio|Jumping SFX|https://freesound.org/people/acebrian/sounds/380471/|
|Audio|Background|https://pixabay.com/sound-effects/creepy-soundscape-7036/|
|Audio|Heartbeat| https://pixabay.com/sound-effects/heartbeat-fast-slowdown-31706/|
|Audio|Breathing SFX|https://pixabay.com/sound-effects/heavy-breathing-14431/|
|Audio|Attack/Grunting SFX| https://pixabay.com/sound-effects/not-quite-human-grunts-1-74809/|
|3D Models|Bathroom Set| https://assetstore.unity.com/packages/3d/props/furniture/bathroom-set-interior-263462|
|3D Models|Flower Vases|https://assetstore.unity.com/packages/3d/vegetation/flowers/free-flower-ceramic-vases-187046|
|3D Models|Shed| https://assetstore.unity.com/packages/3d/environments/urban/the-shed-10303|
|3D Models|Toilet|https://assetstore.unity.com/packages/3d/props/furniture/toilet-pbr-148108|
|3D Models|Characters|https://www.mixamo.com/|
|Shader|Outline effect|https://assetstore.unity.com/packages/tools/particles-effects/quick-outline-115488|

Tutorials loosely followed:
- https://www.youtube.com/watch?v=JdGgrMWIknE
- https://www.youtube.com/watch?v=2FTDa14nryI&list=PLfhbBaEcybmgidDH3RX_qzFM0mIxWJa21
- https://www.youtube.com/watch?v=pPcYr3tL3Sc
- https://www.youtube.com/watch?v=2H6hD-rH6wM&t=99s


All other assets, textures, code made by us, excluding basic Unity shapes like capsules etc.

### Builds:
Web GL build: (in /build) https://tom-knighton.github.io/The-Hunted-House--ECS657U-Coursework-/build/

Desktop build: in (/build-desktop)
