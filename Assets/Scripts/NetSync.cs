using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetSync : NetworkBehaviour {
    [SerializeField, Range(.05f, 1f)]
    protected float m_syncRate = .1f;

    [SerializeField]
    protected float m_LerpMultiplier = 12;    
}
