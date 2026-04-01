# Weather System Implementation

## Overview
Simitone now includes a basic, cosmetic-only weather system.
Available weather is rain, snow, and hail.
It can only be enabled via console commands. Hopefully future updates will have a more natural system.
For now, this is just to get it available for others to play with and use for storyboarding, atmospheric effects, etc.

## How to Use
### Commands (via Ctrl+Shift+C)

- `weather rain [0-100]` - Enable rain (optional intensity, default 50)
  - Plays rain loop sound
  - Scene darkening effect

- `weather storm [0-100]` - Rain with thunder (default 75)
  - All rain effects plus random thunder sounds

- `weather snow [0-100]` - Enable snow (default 50)
  - Snow particles
  - **Grass turns white** (basically like on a snowy Vacation Island lot)

- `weather hail [0-100]` - Hail particles (experimental)

- `weather clear` - Clear all weather
  - Stops sounds
  - Restores terrain

- `weather auto` - Automatic time-based weather

## Sound Assets

- **Rain loop** - Rubaoliva (CC0) - https://freesound.org/s/624645/
- **Thunder** - Raclure (CC0) - https://freesound.org/s/458870/
