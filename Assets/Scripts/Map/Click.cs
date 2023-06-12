using UnityEngine;

namespace SupeRPG.Map
{
    public class Click : MonoBehaviour
    {
        public bool Clickable;

        public void TriggerClick()
        {
            if (this.Clickable)
            {
                Debug.Log("Moving to " + (this.transform.position - this.transform.parent.transform.position));

                OverworldManager.Instance.SetDestination(this.transform.position - this.transform.parent.transform.position);
            }
        }
    }
}
