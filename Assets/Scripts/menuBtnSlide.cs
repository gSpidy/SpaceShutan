using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class menuBtnSlide : MonoBehaviour {

    public float Delay;

    RectTransform rt;
    Vector2 originPos;

    private void Awake() {
        rt = GetComponent<RectTransform>();
        originPos = rt.anchoredPosition;
    }

    private void OnEnable() {
        rt.DOKill();        
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -rt.sizeDelta.y);        
        DOTween.Sequence()
            .AppendInterval(Delay)
            .Append(
                rt.DOAnchorPos(originPos,1).SetEase(Ease.OutBack)
            );
    }



}
