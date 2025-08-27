using UnityEngine;
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(T)) as T;
                if (instance == null)
                {
                    Debug.LogError($"There is no {typeof(T).Name} in current scene!");
                }
            }
            return instance;
        }
    }
    public void Reset()
    {
        instance = null;
    }
    public static bool Exists()
    {
        return (instance != null);
    }
}