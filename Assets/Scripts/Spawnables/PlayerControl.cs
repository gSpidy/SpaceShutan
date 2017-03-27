using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using UniRx.Triggers;
using UnityStandardAssets.CrossPlatformInput;
using DG.Tweening;

public class PlayerControl : SpaceObject {
    
    [SerializeField]
    GameObject m_shotPref, m_missilePref;

    [SerializeField]
    List<Transform> m_guns;    

    [SerializeField]
    Material m_nonplayerMaterial;

    [SerializeField]
    float m_originalSpeed=14;
    float overrideSpeed;

    [SerializeField]
    GameObject m_shielding;

    [SerializeField]
    AudioSource m_shotAsc, m_laserAsc;

    [SyncVar]
    Vector2 targetp;

    BoolReactiveProperty invincible = new BoolReactiveProperty(false);

    [SyncVar(hook = "SuperWpnChangedCallback")]
    int curSuperWeapon = -1;
    public int SuperWeapon {
        get { return curSuperWeapon; }
        set {
            if (!isServer) return;
            curSuperWeapon = value;
        }
    }

    float originalFireDelay = .5f, overrideFireDelay;

	// Use this for initialization
	void Start () {       

        Respawn();

        if (!isLocalPlayer) {

            GetComponent<Renderer>().material = m_nonplayerMaterial;

            var curzrt = 0f;

            Observable.EveryFixedUpdate().Subscribe(_=> {
                transform.position = Vector3.Lerp(transform.position, new Vector3(targetp.x, 0, 0), Time.fixedDeltaTime * m_LerpMultiplier);
                curzrt = Mathf.Lerp(curzrt, targetp.y, Time.fixedDeltaTime * m_LerpMultiplier);
                transform.eulerAngles = new Vector3(0,0,curzrt);
            }).AddTo(this);

            return;
        }
        

        Observable.EveryFixedUpdate()/*.Where(_=>!DOTween.IsTweening(transform))*/.Subscribe(_=> { //movin'
            var xs = CrossPlatformInputManager.GetAxis("Horizontal");
            transform.eulerAngles = new Vector3(0, 0, xs * -45);
            transform.position = Vector3.ClampMagnitude(transform.position + new Vector3(xs * Time.fixedDeltaTime*overrideSpeed, 0, 0), 19.5f);
        }).AddTo(this);

        if(isClient)
            Observable.Interval(TimeSpan.FromSeconds(m_syncRate)).Subscribe(_=> {
                CmdSyncData(transform.position.x, CrossPlatformInputManager.GetAxis("Horizontal")*-45);
            }).AddTo(this);


        var canShoot = true;

        Observable.EveryUpdate() //shootin'            
            .Where(_=>CrossPlatformInputManager.GetButton("Jump") && canShoot)            
            .Subscribe(_=> {
                PlaySnd(m_shotAsc);
                CmdShot();
                canShoot = false;
                Observable.ReturnUnit().Delay(TimeSpan.FromSeconds(overrideFireDelay)) //dynamic delay
                    .Subscribe(__=>canShoot = true);                
            }).AddTo(this);

        Observable.EveryUpdate() //evadin'
            .Where(_ => CrossPlatformInputManager.GetButton("Run"))
            .ThrottleFirst(TimeSpan.FromSeconds(2f)) //static delay
            .Select(_=>CrossPlatformInputManager.GetAxis("Horizontal"))
            .Where(x=>x!=0)
            .Subscribe(x => {
                CmdEvade();

                overrideSpeed = m_originalSpeed * 3;

                transform.DORotate(new Vector3(0, 0, Mathf.Sign(-x)*360), .6f, RotateMode.LocalAxisAdd);

                Observable.ReturnUnit().Delay(TimeSpan.FromSeconds(.3f))
                    .TakeUntilDestroy(this)
                    .Subscribe(_ => ResetSpeed());

                GameController.Instance.EvadeRefillUI.DoRefill(2);
            }).AddTo(this);

        Observable.EveryUpdate() //shootin'
            .Where(_ => CrossPlatformInputManager.GetButtonDown("Submit"))            
            .Subscribe(_ => CmdSuperWpn()).AddTo(this);

    }

    void Respawn() {
        overrideSpeed = m_originalSpeed;
        overrideFireDelay = originalFireDelay;

        transform.position = new Vector3(transform.position.x, 0, -5);
        transform.rotation = Quaternion.identity;
        transform.DOComplete();
        transform.DOLocalMoveZ(0, 1.5f).SetEase(Ease.OutBack);
    }

