# The Legend of Zelda: Twilight Princess Randomizer (Decomp Base)

A work-in-progress experimental mod that aims to randomize Twilight Princess.

**Note: This is not meant to compete with the REL Randomizer and is a seperate project, although work may be shared between them. No feature suggestions will be accepted for the time being.**

The code for the GameCube releases is completely matching. However, not every Translation Unit (TU) has been linked yet. Work is continuing by aligning the Debug version and getting the Wii versions to match. All versions are built from the same codebase using conditional compilation.

> [!IMPORTANT]
> This repository does **not** contain any game assets or assembly whatsoever. An existing copy of the game is required.
>
> This project itself **is not**, and will not, produce a port, to PC or any other platform.

The project can target the following supported versions:

- **`GZ2E01`**: GameCube - North America
- **`GZ2P01`**: GameCube - Europe/Australia
- **`GZ2J01`**: GameCube - Japan
- `RZDE01_00`: Wii - North America (Rev 0)
- `RZDE01_02`: Wii - North America (Rev 2)
- `RZDP01`: Wii - Europe/Australia
- `RZDJ01`: Wii - Japan
- `Shield`: Nvidia Shield - China
- `ShieldD`: Nvidia Shield - China (Debug Version)

<!--ts-->

- [Dependencies](#dependencies)
- [Building](#building)
- [Diffing](#diffing)
- [Contributing](#contributing)
- [FAQ](https://zsrtp.link/about)

## Dependencies

You will need the following dependencies:

- git
- ninja
- python3
- clang-format (optional)

### Windows

On Windows, it's **highly recommended** to use native tooling. WSL or msys2 are **not** required.

- Install [Python](https://www.python.org/downloads/) and add it to `%PATH%`.
  - Also available from the [Windows Store](https://apps.microsoft.com/store/detail/python-311/9NRWMJP3717K).
- Install Lago's gclib tools:

  ```
  pip install "gclib[speedups] @ git+https://github.com/LagoLunatic/gclib.git"
  ```

- Install the latest version of [.NET](https://dotnet.microsoft.com/en-us/download)
- Download [ninja](https://github.com/ninja-build/ninja/releases) and add it to `%PATH%`.
  - Quick install via pip: `pip install ninja`

### macOS

- Install [ninja](https://github.com/ninja-build/ninja/wiki/Pre-built-Ninja-packages):

  ```sh
  brew install ninja
  ```

- Install Lago's gclib tools:

  ```
  pip install "gclib[speedups] @ git+https://github.com/LagoLunatic/gclib.git"
  ```

- Install the latest version of [.NET](https://dotnet.microsoft.com/en-us/download)

[wibo](https://github.com/decompals/wibo), a minimal 32-bit Windows binary wrapper, will be automatically downloaded and used.

### Linux

- Install [ninja](https://github.com/ninja-build/ninja/wiki/Pre-built-Ninja-packages).
- Install Lago's gclib tools:

  ```
  pip install "gclib[speedups] @ git+https://github.com/LagoLunatic/gclib.git"
  ```

- For non-x86(\_64) platforms: Install wine from your package manager.
  - For x86(\_64), [wibo](https://github.com/decompals/wibo), a minimal 32-bit Windows binary wrapper, will be automatically downloaded and used.

- Install the latest version of [.NET](https://dotnet.microsoft.com/en-us/download)

## Building

- Clone the repository:

  ```sh
  git clone https://github.com/zsrtp/decompimizer.git
  ```

- Copy your game's disc image (ISO) to `orig/GZ2E01/GZ2E01.iso` (or appropriate version, e.g., `orig/GZ2P01/GZ2P01.iso`).
  - Supported formats: ISO (GCM), RVZ, WIA, WBFS, CISO, NFS, GCZ, TGC.
  - This is used for both asset extraction and rebuilding the final ISO.

- Configure and build:
  - To build an ISO with custom code changes, run:

  ```sh
  python configure.py -v GZ2E01
  ninja
  ```

  - Replace `GZ2E01` with your desired version (e.g., `GZ2P01` for PAL)
  - Currently supported versions: `GZ2E01`, `GZ2P01`, `GZ2J01`
  - To build an ISO with custom archive changes, run:

  ```sh
  dotnet run --project ./Generator/Generator.csproj
  ```

## Adding Custom Code

To add new features to the randomizer:

- Create new .cpp or .h files in appropriate directories (e.g., `src/rando/` for randomizer-specific custom code)
- Add them to `configure.py` in the `config.libs` list, marked as `Custom`
- Look at existing custom files in `src/rando/` for examples of how to structure and integrate new code
- Re-run `python configure.py -v VERSION` and `ninja` to include your changes

## Adding Custom Assets

- Place your custom asset into the `mod_assets` directory. The directory structure will be copied 1:1 to the final disc image.
- Re-run the build process above to include your changes
