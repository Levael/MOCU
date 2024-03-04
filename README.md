---

# MOCU

## Overview

MOCU (Moog Oculus Cedrus Unity) is an integrated system designed for the efficient integration and management of various devices such as robotic systems (e.g., Moog) and virtual reality headsets. At its core is a Unity-based project that encompasses the primary user interface and control logic, ensuring convenience and flexibility for laboratory experiments. To extend its functionality, MOCU utilizes add-ons in the form of daemons - standalone executable files intended for Windows 64-bit systems, which cover specialized processes.

## Project Structure

- **UnityCore**: The central Unity project containing the user interface and main control logic. It is the key component for managing experiments.

- **AudioControl**: A separate project for managing everything related to audio. Compiled separately and operates as a daemon.

- **PortFinder**: A tool for identifying device ports by their ID. Also compiled separately and functions as a daemon, facilitating the connection and setup of devices.

## Getting Started

For researchers and those conducting experiments, MOCU offers a ready-to-use application, fully compiled and available for download and immediate use. This ensures the system can be quickly deployed for research purposes, even without Unity installed on your computer.

To launch the MOCU system, simply use the `START_MOCU_newest_version.bat` script. This script automatically configures and starts all necessary components of the system, providing a hassle-free beginning to your experiment.

---
