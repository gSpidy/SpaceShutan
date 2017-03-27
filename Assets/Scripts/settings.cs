using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class settings : MonoBehaviour {

    public Slider Music, Sound;

	// Use this for initialization
	void Start () {
        Observable.ReturnUnit().Concat(this.OnEnableAsObservable()).Subscribe(_ => {
            Music.value = SoundController.Instance.Music;
            Sound.value = SoundController.Instance.Sound;
        }).AddTo(this);

        Music.onValueChanged.AddListener((x) => SoundController.Instance.Music=x);
        Sound.onValueChanged.AddListener((x) => SoundController.Instance.Sound=x);
    }
}
