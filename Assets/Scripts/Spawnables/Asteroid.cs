using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using Random = UnityEngine.Random;

public class Asteroid : SpaceObject {

    [SerializeField]
    GameObject m_cratePrefab;

    bool blownUp = false;

    // Use this for initialization
    void Start () {
        int model = Random.Range(0, transform.childCount);
        for (var i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(i == model);

        transform.DORotate(new Vector3(Random.value * 30, Random.value * 30, Random.value * 30), 1).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);

        if (!isServer) return;

        //SERVERSIDE
        
        int astroBehaviour = Random.Range(0, 4);

        switch (astroBehaviour) {
            case 0:
                transform.position += Vector3.right * Random.Range(-19.5f, 19.5f);
                break;
            case 1:
                transform.position += Vector3.right * -19.5f;
                transform.DOLocalMoveX(19.5f, 2.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                break;
            case 2:
                transform.position += Vector3.right * 19.5f;
                transform.DOLocalMoveX(-19.5f, 2.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                break;
            case 3:
                transform.position += Vector3.right * Random.Range(-19.5f, 15.5f);
                DOTween.Sequence()
                    .Append(transform.DOLocalMoveX(4, 1.5f).SetRelative().SetEase(Ease.OutElastic))
                    .Append(transform.DOLocalMoveX(-4, 1.5f).SetRelative().SetEase(Ease.OutElastic))
                    .SetLoops(-1, LoopType.Restart);
                break;
        }

        var spd = 7f + GameController.Instance.Difficulty*.8f;

        //moving asteroids
        Observable.EveryUpdate().Subscribe(_=> {            
            transform.position += Vector3.back * spd * Time.deltaTime;

            if(transform.position.z<-10) NetworkServer.Destroy(gameObject);
        }).AddTo(this);

        this.OnTriggerEnterAsObservable().Subscribe(c => {
            var dmg = c.GetComponentInParent<SpaceObject>();
            if (!dmg) return;

            dmg.GetHit();
            
            NetworkServer.Destroy(gameObject);
        }).AddTo(this);

    }

    public override void GetHit() {
        if (blownUp) return;
        blownUp = true;

        base.GetHit();

        GameController.Instance.AddScore();        

        Observable.ReturnUnit().Delay(TimeSpan.FromSeconds(1))
            .TakeUntilDestroy(this)
            .Subscribe(_ => NetworkServer.Destroy(gameObject));

        if (Random.value < .15f) { //15% shans yashika
            var crate = Instantiate(m_cratePrefab, transform.position, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(crate);
        }
    }

    protected override void RpcGotHit() {
        gameObject.SetActive(false);
        GameObject xplosion = Instantiate(m_destructionSpawn, transform.position, transform.rotation);
        Destroy(xplosion, 2);
    }

}
