using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {

    static SoundController _instance;

    public static SoundController Instance {
        get { if (!_instance) {
                var go = new GameObject("SOUNDCONTROLLER");                
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<SoundController>();

                _instance.musicSource = go.AddComponent<AudioSource>();
                _instance.musicSource.loop = true;
                _instance.musicSource.spatialBlend = 0;                

                _instance._mVol = PlayerPrefs.GetFloat("msc", .3f);
                _instance._sVol = PlayerPrefs.GetFloat("snd", 1);
                
                _instance.musicSource.volume = _instance._mVol;
                _instance.musicSource.ignoreListenerVolume = true;
                _instance.ApplyVolume();
            }
            return _instance;
        }        
    }

    AudioSource musicSource;
    float _mVol, _sVol;    
    
    public float Sound {
        get {
            return _sVol;
        }
        set {
            _sVol = value;
            PlayerPrefs.SetFloat("snd", _sVol);
            ApplyVolume();
        }
    }

    public float Music {
        get {
            return _mVol;
        }
        set {
            _mVol = value;
            PlayerPrefs.SetFloat("msc", _mVol);
            musicSource.volume = _mVol;
        }
    }
    
    public void ApplyVolume() {
        /*FindObjectsOfType<AudioSource>()
            .Where(x => x != musicSource)
            .ToList().ForEach(x => x.volume=Sound);*/
        AudioListener.volume = Sound;
    }

    public void PlayMusic(string filename) {
        var cl = Resources.Load<AudioClip>(filename);
        musicSource.Stop();
        musicSource.clip = cl;
        musicSource.time = 0;
        musicSource.Play();
    }

    public static void Play(string filename)
    {
        var cl = Resources.Load<AudioClip>(filename);

        var src = Instance.gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 0;                
        src.clip = cl;        
        src.Play();

        Destroy(src, cl.length + .1f);
    }

    public static float Volume {
        get {
            return Instance.Sound;
        }
        set {
            Instance.Sound = value;
        }
    }

}
