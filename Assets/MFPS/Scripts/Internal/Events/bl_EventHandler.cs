using UnityEngine;
using System;
using MFPS.Core.Motion;
using MFPS.Internal.Structures;
using MFPS.Internal.Scriptables;
using UnityEngine.Events;
using Photon.Pun;

public static class bl_EventHandler
{
    [Serializable]public class UEvent : UnityEvent { }

    /// <summary>
    /// Event called when the LOCAL player pick up a health in game
    /// </summary>
    public delegate void ItemsPickUpEvent(int Amount);
    public static ItemsPickUpEvent onPickUpHealth;

    /// <summary>
    /// Event called when the LOCAL player call an air drop
    /// </summary>
    public delegate void EventAirDrop(Vector3 m_position, int type);
    public static EventAirDrop onAirKit;

    /// <summary>
    /// Event called when the LOCAL player pick up ammo in game
    /// </summary>
    /// <param name="bullets"></param>
    /// <param name="projectiles"></param>
    /// <param name="gunID"></param>
    public static Func<int, int, int, bool> onAmmoPickUp;

    /// <summary>
    /// Event called when the Local player get a kill or get killed in game.
    /// </summary>
    public delegate void LocalKillEvent(KillInfo killInfo);
    public static LocalKillEvent onLocalKill;

    /// <summary>
    /// Event called when a game round finish
    /// </summary>
    public static Action onRoundEnd;

    /// <summary>
    /// Event called when the LOCAL player land a surface after falling
    /// </summary>
    public static Action<float> onPlayerLand;

    /// <summary>
    /// Event called when the LOCAL player die in game
    /// </summary>
    public static Action onLocalPlayerDeath;

    /// <summary>
    /// Event called when a REMOTE player die in game
    /// </summary>
    public static Action<MFPSPlayer> onRemotePlayerDeath;
    public static void DispatchRemotePlayerDeath(MFPSPlayer player) => onRemotePlayerDeath?.Invoke(player);

    /// <summary>
    /// Event called when the LOCAL player spawn
    /// </summary>
    public static Action onLocalPlayerSpawn;

    /// <summary>
    /// 
    /// </summary>
    public delegate void LocalPlayerShakeEvent(ShakerPresent present, string key, float influence = 1);
    public static LocalPlayerShakeEvent onLocalPlayerShake;

    /// <summary>
    /// Event called when the local player change one or more post-process effect option in game.
    /// </summary>
    public delegate void EffectChange(bool chrab, bool anti, bool bloom, bool ssao, bool motionb);
    public static EffectChange onEffectChange;

    /// <summary>
    /// Event called when the local player pick up a weapon
    /// </summary>
    public delegate void PickUpWeapon(GunPickUpData e);
    public static PickUpWeapon onPickUpGun;

    /// <summary>
    /// Event Called when the local player change of weapon
    /// </summary>
    /// <param name="GunID"></param>
    public delegate void EventChangeWeapon(int GunID);
    public static EventChangeWeapon onChangeWeapon;

    public struct PlayerChangeData
    {
        public string PlayerName;
        public MFPSPlayer MFPSActor;
        public bool IsAlive;
        public PhotonView NetworkView;
    }
    /// <summary>
    /// Event called when a player that is not the local player spawn or die
    /// </summary>
    public static Action<PlayerChangeData> onRemoteActorChange;
    public static void DispatchRemoteActorChange(PlayerChangeData changeData) => onRemoteActorChange?.Invoke(changeData);

    /// <summary>
    /// Event Called when the room match start
    /// </summary>
    public static Action onMatchStart;
    public static void CallOnMatchStart() { if (onMatchStart != null) { onMatchStart.Invoke(); } }

    /// <summary>
    /// Event Called when the local player change their Aim state
    /// </summary>
    public static Action<bool> onLocalAimChanged;
    public static void DispatchLocalAimEvent(bool isAiming) { onLocalAimChanged?.Invoke(isAiming); }

    /// <summary>
    /// Event called when the local player change an in-game setting/option
    /// </summary>
    public static Action onGameSettingsChange;
    public static void DispatchGameSettingsChange() { onGameSettingsChange?.Invoke(); }

    /// <summary>
    /// Event called when the pause menu is open and hided
    /// </summary>
    public static Action<bool> onGamePause;
    public static void DispatchGamePauseEvent(bool paused) { onGamePause?.Invoke(paused); }

    /// <summary>
    /// Called on all clients when a bot die
    /// </summary>
    public static Action<string> onBotDeath;
    public static void EventBotDeath(string botName) => onBotDeath?.Invoke(botName);

    /// <summary>
    /// Called once when all the bots info is fetched
    /// </summary>
    public static Action onBotsInitializated;

    /// <summary>
    /// Called when the local player change his class/loadout
    /// </summary>
    public static Action onPlayerClassChanged;
    public static void DispatchPlayerClassChange(PlayerClass newClass) => onPlayerClassChanged?.Invoke();

    /// <summary>
    /// Called when the local player shoot a weapon
    /// </summary>
    public static Action<int> onLocalPlayerFire;
    public static void DispatchLocalPlayerFire(int gunID) => onLocalPlayerFire?.Invoke(gunID);

