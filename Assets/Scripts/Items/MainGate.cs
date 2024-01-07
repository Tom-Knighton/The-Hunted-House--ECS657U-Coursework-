using Game;

namespace Items
{
    public class MainGate : PickupObject
    {
        private bool _hasSeenBefore = false; // If the player has interacted with this gate before
        private static float KeysRequired => CurrentGameSettings.Settings.KeysRequired;
        private const string GateKeyName = "GateKey";
        
        /// <summary>
        /// When the player interacts with the gate, open it and win the game if player has enough keys
        /// </summary>
        public override void OnInteract()
        {
            var totalKeyCount = GameManager.Instance.player.Inventory.GetItemCount("GateKey");
            // Access the FirstPersonController instance to check the current equipped item

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

            
            // If player has keys in hotbar
            var playerController = GameManager.Instance.player.GetComponent<FirstPersonController>();
            var currentEquippedItem = playerController?.Inventory.GetHotbarSlot(playerController.currentEquippedSlot)?.Item;

            if (currentEquippedItem?.Name != GateKeyName)
            {
                UIManager.Instance.ShowHint("You need to equip the keys to your first hotbar slot to use them");
                return;
            }
            
            UIManager.Instance.ShowEndingCutscene();
        }
    }
}
