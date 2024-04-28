using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroDashSettings
{
    [SerializeField] private float _dashSpeed;
    public float DashSpeed = 40f;

    [SerializeField] private float _dashDuration;
    public float DashDuration = 2f;

    // Ajoutez d'autres paramètres du dash selon vos besoins
}

