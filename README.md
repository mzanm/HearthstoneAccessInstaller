# HearthstoneAccessInstaller

The **HearthstoneAccessInstaller** is a tool designed to simplify the installation of the [Hearthstone Access Community Version](https://hearthstoneaccess.com/). Hearthstone Access is a patch that adds screen reader accessibility to the game **Hearthstone**. This installer streamlines the process with a user-friendly interface, options to choose patch channels, view patch updates, and more.

---

## Features

- Allows users to choose from different patch channels, for example it's possible to install the Duos version of the patch And other channels when they're available.

- Displays detailed channel information, including:
    - Channel name and description.
    - Latest release details, such as changes and upload time.

- View the patch README and changelog after installation.

---

## How to Use

Navigate to the [Latest Release page](https://github.com/mzanm/HearthstoneAccessInstaller/releases/latest/) .

Download and unpack the program archive.

After running the program, it will try to detect your Hearthstone installation directory by:
   - Checking the default location:
     `C:\Program Files (x86)\Hearthstone`
   - Using the `HEARTHSTONE_HOME` environment variable if it is set.
   - If no valid directory is found, a message box will notify you, and you can manually select the directory from the main screen.

### Configuring

The main screen contains the following controls:
- **Install Directory Text Box**:
  Displays the detected Hearthstone directory.

- **Change Button**:
  Opens a folder picker dialog to select the Hearthstone installation directory interactively.

- **Channel List View**:
  Displays available patch channels, with details such as:
  - Channel name and description.
  - Latest release information (changelog and upload time).

### Installing the Patch using the program

1. Select a patch channel from the list. If you're unsure, Choose the stable channel.

2. Click the **Start** button to begin the installation process.
   The installer will:
   - Download the selected patch channel archive.
   - Unpack the archive and update files if necessary.
   - Display options to view the patch README and changelog.

---

## Contributions and Feedback

If you encounter any issues or have suggestions for improvement, feel free to create an issue or submit a pull request.
