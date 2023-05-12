# UAssetGUI
Open-source software for viewing and modifying Unreal Engine 4 game assets by hand.

<img src="https://i.imgur.com/G31NzYA.png" align="center">

## Installation
You can find pre-built binaries of UAssetGUI in the [Releases tab of this repository](https://github.com/atenfyr/UAssetGUI/releases).

## Compilation
If you'd like to compile UAssetGUI for yourself, read on:

### Prerequisites
* Visual Studio 2019 or later
* Git

### Initial Setup
1. Clone the UAssetGUI repository:

```sh
git clone https://github.com/atenfyr/UAssetGUI.git
```

2. Switch to the new UAssetGUI directory:

```sh
cd UAssetGUI
```

3. Pull the required submodules:

```sh
git submodule update --init
```

4. Open the `UAssetGUI.sln` solution file in Visual Studio, right-click on the UAssetGUI project in the Solution Explorer, and click "Set as Startup Project."

5. Right-click on the solution name in the Solution Explorer, and press "Restore Nuget Packages."

6. Press the "Start" button or press F5 to compile and open UAssetGUI.

## Contributing
Any contributions, whether through pull requests or issues, that you make are greatly appreciated.

If you find an Unreal Engine 4 .uasset that display "failed to maintain binary equality," feel free to submit an issue on [the UAssetAPI issues page](https://github.com/atenfyr/UAssetAPI/issues) with a copy of the asset in question along with the name of the game and the Unreal version that it was cooked with.

## License
UAssetAPI and UAssetGUI are distributed under the MIT license, which you can view in detail in the [LICENSE file](LICENSE).
