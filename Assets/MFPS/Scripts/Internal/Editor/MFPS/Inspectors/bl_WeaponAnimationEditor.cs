using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Animations;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using System.Linq;

[CustomEditor(typeof(bl_WeaponAnimation))]
public class bl_WeaponAnimationEditor : Editor
{
    private bl_Gun Gun;
    GunType gType;
    private ReorderableList list;
    private Animator _animator;
    bl_WeaponAnimation script;
    bool allowSceneObjects = false;
    AnimationClip WalkAnim;
    AnimationClip RunAnim;

    private void OnEnable()
    {
        script = (bl_WeaponAnimation)target;
        Gun = script.transform.parent.GetComponent<bl_Gun>();
        gType = bl_GameData.Instance.GetWeapon(Gun.GunID).Type;
        if (script.m_AnimationType == bl_WeaponAnimation.AnimationType.Animator)
        {
            _animator = script.GetComponent<Animator>();
        }
        list = new ReorderableList(serializedObject, serializedObject.FindProperty("FireAnimations"), true, true, true, true);
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
        list.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Fire Animations");  };
    }

    public override void OnInspectorGUI()
    {
        if(script == null) { script = (bl_WeaponAnimation)target; }
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();
        allowSceneObjects = !EditorUtility.IsPersistent(script);

        GUILayout.BeginVertical("box");

        GUILayout.BeginVertical("box");
        script.m_AnimationType = (bl_WeaponAnimation.AnimationType)EditorGUILayout.EnumPopup("Animation Type", script.m_AnimationType);
        GUILayout.EndVertical();

        if (script.m_AnimationType == bl_WeaponAnimation.AnimationType.Animation)
        {
            AnimationGUI();
        }
        else
        {
            AnimatorGUI();
        }
        GUILayout.EndVertical();
        if (Gun.SoundReloadByAnim && gType != GunType.Knife)
        {
            GUILayout.BeginVertical("box");
            script.Reload_1 = EditorGUILayout.ObjectField("Clip Out Audio", script.Reload_1, typeof(AudioClip), allowSceneObjects) as AudioClip;
            script.Reload_2 = EditorGUILayout.ObjectField("Clip In Audio", script.Reload_2, typeof(AudioClip), allowSceneObjects) as AudioClip;
            script.Reload_3 = EditorGUILayout.ObjectField("Slide Audio", script.Reload_3, typeof(AudioClip), allowSceneObjects) as AudioClip;
            GUILayout.EndVertical();
        }
        EditorGUI.EndChangeCheck();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
        serializedObject.ApplyModifiedProperties();
    }

