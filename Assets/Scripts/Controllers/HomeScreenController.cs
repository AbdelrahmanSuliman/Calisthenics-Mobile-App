using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

public enum NodeStatus
{
    Completed,
    Current,
    Locked
};


public class HomeScreenController : MonoBehaviour
{
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;

    private UserModel _currentUser;

    private List<ExerciseModel> _mainModel = new List<ExerciseModel>();

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _db = FirebaseFirestore.DefaultInstance;
        _auth = FirebaseAuth.DefaultInstance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void LoadUserData()
    {
        string userId = _auth.CurrentUser.UserId;

        _db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to load user profile.");
                return;
            }

            _currentUser = task.Result.ConvertTo<UserModel>();
        
        });
    }

    private void LoadExercises()
    {
      
    }
}
