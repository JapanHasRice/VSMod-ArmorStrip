# VSMod-DoffAndDonAgain

## Latest Update
- Vintage Story v1.18.x compatible.
- Built-in compatibility with the *Mr. Mannequin* mod.
  - Default settings only allow the exchange of clothing.
- Worldconfig: Servers (and singleplayer worlds) can set the mod's behaviors in worldconfig.
  - When creating a new world, the Customize screen will display the worldconfig options for *Doff and Don Again*. (Note: please read the Known Issues section)
  - Edit *Doff and Don Again*'s behavior for the current world via the `/worldconfig` vanilla command. See below for a list of keys and their descriptions.
  - CONFIGS FROM PREVIOUS VERSION OF THE MOD ARE IGNORED. PLEASE CHECK YOUR SETTINGS.
- ClientConfig: Clients can adjust their personal preferences for *Doff and Don Again* in `../VintagestoryData/ModConfig/DoffAndDonAgain/ClientConfig.json`
  - Allows individual players to tweak *Doff and Don Again*'s behavior while still operating within the Server's rules.
  - If a Server's (or singleplayer world's) worldconfig is more restrictive than the client's settings, the Server's settings will be used.
  - These settings will follow you between worlds, singleplayer or multiplayer.
- New `doffanddonnable` entity behavior to customize what *Doff and Don Again* can interact with.

## Description
Out spelunking and need to patch up your wounds but you're wearing full plate? *Doff* it! Drop all that armor at the touch of a button (or two, default: Ctrl + U).

Back at base and want to get comfortable quickly? Target an armor stand and *Doff*! Any equipped armor that can be placed on the armor stand will be.

Woke up to a temporal storm in the middle off the night and you forgot to wear protection to bed? Run to your armor stand and *Don* your armor! All armor from the stand will be equipped to any empty armor slots you have (default: U).

There's also *Swap*! Exchange your equipped armor with an armor stand's. (default: Shift + U)

*Doff and Don Again* is configurable to suit your playstyle, disable specific actions, adjust saturation costs, number of free hands required to act, etc.

## Known Issues
### The settings on the Customize screen are unreadable
When creating a new world and viewing the Customize screen, the settings for *Doff and Don Again* are not translated. This is due to an issue with how language files are loaded in Vintage Story that Tyron is aware of.

WORKAROUND: Load an existing world while *Doff and Don Again* is installed and exit back to the main menu. When creating a world, the settings will be translated.

### There are no options to set the Saturation Cost or Number of Hands Needed
When creating a new world and viewing the Customize screen, these two settings are not shown. The settings are hidden due to (reported) bugs in Vintage Story.

WORKAROUND: Once the world is created use `/worldconfig doffanddonagainSaturationCost` to set the amount of saturation taken per action or `/worldconfig doffanddonagainHandsNeeded` to set how many empty hands are needed to act.

## Worldconfig
### General Settings
- `/worldconfig doffanddonagainAllowArmorStandArmor [true|false]`

`true`: Allow interaction with the armor placed on vanilla Armor Stands.
`false`: Doff and Don Again actions will ignore armor on Armor Stands.

Default: true.

- `/worldconfig doffanddonagainAllowArmorStandHands [true|false]`

`true`: Allow interaction with items placed into the hands of vanilla Armor Stands.
`false`: Doff and Don Again actions will ignore items in the hands of Armor Stands.

Default: true

- `/worldconfig doffanddonagainSaturationCost [0 ...]`

The amount of Saturation consumed when performing an action.

Default: 0, accepts any positive numeric value.

- `/worldconfig doffanddonagainHandsNeeded [0, 1, 2]`

The number of empty hands needed to perform an action.

Default: 2

### *Mannequin Stand* Settings
- `/worldconfig doffanddonagainAllowMannequinArmor [true|false]`

`true`: Allow interaction with the armor placed on Mannequins from the *Mannequin Stand* mod.
`false`: Doff and Don Again actions will ignore armor on Mannequins.

Default: false

- `/worldconfig doffanddonagainAllowMannequinClothing [true|false]`

`true`: Allow interaction with the clothing placed on Mannequins from the *Mannequin Stand* mod.
`false`: Doff and Don Again actions will ignore clothing on Mannequins.

Default: true

- `/worldconfig doffanddonagainAllowMannequinHands [true|false]`

`true`: Allow interaction with items placed into the hands of Mannequins from the *Mannequin Stand* mod.
`false`: Doff and Don Again actions will ignore items in the hands of Mannequins.

Default: false

- `/worldconfig doffanddonagainAllowMannequinBackpack [true|false]`

`true`: Allow interaction with backpacks placed on Mannequins from the Mannequin Stand mod.
`false`: Doff and Don Again actions will ignore backpacks on Mannequins.

Default: false

### Doff Settings
- `/worldconfig doffanddonagainDoffArmorToGround [true|false]`

`true`: Doff can drop a player's equipped armor onto the ground.
`false`: Players will not be able to drop equipped armor to the ground.

Default: true

- `/worldconfig doffanddonagainDoffArmorToEntities [true|false]`

`true`: Doff can move a player's equipped armor onto a targeted entity.
`false`: Players will not be able to move equipped armor onto a targeted entity.

Default: true

- `/worldconfig doffanddonagainDropUnplaceableArmor [true|false]`

`true`: When Doffing to an entity, any armor that cannot be placed on the entity can be dropped to the ground.
`false`: Armor that cannot be placed on the targeted entity will remain equipped to the player.

Default: false

- `/worldconfig doffanddonagainDoffClothingToGround [true|false]`

`true`: Doff can drop a player's equipped clothing onto the ground.
`false`: Players will not be able to drop equipped clothing to the ground.

Default: false

- `/worldconfig doffanddonagainDoffClothingToEntities [true|false]`

`true`: Doff can move a player's equipped clothing onto a targeted entity.
`false`: Players will not be able to move equipped clothing onto a targeted entity.

Default: true

- `/worldconfig doffanddonagainDropUnplaceableClothing [true|false]`

`true`: When Doffing to an entity, any clothing that cannot be placed on the entity can be dropped to the ground.
`false`: Clothing that cannot be placed on the targeted entity will remain equipped to the player.

Default: false

### Don Settings
- `/worldconfig doffanddonagainDonArmorFromEntities [true|false]`

`true`: Don can take armor from a targeted entity and equip it to the player.
`false`: Players will not be able to equip armor from entities.

Default: true

- `/worldconfig doffanddonagainDonClothingFromEntities [true|false]`

`true`: Don can take clothing from a targeted entity and equip it to the player.
`false`: Players will not be able to equip clothing from entities.

Default: true

- `/worldconfig doffanddonagainDonMiscFromEntities [true|false]`

`true`: Don can take misc. items from a targeted entity and give them to the player.
`false`: Players will not be able to take misc. items from entities.

Default: true

### Swap Settings
- `/worldconfig doffanddonagainSwapArmorWithEntities [true|false]`

`true`: Swap can exchange armor between a player and a targeted entity.
`false`: Players will not be able to swap armor with entities.

Default: true

- `/worldconfig doffanddonagainSwapClothingWithEntities [true|false]`

`true`: Swap can exchange clothing between a player and a targeted entity
`false`: Players will not be able to swap clothing with entities.

Default: true
