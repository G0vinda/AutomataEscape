using UnityEngine;

namespace LevelGrid
{
    public class PortalTileAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private int _idleHash;
        private int _teleportHash;
        private int _reverseTeleportHash;

        private void Awake()
        {
            _idleHash = Animator.StringToHash("idle");
            _teleportHash = Animator.StringToHash("teleport");
            _reverseTeleportHash = Animator.StringToHash("reverseTeleport");
        }

        public void PlayTeleport()
        {
            animator.CrossFade(_teleportHash, 0, 0);    
        }
    
        public void PlayReverseTeleport()
        {
            animator.CrossFade(_reverseTeleportHash, 0, 0);    
        }
    }
}
