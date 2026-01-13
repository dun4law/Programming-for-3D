using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [Header("=== MUSIC ===")]
    [Tooltip("Main menu background music")]
    public AudioClip menuMusic;

    [Tooltip("In-game background music")]
    public AudioClip gameMusic;

    [Tooltip("Victory screen music")]
    public AudioClip victoryMusic;

    [Tooltip("Defeat/Game over music")]
    public AudioClip defeatMusic;

    [Header("=== UI SOUNDS ===")]
    [Tooltip("Button click sound")]
    public AudioClip buttonClick;

    [Tooltip("Button hover sound")]
    public AudioClip buttonHover;

    [Tooltip("Menu open sound")]
    public AudioClip menuOpen;

    [Tooltip("Menu close sound")]
    public AudioClip menuClose;

    [Tooltip("Notification/alert sound")]
    public AudioClip notification;

    [Header("=== WEAPON SOUNDS ===")]
    [Tooltip("Missile lock-on beep")]
    public AudioClip missileLock;

    [Tooltip("Missile fire/launch")]
    public AudioClip missileFire;

    [Tooltip("Missile incoming warning")]
    public AudioClip missileIncoming;

    [Tooltip("Cannon/gun fire")]
    public AudioClip cannonFire;

    [Tooltip("Hit/impact sound")]
    public AudioClip hit;

    [Tooltip("Explosion sound")]
    public AudioClip explosion;

    [Tooltip("Flare deployment")]
    public AudioClip flareSound;

    [Tooltip("Chaff deployment")]
    public AudioClip chaffSound;

    [Header("=== ENGINE SOUNDS ===")]
    [Tooltip("Engine idle/loop")]
    public AudioClip engineLoop;

    [Tooltip("Afterburner sound")]
    public AudioClip afterburner;

    [Tooltip("Engine startup")]
    public AudioClip engineStart;

    [Tooltip("Engine shutdown")]
    public AudioClip engineShutdown;

    [Header("=== WARNING SOUNDS ===")]
    [Tooltip("Stall warning")]
    public AudioClip stallWarning;

    [Tooltip("Stall horn (continuous)")]
    public AudioClip stallHorn;

    [Tooltip("Pull up warning")]
    public AudioClip pullUpWarning;

    [Tooltip("Map boundary warning")]
    public AudioClip boundaryWarning;

    [Tooltip("Low health/damage warning")]
    public AudioClip lowHealthWarning;

    [Tooltip("Fire alarm/critical damage")]
    public AudioClip fireAlarm;

    [Header("=== G-FORCE SOUNDS ===")]
    [Tooltip("Heavy breathing under G")]
    public AudioClip breathingHeavy;

    [Tooltip("Heartbeat sound")]
    public AudioClip heartbeat;

    [Tooltip("G-LOC (blackout) sound")]
    public AudioClip glocSound;

    [Header("=== VOICE / RADIO ===")]
    [Tooltip("Fox Two radio call")]
    public AudioClip foxTwo;

    [Tooltip("Splash radio call")]
    public AudioClip splash;

    [Tooltip("Enemy down confirmation")]
    public AudioClip enemyDown;

    [Tooltip("Return to base call")]
    public AudioClip rtb;

    [Tooltip("Bingo fuel warning")]
    public AudioClip bingoFuel;

    [Tooltip("Missile launch warning voice")]
    public AudioClip missileLaunchVoice;

    [Tooltip("Chaff flare voice")]
    public AudioClip chaffFlareVoice;

    [Header("=== ENVIRONMENT ===")]
    [Tooltip("Rain ambient loop")]
    public AudioClip rainLoop;

    [Tooltip("Wind ambient loop")]
    public AudioClip windLoop;

    [Tooltip("Thunder sound")]
    public AudioClip thunder;

    [Tooltip("Sonic boom")]
    public AudioClip sonicBoom;

    [Header("=== DAMAGE SOUNDS ===")]
    [Tooltip("Fire loop sound")]
    public AudioClip fireLoop;

    [Tooltip("Metal stress/creak")]
    public AudioClip metalStress;

    [Tooltip("Warning beep")]
    public AudioClip warningBeep;

    [Header("=== KILL CONFIRMATION ===")]
    [Tooltip("Kill confirmation sound")]
    public AudioClip killConfirm;

    [Header("=== KILL STREAK CALLOUTS ===")]
    [Tooltip("Double kill announcement")]
    public AudioClip doubleKill;

    [Tooltip("Triple kill announcement")]
    public AudioClip tripleKill;

    [Tooltip("Enemy eliminated callout")]
    public AudioClip enemyEliminated;

    [Header("=== TACTICAL CALLOUTS ===")]
    [Tooltip("Under attack warning")]
    public AudioClip underAttack;

    [Tooltip("We're hit voice callout")]
    public AudioClip weHit;

    [Tooltip("Mayday/emergency voice")]
    public AudioClip mayday;

    [Tooltip("Watch your six warning")]
    public AudioClip watchYourSix;

    [Tooltip("Target locked callout")]
    public AudioClip targetLocked;

    [Tooltip("Air support callout")]
    public AudioClip airSupport;

    [Tooltip("Good work/praise")]
    public AudioClip goodWork;

    [Header("=== MISSION STATUS ===")]
    [Tooltip("Game start sound")]
    public AudioClip gameStart;

    [Tooltip("Game over sound")]
    public AudioClip gameOver;

    [Tooltip("Mission accomplished announcement")]
    public AudioClip missionAccomplished;

    [Tooltip("Objective secured callout")]
    public AudioClip objectiveSecured;

    [Header("=== RADIO EFFECTS ===")]
    [Tooltip("Radio check/test")]
    public AudioClip radioCheck;

    [Tooltip("Radio static noise")]
    public AudioClip radioStatic;

    [Header("=== ALTERNATE SOUNDS ===")]
    [Tooltip("Alternate explosion sound")]
    public AudioClip explosion2;

    [Tooltip("Minigun/rapid fire sound")]
    public AudioClip minigun;

    [Tooltip("Loading screen sound 1")]
    public AudioClip loadingSound1;

    [Tooltip("Loading screen sound 2")]
    public AudioClip loadingSound2;

    public AudioClip GetClipByName(string clipName)
    {
        switch (clipName.ToLower().Replace(" ", "").Replace("_", ""))
        {
            case "menumusic":
                return menuMusic;
            case "gamemusic":
                return gameMusic;
            case "victorymusic":
                return victoryMusic;
            case "defeatmusic":
                return defeatMusic;

            case "buttonclick":
                return buttonClick;
            case "buttonhover":
                return buttonHover;
            case "menuopen":
                return menuOpen;
            case "menuclose":
                return menuClose;
            case "notification":
                return notification;

            case "missilelock":
                return missileLock;
            case "missilefire":
                return missileFire;
            case "missileincoming":
                return missileIncoming;
            case "cannonfire":
                return cannonFire;
            case "hit":
                return hit;
            case "explosion":
                return explosion;
            case "flaresound":
            case "flare":
                return flareSound;
            case "chaffsound":
            case "chaff":
                return chaffSound;

            case "engineloop":
                return engineLoop;
            case "afterburner":
                return afterburner;
            case "enginestart":
                return engineStart;
            case "engineshutdown":
                return engineShutdown;

            case "stallwarning":
            case "stall":
                return stallWarning;
            case "stallhorn":
                return stallHorn;
            case "pullupwarning":
            case "pullup":
                return pullUpWarning;
            case "boundarywarning":
            case "boundary":
                return boundaryWarning;
            case "lowhealthwarning":
            case "lowhealth":
                return lowHealthWarning;
            case "firealarm":
                return fireAlarm;

            case "breathingheavy":
            case "breathing":
                return breathingHeavy;
            case "heartbeat":
                return heartbeat;
            case "glocsound":
            case "gloc":
                return glocSound;

            case "foxtwo":
            case "fox2":
                return foxTwo;
            case "splash":
                return splash;
            case "enemydown":
                return enemyDown;
            case "rtb":
            case "returntobase":
                return rtb;
            case "bingofuel":
            case "bingo":
                return bingoFuel;
            case "missilelaunchvoice":
                return missileLaunchVoice;
            case "chaffflarevoice":
                return chaffFlareVoice;

            case "rainloop":
            case "rain":
                return rainLoop;
            case "windloop":
            case "wind":
                return windLoop;
            case "thunder":
                return thunder;
            case "sonicboom":
                return sonicBoom;

            case "fireloop":
            case "fire":
                return fireLoop;
            case "metalstress":
                return metalStress;
            case "warningbeep":
                return warningBeep;

            case "killconfirm":
            case "kill":
                return killConfirm;

            case "doublekill":
                return doubleKill;
            case "triplekill":
                return tripleKill;
            case "enemyeliminated":
                return enemyEliminated;

            case "underattack":
                return underAttack;
            case "wehit":
            case "werehit":
            case "hit_voice":
                return weHit;
            case "mayday":
            case "emergency":
                return mayday;
            case "watchyoursix":
            case "watchsix":
                return watchYourSix;
            case "targetlocked":
                return targetLocked;
            case "airsupport":
                return airSupport;
            case "goodwork":
                return goodWork;

            case "gamestart":
                return gameStart;
            case "gameover":
                return gameOver;
            case "missionaccomplished":
                return missionAccomplished;
            case "objectivesecured":
                return objectiveSecured;

            case "radiocheck":
                return radioCheck;
            case "radiostatic":
                return radioStatic;

            case "explosion2":
                return explosion2;
            case "minigun":
                return minigun;
            case "loadingsound1":
            case "loading1":
                return loadingSound1;
            case "loadingsound2":
            case "loading2":
                return loadingSound2;

            default:
                Debug.LogWarning($"[AudioLibrary] Clip not found: {clipName}");
                return null;
        }
    }
}
