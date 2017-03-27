using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using UniRx.Triggers;

public class Missile : NetSync {

    [HideInInspector]
    public SpaceObject Target;

    // Use this for initialization
    void Start () {

        if (!isServer) {
            transform.ObserveEveryValueChanged(x => x.position)
                .Pairwise()
                .Subscribe(x => {
                    transform.forward = x.Current - x.Previous; //поворот по направлению движения
                })
                .AddTo(this);
            return;
        }

        //SERVERSIDE

        this.OnTriggerEnterAsObservable().Subscribe(c => {
            var dmg = c.GetComponentInParent<SpaceObject>();
            if (!dmg) return;

            dmg.GetHit();

            NetworkServer.Destroy(this.gameObject);
        }).AddTo(this);

        Observable.EveryFixedUpdate().Subscribe(_ => { //movement to target
            var dir = transform.forward;

            if (Target) {
                dir = (Target.transform.position - transform.position).normalized;
            }

            var spd = (7f + GameController.Instance.Difficulty * .8f)*2;
                        
            transform.position += dir * spd * Time.fixedDeltaTime;

            transform.forward = dir;
        }).AddTo(this);

        Observable.Interval(System.TimeSpan.FromSeconds(.5f))
            .Where(_=>!Target || (Target && Target.transform.position.z<1))
            .Subscribe(_ => {
                Target = FindObjectsOfType<SpaceObject>()
                    .Where(x => x.gameObject.layer == LayerMask.NameToLayer("enemies"))
                    .OrderBy(x => (x.transform.position - transform.position).magnitude)
                    .FirstOrDefault();
            })
            .AddTo(this);
    }    

    

}
