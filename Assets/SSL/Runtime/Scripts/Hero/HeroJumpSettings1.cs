using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class HeroJumpSettings1
{
    public float jumpSpeed = 10f;
    public float jumpMinDuration = 0.5f;
    public float jumpMaxDuration = 0.15f;
    public bool IsJumpImpulsion => _jumpState == JumpState.JumpingImpulsion;
    public bool IsJumpMinDurationReached => _jumpTimer >= _jumpSettings.jumpMinDuration;

    public void StopJumpImpulsion()
    {
        _jumpState = JumpState.Falling;
    }



}
