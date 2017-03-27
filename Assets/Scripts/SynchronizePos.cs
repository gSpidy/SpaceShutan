using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;

public class SynchronizePos : NetSync {
    [SyncVar]
    Vector2 targetp;

    // Use this for initialization
    void Start () {
        targetp = new Vector2(transform.position.x, transform.position.z);

        if (!isServer) {
            Observable.EveryFixedUpdate().Subscribe(_ => {
                transform.position = Vector3.Lerp(transform.position, new Vector3(targetp.x, 0, targetp.y), Time.fixedDeltaTime * m_LerpMultiplier);
            }).AddTo(this);

            return;
        }

        Observable.Interval(TimeSpan.FromSeconds(m_syncRate)).Subscribe(_ => {            
            targetp = new Vector2(transform.position.x, transform.position.z);
        }).AddTo(this);
    }	
	
}
