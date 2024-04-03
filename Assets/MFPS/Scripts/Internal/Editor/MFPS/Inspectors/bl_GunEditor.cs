using UnityEditor;
using MFPSEditor;
using UnityEngine;
using UnityEditorInternal;
using MFPS.Core.Motion;

[CustomEditor(typeof(bl_Gun))]
public class bl_GunEditor : Editor
{

    private ReorderableList list;
    private bl_GameData GameData;
    private bl_Gun script;
    bool allowSceneObjects;
    private GameObject AimReference;
    bl_PlayerReferences playerReferences;
    bl_GunManager GunManager;
    Texture2D aimIcon;
    private int oldID = -1;

    private void OnEnable()
    {
        list = new ReorderableList(serializedObject, serializedObject.FindProperty(Dependency.GOListPropiertie), true, true, true, true);
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
        list.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "On No Ammo Desactive"); };
        GameData = bl_GameData.Instance;
        script = (bl_Gun)target;
        playerReferences = script.transform.GetComponentInParent<bl_PlayerReferences>();
        if (playerReferences != null) { AimReference = playerReferences.playerSettings.AimPositionReference; }
        aimIcon = Resources.Load("content/Images/editor-aim-icon", typeof(Texture2D)) as Texture2D;
        if (script != null)
        {
            GunManager = script.transform.parent.GetComponent<bl_GunManager>();
            script.weaponRenders = script.transform.GetComponentsInChildren<Renderer>();
            if (script.playerSettings == null) { script.playerSettings = script.transform.root.GetComponent<bl_PlayerSettings>(); }
        }

        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        allowSceneObjects = !EditorUtility.IsPersistent(script);
        EditorGUI.BeginChangeCheck();
        DrawGlobalSettings();
        EditorGUILayout.BeginVertical("box");
        if (script.Info.Type == GunType.Machinegun || script.Info.Type == GunType.Pistol || script.Info.Type == GunType.Sniper)
        {
            DrawSeparator("Aim Settings");
            DrawAimSettings();
            EditorGUILayout.Space();
            DrawSeparator("References");
            EditorGUILayout.BeginVertical("box");
            script.muzzlePoint = EditorGUILayout.ObjectField("Fire Point", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            script.weaponFX = EditorGUILayout.ObjectField("Weapon FX", script.weaponFX, typeof(bl_WeaponFXBase), true) as bl_WeaponFXBase;
            if (script.weaponFX == null)
            {
                script.muzzleFlash = EditorGUILayout.ObjectField("Muzzle Flash", script.muzzleFlash, typeof(ParticleSystem), allowSceneObjects) as UnityEngine.ParticleSystem;
                script.shell = EditorGUILayout.ObjectField("Shell", script.shell, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            }
            EditorGUILayout.EndVertical();

            DrawSeparator("Settings");
            EditorGUILayout.BeginVertical("box");
            script.BulletName = EditorGUILayout.TextField("Bullet", script.BulletName, EditorStyles.helpBox);
            script.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", script.bulletSpeed);
            script.bulletDropFactor = EditorGUILayout.Slider("Bullet Drop Factor", script.bulletDropFactor, 0, 10);
            script.impactForce = EditorGUILayout.IntSlider("Impact Force", script.impactForce, 0, 30);
            script.delayFireOnSprinting = EditorGUILayout.Slider("First Shot Delay On Sprint", script.delayFireOnSprinting, 0, 1);
            EditorGUILayout.Space();
            DrawRecoil();
            EditorGUILayout.Space();
            DrawAmmoSettings();
            EditorGUILayout.Space();
            DrawSpreadSettings();
            EditorGUILayout.EndVertical();

            DrawAudioSettings();

        }
        else
        if (script.Info.Type == GunType.Burst)
        {
            DrawSeparator("Gun Settings");
            DrawAimSettings();
            DrawSeparator("References");
            script.muzzlePoint = EditorGUILayout.ObjectField("Fire Point", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            script.weaponFX = EditorGUILayout.ObjectField("Weapon FX", script.weaponFX, typeof(bl_WeaponFXBase), true) as bl_WeaponFXBase;
            if (script.weaponFX == null)
            {
                script.muzzleFlash = EditorGUILayout.ObjectField("Muzzle Flash", script.muzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
                script.shell = EditorGUILayout.ObjectField("Shell", script.shell, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            }
            DrawSeparator("Settings");
            script.BulletName = EditorGUILayout.TextField("Bullet", script.BulletName, EditorStyles.helpBox);
            script.roundsPerBurst = EditorGUILayout.IntSlider("Rounds Per Burst", script.roundsPerBurst, 1, 10);
            script.lagBetweenBurst = EditorGUILayout.Slider("Lag Between Burst", script.lagBetweenBurst, 0.01f, 5.0f);
            script.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", script.bulletSpeed);
            script.bulletDropFactor = EditorGUILayout.Slider("Bullet Drop Factor", script.bulletDropFactor, 0, 10);
            script.impactForce = EditorGUILayout.IntField("Impact Force", script.impactForce);
            script.delayFireOnSprinting = EditorGUILayout.Slider("First Shot Delay On Sprint", script.delayFireOnSprinting, 0, 1);
            EditorGUILayout.Space();
            DrawRecoil();
            EditorGUILayout.Space();
            DrawAmmoSettings();
            EditorGUILayout.Space();
            DrawSpreadSettings();
            DrawAudioSettings();
        }
        else
        if (script.Info.Type == GunType.Shotgun)
        {
            DrawSeparator("Shotgun Settings");
            DrawAimSettings();
            DrawSeparator("References");
            script.muzzlePoint = EditorGUILayout.ObjectField("Fire Point", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            script.weaponFX = EditorGUILayout.ObjectField("Weapon FX", script.weaponFX, typeof(bl_WeaponFXBase), true) as bl_WeaponFXBase;
            if (script.weaponFX == null)
            {
                script.muzzleFlash = EditorGUILayout.ObjectField("Muzzle Flash", script.muzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
                script.shell = EditorGUILayout.ObjectField("Shell", script.shell, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            }
            DrawSeparator("Settings");
            script.BulletName = EditorGUILayout.TextField("Bullet", script.BulletName, EditorStyles.helpBox);
            script.pelletsPerShot = EditorGUILayout.IntSlider("Bullets Per Shots", script.pelletsPerShot, 1, 10);
            script.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntField("Impact Force", script.impactForce);
            script.delayFireOnSprinting = EditorGUILayout.Slider("First Shot Delay On Sprint", script.delayFireOnSprinting, 0, 1);
            EditorGUILayout.Space();
            DrawRecoil();
            EditorGUILayout.Space();
            DrawAmmoSettings();
            EditorGUILayout.Space();
            DrawSpreadSettings();
            DrawAudioSettings();
        }
        else
        if (script.Info.Type == GunType.Grenade)
        {
            DrawSeparator("Grenade Settings");
            EditorGUILayout.Space();
            script.grenade = EditorGUILayout.ObjectField("Grenade", script.grenade, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            script.muzzlePoint = EditorGUILayout.ObjectField("Fire Point", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            DrawSeparator("Settings");
            script.ThrowByAnimation = EditorGUILayout.ToggleLeft("Throw By Animation Event", script.ThrowByAnimation, EditorStyles.toolbarButton);
            script.canBeTakenWhenIsEmpty = EditorGUILayout.ToggleLeft("Can Be Taken When is Empty", script.canBeTakenWhenIsEmpty, EditorStyles.toolbarButton);
            GUILayout.Space(2);
            if (!script.ThrowByAnimation)
            {
                script.DelayFire = EditorGUILayout.FloatField("Delay Fire", script.DelayFire);
            }
            script.bulletSpeed = EditorGUILayout.FloatField("Projectile Speed", script.bulletSpeed);
            script.bulletDropFactor = EditorGUILayout.FloatField("Upward Force", script.bulletDropFactor);
            script.impactForce = EditorGUILayout.IntField("Impact Force", script.impactForce);
            script.m_AllowQuickFire = EditorGUILayout.ToggleLeft("Allow Quick Fire", script.m_AllowQuickFire, EditorStyles.toolbarButton);
            EditorGUILayout.Space();
            DrawRecoil();
            EditorGUILayout.Space();
            DrawAmmoSettings();
            EditorGUILayout.Space();
            list.DoLayoutList();
            DrawAudioSettings();

        }
        else if (script.Info.Type == GunType.Launcher)
        {
            DrawAimSettings();
            DrawSeparator("Launcher Settings");
            DrawWeaponBinding();
            EditorGUILayout.Space();
            DrawBullet();
            script.muzzlePoint = EditorGUILayout.ObjectField("Fire Point", script.muzzlePoint, typeof(UnityEngine.Transform), allowSceneObjects) as UnityEngine.Transform;
            script.weaponFX = EditorGUILayout.ObjectField("Weapon FX", script.weaponFX, typeof(bl_WeaponFXBase), true) as bl_WeaponFXBase;
            if (script.weaponFX == null)
            {
                script.muzzleFlash = EditorGUILayout.ObjectField("Fire Effect", script.muzzleFlash, typeof(ParticleSystem), true) as ParticleSystem;
            }
            script.bulletSpeed = EditorGUILayout.FloatField("Projectile Force", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntField("Impact Force", script.impactForce);
            script.delayFireOnSprinting = EditorGUILayout.Slider("First Shot Delay On Sprint", script.delayFireOnSprinting, 0, 1);
            script.m_AllowQuickFire = EditorGUILayout.ToggleLeft("Allow Quick Fire", script.m_AllowQuickFire, EditorStyles.toolbarButton);
            EditorGUILayout.Space();
            DrawAmmoSettings();
            EditorGUILayout.Space();
            DrawRecoil();
            EditorGUILayout.Space();
            DrawAudioSettings();
        }
        if (script.Info.Type == GunType.Knife)
        {
            DrawSeparator("Knife Settings");
            EditorGUILayout.Space();
            script.BulletName = EditorGUILayout.TextField("Bullet", script.BulletName, EditorStyles.helpBox);
            script.impactEffect = EditorGUILayout.ObjectField("Impact Effect", script.impactEffect, typeof(UnityEngine.GameObject), allowSceneObjects) as UnityEngine.GameObject;
            DrawSeparator("Settings");
            script.bulletSpeed = EditorGUILayout.FloatField("Ray Speed", script.bulletSpeed);
            script.impactForce = EditorGUILayout.IntSlider("Impact Force", script.impactForce, 0, 30);
            script.m_AllowQuickFire = EditorGUILayout.ToggleLeft("Allow Quick Fire", script.m_AllowQuickFire, EditorStyles.toolbarButton);
            EditorGUILayout.Space();
            DrawSeparator("Audio Settings");
            EditorGUILayout.BeginVertical("box");
            script.FireSound = EditorGUILayout.ObjectField("Fire Sound", script.FireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.TakeSound = EditorGUILayout.ObjectField("Take Sound", script.TakeSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();

        if (GunManager != null && !GunManager.AllGuns.Contains(script))
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox("This weapon is not listed in bl_GunManager list yet, you wanna add now?", MessageType.Info);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add to list", EditorStyles.toolbarButton))
            {
                if (GunManager != null)
                {
                    GunManager.AllGuns.Add(script);
                    EditorUtility.SetDirty(GunManager);
                }
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
    
    void DrawGlobalSettings()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Weapon Info", EditorStyles.toolbarButton);
        if (playerReferences != null)
        {
            GUILayout.Space(2);
            if (GUILayout.Button("GameData", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                Selection.activeObject = bl_GameData.Instance;
                EditorGUIUtility.PingObject(bl_GameData.Instance);
            }
            GUILayout.Space(2);
            if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                EditorWindow.GetWindow<bl_ImportExportWeapon>("Export", true).PrepareToExport(script, playerReferences.playerNetwork);
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        script.GunID = EditorGUILayout.Popup("Gun ID ", script.GunID, GameData.AllWeaponStringList());
        if(oldID != script.GunID)
        {
            script.Info = null;
            oldID = script.GunID;
        }
        script.Info.Type = bl_GameData.Instance.GetWeapon(script.GunID).Type;
        GunType t = script.Info.Type;
        if (t == GunType.Machinegun || t == GunType.Pistol || t == GunType.Burst)
        {
            EditorGUILayout.BeginHorizontal("box");
            int w = ((int)EditorGUIUtility.currentViewWidth / 3) - 25;
            GUI.enabled = t != GunType.Machinegun;
            script.CanAuto = EditorGUILayout.ToggleLeft("Auto", script.CanAuto, GUILayout.Width(w));
            GUI.enabled = t != GunType.Burst;
            script.CanSemi = EditorGUILayout.ToggleLeft("Semi", script.CanSemi, GUILayout.Width(w));
            GUI.enabled = t != GunType.Pistol;
            script.CanSingle = EditorGUILayout.ToggleLeft("Single", script.CanSingle, GUILayout.Width(w));
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        script.CrossHairScale = EditorGUILayout.Slider("CrossHair Scale: ", script.CrossHairScale, 1, 30);
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawAimSettings()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        script.AimPosition = EditorGUILayout.Vector3Field("Aim Position", script.AimPosition);
        if (AimReference != null && script.gameObject.activeSelf)
        {
            EditorStyles.toolbarButton.richText = true;
            Color ac = (AimReference.activeSelf) ? Color.red : Color.yellow;
            GUI.color = ac;
            if (GUILayout.Button(new GUIContent(aimIcon, "Simulate Aim"), EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                AimReference.SetActive(!AimReference.activeSelf);
                if (AimReference.activeSelf)
                {
                    script._defaultPosition = script.transform.localPosition;
                    script._defaultRotation = script.transform.localEulerAngles;
                    script.transform.localPosition = script.AimPosition;
                    script.transform.localEulerAngles = script.aimRotation;
                    script._aimRecord = true;
                    ActiveEditorTracker.sharedTracker.isLocked = true;

#if LOVATTO_DEV
                    var adjuster = EditorWindow.GetWindow<AimAdjuster>();
                    adjuster.Set(script);
#endif
                }
                else if (script._aimRecord)
                {
                    script.AimPosition = script.transform.localPosition;
                    script.aimRotation = script.transform.localEulerAngles;
                    script.transform.localPosition = script._defaultPosition;
                    script.transform.localEulerAngles = script._defaultRotation;
                    script._aimRecord = false;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                    ActiveEditorTracker.sharedTracker.isLocked = false;
                }
            }
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        script.aimRotation = EditorGUILayout.Vector3Field("Aim Rotation", script.aimRotation);
        GUILayout.Space(2);
        script.useSmooth = EditorGUILayout.ToggleLeft("Smoothed Aim", script.useSmooth, EditorStyles.toolbarButton);
        GUILayout.Space(2);
        script.aimZoom = EditorGUILayout.Slider("Aim FoV (Zoom)", script.aimZoom, 0.0f, 179);
        script.AimSmooth = EditorGUILayout.Slider("Aim Smooth", script.AimSmooth, 0.01f, 30f);
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawWeaponBinding()
    {
        GUILayout.BeginVertical("box");
        script.customWeapon = EditorGUILayout.ObjectField("Custom Weapon Logic", script.customWeapon, typeof(bl_CustomGunBase), true) as bl_CustomGunBase;
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawBullet()
    {
        script.bulletInstanceMethod = (bl_Gun.BulletInstanceMethod)EditorGUILayout.EnumPopup("Projectile Instance Method", script.bulletInstanceMethod, EditorStyles.toolbarDropDown);
        GUILayout.Space(2);
        if (script.bulletInstanceMethod == bl_Gun.BulletInstanceMethod.Pooled)
        {
            script.BulletName = EditorGUILayout.TextField("Bullet", script.BulletName, EditorStyles.helpBox);
        }
        else
        {
            script.bulletPrefab = EditorGUILayout.ObjectField("Bullet Prefab", script.bulletPrefab, typeof(GameObject), false) as GameObject;
        }
    }

    void DrawAmmoSettings()
    {
        script.AutoReload = EditorGUILayout.ToggleLeft("Auto Reload", script.AutoReload, EditorStyles.toolbarButton);
        GUILayout.Space(2);
        if (script.Info.Type == GunType.Sniper || script.Info.Type == GunType.Shotgun)
        {
            script.reloadPer = (bl_Gun.ReloadPer)EditorGUILayout.EnumPopup("Reload Per", script.reloadPer, EditorStyles.toolbarPopup);
            GUILayout.Space(2);
        }
        else
        {
            script.reloadPer = bl_Gun.ReloadPer.Magazine;
        }
        script.bulletsPerClip = EditorGUILayout.IntField("Ammo Per Clip", script.bulletsPerClip);
        script.numberOfClips = EditorGUILayout.IntSlider("Number Of Clips", script.numberOfClips, 0, script.maxNumberOfClips);
        script.maxNumberOfClips = EditorGUILayout.IntField("Max Number Of Clips", script.maxNumberOfClips);
    }

    void DrawRecoil()
    {
        script.shakerPresent = EditorGUILayout.ObjectField("Shake Present", script.shakerPresent, typeof(ShakerPresent), false) as ShakerPresent;
        script.RecoilAmount = EditorGUILayout.Slider("Recoil", script.RecoilAmount, 0.1f, 10);
        script.RecoilSpeed = EditorGUILayout.Slider("Recoil Speed", script.RecoilSpeed, 1, 10);
    }

    void DrawSpreadSettings()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("Spread Range", GUILayout.Width(EditorGUIUtility.labelWidth - 10));
            script.spreadMinMax.x = EditorGUILayout.FloatField(script.spreadMinMax.x, GUILayout.Width(30));
            EditorGUILayout.MinMaxSlider(ref script.spreadMinMax.x, ref script.spreadMinMax.y, 0, 7);
            script.spreadMinMax.y = EditorGUILayout.FloatField(script.spreadMinMax.y, GUILayout.Width(30));
        }
        script.spreadAimMultiplier = EditorGUILayout.Slider("Aim Spread Multiplier", script.spreadAimMultiplier, 0, 1);
        script.spreadPerSecond = EditorGUILayout.Slider("Spread Per Seconds", script.spreadPerSecond, 0, 1);
        script.decreaseSpreadPerSec = EditorGUILayout.Slider("Decrease Spread Per Sec", script.decreaseSpreadPerSec, 0, 1);
    }

    void DrawAudioSettings()
    {
        DrawSeparator("Audio Settings");
        EditorGUILayout.BeginVertical("box");
        script.FireSound = EditorGUILayout.ObjectField("Fire Sound", script.FireSound, typeof(AudioClip), allowSceneObjects) as AudioClip;
        if (script.Info.Type != GunType.Grenade)
        {
            script.DryFireSound = EditorGUILayout.ObjectField("Empty Fire Sound", script.DryFireSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
        }
        if (script.Info.Type == GunType.Sniper)
        {
            script.delayForSecondFireSound = EditorGUILayout.Slider("Delay Second Fire Sound", script.delayForSecondFireSound, 0.0f, 2.0f);
            script.DelaySource = EditorGUILayout.ObjectField("Second Source", script.DelaySource, typeof(UnityEngine.AudioSource), allowSceneObjects) as UnityEngine.AudioSource;
        }
        script.TakeSound = EditorGUILayout.ObjectField("Take Weapon Sound", script.TakeSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
        script.SoundReloadByAnim = EditorGUILayout.ToggleLeft("Sounds Reload By Animation", script.SoundReloadByAnim, EditorStyles.toolbarButton);
        if (!script.SoundReloadByAnim)
        {
            script.ReloadSound = EditorGUILayout.ObjectField("Reload Begin", script.ReloadSound, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.ReloadSound2 = EditorGUILayout.ObjectField("Reload Middle", script.ReloadSound2, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
            script.ReloadSound3 = EditorGUILayout.ObjectField("Reload End", script.ReloadSound3, typeof(UnityEngine.AudioClip), allowSceneObjects) as UnityEngine.AudioClip;
        }
        EditorGUILayout.EndVertical();
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!script._aimRecord || playerReferences == null || playerReferences.playerCamera == null) return;

        Vector3 origin = playerReferences.playerCamera.transform.position;
        Vector3 target = playerReferences.playerCamera.transform.position + (playerReferences.playerCamera.transform.forward * 25);

        Handles.color = Color.yellow;
        Handles.CircleHandleCap(0, origin, Quaternion.identity, 0.2f, EventType.Repaint);
        Handles.DrawDottedLine(origin, target, 3f);
        Handles.color = Color.white;
    }

    void DrawSeparator(string title)
    {
        GUILayout.Label(title, EditorStyles.toolbarButton);
    }
}