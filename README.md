<p align="center">
  <img src="https://github.com/fugodev/TenkaiMenu/blob/main/assets/TenkaiMenuBanner.png?raw=true">
</p>

<p align="center">
  <a href="https://discord.gg/rJz747UQhG">
    <img hspace="6" src="https://img.shields.io/badge/Join%20Us%20on-Discord-blue?style=flat&logo=discord" alt="Discord">
  </a>
</p>

<p align="center">
  <b>An open-source, Unity-based Among Us mod menu with a clean GUI and powerful game modules.</b>
</p>

<!-- omit in toc -->
# 🧭 Quick Navigation

- [📦 Download Versions](#-download-versions)
- [🔧 Installation Guide](#-installation-guide)
  - [🪟 Windows Setup](#-windows-setup)
  - [🐧 Linux / SteamOS](#-linux--steamos)
- [🔥 Menu Features](#-menu-features)
- [💬 Troubleshooting & FAQ](#-troubleshooting--faq)
- [📜 Credits](#-credits)
- [⚠️ Disclaimer](#%EF%B8%8F-disclaimer)

---

# 📦 Download Versions

| Mod Version | Among Us - Version | Link |
|-------------|--------------------|------|
| v1.0.0 | 17.4 ( 2026.06.05 ) | [Download](https://github.com/fugodev/TenkaiMenu/releases/tag/v1.0.0) |

# 🔧 Installation Guide

## 🪟 Windows Setup

1. Download the latest **TenkaiMenu zip pack** from [here](https://github.com/fugodev/TenkaiMenu/releases/latest).
    - **For Steam or Itch.io:** Download `TenkaiMenu-v1.0.0-Steam-Itch.zip`.
    - **For Microsoft Store, Epic Games Store, or Xbox App:** Download `TenkaiMenu-v1.0.0-MicrosoftStore-EpicGames-XboxApp.zip`.

2. Open the zip file you just downloaded and copy all of its contents.

3. Paste these files directly into your Among Us game folder:
    - **Steam:** Right-click Among Us in your Library → Click `Manage` → Click `Browse local files`.
    - **Epic Launcher:** Right-click Among Us in your Library → Click `Manage` → Click the folder icon in the `Installation` box.
    - **Itch.io:** Open the Itch.io app → Right-click Among Us in your Library → Click `Manage` → Click `Open folder in Explorer`.
    - **Microsoft Store:** Open the folder where Windows apps are installed (typically `C:\Program Files\WindowsApps\`) by following the tutorial [here](https://youtu.be/qCeoEIy_vrw) → In File Explorer, search for `Among Us.exe` → Right-click the result → Select `Open file location`.
    - **Xbox App:** Right-click Among Us in your Library → Click `Manage` → Open the `FILES` tab → Click `BROWSE...` → Open the `Among Us` folder → Open the `Content` folder.

4. Launch Among Us as you normally would. You should see a console window appear, installing the mod's requirements.

5. Wait for the console window to finish the installation.

6. After installation, Among Us will automatically open with **TenkaiMenu** successfully installed!
    - By default, you can toggle the cheat GUI on and off by pressing **DELETE** on your keyboard.

7. If the installation doesn't work, check out our [FAQ](#-troubleshooting--faq).

## 🐧 Linux / SteamOS

1. Run Among Us under **Proton (or Wine)**.
   - **In Steam:** Right-click Among Us in your Library → Click `Properties` → Click `Compatibility` → Enable `Force the use of a specific Steam Play compatibility tool`.
   - Test different Proton versions if you're having issues launching the game.

2. Set up **BepInEx** (the framework TenkaiMenu is built upon).
   - Follow the official Proton / Wine setup guide found [here](https://docs.bepinex.dev/articles/advanced/proton_wine.html).
   - If you are using Proton with Steam, specify the DLL override:
     - **In Steam:** Right-click Among Us in your Library → Click `Properties` → Click `General` → Click `Launch Options`.
     - Add this to your launch options:
       ```
       WINEDLLOVERRIDES="winhttp.dll=n,b" %command%
       ```
   - After that, continue with the Windows installation steps found [here](#-windows-setup).

3. Fix crashes or errors (like `Unable to execute IL2CPP chainloader`).
   - **In Steam:** Right-click Among Us in your Library → Click `Properties` → Click `General` → Click `Launch Options`.
   - Set your launch options to:
     ```
     PROTON_NO_ESYNC=1 PROTON_USE_WINED3D=1 WINEDLLOVERRIDES="winhttp.dll=n,b" %command%
     ```

# 🔥 Menu Features

<img alt="image" src="https://fugodev.dev/static/images/FabIngame.png">

- An intuitive, custom GUI packed with our latest Among Us cheats
- See ghosts & reveal the impostors instantly
- Track every player's position using the minimap
- Teleport anywhere you want on the map
- Change your role whenever you please (Custom Role Assignments!)
- Remove kill cooldown & spam-kill everyone
- Murder any distant player from across the map
- Unlock all of the game's cosmetics for FREE
- No more annoying disconnect penalties

For a complete list of all of TenkaiMenu's features, click [here](https://github.com/fugodev/TenkaiMenu/blob/main/FEATURES.md).

# 💬 Troubleshooting & FAQ

Click to expand each topic:

<details>

<summary><h2>❗ I'm having issues installing TenkaiMenu</h2></summary>

First of all, make sure you are running the most recent version of Among Us (`17.4` / ` 2026.6.5`) with **TenkaiMenu** (`v1.0.0`).

Also, check if your platform is officially supported:

- ✅ Steam
- ✅ Itch.io
- ✅ Epic Games Launcher
- ✅ Microsoft Store
- ✅ Xbox App
- ❔ Cracked (50/50)
- ❌ iOS App Store & Google Play
- ❌ PS & Switch & Xbox Console

Now ensure that you have downloaded the correct zip file for your platform:
- **For Steam or Itch.io:** Download `TenkaiMenu-v1.0.0-Steam-Itch.zip`
- **For Microsoft Store, Epic Games Store, or Xbox App:** Download `TenkaiMenu-v1.0.0-MicrosoftStore-EpicGames-XboxApp.zip`

Make sure you followed the installation guide precisely. This is what your `Among Us` folder should look like after a successful installation:

<img src="https://fugodev.dev/static/images/amongUsFolder.png" alt="drawing" width="550"/>

<br>Some antiviruses might cause issues when installing the mod, so consider temporarily deactivating your antivirus if the game isn't booting.

When installing TenkaiMenu for the first time, it will take **MUCH** longer than usual for the game to load. This is completely normal! You can keep track of the installation progress through the BepInEx console window that pops up when you start the game:

<img src="https://fugodev.dev/static/images/bepinexConsole.png" alt="drawing" width="550"/>

<br>If you are still having issues, feel free to open a new GitHub issue [here](https://github.com/fugodev/TenkaiMenu/issues/new), or ask for help in our Discord server: [discord.gg/rJz747UQhG](https://discord.gg/rJz747UQhG)

</details>

<details>

<summary><h2>👾 I found a bug OR I would like to suggest a new feature</h2></summary>

To let me know, you can open a new GitHub issue [here](https://github.com/fugodev/TenkaiMenu/issues/new), or you can discuss it with the community on our Discord server: [discord.gg/rJz747UQhG](https://discord.gg/rJz747UQhG). All contributions and ideas are welcome!

</details>

<details>

<summary><h2>👨‍💻 I want to contribute to this project</h2></summary>

To get started, I suggest you first learn the basics of C# and Unity, since that's what Among Us is written in. 

You should also learn about GitHub forking and pull requests, since you will need to use those to make any contributions to the project. [Here](https://docs.github.com/en/get-started/exploring-projects-on-github/contributing-to-a-project) is the official documentation on the topic.

In this project, I use BepInEx and Harmony to patch the game. I highly suggest taking a look at [this](https://docs.reactor.gg) great guide to learn how to work with those frameworks.

Here are some other useful resources:
- The [Reactor](https://reactor.gg/discord) Discord server (A great community of Among Us modders).
- [sus.wiki](https://github.com/roobscoob/among-us-protocol) (Useful resource to learn more about the Among Us network protocol, though slightly outdated).

</details>

# 📜 Credits

* **[MalumMenu](https://github.com/scp222thj/MalumMenu)** - Built upon and inspired by the original open-source MalumMenu project. Special thanks to the original developers for their foundational architecture and RPC implementation!
* **[SickoMenu](https://github.com/g0aty/SickoMenu)** - Credit for module feature integrations and custom utility code.
* **[BanMod](https://github.com/GiannBart/BanMod)** - Credit for module feature integrations and custom utility code.
* **[BepInEx](https://github.com/bepinex/bepinex)** - Modding framework and runtime patching tools.

---

# ⚠️ Disclaimer

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

This mod is not intended to be used in any manner that interferes with Innersloth's services, Innersloth's operation of Among Us, the integrity or availability of the game, or the normal gameplay experience of other players. The creator does not endorse, encourage, or condone using this mod to disrupt games, negatively affect other users, bypass rules or protections, or gain an unfair advantage in any setting where such use is prohibited. Any misuse is solely the responsibility of the user. Usage of this mod can violate the terms of service of Among Us, which may lead to punitive action. The creator is not responsible for any consequences you may face due to usage. Use at your own risk.

