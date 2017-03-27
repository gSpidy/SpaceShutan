using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class SpaceObject : NetSync {
    
    [SerializeField]
    protected GameObject m_destructionSpawn;    

    [Server]
    public virtual void GetHit() { 
        RpcGotHit();
    }

    [ClientRpc]
    protected abstract void RpcGotHit();

}
