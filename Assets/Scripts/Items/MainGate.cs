using Game;
using System.Linq;

namespace Items
{
    public class MainGate : PickupObject
    {
        private bool IsOpen = false;

        public override void OnInteract()
        {
            if (IsOpen)
            {
                UIManager.Instance.ShowHint("This gate is already open!");
                return;
            }

            int totalKeyCount = GameManager.Instance.player.Inventory.GetItemCount("GateKey");

            if (totalKeyCount < 3)
            {
                UIManager.Instance.ShowHint($"It looks like I need 3 keys to open this gate, I only have {totalKeyCount}...");
                return;
            }

            UIManager.Instance.ShowHint("This will give weapon or open gate in the future (not implemented yet)");
            IsOpen = true;
        }
    }
}
