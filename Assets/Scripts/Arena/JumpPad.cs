using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float _launchForce = 18f;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null && player.IsAlive)
        {
            player.ApplyLaunch(_launchForce);
            return;
        }

        var bot = other.GetComponent<BotController>();
        if (bot != null && bot.IsAlive)
        {
            bot.ApplyLaunch(_launchForce);
        }
    }
}
