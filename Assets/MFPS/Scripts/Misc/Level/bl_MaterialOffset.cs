using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_MaterialOffset : bl_MonoBehaviour
{

    [Range(-0.02f, 0.02f)] public float XOffset;
    [Range(-0.02f, 0.02f)] public float YOffset;
    public bool useSecondLayer = false;
    [Range(-0.02f, 0.02f)] public float SecondXOffset;
    [Range(-0.02f, 0.02f)] public float SecondYOffset;

    private Material Mat;
    private float X = 0;
    private float Y = 0;
    private float SecondX = 0;
    private float SecondY = 0;

    protected override void Awake()
    {
        base.Awake();
        Mat = GetComponent<Renderer>().material;
    }

    public override void OnUpdate()
    {
        if (Mat == null)
            return;
        float delta = Time.deltaTime;

        X += delta * XOffset;
        Y += delta * YOffset;
        Mat.SetTextureOffset("_MainTex", new Vector2(X, Y));
        if (useSecondLayer)
        {
            SecondX += delta * SecondXOffset;
            SecondY += delta * SecondYOffset;
            Mat.SetTextureOffset("_DetailAlbedoMap", new Vector2(SecondX, SecondY));
        }
    }
}