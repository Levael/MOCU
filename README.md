# MOCU

MOCU (Moog Oculus Cedrus Unity) is a comprehensive control system designed for integration and management of various devices and processes. Its core is a Unity project that encompasses the main user interface and control logic. Additionally, it supports extensions in the form of daemons for specialized tasks, functioning as standalone executable files tailored for Windows 64-bit systems.

## Project Structure

- **UnityCore**: The primary Unity project containing the user interface and main control logic.
- **AudioControl**: A separate project dedicated to audio device management. Compiled separately and operates as a daemon.
- **PortFinder**: A tool for identifying device ports by their ID. Also compiled separately and runs as a daemon.

## Getting Started

To launch the project, use the `START_MOCU_newest_version.bat` script, which automatically configures and initiates all necessary components of the system.
