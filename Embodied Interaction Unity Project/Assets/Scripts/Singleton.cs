using UnityEngine;

public abstract class Singleton<Type> : MonoBehaviour where Type : MonoBehaviour
{
    public static Type Instance { get; private set; }

    protected void Awake()
    {
        if (Instance == null)
        {
            Instance = gameObject.GetComponent<Type>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}