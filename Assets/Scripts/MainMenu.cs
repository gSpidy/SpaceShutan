using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MainMenu : MonoBehaviour {

    public InputField AddressInput;

	// Use this for initialization
	void Start () {
        Time.timeScale = 1;

        SwitchMenu("main");

        AddressInput.text = PlayerPrefs.GetString("lastip", "localhost");

        SoundController.Instance.PlayMusic("yokohama");
	}
	

    public void Host() {
        FindObjectOfType<NetworkManager>().StartHost();
    }

    public void Join() {
        PlayerPrefs.SetString("lastip", AddressInput.text);
        PlayerPrefs.Save();

        var mgr = FindObjectOfType<NetworkManager>();
        mgr.networkAddress = AddressInput.text;
        mgr.StartClient();
    }    

    public void SwitchMenu(string menuName) {
        gameMenus.ShowMenu(menuName);
    }

    public void Quit() {
        Application.Quit();
    }

    
        

}
