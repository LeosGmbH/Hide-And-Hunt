using UnityEngine;

public class PersistentAudio : MonoBehaviour
{
    private static PersistentAudio instance;

    private void Awake()
    {
        // Singleton-Pattern: Verhindert, dass mehrere Instanzen existieren
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Dieses GameObject bleibt über Szenenwechsel erhalten
        }
        else
        {
            Destroy(gameObject); // Entfernt doppelte Instanzen
        }
    }
}
