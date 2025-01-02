using UnityEngine;

public class StatsStorage : MonoBehaviour
{
    public static StatsStorage Instance;

    [SerializeField] public DupePlayerStats PlayerStats; // Serialized for display in Inspector

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}