using UnityEngine;

public class SoundPlayerHandle : MonoBehaviour
{
    [SerializeField] private SoundPlayer soundPlayerPrefab;
    
    private static SoundPlayerHandle _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (SoundPlayer.Instance == null)
        {
            SoundPlayer.Instance = Instantiate(soundPlayerPrefab, transform);
        }
    }
}
