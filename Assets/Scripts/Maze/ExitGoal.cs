using UnityEngine;

public class ExitGoal : MonoBehaviour
{
    [Header("Visual Effects")]
    public Light GoalLight;
    public ParticleSystem CelebrationParticles;

    private bool _reached = false;

    private void Start()
    {
        gameObject.tag = "Goal";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_reached) return;

        // Check if player entered
        if (other.CompareTag("Player") || other.GetComponent<StarterAssets.ThirdPersonController>() != null)
        {
            _reached = true;

            if (GoalLight != null)
            {
                GoalLight.intensity *= 2.5f;
            }

            if (CelebrationParticles != null)
            {
                CelebrationParticles.Play();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.TriggerLevelComplete();
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLevelComplete();
            }
        }
    }
}
