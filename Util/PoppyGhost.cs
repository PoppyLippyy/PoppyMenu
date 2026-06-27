using System.Collections.Generic;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace PoppyMenu
{
    internal class PoppyGhost : MonoBehaviour
    {
        private Collider[] _cols;
        private Rigidbody _rb;
        private readonly HashSet<Collider> _ignored = new HashSet<Collider>();

        private void Start()
        {
            ProjectileController pc = GetComponent<ProjectileController>();
            _cols = (pc != null && pc.myColliders != null && pc.myColliders.Length > 0)
                ? pc.myColliders
                : GetComponentsInChildren<Collider>(true);
            _rb = GetComponent<Rigidbody>();
            Scan();
        }

        private void FixedUpdate()
        {
            if (Aim.MagicBullet) Scan();
        }

        private void Scan()
        {
            if (_cols == null || _cols.Length == 0) return;
            float r = 10f;
            if (_rb != null) r = Mathf.Max(r, _rb.velocity.magnitude * Time.fixedDeltaTime * 3f);

            Collider[] hits = Physics.OverlapSphere(transform.position, r, LayerIndex.world.mask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider wc = hits[i];
                if (wc == null || _ignored.Contains(wc)) continue;
                _ignored.Add(wc);
                for (int j = 0; j < _cols.Length; j++)
                    if (_cols[j] != null) Physics.IgnoreCollision(_cols[j], wc, true);
            }
        }
    }
}
