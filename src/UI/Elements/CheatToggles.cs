using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AmongUs.GameOptions;
using UnityEngine;

namespace TenkaiMenu;

public struct CheatToggles
{
    // Movement
    public static bool noClip;
    public static bool teleportPlayer;
    public static bool teleportCursor;
    public static bool invertControls;
    public static bool movePlayerNonHost;
    public static bool movePlayerMethod2;
    public static bool invisibility;

    
    public static bool speedHackEnabled;
    public static float gameSpeed = 1f;
    public static bool autoKill;
    public const float autoReportDefaultRange = 1.5f;
    public const float autoReportNormalMaxRange = 7f;
    public const float autoReportAbsoluteMaxRange = 10000f;
    public static bool autoReport;
    public static float autoReportRange = autoReportNormalMaxRange + 1f;
    public static bool confuseNow;

    // Roles
    public static bool setFakeRole;
    public static bool setFakeAlive;
    public static bool noKillCd;
    public static bool showTasksMenu;
    public static bool completeMyTasks;
    public static bool impostorTasks;
    public static bool killReach;
    public static bool killOtherImpostors;
    public static bool killAnyone;
    public static bool endlessSsDuration;
    public static bool spawnMeetingHud;
    public static bool endlessBattery;
    public static bool endlessTracking;
    public static bool noTrackingCooldown;
    public static bool noTrackingDelay;
    public static bool trackReach;
    public static bool interrogateReach;
    public static bool noVitalsCooldown;
    public static bool noVentCooldown;
    public static bool endlessVentTime;
    public static bool endlessVanish;
    public static bool killVanished;
    public static bool noVanishAnim;
    public static bool noShapeshiftAnim;

    // ESP
    public static bool noShadows;
    public static bool seeGhosts;
    public static bool seeRoles;
    public static bool seePlayerInfo;
    public static bool seeDisguises;
    public static bool taskArrows;
    public static bool revealVotes;
    public static bool seeLobbyInfo;

    // Camera
    public static bool spectate;
    public static bool zoomOut;
    public static bool freecam;

    // Minimap
    public static bool mapCrew;
    public static bool mapImps;
    public static bool mapGhosts;
    public static bool colorBasedMap;

    // Tracers
    public static bool tracersImps;
    public static bool tracersCrew;
    public static bool tracersGhosts;
    public static bool tracersBodies;
    public static bool colorBasedTracers;
    public static bool distanceBasedTracers;

    // Chat
    public static bool enableChat;
    public static bool unlockCharacters;
    public static bool bypassUrlBlock;
    public static bool longerMessages;
    public static bool unlockClipboard;
    public static bool lowerRateLimits;
    public static bool chatJailbreak;

    // Ship
    public static bool closeMeeting;
    public static bool closeCurrentDoors;
    public static bool autoOpenDoorsOnUse;
    public static bool unfixableLights;
    public static bool callMeeting;
    public static bool reportBody;
    

    // Sabotage
    public static bool commsSab;
    public static bool elecSab;
    public static bool reactorSab;
    public static bool oxygenSab;
    public static bool mushSab;
    public static bool mushSpore;
    public static bool showDoorsMenu;
    public static bool openAllDoors;
    public static bool closeAllDoors;
    public static bool spamOpenAllDoors;
    public static bool spamCloseAllDoors;
    public static bool sabotageMap;

    // Vents
    public static bool unlockVents;
    public static bool walkInVents;
    public static bool kickVents;
    public static bool spamTpImps;
    public static bool spamTpAll;
    public static bool destroyInGame;
    public static int destroyInGamePlayerId = -1;
    public static bool immortality;
    public static bool reversedSkeld;
    public static bool blockSabotages;
    public static bool disableVents;
    public static bool doorHallucinationAll;
    public static bool ventTPAll;
    public static int ventTPAllVentIdx = 0;
    public static bool followSelectedPlayer;
    public static int followSelectedPlayerId = -1;

