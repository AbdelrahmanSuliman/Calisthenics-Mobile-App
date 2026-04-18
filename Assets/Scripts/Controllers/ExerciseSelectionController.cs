using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UIElements;

public class ExerciseSelectionController : MonoBehaviour
{

    private int _exerciseCount;
    private UIManager _uiManager;
    
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;

    private ScrollView _exerciseScrollView;
    private Button _confirmAddBtn;

    private SkillPathModel _selectedPath;
    private ExerciseModel _selectedExercise;

    
    private void Awake()
    {
        _db = FirebaseManager.Instance.Db;
        _auth = FirebaseManager.Instance.Auth;
        _uiManager = GetComponent<UIManager>();
    }
    private void OnEnable()
    {
        _exerciseCount = 0;

        var root = GetComponent<UIDocument>().rootVisualElement;
        
        _exerciseScrollView = root.Q<ScrollView>("ExerciseScrollView");
        _exerciseScrollView.style.paddingTop = 100;
        _confirmAddBtn = root.Q<Button>("ConfirmAddButton");

        if (_confirmAddBtn == null) return;
        _confirmAddBtn.SetEnabled(false);
        _confirmAddBtn.clicked += AddSelectedExerciseToProfile;

    }

    public void FetchAvailableExercises()
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

                if (path.Exercises is not { Count: > 0 }) continue;
                ExerciseModel firstExercise = path.Exercises.Find(ex => ex.Order == 1);
                    
                if (firstExercise != null)
                {
                    _exerciseScrollView.Add(CreateExerciseCard(path, firstExercise));
                }
            }
        });
    }
    
    private VisualElement CreateExerciseCard(SkillPathModel path, ExerciseModel exercise)
    {
        Button cardBtn = new Button
        {
            text = "",
            style =
            {
                backgroundColor = new StyleColor(new Color(0.89f, 0.3f, 0.4f)),
                width = 480,
                height = 700,
                marginRight = 32,
                marginLeft = 32,
                marginTop = 32,
                whiteSpace = WhiteSpace.Normal,
                fontSize = 32,
                flexShrink = 0,
                flexDirection = FlexDirection.Column, 
                paddingTop = 0, paddingBottom = 0, paddingLeft = 0, paddingRight = 0,
                overflow = Overflow.Hidden
            }
        };
        
        VisualElement gifBox = new VisualElement
        {
            style = { height = 400, width = Length.Percent(100)}
        };
        cardBtn.Add(gifBox);
        
        var frames = Resources.LoadAll<Texture2D>(exercise.GifUrl);
        if (frames is { Length: > 0 })
        {
            int currentFrame = 0;
            gifBox.style.backgroundImage = new StyleBackground(frames[0]);
            gifBox.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
            gifBox.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            gifBox.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            
            
            gifBox.schedule.Execute(() =>
            {
                currentFrame = (currentFrame + 1) % frames.Length;
                gifBox.style.backgroundImage = new StyleBackground(frames[currentFrame]);
            }).Every(200);
        }
        
        Label titleLabel = new Label($"[{path.PathType}]\n{exercise.Name}")
        {
            style =
            {
                fontSize = 32, color = Color.white, whiteSpace = WhiteSpace.Normal,
                unityTextAlign = TextAnchor.MiddleCenter, marginTop = 60 
            }
        };
        cardBtn.Add(titleLabel);

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
                
                _exerciseCount++;
                
                if(_exerciseCount >= 3)
                    _uiManager.OpenHomePage();
            });
        });

    }
    
    private void OnDisable()
    {
        if (_confirmAddBtn != null)
            _confirmAddBtn.clicked -= AddSelectedExerciseToProfile;
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