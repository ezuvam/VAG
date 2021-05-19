# VAG for Virt-A-Mate Plugin

## What is VAG
VAG stands for Virt-A-Game.

VAG is a plugin for VAM (Virt-A-Mate) to create games (multiple scenes) with different quests and branching dialogs that take place in different locations.

## Features

- Multiple locations (scenes). (not implemented yet, only one location supported for now)
- Multiple places in each location. You can think of a place as a stage where VAG switches the required atoms on/off. So the story can change places (inside the same location) extremly fast
- Multiple character (NPC's)
- Wardrobe for each character
- Moods for each character
- Action system to execute actions. e. g. change wardrobe, change place, change mood, calling vam-triggers, interacting with other VAM-Plugins...
- Branching dialogs with multiple choices. Each choice can have many actions.
- Branching quests and child quests
- Configurable transitions. eg. to dim all lights during place change progress
- Simple atom modifier. eg. set on/off, set position
- Game-Defintions in external JSON file.
- Savegame support. So the player can save and restore his progress.
- External Game Designer Editor.

## License

[MIT](LICENSE.md)
