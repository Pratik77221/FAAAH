# FAAAH 🔊

**Plays a custom audio clip whenever an error or exception appears in the Unity Console.**

Editor-only — does not run in builds. Auto-initializes after every script reload.

## Installation

**Unity Package Manager → Add package from git URL:**

```
https://github.com/AshwinMPaii/FAAAH.git
```

Or clone this repo and use **Add package from disk...** → select `package.json`.

## Usage

- Works out of the box. Errors trigger the sound automatically.
- Toggle on/off: **Tools → FAAAH → Enabled**
- State persists across sessions via `EditorPrefs`.

## Features

- Triggers only on **errors**, **exceptions**, and **asserts** — not warnings or logs
- Prevents overlapping playback on rapid errors
- Compatible with Unity 2020.3+

## License

MIT
