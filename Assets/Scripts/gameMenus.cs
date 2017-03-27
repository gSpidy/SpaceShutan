using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class gameMenus : MonoBehaviour {

    static List<gameMenus> All {
        get {
            return new List<gameMenus>(Resources.FindObjectsOfTypeAll<gameMenus>());
        }
    }

    public static void HideAll() {
        foreach (gameMenus m in All) m.gameObject.SetActive(false);
    }

    public static gameMenus ShowMenu(string mid) {
        HideAll();
        gameMenus m = All.Find(x => x.MenuID == mid);
        if (m) {
            m.gameObject.SetActive(true);
            return m;
        }
        return null;
    }

    public string MenuID;
}
