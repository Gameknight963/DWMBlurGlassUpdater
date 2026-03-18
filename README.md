# DWMBlurGlassUpdater
I was tired of updating [DWMBlurGlass](https://github.com/Maplespe/DWMBlurGlass) manually so I spent 5 hours automating the 5 minute task




### Usage

 - Download the latest version from [Github Releases](https://github.com/Gameknight963/DWMBlurGlassUpdater/releases/latest).

 - Place the contents of the zip you downloaded one directory above your DWMBlurGlassFolder (which should be named 'Release')

 - Run the exe to update DWMBlurGlass. Make sure to uninstall in the DWMBlurGLass GUI and close the GUI. It ignores the config.ini file by default, so your settings don't get deleted.

### Arguments

Commands:

- ``install latest``: Install the latest stable release
- ``install unstable``: Install the latest unstable (pre-release) version
- ``install <version>`` Install a specific version, e.g. 2.3.1
- ``check`` Print the latest version tag from GitHub
- ``--help, -h, /?``: Show help message
- ``--version, -v, /v ``: Print updater version

Flags:

- ``--no-pause``: Do not wait for Enter after completion
- ``--silent``: Suppress output messages (implies --no-pause)
- ``--hard``: Fully delete the previous installation

warning: if you don't use ``--hard`` when updating between certain versions, it can cause dwm to crash loop
