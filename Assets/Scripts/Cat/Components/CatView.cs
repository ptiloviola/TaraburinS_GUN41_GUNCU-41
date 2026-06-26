using UnityEngine;
using VacuumSim.Cat.Contracts;

namespace VacuumSim.Cat.Components
{
    public class CatView : MonoBehaviour, ICatView
    {
        [SerializeField] private Animator _animator;
        private const float CROSSFADE_TIME = 0.2f;

        public void PlayIdle() => _animator.CrossFade("Idle", CROSSFADE_TIME);
        public void PlayWalk() => _animator.CrossFade("Walk", CROSSFADE_TIME);
        public void PlaySitDown() => _animator.CrossFade("SitDown", CROSSFADE_TIME);
        public void PlayStandUp() => _animator.CrossFade("StandUp", CROSSFADE_TIME);
    }
}