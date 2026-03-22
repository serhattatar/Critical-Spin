using UnityEngine;
using DG.Tweening;
using CriticalSpin.Core;

namespace CriticalSpin
{
    /// <summary>
    /// This is the entry point for my game scene. 
    /// It basically sets up DOTween limits so the game animations run smoothly without memory spikes.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("DOTween Settings")]
        [Tooltip("Max active animations at the same time. Default is 200, which is enough for my UI.")]
        [SerializeField] private int dotweenMaxTweenersCapacity = 200;
        
        [Tooltip("Max active sequences. I keep this at 50.")]
        [SerializeField] private int dotweenMaxSequencesCapacity = 50;
        
        [Tooltip("Safe mode stops Unity from crashing if an object is destroyed while animating.")]
        [SerializeField] private bool dotweenSafeMode = true;

        private void Awake()
        {
            if (!DOTween.instance)
            {
                DOTween.Init(recycleAllByDefault: true, useSafeMode: dotweenSafeMode, logBehaviour: LogBehaviour.ErrorsOnly)
                    .SetCapacity(dotweenMaxTweenersCapacity, dotweenMaxSequencesCapacity);
            }
        }

        private void OnApplicationQuit()
        {
            DOTween.KillAll();
            GameEvents.Reset();
        }
    }
}
