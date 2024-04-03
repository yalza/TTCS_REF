using UnityEngine;
using System.Collections;

/// <summary>
/// This script handle the default MFPS drop/package delivery
/// If you want to create a custom drop delivery, create a new script and inherited from bl_DropBase
/// and use this ONLY as reference.
/// </summary>
public class bl_DropDelivery : bl_DropBase
{
    public Transform PackageHolder;
    public Transform CircleTransform;
    public GameObject DeliveryEffect;
    public TextMesh TimeText;
    public AnimationCurve DropCurve;
    public AnimationCurve LineExpandCurve;
    public AudioClip DropSound;

    private float DeliveryTime = 4;
    private GameObject InstacePrefab;
    private LineRenderer lineRender;

    /// <summary>
    /// 
    /// </summary>
    public override void Dispatch(DropData dropData)
    {
        DeliveryTime = dropData.DeliveryDuration;
        InstacePrefab = dropData.DropPrefab;
        lineRender = GetComponent<LineRenderer>();

        //set up initial position of all objects
        transform.position = dropData.DropPosition + new Vector3(0, -0.1f, 0);
        PackageHolder.position = transform.position;
        PackageHolder.position += new Vector3(0, 500, 0);
        CircleTransform.localScale = Vector3.zero;
        lineRender.SetPosition(1, Vector3.zero);
        if(TimeText != null) { TimeText.gameObject.SetActive(false); }
        StartCoroutine(Delivery());
    }
  
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Delivery()
    {
        float d = 0;
        //start animation
        while(d < 1)
        {
            d += Time.deltaTime / 2;
            float t = LineExpandCurve.Evaluate(d);
            CircleTransform.localScale = ((Vector3.one * 3) * t);
            lineRender.SetPosition(1, new Vector3(0, 500 * t, 0));
            yield return null;
        }
        d = 0;
        CircleTransform.GetComponent<Animator>().enabled = true;
        Vector3 initPosition = PackageHolder.position;
        if (TimeText != null) { TimeText.gameObject.SetActive(true); }

        //start delivery package
        while (d < 1)
        {
            d += Time.deltaTime / DeliveryTime;
            float t = DropCurve.Evaluate(d);
            PackageHolder.position = Vector3.Lerp(initPosition, transform.position, t);
            if(TimeText != null)
            {
                float seconds = DeliveryTime - (DeliveryTime * d);
                string ti = bl_StringUtility.GetTimeFormat(0, seconds);
                TimeText.text = ti;
            }
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);

        //instance package
        if(DeliveryEffect != null) { Instantiate(DeliveryEffect, transform.position, Quaternion.identity); }
        if (InstacePrefab != null)
        {
            Instantiate(InstacePrefab, transform.position, Quaternion.identity);
        }
        else { Debug.Log("No kit instance"); }
        if(DropSound != null)
        {
            AudioSource.PlayClipAtPoint(DropSound, transform.position, 1);
        }

        Destroy(gameObject);
    }
}