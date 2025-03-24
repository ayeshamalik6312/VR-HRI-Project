using UnityEngine;

public class BackgroundAnimationManager : MonoBehaviour
{

    public Animator[] humanAnimators; 
    public Animator[] robotAnimators; 

    private string[] humanAnimations = { "human-1", "human-2", "human-3" };
    private string[] robotAnimations = { "robot-1", "robot-2", "robot-3" };

    private float[] animationDurations = { 18.267f, 18.267f, 21.5f };
    private float[] staggeredStartTimes = { 0f, 10f, 15f };

    void Start()
    {
        PlayInitialAnimations();
    }

    void Update()
    {
        CheckAndTriggerNextAnimation();
    }

    void PlayInitialAnimations()
    {
        for (int i = 0; i < humanAnimators.Length; i++)
        {
            int randomIndex = Random.Range(0, humanAnimations.Length);

            float duration = animationDurations[randomIndex];

            float normalizedTime = staggeredStartTimes[i] / duration;

            humanAnimators[i].Play(humanAnimations[randomIndex], 0, normalizedTime);
            robotAnimators[i].Play(robotAnimations[randomIndex], 0, normalizedTime);
        }
    }

    void CheckAndTriggerNextAnimation()
    {
        for (int i = 0; i < humanAnimators.Length; i++)
        {
            if (humanAnimators[i].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                int randomIndex = Random.Range(0, humanAnimations.Length);

                humanAnimators[i].Play(humanAnimations[randomIndex]);
                robotAnimators[i].Play(robotAnimations[randomIndex]);
            }
        }
    }
}
