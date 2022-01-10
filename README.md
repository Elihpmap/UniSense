UniSense: DualSense plugin for Unity InputSystem
====
---
> This is a forked version I'm currently trying to improve, it is currently more complete than the original but I intend to work on it a little more before sending a Pull Request (See Unreleased section of the CHANGELOG.md file for more details).
---

This plugin enables Unity apps to use DualSense, the controller for PS5 in Windows.


This project is incomplete state for now. For example:

- the documentation including this README is work in progress.
- The code is yet to be optimized.

Any contribution is appreciated. Please do not hesitate to send PR if you have any proposal regarding this project.

## Requirements

- Unity 2019.1 or above
- Input System [v1.0.1](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/changelog/CHANGELOG.html#101---2020-11-20) or above

## Installation

You can install this package using Unity Package Manager (UPM).

1. On Unity Editor, open UPM window by going to Window > Package Manager
2. In this window top left corner, click on the **+** button and select **Add package from git URL...**
3. Enter this url ```https://github.com/nullkal/UniSense.git``` (or ```https://github.com/Elihpmap/UniSense.git``` for this forked version)

## Samples

A main sample is available. There, you can see how to use the DualSense main features.

On the UPM windows, select **Input System-UniSense** package, go to the Samples section and import the **DualSense Main Sample**.

## Special thanks

- [Ohjurot/DualSense-Windows](https://github.com/Ohjurot/DualSense-Windows)
- [DualSense Haptics, LEDs and more (HID Output Report) in /r/gamedev](https://www.reddit.com/r/gamedev/comments/jumvi5/dualsense_haptics_leds_and_more_hid_output_report/)

Added sources : 
- [Pre deletion save of DualSense Haptics, LEDs and more (HID Output Report) in /r/gamedev](https://gist.github.com/stealth-alex/10a8e7cc6027b78fa18a7f48a0d3d1e4)
- [Game Controller Collective Wiki => Sony Dualsense](https://controllers.fandom.com/wiki/Sony_DualSense#FFB_Trigger_Effect_Factories)
- [Nielk1 trigger effect generators](https://gist.github.com/Nielk1/6d54cc2c00d2201ccb8c2720ad7538db)