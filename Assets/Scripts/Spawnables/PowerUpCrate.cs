using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using Random = UnityEngine.Random;

public class PowerUpCrate : NetSync {

    const int
        t_health=0,
        t_laser=1,
        t_rocket=2,
        t_speedup=3;
    

    [SyncVar(hook = "changeSprite")]
    int powerupType=-1;

    [SerializeField]
    Sprite[] m_powerupSprites;

    void Start () {
        transform.DORotate(new Vector3(Random.value * 30, Random.value * 30, Random.value * 30), 1).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);

        if (!isServer) return;

        //SERVERSIDE

        
        float ptype = Random.value;

        if (ptype < .4f) {
            powerupType = t_speedup;
        }else if(ptype < .55f) {
            powerupType = t_health;
        }else {
            powerupType = Random.value < .5f ? t_rocket : t_laser;
        }
        

        //moving
        Observable.EveryUpdate().Subscribe(_ => {
            transform.position += Vector3.back * 7 * Time.deltaTime;

            if (transform.position.z < -10) NetworkServer.Destroy(gameObject);
        }).AddTo(this);

        this.OnTriggerEnterAsObservable()
            .Subscribe(c=> {                
                var plr = c.GetComponent<PlayerControl>();
                if (!plr) return;

                switch(powerupType){
                    case t_health:
                        GameController.Instance.CollectLife();
                        break;
                    case t_speedup:
                        plr.CollectSpeedup();
                        break;
                    case t_laser:
                        plr.SuperWeapon = 0;
                        break;
                    case t_rocket:
                        plr.SuperWeapon = 1;
                        break;
                }

                NetworkServer.Destroy(gameObject);
            })
            .AddTo(this);

    }

    void changeSprite (int spriteId){
        var outOfBounds = spriteId < 0 || spriteId >= m_powerupSprites.Length;

        GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(x => {
            x.sprite = outOfBounds?null:m_powerupSprites[spriteId];
        });
    }

    
}
