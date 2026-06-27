using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace PoppyMenu
{
    internal class PoppyHoming : MonoBehaviour
    {
        private const float CatchTime = 0.12f;
        private const float SnapRadius = 3f;

        private Transform _target;
        private TeamIndex _team;
        private ProjectileSimple _simple;
        private Rigidbody _rb;
        private float _baseSpeed;
        private float _reacquire;

        internal void Init(Transform target, TeamIndex ownerTeam)
        {
            _target = target;
            _team = ownerTeam;
            _simple = GetComponent<ProjectileSimple>();
            _rb = GetComponent<Rigidbody>();
            _baseSpeed = _simple != null ? Mathf.Max(_simple.desiredForwardSpeed, 1f) : 0f;
        }

        private void FixedUpdate()
        {
            if (_target == null)
            {
                _reacquire -= Time.fixedDeltaTime;
                if (_reacquire <= 0f) { _target = FindNearest(); _reacquire = 0.2f; }
                if (_target == null) return;
            }

            if (_baseSpeed <= 1f && _rb != null) _baseSpeed = Mathf.Max(_rb.velocity.magnitude, 1f);
            if (_baseSpeed <= 1f) _baseSpeed = 60f;

            Vector3 dir = _target.position - transform.position;
            float dist = dir.magnitude;
            if (dist < 0.0001f) return;
            Vector3 ndir = dir / dist;

            transform.rotation = Quaternion.LookRotation(ndir);

            float dt = Time.fixedDeltaTime;
            float spd = Mathf.Min(Mathf.Max(_baseSpeed, dist / CatchTime), dist / dt);
            Vector3 vel = ndir * spd;

            bool moved = false;
            if (_rb != null && !_rb.isKinematic) { _rb.velocity = vel; moved = true; }
            if (_simple != null) { _simple.desiredForwardSpeed = spd; _simple.velocity = spd; moved = true; }
            if (!moved)
            {
                Vector3 np = transform.position + ndir * Mathf.Min(dist, spd * dt);
                if (_rb != null) _rb.MovePosition(np); else transform.position = np;
            }

            if (dist <= SnapRadius)
            {
                transform.position = _target.position;
                if (_rb != null) _rb.position = _target.position;
            }
        }

        private Transform FindNearest()
        {
            Vector3 pos = transform.position;
            float best = Aim.MaxRange * Aim.MaxRange;
            Transform res = null;
            foreach (CharacterBody b in CharacterBody.readOnlyInstancesList)
            {
                if (b == null || b.healthComponent == null || !b.healthComponent.alive || b.teamComponent == null) continue;
                TeamIndex t = b.teamComponent.teamIndex;
                if (t == _team || t == TeamIndex.None || t == TeamIndex.Neutral || t == TeamIndex.Player) continue;
                float d = (b.corePosition - pos).sqrMagnitude;
                if (d >= best) continue;
                HurtBox hb = Util.FindBodyMainHurtBox(b);
                if (hb != null) { best = d; res = hb.transform; }
            }
            return res;
        }
    }
}
