using System;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
public class FirebaseManager : MonoBehaviour
{
    // Create a static instance for global access
    public static FirebaseManager Instance { get; private set; }
    
    public FirebaseFirestore Db { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    private void Awake()
    {
        // Make sure only one instance of this object exists (singleton!!!!) :)
        if (Instance == null)
        {
            Instance = this;
            // Make sure the instance is alive across scenes (not really important since I just manage UI and no scenes really change but yk)
            DontDestroyOnLoad(gameObject); 
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (Instance == null)
        {
            Debug.Log("Firebase Instance is null");
            return;
        }
    }

    private void InitializeFirebase()
    {
        Db = FirebaseFirestore.DefaultInstance;
        Auth = FirebaseAuth.DefaultInstance;
        Debug.Log("Firebase Initialized");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