    // Animations
    public static bool animShields;
    public static bool animAsteroids;
    public static bool animEmptyGarbage;
    public static bool animMedScan;
    public static bool animCamsInUse;
    public static bool animPet;
    public static bool moonWalk;

    public static bool olAutoStart;
    public static bool olAutoAdapt;
    public static bool olShowRpcTotal;
    public static bool olAutoStop;
    public static bool olLockTargets;
    public static bool olKillSwitch;
    public static bool olPlayerCooldown;
    public static bool olAutoClear;
    public static bool olLogStartStop;
    public static bool olLogAddRemove;
    public static bool olLogDisconnect;
    public static bool olLogAttack;
    public static bool olVerboseLogs;

    // Console
    public static bool showConsole;
    public static bool logDeaths;
    public static bool logShapeshifts;
    public static bool logVents;

    // Host-Only
    public static bool voteImmune;
    
    public static bool showRolesMenu;
    public static bool skipMeeting;
    public static bool forceStartGame;
    public static bool noGameEnd;
    public static bool showProtectMenu;
    public static bool showAssignRoleMenu;
    public static bool alwaysImpostor;
    public static bool noOptionsLimits;
    public static bool ejectPlayer;
    public static bool killPlayer;
    public static bool telekillPlayer;
    public static bool killAll;
    public static bool killAllCrew;
    public static bool killAllImps;
    public static bool levelFarm;
    public static bool glitchLobbyEngine;
    public static bool voteKick;
    public static bool copyOutfit;
    public static bool copyLevel;
    public static bool voteLockEnabled;
    public static bool disableReportsAndMeetings;
    public static bool endGame;
    public static bool destroyLobby;
    public static bool recreateLobby;
    public static bool assignNextRoundRole;
    public static int nextRoundRoleIndex;
    public static bool lobbyDiscoMode;
    public static int seekersCount = 1;

    // Passive
    public static bool unlockFeatures;
    public static bool freeCosmetics;
    public static bool avoidPenalties;
    public static bool copyLobbyCodeOnDisconnect;
    public static bool showLobbyTimer;
    public static bool spoofAprilFoolsDate;

    // Anticheat
    public static bool anticheatEnabled = true;
    public static bool anticheatDetectPlayerLevels;
    public static int anticheatDetectPlayerLevelAbove = 500;
    public static bool anticheatKickPlayerLevels;
    public static int anticheatKickPlayerLevelBelow = 20;
    public static bool flagSpoofedPlatformData = true;
    public static bool anticheatSendNotification = true;
    public static bool anticheatDiscardRpc = true;
    public static int anticheatPunishment;

    // Modes
    public static bool rgbMode;
    
    public static bool panicMode;

    // Config
    public static bool reloadConfig;
    public static bool openConfig;
    public static bool loadProfile;
    public static bool saveProfile;

    // Keybind Map: Toggle Name -> KeyCode (KeyCode.None == No Key)
    public static readonly Dictionary<string, KeyCode> Keybinds = new();

    // Map for Reflection Access: Toggle Name -> FieldInfo
    public static readonly Dictionary<string, FieldInfo> ToggleFields = new();

    // Populate reflection map once at startup and initialize Keybinds with KeyCode.None
    static CheatToggles()
    {
        var fields = typeof(CheatToggles).GetFields(BindingFlags.Static | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (field.FieldType != typeof(bool)) continue;

            ToggleFields[field.Name] = field;
            Keybinds[field.Name] = KeyCode.None;
        }

        Keybinds["closeCurrentDoors"] = KeyCode.C;
    }

