# FirefoxRealityPC
Firefox Reality for PC-connected VR platforms

## About
Firefox Reality PC Preview (FxRPC) is a new VR web browser that provides 2D overlay browsing alongside immersive apps and supports web-based immersive experiences for PC-connected VR headsets.

This experimental browser consists of two components:
- A Launcher written in Unity, `FirefoxReality.exe`. This is responsible for launching a bundled, custom build of Firefox (for Preview or Local builds) or for downloading/installing Firefox. The first run of the launcher also has additional operations for set up, such as creating an FxRPC-specific profile and installing VR-specific webextensions.
- A custom build of Firefox with additional VR code. This build of Firefox is forked from the latest Release build in [this repo](https://github.com/MozillaReality/gecko-dev). This fork is provisional for faster prototyping and development until all of the code in this branch is landed in Firefox Nightly and shipped in a release build of Firefox.

## Building
Building FxRPC involves building both components mentioned above, which also involves installing tools for both components.

### Firefox
Comprehensive instructions for building Desktop Firefox on Windows is available [here](https://firefox-source-docs.mozilla.org/setup/windows_build.html).<br/>
The only divergence from the instructions is to account for Git instead of Mercurial:
- In the section ["Getting the source"](https://firefox-source-docs.mozilla.org/setup/windows_build.html#getting-the-source), please clone instead from the Github fork:
```
    git clone https://github.com/MozillaReality/gecko-dev.git
```
- Ensure that git is in your path so that you can make git commands in the mozilla build environment.

**Note**: After building, you will need the path to the folder that contains `firefox.exe` for the Launcher. To find this path, run
```
    ./mach environment
```
to find the object directory. The executable will be in the subdirectory `dist\bin`

### Launcher and Package
1. Install Unity Hub: https://docs.unity3d.com/Manual/GettingStartedInstallingHub.html
2. Install 2019.3.0b9, with Windows Build Support: [unityhub://2019.3.0b9/de32b4c0dd7a](unityhub://2019.3.0b9/de32b4c0dd7a)
3. Clone this repo
4. In Unity Hub, open this project in subfolder `Source\FirefoxRealityUnity` with Unity version 2019.3.0b9
5. If Unity warns about version matching, click 'Continue'
6. After the project loads, there will be a custom menu item named 'FxR' (between Component and Window). Click on 'Windows Build' inside that menu.
7. In the following prompts, select the folder that contains `firefox.exe` (see Note in section above), a folder for the profile, and a folder for the build output. The title of the dialogs indicate which type folder to select.

## Running
To run FxRPC, simply launch `FirefoxReality.exe` from the output folder in step 7 above.
