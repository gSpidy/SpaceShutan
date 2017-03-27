using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using UniRx.Triggers;

public class Shot : NetworkBehaviour {

    [SerializeField]
    float m_despawnTime = 3;
    [SerializeField]
    float m_speed = 30;

    void Start () {
        Observable.EveryFixedUpdate().Subscribe(_=> {
            transform.position += Vector3.forward * m_speed* Time.fixedDeltaTime;
        }).AddTo(this);

        if (!isServer) return;

        this.OnTriggerEnterAsObservable().Subscribe(c => {
            var dmg = c.GetComponentInParent<SpaceObject>();
            if (!dmg) return;

            dmg.GetHit();

            Despawn();
        }).AddTo(this);

        Invoke("Despawn", m_despawnTime);
	}

    void Despawn() {
        NetworkServer.Destroy(this.gameObject);
    }
}
