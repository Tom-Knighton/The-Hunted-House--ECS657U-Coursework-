using Game;

namespace Items
{
    public class BasementPhoneBehaviour: PickupObject
    {
        private bool _interacted = false;
        
        
        public override void OnInteract()
        {
            if (_interacted)
            {
                UIManager.Instance.ShowHint("You've already interacted with this object...");
                return;
            }
            
            UIManager.Instance.ShowPhoneCutscene();
            AudioManager.Instance.PlayPhoneCall();
            _interacted = true;
        }
    }
}