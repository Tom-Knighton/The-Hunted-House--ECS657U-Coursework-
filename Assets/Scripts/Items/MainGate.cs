using Game;

namespace Items
{
    public class MainGate : PickupObject
    {
        private bool _hasSeenBefore = false; // If the player has interacted with this gate before
        private static float KeysRequired => GameManager.Instance.GameSettings.KeysRequired;
        
        /// <summary>
        /// When the player interacts with the gate, open it and win the game if player has enough keys
        /// </summary>
        public override void OnInteract()
        {
            var totalKeyCount = GameManager.Instance.player.Inventory.GetItemCount("GateKey");

            if (totalKeyCount < KeysRequired)
            {
                if (!_hasSeenBefore)
                {
                    UIManager.Instance.ShowHint($"This looks like the way out! But it looks like I need {KeysRequired} keys to open it...");
                    _hasSeenBefore = true;
                }
                UIManager.Instance.ShowHint($"It looks like I need {KeysRequired} keys to open this gate, I only have {totalKeyCount}...");
                return;
            }

            UIManager.Instance.ShowVictoryScreen();
        }
    }
}
