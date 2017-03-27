using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuperWpnUI : MonoBehaviour {

    [SerializeField]
    Sprite[] m_icons;

    [SerializeField]
    Image m_wpnImg;

    public int IconWpn {
        set {
            var show = value >= 0 && value < m_icons.Length;
            gameObject.SetActive(show);

            if (!show) return;

            m_wpnImg.sprite = m_icons[value];
        }
    }

	// Use this for initialization
	void Start () {
        IconWpn = -1;
	}	
	
}
