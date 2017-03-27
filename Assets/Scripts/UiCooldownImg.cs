using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UiCooldownImg : MonoBehaviour {
    [SerializeField]
    Image m_filler;

    void Start() {
        m_filler.fillAmount = 0;
    }

    public void DoRefill(float time) {
        m_filler.DOKill();
        m_filler.fillAmount = 1;
        m_filler.DOFillAmount(0, time);
    }   
}