    void ResetSpeed() {
        overrideSpeed = m_originalSpeed;
    }

    void SuperWpnChangedCallback(int num) { 
        GameController.Instance.WeaponUI.IconWpn = num; //super weapon ui
    }

    [Command]
    void CmdSyncData(float xpos, float zrt) {
        targetp = new Vector2(xpos,zrt);
    }

    [Command]
    void CmdShot() {
        m_guns.ForEach(x => {
            GameObject sht = Instantiate(m_shotPref, x.position+Vector3.right*(targetp.x-transform.position.x), Quaternion.Euler(90,0,0));
            NetworkServer.Spawn(sht);
        });

        RpcEffect(2);
    }

    public override void GetHit() {
        base.GetHit();

        if (invincible.Value) return;

        GameController.Instance.MinusLife();
    }

    protected override void RpcGotHit() {        
        GameObject xplosion = Instantiate(m_destructionSpawn, transform.position, transform.rotation);
        Destroy(xplosion, 2);

        if (invincible.Value) return;

        Respawn();
    }

    [Command]
    void CmdEvade() {
        RpcEffect(0);
                
        invincible.Value = true;

        Observable.ReturnUnit().Delay(TimeSpan.FromSeconds(.6f))
            .TakeUntilDestroy(this)
            .Subscribe(_ => invincible.Value = false);
    }

    [Command]
    void CmdSuperWpn() {

        switch (SuperWeapon) {
            case 0: //lasers
                RpcEffect(1);

                Observable.Timer(TimeSpan.FromSeconds(.5f), TimeSpan.FromSeconds(.1f))
                    .Take(10)
                    .Subscribe(_ => m_guns.ForEach(x=>{ //laser hits check
                        Physics.RaycastAll(x.position, Vector3.forward, 50, LayerMask.GetMask("enemies"))
                            .Select(c => c.collider.GetComponent<SpaceObject>())
                            .Where(dmg => dmg)
                            .ToList()
                            .ForEach(dmg => dmg.GetHit());
                    })).AddTo(this);

                break;
            case 1: //rockets

                var targets = FindObjectsOfType<SpaceObject>()
                    .Where(x => x.gameObject.layer == LayerMask.NameToLayer("enemies"))
                    .OrderBy(x => (x.transform.position - transform.position).magnitude)
                    .ToArray();

                Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(.2f))
                    .Take(6)                    
                    .Subscribe(x=> {
                        var pos = m_guns[(int)(x % m_guns.Count)].position;

                        GameObject msl = Instantiate(m_missilePref, pos + Vector3.right * (targetp.x - transform.position.x), Quaternion.identity);

                        if (x < targets.Length) {
                            msl.GetComponent<Missile>().Target = targets[x];
                        }

                        NetworkServer.Spawn(msl);
                    }).AddTo(this);

                break;
            //...
        }

        SuperWeapon = -1;
        
    }

    [ClientRpc]
    void RpcEffect(int fxId) {

        switch (fxId) {
            case 0: //shield                
                m_shielding.SetActive(true);
                invincible.Value = true;

                Observable.ReturnUnit().Delay(TimeSpan.FromSeconds(.6f))
                    .TakeUntilDestroy(this)
                    .Subscribe(_ => {
                        m_shielding.SetActive(false);
                        invincible.Value = false;
                    });
               
                break;
            case 1: //FIRIN DA LAZORS!!111
                m_guns.ForEach(x => {
                    var lr = x.GetComponent<LineRenderer>();
                    lr.enabled = true;
                    lr.material.color = new Color(1, 1, 1, 0);

                    DOTween.Sequence()
                        .Append(lr.material.DOFade(1, .5f))
                        .AppendInterval(1f)
                        .Append(lr.material.DOFade(0, .5f))
                        .AppendCallback(() => lr.enabled = false);

                    PlaySnd(m_laserAsc);
                });

                break;

            case 2: //shotsound
                if (!isLocalPlayer)
                    PlaySnd(m_shotAsc);
                break;
        }        
    }

    [Server]
    public void CollectSpeedup() {
        TargetShootSpeedup(connectionToClient);
    }

    [TargetRpc]
    void TargetShootSpeedup(NetworkConnection target) {
        if(overrideFireDelay*.83f>=.1f)
            overrideFireDelay *= .83f;
    }

    void PlaySnd(AudioSource asc) {
        asc.Stop();
        asc.time = 0;
        asc.Play();
    }

}
