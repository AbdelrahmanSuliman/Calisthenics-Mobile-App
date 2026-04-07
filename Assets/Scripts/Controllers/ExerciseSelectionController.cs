using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UIElements;

public class ExerciseSelectionController : MonoBehaviour
{
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;

    private int exerciseCount;
    private UIManager _uiManager;

    private ScrollView _exerciseScrollView;
    private Button _confirmAddBtn;

    private ExerciseModel _selectedExercise;

    private void OnEnable()
    {
        exerciseCount = 0;
        _db = FirebaseFirestore.DefaultInstance;
        _auth = FirebaseAuth.DefaultInstance;
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

        _db.Collection("exercises").WhereEqualTo("Order", 1).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch exercises: " + task.Exception);
                return;
            }

            foreach (DocumentSnapshot doc in task.Result.Documents)
            {
                ExerciseModel exercise = doc.ConvertTo<ExerciseModel>();
                exercise.Id = doc.Id; 
            
                _exerciseScrollView.Add(CreateExerciseCard(exercise));
            }
        });
    }
    
    private VisualElement CreateExerciseCard(ExerciseModel exercise)
    {
        Button cardBtn = new Button();
        cardBtn.text = $"[{exercise.SkillPath}]\n{exercise.Name}\n\n{exercise.Description}";
    
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

            _selectedExercise = exercise;
            _confirmAddBtn.SetEnabled(true);
            _confirmAddBtn.text = $"Start {exercise.Name}";
        };

        return cardBtn;
    }
    
    private void AddSelectedExerciseToProfile()
    {
        if (_selectedExercise == null || _auth.CurrentUser == null) return;

        DocumentReference userDoc = _db.Collection("users").Document(_auth.CurrentUser.UserId);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { $"CurrentRoadmaps.{_selectedExercise.SkillPath}", _selectedExercise.Id }
        };

        userDoc.UpdateAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted) return;

            Debug.Log($"Set {_selectedExercise.SkillPath} roadmap to {_selectedExercise.Name}");
            
            
            
            _selectedExercise = null;
            _confirmAddBtn.SetEnabled(false);
            _confirmAddBtn.text = "Select Next Exercise";
            
            _exerciseScrollView.Query<Button>().ForEach(btn => btn.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f));
            exerciseCount++;


        });
        if(exerciseCount == 2)
            _uiManager.OpenHomePage();
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