    void AnimatorGUI()
    {
        if (_animator == null)
        {
            _animator = script.GetComponent<Animator>();
            if (_animator == null)
            {
                EditorGUILayout.HelpBox("This weapon don't have a Animator Component!", MessageType.Warning);
                return;
            }
        }
        if (_animator.runtimeAnimatorController == null)
        {
            EditorGUILayout.HelpBox("The animator for this weapons has not been assigned yet, if you already have it, assign it in the Animator Component, otherwise you can create it here," +
                "simply draw the AnimationClips in the respective field below and click in the button SetUp", MessageType.Info);

            GUILayout.BeginVertical("box");
            script.DrawName = EditorGUILayout.ObjectField("Draw Animation", script.DrawName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.TakeOut = EditorGUILayout.ObjectField("Hide Animation", script.TakeOut, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.SoloFireClip = EditorGUILayout.ObjectField("Fire Animation", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            if (gType != GunType.Knife)
            {
                if (Gun.reloadPer == bl_Gun.ReloadPer.Bullet)
                {
                    script.StartReloadAnim = EditorGUILayout.ObjectField("Begin Reload", script.StartReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.InsertAnim = EditorGUILayout.ObjectField("Insert Bullet", script.InsertAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.AfterReloadAnim = EditorGUILayout.ObjectField("After Reload", script.AfterReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                }
                else
                {
                    script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                }
            }
            else
            {
                script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            }
            if (gType == GunType.Grenade || gType == GunType.Launcher)
            {
                script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            }
            script.IdleClip = EditorGUILayout.ObjectField("Idle Animation", script.IdleClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.AnimatedMovements = EditorGUILayout.ToggleLeft("Custom Animations For Movements", script.AnimatedMovements, EditorStyles.toolbarButton);
            GUILayout.Space(4);
            if (script.AnimatedMovements)
            {
                WalkAnim = EditorGUILayout.ObjectField("Walk Animation", WalkAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                RunAnim = EditorGUILayout.ObjectField("Run Animation", RunAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            }
            if (GUILayout.Button("SetUp", EditorStyles.toolbarButton))
            {
                CreateAnimator();
            }
            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.BeginHorizontal("box");
            script.DrawName = EditorGUILayout.ObjectField("Draw Animation", script.DrawName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.DrawSpeed = EditorGUILayout.Slider(script.DrawSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.TakeOut = EditorGUILayout.ObjectField("Hide Animation", script.TakeOut, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.HideSpeed = EditorGUILayout.Slider(script.HideSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            if (gType == GunType.Machinegun || gType == GunType.Pistol || gType == GunType.Burst)
            {
                script.fireBlendMethod = (bl_WeaponAnimation.FireBlendMethod)EditorGUILayout.EnumPopup("Fire Blend Method", script.fireBlendMethod);
                GUILayout.BeginHorizontal("box");
                script.SoloFireClip = EditorGUILayout.ObjectField("Fire Animation", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                if(script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireSpeed || script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireSpeedCrossFade)
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }
            else if (gType == GunType.Shotgun || gType == GunType.Sniper)
            {
                GUILayout.BeginHorizontal("box");
                script.SoloFireClip = EditorGUILayout.ObjectField("Fire Animation", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                if (Gun.reloadPer == bl_Gun.ReloadPer.Bullet)
                {
                    GUILayout.BeginHorizontal("box");
                    script.StartReloadAnim = EditorGUILayout.ObjectField("Start Reload", script.StartReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal("box");
                    script.InsertAnim = EditorGUILayout.ObjectField("Insert Bullet", script.InsertAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.InsertSpeed = EditorGUILayout.Slider(script.InsertSpeed, 0.1f, 3, GUILayout.Width(125));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal("box");
                    script.AfterReloadAnim = EditorGUILayout.ObjectField("End Reload", script.AfterReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal("box");
                    script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                }
            }
            else if (gType == GunType.Grenade || gType == GunType.Launcher)
            {
                GUILayout.BeginHorizontal("box");
                script.SoloFireClip = EditorGUILayout.ObjectField("Fire Animation", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                script.HasParticles = EditorGUILayout.ToggleLeft("Use Particles", script.HasParticles, EditorStyles.toolbarPopup);
                if (script.HasParticles)
                {
                    script.ParticleRate = EditorGUILayout.Slider("Particle Rate", script.ParticleRate, 0.1f, 10);
                    var prop = serializedObject.FindProperty("Particles");
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(prop, true);
                    serializedObject.ApplyModifiedProperties();
                }
                GUILayout.Space(2);
                script.DrawAfterFire = EditorGUILayout.ToggleLeft("Draw After Fire", script.DrawAfterFire, EditorStyles.toolbarButton);
            }
            else if (gType == GunType.Knife)
            {
                GUILayout.BeginHorizontal("box");
                script.FireAimAnimation = EditorGUILayout.ObjectField("Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal("box");
            script.IdleClip = EditorGUILayout.ObjectField("Idle Animation", script.IdleClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            script.AnimatedMovements = EditorGUILayout.ToggleLeft("Custom Animations For Movements", script.AnimatedMovements, EditorStyles.toolbarButton);
            GUILayout.EndHorizontal();
        }
    }

    void CreateAnimator()
    {
        string lastFolder = PlayerPrefs.GetString("mfpseditor.wanimator.save", "Assets/");
        string path = EditorUtility.SaveFolderPanel("Save Animator Folder", lastFolder, script.gameObject.name);
        if (string.IsNullOrEmpty(path)) { Debug.Log("Setup canceled"); return; }

        PlayerPrefs.SetString("mfpseditor.wanimator.save", path);

        path += string.Format("/{0}.controller", Gun.gameObject.name);
        string relativepath = "Assets" + path.Substring(Application.dataPath.Length);
        string copyName = string.Format("Assets/MFPS/Content/Prefabs/Weapons/Animators/FPWeapon [{0}].controller", gType.ToString());

        if (AssetDatabase.CopyAsset(copyName, relativepath))
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath(relativepath, typeof(AnimatorController)) as AnimatorController;
            // Add StateMachines
            var rootStateMachine = controller.layers[0].stateMachine;
            var movementStateMachine = controller.layers[1].stateMachine;

            ChildAnimatorState s = rootStateMachine.states.ToList().Find(x => x.state.name == "Draw");
            s.state.motion = script.DrawName;
            s = rootStateMachine.states.ToList().Find(x => x.state.name == "Hide");
            s.state.motion = script.TakeOut;
            s = rootStateMachine.states.ToList().Find(x => x.state.name == "Fire");
            s.state.motion = script.SoloFireClip;
            if (gType != GunType.Knife && gType != GunType.Grenade)
            {
                s = rootStateMachine.states.ToList().Find(x => x.state.name == "AimFire");
                s.state.motion = script.FireAimAnimation;
            }
            else
            {
                s = rootStateMachine.states.ToList().Find(x => x.state.name == "QuickFire");
                s.state.motion = script.QuickFireAnim;
            }
            if (gType == GunType.Machinegun || gType == GunType.Pistol || gType == GunType.Burst || gType == GunType.Grenade || gType == GunType.Launcher)
            {
                s = rootStateMachine.states.ToList().Find(x => x.state.name == "Reload");
                s.state.motion = script.ReloadName;
            }
            else if (gType == GunType.Sniper || gType == GunType.Shotgun)
            {
                if (Gun.reloadPer != bl_Gun.ReloadPer.Bullet)
                {
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "Reload");
                    s.state.motion = script.ReloadName;
                }
                else
                {
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "StartReload");
                    s.state.motion = script.StartReloadAnim;
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "Insert");
                    s.state.motion = script.InsertAnim;
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "EndReload");
                    s.state.motion = script.AfterReloadAnim;
                }
            }
            s = rootStateMachine.states.ToList().Find(x => x.state.name == "Idle");
            s.state.motion = script.IdleClip;

            if (script.AnimatedMovements)
            {
                 s = rootStateMachine.states.ToList().Find(x => x.state.name == "Run");
                 s.state.motion = RunAnim;

                s = movementStateMachine.states.ToList().Find(x => x.state.name == "Movement");
                var moveBlend = (UnityEditor.Animations.BlendTree)s.state.motion;

                var childs = moveBlend.children;
                childs[0].motion = script.IdleClip;
                childs[1].motion = script.IdleClip;
                childs[2].motion = WalkAnim;
                childs[3].motion = RunAnim;
                moveBlend.children = childs;
            }

            EditorUtility.SetDirty(controller);
            _animator.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(_animator);
        }
    }

    void AnimationGUI()
    {
        GUILayout.BeginHorizontal("box");
        script.DrawName = EditorGUILayout.ObjectField("Draw Animation", script.DrawName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
        script.DrawSpeed = EditorGUILayout.Slider(script.DrawSpeed, 0.1f, 3, GUILayout.Width(125));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        script.TakeOut = EditorGUILayout.ObjectField("Hide Animation", script.TakeOut, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
        script.HideSpeed = EditorGUILayout.Slider(script.HideSpeed, 0.1f, 3, GUILayout.Width(125));
        GUILayout.EndHorizontal();
        if (gType == GunType.Machinegun || gType == GunType.Pistol || gType == GunType.Burst)
        {
            GUILayout.BeginHorizontal("box");
            script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();
            list.DoLayoutList();
        }
        else if (gType == GunType.Shotgun || gType == GunType.Sniper)
        {
            GUILayout.BeginHorizontal("box");
            script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            if (Gun.reloadPer == bl_Gun.ReloadPer.Bullet)
            {
                GUILayout.BeginHorizontal("box");
                script.StartReloadAnim = EditorGUILayout.ObjectField("Start Reload", script.StartReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.InsertAnim = EditorGUILayout.ObjectField("Insert Bullet", script.InsertAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.InsertSpeed = EditorGUILayout.Slider(script.InsertSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.AfterReloadAnim = EditorGUILayout.ObjectField("End Reload", script.AfterReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal("box");
                script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }
            list.DoLayoutList();
        }
        else if (gType == GunType.Grenade || gType == GunType.Launcher)
        {
            GUILayout.BeginHorizontal("box");
            script.FireAimAnimation = EditorGUILayout.ObjectField("Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();
            script.HasParticles = EditorGUILayout.ToggleLeft("Use Particles", script.HasParticles, EditorStyles.toolbarPopup);
            if (script.HasParticles)
            {
                script.ParticleRate = EditorGUILayout.Slider("Particle Rate", script.ParticleRate, 0.1f, 10);
                var prop = serializedObject.FindProperty("Particles");
                serializedObject.Update();
                EditorGUILayout.PropertyField(prop, true);
                serializedObject.ApplyModifiedProperties();
            }
        }
        else if (gType == GunType.Knife)
        {
            GUILayout.BeginHorizontal("box");
            script.FireAimAnimation = EditorGUILayout.ObjectField("Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();
        }
    }
}