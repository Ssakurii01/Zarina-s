using UnityEngine;

/// <summary>
/// Basic swarmer enemy — rushes straight at the player. 
/// Low HP, fast, appears from Wave 1.
/// </summary>
public class EnemySwarmer : EnemyBase
{
    protected override void Start()
    {
        _maxHP = 1;
        _moveSpeed = 5f;
        _scoreValue = 100;
        base.Start();
    }
}
