# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### [Need to be] Added
- Newer trigger force feedback effects (check compatibility depending on the controller updates?)
- Search and implement Bluetooth version?
- Mics parameters implementation

### [Need to be] Fixed
- Internal speaker volume setter (need to find the right flags)
- multiEditing a DualSenseSerializableTriggerState property is impossible if any of their value differ (even if hidden) : probably comes from hasMultipleDifferentValues returns true

### [Need to be] Changed
- Use of UseLegacyRumble bool to a RumbleType enum {LegacyOnly (always sending legacy values even when 0), HDAndPause (discard any motor value but still automaticaly send 0 values whan HapticPaused is true), HDOnly (always discard motor values)}
- Restandardisation of some name (especially TriggerEffects)

### [Need to be reviewed for] Security
- Some values can be changed outside of the plugin as it is mainly using shallow copies


## [0.2.5] - 2022-01-12

### Added
- Crunch, SnapBack and AmplitudeVibration Effect for the triggers (some user accessible parameter still needs to be explicited and not (yet) added to the sample project)
- DynamicResizableRange display in editor
- A simple Utility Component to easily setup and test triggerStates

### Fixed
- Byte bit-per-bit display in editor
- Updated the Sample scene with the newer terms


## [0.2.4] - 2022-01-11

### Added
- VibratingResistance Effect for the triggers (but not (yet) added to the sample project)
- Base for a byte bit-per-bit display in editor for not-yet-tested/understood trigger parameter values (still need debug)

### Changed
- Separated from the define all the code concerning unity editor display (now in a new file called DualSenseEditor.cs)
- United triggerStates parameters with their formatting in DualSenseDefine.cs


## [0.2.3] - 2022-01-10

### Added
- The possibility to set only one triggerState with the SetTriggerState() QuickSet (the other one can now be a null value)
- The bases for new Trigger Effect Types
- Sources and thanks at the end of the README.md

### Fixed
- Corrected DualSenseGamepadHID.FindCurrent() description

### Changed 
- TriggerState Struct is not serializable anymore instead another similar struct : DualSenseSerializableTriggerState, take that role as the original can still be Layout.explicit and thus memory light even with the newer TriggerEffectType


## [0.2.2] - 2022-01-05

### Added
- "ResetResitance" option for triggerState (retract the actuator back instead of leaving it as with the NoResitance state)
- Inspector displaying rules for TriggerState struct

### Changed
- TriggerState struct is now Serializable and not Layout.explicit anymore to ensure unity inspector compatibilty


## [0.2.1] - 2021-12-01

### Added
- Possibility to change the volume of the controller (internal speaker or plugged in device) and access to these parameters in the sample scene. (internal speaker volume seems to be not fully working yet)

### Deprecated
- SetGamepadState method : use UpdateState() or directly set NewState property followed by UpdateGamepad instead.

### Fixed
- Restored the missing method SetGamepadState as deprecated for compatibility with 0.1.0 sample. This will surely be removed for the 1.0.0.
- Bug when setting legacy rumble to 0 while in haptic pause (they used to restart once the pause was over anyway);
- Side of rumble in the sample scene (the right and left used to be switched) 


## [0.2.0] - 2021-11-19

### Added
- Intelligent gamepad Update (limits the Updates to stricktly necessary ones). Still improvable (see "//TODO" comments...)
- Decoupling of gamepadState update (inner memory value) and gamepad update (physical update of the controller according to the gamepadStatestate)
- Possibility to deactivate legacy rumbles to use the HD one (with an external implementation).
- Events called for reset, pause and resume haptics to link a possible HD rumble implementation.
- New component in sample for regular auto updates
- New component in sample to pause and resume haptics (force feedbacks and rumbles)

### Changed
- Pause and resume haptics inner working
- Sample calls for gamepad updates (a lot less to avoid flooding the gamepad with outputReport)

### Fixed
- Bug which prevented legacy rumble or trigger force feedbacks wether the sample was executed in the Unity editor or a build.


## [0.1.0] - ????-??-??

### Added
- Main sample
- Reset functions
- CHANGELOG
- Gyro and Accelerometer
- DualSenseGamepadState struct
- README
- Initial commit

