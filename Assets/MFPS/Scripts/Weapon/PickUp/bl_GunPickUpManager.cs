using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class bl_GunPickUpManager : bl_GunPickUpManagerBase
{

    [Range(100,500)] public float ForceImpulse = 350;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.WeaponPickUpEvent, OnNetworkCall);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        if (bl_PhotonNetwork.Instance != null)
        bl_PhotonNetwork.Instance.RemoveCallback(OnNetworkCall);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnNetworkCall(Hashtable data)
    {
        int code = (int)data["type"];
        if (code == 0)//Pick up a weapon from the map
        {
            NetworkPickUp(data);
        }
        else if (code == 1)//Instance a weapon in all clients
        {
            NetworkInstanceWeapon(data);
        }
    }

    /// <summary>
    /// Call to sync the instantiation of a pick up weapon
    /// </summary>
    public override void ThrowGun(ThrowData throwData)
    {
        int[] i = new int[3];
        i[0] = throwData.Data[0];
        i[1] = throwData.Data[1];
        i[2] = bl_GameManager.LocalPlayerViewID;
        //prevent the go has an existing name
        int rand = Random.Range(0, 9999);
        //unique go name
        string prefix = bl_PhotonNetwork.NickName + rand;

        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", 1);
        data.Add("name", prefix);
        data.Add("gunID", throwData.GunID);
        data.Add("info", i);
        data.Add("destroy", throwData.AutoDestroy);
        data.Add("origin", throwData.Origin);
        data.Add("dir", throwData.Direction);

        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.WeaponPickUpEvent, data);
    }

    /// <summary>
    /// Called on all clients when a player instance a weapon (a pick up weapon)
    /// </summary>
    void NetworkInstanceWeapon(Hashtable data)
    {
        bl_GunInfo ginfo = bl_GameData.Instance.GetWeapon((int)data["gunID"]);
        if (ginfo.PickUpPrefab == null)
        {
            Debug.LogError(string.Format("The weapon: '{0}' not have a pick up prefab in Gun info", ginfo.Name));
            return;
        }

        GameObject trow;
        trow = ginfo.PickUpPrefab.gameObject;

        int[] info = (int[])data["info"];
        GameObject p = FindPlayerRoot(info[2]);
        if (p == null) return;

        var direction = (Vector3)data["dir"];
        GameObject gun = Instantiate(trow, (Vector3)data["origin"], Quaternion.identity) as GameObject;
        Collider[] c = p.GetComponentsInChildren<Collider>();
        for (int i = 0; i < c.Length; i++)
        {
            Physics.IgnoreCollision(c[i], gun.GetComponent<Collider>());
        }
        gun.GetComponent<Rigidbody>().AddForce(direction * ForceImpulse);
        int clips = info[0];
        var gp = gun.GetComponent<bl_GunPickUpBase>();
        gp.Ammunition.Clips = clips;
        gp.Ammunition.Bullets = info[1];
        gp.AutoDestroy = (bool)data["destroy"];
        gun.name = gun.name.Replace("(Clone)", string.Empty);
        gun.name += (string)data["name"];
        gun.transform.parent = transform;
    }

    /// <summary>
    /// Send a call to all the room clients to let them know that player will pick up a weapon
    /// </summary>
    public override void SendPickUp(PickUpData pickUpData)
    {
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", 0);
        data.Add("name", pickUpData.Identifier);
        data.Add("gunID", pickUpData.GunID);
        data.Add("bullets", pickUpData.Ammunition.Bullets);
        data.Add("clips", pickUpData.Ammunition.Clips);
        data.Add("actorID", bl_PhotonNetwork.LocalPlayer.ActorNumber);

        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.WeaponPickUpEvent, data);
    }

    /// <summary>
    /// Called on all clients when a player pick up a weapon from the map
    /// </summary>
    /// <param name="data"></param>
    void NetworkPickUp(Hashtable data)
    {
        // one of the messages might be ours
        // note: you could check "active" first, if you're not interested in your own, failed pickup-attempts.
        GameObject g = GameObject.Find((string)data["name"]);
        if (g == null)
        {
            Debug.LogWarning("This Gun does not exist in this scene");
            return;
        }
        //is mine
        if ((int)data["actorID"] == bl_PhotonNetwork.LocalPlayer.ActorNumber)
        {
            GunPickUpData pi = new GunPickUpData();
            pi.ID = (int)data["gunID"];
            pi.ItemObject = g;
            pi.Clips = (int)data["clips"];
            pi.Bullets = (int)data["bullets"];
            bl_EventHandler.onPickUpGun(pi);//that call will be received in bl_GunManager.cs
        }
        Destroy(g);
    }
}