    public static void DisablePPMCheats(string variableToKeep)
    {
        ejectPlayer = variableToKeep == "ejectPlayer" && ejectPlayer;
        reportBody = variableToKeep == "reportBody" && reportBody;
        killPlayer = variableToKeep == "killPlayer" && killPlayer;
        telekillPlayer = variableToKeep == "telekillPlayer" && telekillPlayer;
        spectate = variableToKeep == "spectate" && spectate;
        setFakeRole = variableToKeep == "setFakeRole" && setFakeRole;
        setFakeAlive = variableToKeep == "setFakeAlive" && setFakeAlive;
        teleportPlayer = variableToKeep == "teleportPlayer" && teleportPlayer;
        voteKick = variableToKeep == "voteKick" && voteKick;
        copyOutfit = variableToKeep == "copyOutfit" && copyOutfit;
        copyLevel = variableToKeep == "copyLevel" && copyLevel;
    }

    public static bool ShouldPPMClose()
    {
        return !setFakeRole && !setFakeAlive && !ejectPlayer && !reportBody && !telekillPlayer && !killPlayer && !spectate && !teleportPlayer && !voteKick && !copyOutfit && !copyLevel;
    }

    // Disables all cheat toggles by setting all to false using the cached ToggleFields
    public static void DisableAll()
    {
        foreach (var field in ToggleFields.Values)
        {
            field.SetValue(null, false);
        }
    }

    // Saves cheat toggles and their keybinds to TenkaiProfile.txt
    // Format per line: ToggleName = True/False = KeyCode.KEY
    public static void SaveTogglesToProfile()
    {
        using var writer = new StreamWriter(TenkaiMenu.ProfilePath);

        writer.WriteLine("# TenkaiProfile");
        writer.WriteLine("# Format: ToggleName = True/False = KeyCode.KEY");
        writer.WriteLine("# - List of supported keycodes: https://docs.unity3d.com/Packages/com.unity.tiny@0.16/api/Unity.Tiny.Input.KeyCode.html");
        writer.WriteLine("# - Setting a keybind is optional. Use KeyCode.None to not set a keybind");
        writer.WriteLine("# - Multiple toggles may have the same key, but multiple keys per toggle are NOT supported");
        writer.WriteLine("# - Keybinds are only applied after loading this profile by pressing 'Load from Profile' in the Config menu");
        writer.WriteLine();

        foreach (var field in ToggleFields.Values)
        {
            Keybinds.TryGetValue(field.Name, out var key);  // If no key is set then write KeyCode.None
            writer.WriteLine($"{field.Name} = {field.GetValue(null)} = KeyCode.{key}");
        }
    }

    // Loads cheat toggles and their keybinds from TenkaiProfile.txt if the file is present
    // Format per line: ToggleName = True/False = KeyCode.KEY
    public static void LoadTogglesFromProfile()
    {
        if (!File.Exists(TenkaiMenu.ProfilePath)) return;

        using var reader = new StreamReader(TenkaiMenu.ProfilePath);

        while (reader.ReadLine() is { } line)
        {
            // Skips empty lines
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Skips lines that are commented out
            line = line.Trim();
            if (line.StartsWith("#")) continue;

            // Extracts the three relevant config values for each remaining line
            var parts = line.Split('=', 3);
            if (parts.Length < 2) continue;

            // Gets the cheat's FieldInfo from its name
            var name = parts[0].Trim();
            if (!ToggleFields.TryGetValue(name, out var field)) continue;

            // Loads whether the cheat is enabled or disabled by default
            if (bool.TryParse(parts[1].Trim(), out var boolVal))
            {
                field.SetValue(null, boolVal);
            }

            // Loads the keybind associated with each cheat
            KeyCode key = KeyCode.None;
            if (parts.Length >= 3)
            {
                var keyPart = parts[2].Trim();
                if (keyPart.StartsWith("KeyCode."))
                {
                    keyPart = keyPart["KeyCode.".Length..];
                }

                if (!string.IsNullOrEmpty(keyPart) && System.Enum.TryParse<KeyCode>(keyPart, true, out var parsed))
                {
                    key = parsed;
                }
            }

            Keybinds[name] = key;
        }
    }
}
