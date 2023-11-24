using Game;

namespace Items
{
    public class MainGate: PickupObject
    {
        private bool IsOpen = false;
        
        public override void OnInteract()
        {
            if (IsOpen)
            {
                UIManager.Instance.ShowHint("This gate is already open!");
                return;
            }
            
            var keyCount = GameManager.Instance.player.Inventory.GetItems("GateKey");
            if (keyCount.Count < 3)
            {
                UIManager.Instance.ShowHint($"It looks like I need 3 keys to open this gate, I only have {keyCount.Count}...");
                return;
            }

            
            UIManager.Instance.ShowHint("This will give weapon or open gate in the future (not implemented yet)");
            IsOpen = true;
        }
    }
}