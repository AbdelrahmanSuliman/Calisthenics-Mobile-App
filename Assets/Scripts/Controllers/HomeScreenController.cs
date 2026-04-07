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

    private List<ExerciseModel> _mainModle = new List<ExerciseModel>();
    
    private List<ExerciseModel> _pushExercises = new List<ExerciseModel>();
    private List<ExerciseModel> _pullExercises = new List<ExerciseModel>();
    private List<ExerciseModel> _legExercises = new List<ExerciseModel>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _db = FirebaseFirestore.DefaultInstance;
        _auth = FirebaseAuth.DefaultInstance;
        LoadUserData();
        LoadExercises();
        
        Debug.Log("Homescreen Reached");

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
        _db.Collection("exercises").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch exercises: " + task.Exception);
                return;
            }

            _pushExercises.Clear();
            _pullExercises.Clear();
            _legExercises.Clear();

            foreach (DocumentSnapshot doc in task.Result.Documents)
            {
                ExerciseModel exercise = doc.ConvertTo<ExerciseModel>();
                exercise.Id = doc.Id; 

                switch (exercise.SkillPath)
                {
                    case "Push":
                        _pushExercises.Add(exercise);
                        break;
                    case "Pull":
                        _pullExercises.Add(exercise);
                        break;
                    case "Legs":
                        _legExercises.Add(exercise);
                        break;
        
                }

                Debug.Log(exercise.ToString());
            }

            Debug.Log($"Loaded: {_pushExercises.Count} Push, {_pullExercises.Count} Pull, {_legExercises.Count} Legs.");
        });
    }
}
