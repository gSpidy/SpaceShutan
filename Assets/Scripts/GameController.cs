using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

public class GameController : NetworkBehaviour {

    static GameController _instance;
    public static GameController Instance {
        get {
            if (!_instance) _instance = FindObjectOfType<GameController>();
            return _instance;
        }
    }

    [SerializeField]
    Text m_diffLvlTxt, m_livesTxt, m_scoreTxt;

    [SerializeField]
    GameObject m_astroPrefab, m_pauseLabel;

    [SerializeField]
    Button m_pauseBtn;

    [SerializeField]
    Sprite m_pauseImg, m_playImg;
        
    public UiCooldownImg EvadeRefillUI;
    public SuperWpnUI WeaponUI;

    [SyncVar]
    int difficulty = 0,
        overallLives = 3, //общие жизни для всех игроков    
        score = 0;

    [SyncVar]
    bool gamePaused = false;

    public int Difficulty { get { return difficulty; } }    
    public int Lives { get { return overallLives; } }
    public bool Paused { get { return gamePaused; } }

    // Use this for initialization
    void Start () {
        this.ObserveEveryValueChanged(x => x.difficulty)
            .Select(x=>x+1)
            .Subscribe(x=> {
                m_diffLvlTxt.text = "Сложность: " + x.ToString();
            })
            .AddTo(this);

        this.ObserveEveryValueChanged(x => x.score).Subscribe(x => {
            m_scoreTxt.text = "Очки: " + x.ToString();
        }).AddTo(this);

        var observableLives = this.ObserveEveryValueChanged(x => x.overallLives);

        observableLives.Subscribe(x=> {
            m_livesTxt.text = "x"+x.ToString();
        }).AddTo(this);

        observableLives.Where(x => x < 1).Take(1).Subscribe(_=> {//GAMEOVER
            if (isServer) FindObjectOfType<NetworkManager>().StopHost();
        }).AddTo(this);

        var observablePause = this.ObserveEveryValueChanged(x => gamePaused);

        observablePause.Subscribe(x=> {
            Time.timeScale = x ? 0 : 1;
            m_pauseBtn.image.sprite = x ? m_playImg : m_pauseImg;
        }).AddTo(this);

        observablePause.Skip(1).Subscribe(x => {
            m_pauseLabel.transform.DOComplete();

            m_pauseLabel.transform.localScale = x ? new Vector3(.5f, .5f, .5f) : Vector3.one;

            var s = DOTween.Sequence();

            if (x) s.AppendCallback(() => m_pauseLabel.SetActive(true));
            s.Append(m_pauseLabel.transform.DOScale(x ? 1 : .5f, .6f).SetEase(x ? Ease.OutBack : Ease.InBack));
            if (!x) s.AppendCallback(() => m_pauseLabel.SetActive(false));
            s.SetUpdate(true);
        }).AddTo(this);

        m_pauseBtn.gameObject.SetActive(isServer);
        
        if (!isServer) return;
        
        //SERVERSIDE

        AstroSpawner();        
	}   

    public void ToMenu() {
        var mgr = FindObjectOfType<NetworkManager>();
        if (!mgr) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("menu");
            return;
        }

        if (isServer) {
            mgr.StopHost();
        } else {
            mgr.StopClient();
        }
    }

    [Server]
    void AstroSpawner() {

        var diffIncrease = Observable.Interval(TimeSpan.FromSeconds(15));

        float spawnrate = 1;

        Action newSpawns = () => {
            Observable.Interval(TimeSpan.FromSeconds(spawnrate))
                .TakeUntil(diffIncrease)
                .Subscribe(__ => {
                    GameObject astr = Instantiate(m_astroPrefab, transform.position, Quaternion.identity);
                    NetworkServer.Spawn(astr);
                }).AddTo(this);
        };

        newSpawns();

        diffIncrease.Subscribe(_=> {
            difficulty++;
            spawnrate *= .93f;

            newSpawns();
        }).AddTo(this);
        
    }
     
    [Server]   
    public void MinusLife() { overallLives--; }

    [Server]
    public void CollectLife() { overallLives++; }

    [Server]
    public void AddScore() { score++; }

    [Server]
    public void SwitchPause() {
        print("PP");
        gamePaused = !gamePaused;
    }   

}