    /// <summary>
    /// Called when the local player hit an enemy.
    /// </summary>
    public static Action<MFPSHitData> onLocalPlayerHitEnemy;
    public static void DispatchLocalPlayerHitEnemy(MFPSHitData hitData) => onLocalPlayerHitEnemy?.Invoke(hitData);

    /// <summary>
    /// Called when the match state change (Waiting, Player, Countdown, etc...), this is called in all the clients
    /// </summary>
    public static Action<MatchState> onMatchStateChanged;
    public static void DispatchMatchStateChange(MatchState state) => onMatchStateChanged?.Invoke(state);

    /// <summary>
    /// Called when the UIMask changed
    /// </summary>
    public static Action<RoomUILayers> onUIMaskChanged;
    public static void DispatchUIMaskChange(RoomUILayers layers) => onUIMaskChanged?.Invoke(layers);

    /// <summary>
    /// Called when a local notification is sent
    /// </summary>
    public static Action<MFPSLocalNotification> onLocalNotification;

    /// <summary>
    /// Called when the player controller state change (from state -> to state)
    /// </summary>
    public static Action<PlayerState, PlayerState> onLocalPlayerStateChanged;
    public static void DispatchLocalPlayerStateChange(PlayerState from, PlayerState to) => onLocalPlayerStateChanged?.Invoke(from, to);

    /// <summary>
    /// Called every time the local player ammo count of the equipped weapon changes
    /// </summary>
    public static Action<int> onLocalPlayerAmmoUpdate;
    public static void DispatchLocalPlayerAmmoUpdate(int newAmmocount) => onLocalPlayerAmmoUpdate?.Invoke(newAmmocount);

    /// <summary>
    /// Called when the local player receive damage from an enemy or any other origin.
    /// </summary>
    public static Action<int> onlocalPlayerReceiveDamage;
    public static void DispatchLocalPlayerReceiveDamage(int damage) => onlocalPlayerReceiveDamage?.Invoke(damage);

    /// <summary>
    /// Called every time that one coins change its value.
    /// </summary>
    public static Action<MFPSCoin> onCoinUpdate;
    public static void DispatchCoinUpdate(MFPSCoin updatedCoin) => onCoinUpdate?.Invoke(updatedCoin);

    /// <summary>
    /// Called when the room properties reset
    /// At the start of a game or new round.
    /// You can use to reset any custom property.
    /// </summary>
    public static Action onRoomPropertiesReset;
    public static void DispatchRoomPropertiesReset() => onRoomPropertiesReset?.Invoke();

    /// <summary>
    /// Called when the LOCAL player change of weapon
    /// </summary>
    public static void ChangeWeaponEvent(int GunID) => onChangeWeapon?.Invoke(GunID);

    /// <summary>
    /// Called event when pick up a med kit
    /// </summary>
    public static void DispatchPickUpHealth(int health) => onPickUpHealth?.Invoke(health);

    /// <summary>
    /// Called event when call a new kit 
    /// </summary>
    public static void DispatchDropEvent(Vector3 t_position, int type) => onAirKit?.Invoke(t_position, type);

    /// <summary>
    /// Called Event when pick up ammo
    /// </summary>
    /// <param name="bullets"></param>
    /// <param name="projectiles"></param>
    /// <param name="gunID"></param>
    public static bool OnAmmo(int bullets, int projectiles, int gunID) => onAmmoPickUp(bullets, projectiles, gunID);

    /// <summary>
    /// Called this when killed a new player
    /// </summary>
    public static void DispatchLocalKillEvent(KillInfo killInfo) => onLocalKill?.Invoke(killInfo);

    /// <summary>
    /// Call This when room is finish a round
    /// </summary>
    public static void DispatchRoundEndEvent() => onRoundEnd?.Invoke();

    /// <summary>
    /// 
    /// </summary>
    public static void DispatchPlayerLandEvent(float impactAmount) => onPlayerLand?.Invoke(impactAmount);

    /// <summary>
    /// 
    /// </summary>
    public static void DispatchPlayerLocalDeathEvent() => onLocalPlayerDeath?.Invoke();

    /// <summary>
    /// 
    /// </summary>
    public static void DispatchPlayerLocalSpawnEvent() => onLocalPlayerSpawn?.Invoke();

    public static void DoPlayerCameraShake(ShakerPresent present, string key, float influence = 1) => onLocalPlayerShake?.Invoke(present, key, influence);

    public static void PlayerLocalSpawnEvent(bool chrab, bool anti, bool bloom, bool ssao, bool motionBlur)
    {
        if (onEffectChange != null)
        {
            onEffectChange(chrab, anti, bloom, ssao, motionBlur);
        }
    }

    public static void PickUpGunEvent(GunPickUpData e)
    {
        if (onPickUpGun != null)
            onPickUpGun(e);
    }

    public static void SetEffectChange(bool chrab, bool anti, bool bloom, bool ssao, bool motionb)
    {
        if (onEffectChange != null)
            onEffectChange(chrab, anti, bloom, ssao, motionb);
    }
}