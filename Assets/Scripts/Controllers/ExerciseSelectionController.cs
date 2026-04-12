using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UIElements;

public class ExerciseSelectionController : MonoBehaviour
{

    private int exerciseCount;
    private UIManager _uiManager;
    
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;

    private ScrollView _exerciseScrollView;
    private Button _confirmAddBtn;

    private SkillPathModel _selectedPath;
    private ExerciseModel _selectedExercise;

    private void OnEnable()
    {
        exerciseCount = 0;
        _db = FirebaseManager.Instance.Db;
        _auth = FirebaseManager.Instance.Auth;
        _uiManager = GetComponent<UIManager>();

        var root = GetComponent<UIDocument>().rootVisualElement;
        
        _exerciseScrollView = root.Q<ScrollView>("ExerciseScrollView");
        _confirmAddBtn = root.Q<Button>("ConfirmAddButton");

        if (_confirmAddBtn != null)
        {
            _confirmAddBtn.SetEnabled(false);
            _confirmAddBtn.clicked += AddSelectedExerciseToProfile;
        }
        FetchAvailableExercises();
   
    }

    private void FetchAvailableExercises()
    {
        _exerciseScrollView.Clear();

        _db.Collection("SkillRoadmaps").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch roadmaps: " + task.Exception);
                return;
            }
            

            foreach (DocumentSnapshot doc in task.Result.Documents)
            {
                SkillPathModel path = doc.ConvertTo<SkillPathModel>();
                
                if (path.Exercises != null && path.Exercises.Count > 0)
                {
                    ExerciseModel firstExercise = path.Exercises.Find(ex => ex.Order == 1);
                    
                    if (firstExercise != null)
                    {
                        _exerciseScrollView.Add(CreateExerciseCard(path, firstExercise));
                    }
                }
            }
        });
    }
    
    private VisualElement CreateExerciseCard(SkillPathModel path, ExerciseModel exercise)
    {
        Button cardBtn = new Button();
        cardBtn.text = $"[{path.PathType}]\n{exercise.Name}\n\n{exercise.Description}";
    
        cardBtn.style.backgroundColor = new StyleColor(new Color(0.89f, 0.3f, 0.4f)); 
        cardBtn.style.width = 480; 
        cardBtn.style.height = 700; 
        cardBtn.style.marginRight = 32;
        cardBtn.style.marginLeft = 32;
        cardBtn.style.marginTop = 32;
        cardBtn.style.whiteSpace = WhiteSpace.Normal;
        cardBtn.style.fontSize = 32;
        cardBtn.style.flexShrink = 0; 

        cardBtn.clicked += () =>
        {
            _exerciseScrollView.Query<Button>().ForEach(btn => 
                btn.style.backgroundColor = new StyleColor(new Color(0.89f, 0.3f, 0.4f)));
        
            cardBtn.style.backgroundColor = new Color(0.6f, 0.1f, 0.2f); 

            _selectedPath = path;
            _selectedExercise = exercise;
            
            _confirmAddBtn.SetEnabled(true);
            _confirmAddBtn.text = $"Start {exercise.Name}";
        };

        return cardBtn;
    }
    
    private void AddSelectedExerciseToProfile()
    {
        if (_selectedExercise == null || _selectedPath == null || _auth.CurrentUser == null) return;

        DocumentReference userDoc = _db.Collection("users").Document(_auth.CurrentUser.UserId);

        userDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists) return;

            UserModel currentUser = task.Result.ConvertTo<UserModel>();
            if (currentUser.CurrentRoadmaps == null) currentUser.CurrentRoadmaps = new List<RoadmapProgressModel>();
            ;

            var existingProgress = currentUser.CurrentRoadmaps.Find(r => r.SkillPathId == _selectedPath.Id);
            if (existingProgress != null)
            {
                existingProgress.CurrentExerciseId = _selectedExercise.Id;
            }
            else
            {
                currentUser.CurrentRoadmaps.Add(new RoadmapProgressModel(_selectedPath.Id, _selectedExercise.Id));
            }
            
            userDoc.SetAsync(currentUser, SetOptions.MergeAll).ContinueWithOnMainThread(updateTask =>
            {
                if (updateTask.IsFaulted) return;

                Debug.Log($"Set {_selectedPath.PathType} roadmap to {_selectedExercise.Name}");
                
                _selectedExercise = null;
                _selectedPath = null;
                _confirmAddBtn.SetEnabled(false);
                _confirmAddBtn.text = "Select Next Exercise";
                _exerciseScrollView.Query<Button>().ForEach(btn => btn.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f));
                
                exerciseCount++;
                
                if(exerciseCount >= 3)
                    _uiManager.OpenHomePage();
            });
        });

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