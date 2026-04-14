using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;


public class HomeScreenController : MonoBehaviour
{
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;

    private UserModel _currentUser;
    private List<SkillPathModel> _allSkillPaths = new List<SkillPathModel>();
    private VisualElement _activePopup;  
    private VisualElement _exerciseContainer;
    private Label _popupTitleLabel;
    private string _currentOpenPath = "";

    private VisualElement _logsPopup;
    private Label _logsTitleLabel;
    private ScrollView _logsContainer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    private void OnEnable()
    {
        _db = FirebaseManager.Instance.Db;
        _auth = FirebaseManager.Instance.Auth;
        // preload all data we need
        LoadUserData();
        LoadExercises();
        
        //make sure UI components are built beforehand
        BuildRoadMapPopup();
        BuildLogsPopup();

        var uiDoc = GetComponent<UIDocument>();

        var root = uiDoc.rootVisualElement;
        var pushNode = root.Q<Button>("PushNode");
        var pullNode = root.Q<Button>("PullNode");
        var legsNode = root.Q<Button>("LegsNode");

        pushNode.clicked += () => PopulateRoadMapPopup("PUSH");
        pullNode.clicked += () => PopulateRoadMapPopup("PULL");
        legsNode.clicked += () => PopulateRoadMapPopup("LEGS");
    }


    // Update is called once per frame
    void Update()
    {
        
    }
    
    //this populates the popup with values so the user does not create a new popup everytime the button is clicked :>
    private void PopulateRoadMapPopup(string skillPath)
{
    
    // Ignore case sensitivity ("PUSH" == "Push")
    var selectedPath = _allSkillPaths.FirstOrDefault(p => 
        string.Equals(p.PathType, skillPath, StringComparison.OrdinalIgnoreCase));
    
    if (selectedPath == null)
    {
        Debug.LogError($"Could not find a SkillPath in Firestore matching '{skillPath}'");
        return;
    }

    _currentOpenPath = skillPath;
    _exerciseContainer.Clear();
    
    var loadingLabel = new Label("Loading your progress...");
    loadingLabel.style.fontSize = 45;
    loadingLabel.style.color = Color.white;
    loadingLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
    loadingLabel.style.marginTop = 50;
    _exerciseContainer.Add(loadingLabel);

    _activePopup.style.display = DisplayStyle.Flex;

    _db.Collection("workout_logs")
        .WhereEqualTo("UserId", _auth.CurrentUser.UserId)
        .GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            _exerciseContainer.Clear();

            if (task.IsFaulted)
            {
                Debug.LogError($"Firebase Log Fetch Failed: {task.Exception}");
                return;
            }

            Dictionary<string, int> highestRepsMap = new Dictionary<string, int>();

            foreach (var doc in task.Result.Documents)
            {
                var log = doc.ConvertTo<WorkoutLogModel>();
                if (!highestRepsMap.ContainsKey(log.ExerciseId) || log.Reps > highestRepsMap[log.ExerciseId])
                {
                    highestRepsMap[log.ExerciseId] = log.Reps;
                }
            }
            
            var sortedExercises = selectedPath.Exercises.OrderBy(e => e.Order).ToList();
            bool previousCompleted = true;
            
            foreach (var exercise in sortedExercises)
            {
                int bestReps = highestRepsMap.GetValueOrDefault(exercise.Id, 0);
                
                bool isCompleted = bestReps >= 8;
                bool isInProgress = !isCompleted && previousCompleted;

                VisualElement card = BuildExerciseCard(exercise, isInProgress, isCompleted, bestReps);
                _exerciseContainer.Add(card);

                previousCompleted = isCompleted;
            }
        });
}

 
    //this runs only once and makes sure the roadmap popup is built
    private void BuildRoadMapPopup()
    {
        _activePopup = new VisualElement();
        _activePopup.style.position = Position.Absolute;
        _activePopup.style.width = Length.Percent(100);
        _activePopup.style.height = Length.Percent(100);
        
        _activePopup.style.backgroundColor = new Color(0, 0, 0, 0.92f);
        
        _activePopup.style.justifyContent = Justify.Center;
        _activePopup.style.alignItems = Align.Center;

        var window = new VisualElement();
        window.style.width = Length.Percent(95);
        window.style.height = Length.Percent(90);
        window.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
        window.style.borderTopLeftRadius = 40;
        window.style.borderTopRightRadius = 40;
        
        window.style.paddingTop = 60; 
        window.style.paddingBottom = 40;
        window.style.paddingLeft = 40;
        window.style.paddingRight = 40;
        _activePopup.Add(window);

        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.justifyContent = Justify.SpaceBetween;
        header.style.alignItems = Align.Center;
        header.style.marginBottom = 40;
        window.Add(header);

        _popupTitleLabel = new Label($"Skill Path"); 
        _popupTitleLabel.style.fontSize = 80;
        _popupTitleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _popupTitleLabel.style.color = Color.white;
        header.Add(_popupTitleLabel);

        var closeButton = new Button(() =>
        {
            _activePopup.style.display = DisplayStyle.None;
        });
        
        closeButton.text = "X";
        closeButton.style.width = 120; 
        closeButton.style.height = 120;
        closeButton.style.fontSize = 50;
        closeButton.style.backgroundColor = new Color(0.9f, 0.2f, 0.2f);
        closeButton.style.color = Color.white;
        closeButton.style.borderTopLeftRadius = 60; 
        closeButton.style.borderTopRightRadius = 60;
        closeButton.style.borderBottomLeftRadius = 60;
        closeButton.style.borderBottomRightRadius = 60;
        header.Add(closeButton);

        _exerciseContainer = new ScrollView();
        _exerciseContainer.style.flexGrow = 1;
        window.Add(_exerciseContainer);

        _activePopup.style.display = DisplayStyle.None;
        GetComponent<UIDocument>().rootVisualElement.Add(_activePopup);
    }

    private void BuildLogsPopup()
    {
        _logsPopup = new VisualElement();
        _logsPopup.style.position = Position.Absolute;
        _logsPopup.style.width = Length.Percent(100);
        _logsPopup.style.height = Length.Percent(100);
        
        _logsPopup.style.backgroundColor = new Color(0, 0, 0, 0.92f);
        _logsPopup.style.justifyContent = Justify.Center;
        _logsPopup.style.alignItems = Align.Center;


        var window = new VisualElement();
        window.style.width = Length.Percent(95);
        window.style.height = Length.Percent(90);
        window.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
        window.style.borderTopLeftRadius = 40;
        window.style.borderTopRightRadius = 40;
        
        window.style.paddingTop = 60; 
        window.style.paddingBottom = 40;
        window.style.paddingLeft = 40;
        window.style.paddingRight = 40;
        _logsPopup.Add(window);
        
        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.justifyContent = Justify.SpaceBetween;
        header.style.alignItems = Align.Center;
        header.style.marginBottom = 40;
        window.Add(header);
        
        _logsTitleLabel = new Label($"Skill Path"); 
        _logsTitleLabel.style.fontSize = 80;
        _logsTitleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _logsTitleLabel.style.color = Color.white;
        header.Add(_logsTitleLabel);
        
        var closeButton = new Button(() => {
            _logsPopup.style.display = DisplayStyle.None;
        });
        
        closeButton.text = "X";
        closeButton.style.width = 120; 
        closeButton.style.height = 120;
        closeButton.style.fontSize = 50;
        closeButton.style.backgroundColor = new Color(0.9f, 0.2f, 0.2f);
        closeButton.style.color = Color.white;
        closeButton.style.borderTopLeftRadius = 60; 
        closeButton.style.borderTopRightRadius = 60;
        closeButton.style.borderBottomLeftRadius = 60;
        closeButton.style.borderBottomRightRadius = 60;
        header.Add(closeButton);
        
        _logsContainer = new ScrollView();
        _logsContainer.style.flexGrow = 1;
        window.Add(_logsContainer);
        
        _logsPopup.style.display = DisplayStyle.None;
        GetComponent<UIDocument>().rootVisualElement.Add(_logsPopup);
    }
    

    private VisualElement BuildExerciseCard(ExerciseModel exercise, bool isInProgress, bool isCompleted, int bestReps)
    {
        var card = new VisualElement();
        card.style.height = StyleKeyword.Auto; 
        card.style.width = Length.Percent(100);
        card.style.flexDirection = FlexDirection.Row;
        card.style.paddingTop = 40;
        card.style.paddingBottom = 40;
        card.style.paddingLeft = 30;
        card.style.paddingRight = 30;
        card.style.marginBottom = 30;
        
        //logic to dynamically change the background of the exercise card depending on its status
        if (isCompleted) 
            card.style.backgroundColor = new Color(0.15f, 0.45f, 0.2f); // Green
        else if (isInProgress) 
            card.style.backgroundColor = new Color(0.8f, 0.45f, 0.1f); // Orange
        else 
            card.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f); // Default Dark Grey
        
        
        card.style.borderTopLeftRadius = 25;
        card.style.borderTopRightRadius = 25;
        card.style.borderBottomLeftRadius = 25;
        card.style.borderBottomRightRadius = 25;
        
        var textColumn = new VisualElement();
        textColumn.style.flexGrow = 1;
        textColumn.style.marginRight = 20;
        
        var nameLabel = new Label(exercise.Name);
        nameLabel.style.fontSize = 55;
        nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        nameLabel.style.color = Color.white;
        nameLabel.style.marginBottom = 10;
        textColumn.Add(nameLabel);

        var descLabel = new Label(exercise.Description);
        descLabel.style.whiteSpace = WhiteSpace.Normal;
        descLabel.style.fontSize = 35;  
        descLabel.style.color = new Color(0.85f, 0.85f, 0.85f);
        textColumn.Add(descLabel);
        
        card.Add(textColumn);
        
        if (isCompleted || isInProgress)
        {
            var bestRepsLabel = new Label($"Best: {bestReps} Reps");
            bestRepsLabel.style.fontSize = 35;
            bestRepsLabel.style.color = new Color(0.9f, 0.9f, 0.3f); 
            bestRepsLabel.style.marginTop = 15;
            textColumn.Add(bestRepsLabel);
            
            
            //make sure only the completed or in progress exercises are tappable
            card.RegisterCallback<ClickEvent>(evt => OpenLogsHistory(exercise));
        }
        else
        {
            var lockedLabel = new Label($"Complete previous exercises to unlock");
            lockedLabel.style.fontSize = 35;
            lockedLabel.style.color = Color.gray; 
            lockedLabel.style.marginTop = 15;
            textColumn.Add(lockedLabel);
        }

        var gifBox = new VisualElement();
        gifBox.style.width = 200;
        gifBox.style.height = 200;
        gifBox.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        gifBox.style.justifyContent = Justify.Center;
        gifBox.style.alignItems = Align.Center;
        gifBox.style.borderTopLeftRadius = 20;
        gifBox.style.borderTopRightRadius = 20;
        gifBox.style.borderBottomLeftRadius = 20;
        gifBox.style.borderBottomRightRadius = 20;

        //this makes sure that the image fills the background entirely as well as making sure the image is centered
        if (!string.IsNullOrEmpty(exercise.GifUrl))
        {
            Texture2D loadedTexture = Resources.Load<Texture2D>(exercise.GifUrl);
            if (loadedTexture != null)
            {
                gifBox.style.backgroundImage = new StyleBackground(loadedTexture);
                gifBox.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                gifBox.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                gifBox.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            }
        }
        card.Add(gifBox);
        
        return card;
    }

    private void OpenLogsHistory(ExerciseModel exercise)
    {
        _logsContainer.Clear();

        var loadingLabel = new Label("Fetching logs...");
        loadingLabel.style.fontSize = 40;
        loadingLabel.style.color = Color.gray;
        _logsContainer.Add(loadingLabel);

        _logsPopup.style.display = DisplayStyle.Flex;

        //make sure we fetch the newest logs first
        _db.Collection("workout_logs")
            .WhereEqualTo("UserId", _auth.CurrentUser.UserId)
            .WhereEqualTo("ExerciseId", exercise.Id)
            .OrderByDescending("Date")
            .GetSnapshotAsync().ContinueWithOnMainThread(task => 
            {
                _logsContainer.Clear(); 

                if (task.IsFaulted)
                {
                    _logsContainer.Add(new Label("Failed to load history."));
                    Debug.LogError(task.Exception);
                    return;
                }

                if (!task.Result.Documents.Any())
                {
                    var emptyLabel = new Label("No logs yet");
                    emptyLabel.style.fontSize = 40;
                    emptyLabel.style.color = Color.gray;
                    _logsContainer.Add(emptyLabel);
                }
                else
                {
                    foreach (var doc in task.Result.Documents)
                    {
                        var log = doc.ConvertTo<WorkoutLogModel>();
                        _logsContainer.Add(BuildLogEntry(log));
                    }
                }

                _logsContainer.Add(BuildNewLogInputArea(exercise));
            });
    }
    private VisualElement BuildLogEntry(WorkoutLogModel log)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.justifyContent = Justify.SpaceBetween;
        row.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        row.style.paddingBottom = 20;
        row.style.paddingLeft = 20;
        row.style.paddingRight = 20;
        row.style.paddingTop = 20;
        row.style.marginBottom = 15;
        row.style.borderTopLeftRadius = 15;
        row.style.borderTopRightRadius = 15;
        row.style.borderBottomLeftRadius = 15;
        row.style.borderBottomRightRadius = 15;

        DateTime date = log.Date.ToDateTime();
        
        var dateLabel = new Label(date.ToString("MMM dd, yyyy - HH:mm"));
        dateLabel.style.fontSize = 35;
        dateLabel.style.color = Color.white;
        row.Add(dateLabel);

        var repsLabel = new Label($"{log.Reps} Reps");
        repsLabel.style.fontSize = 40;
        repsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        repsLabel.style.color = log.Reps >= 8 ? new Color(0.3f, 0.8f, 0.3f) : Color.white;
        row.Add(repsLabel);

        return row;
    }
    
    private VisualElement BuildNewLogInputArea(ExerciseModel exercise)
    {
        var inputContainer = new VisualElement();
        inputContainer.style.flexDirection = FlexDirection.Row;
        inputContainer.style.justifyContent = Justify.Center;
        inputContainer.style.marginTop = 40;
        inputContainer.style.paddingBottom = 20;
        inputContainer.style.paddingLeft = 20;
        inputContainer.style.paddingRight = 20;
        inputContainer.style.paddingTop = 20;
        inputContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
        inputContainer.style.borderTopLeftRadius = 20;
        inputContainer.style.borderTopRightRadius = 20;
        inputContainer.style.borderBottomLeftRadius = 20;
        inputContainer.style.borderBottomRightRadius = 20;

        var repsInput = new TextField();
        repsInput.style.width = 200;
        repsInput.style.fontSize = 50;
        repsInput.style.backgroundColor = Color.white;
        repsInput.style.color = Color.black;
        inputContainer.Add(repsInput);

        var logBtn = new Button();
        logBtn.text = "Save Logs";
        logBtn.style.fontSize = 40;
        logBtn.style.marginLeft = 20;
        logBtn.style.paddingLeft = 20;
        logBtn.style.paddingRight = 20;
        logBtn.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f);
        
        logBtn.clicked += () => 
        {
            if (int.TryParse(repsInput.value, out int repsLogged))
            {
                logBtn.SetEnabled(false);
                logBtn.text = "Saving...";
                
                WorkoutLogModel newLog = new WorkoutLogModel(_auth.CurrentUser.UserId, exercise.Id, repsLogged);
                _db.Collection("workout_logs").AddAsync(newLog).ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted) return;
                    
                    OpenLogsHistory(exercise);
                    PopulateRoadMapPopup(_currentOpenPath); 
                });
            }
        };

        inputContainer.Add(logBtn);
        return inputContainer;
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
        Query skillRoadmapsRef = _db.Collection("SkillRoadmaps");
        skillRoadmapsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted) return;

            _allSkillPaths.Clear();
            QuerySnapshot skillRoadmapsSnapshot = task.Result;
            foreach (DocumentSnapshot skillPath in skillRoadmapsSnapshot.Documents)
            {
                _allSkillPaths.Add(skillPath.ConvertTo<SkillPathModel>());
            }
            Debug.Log(_allSkillPaths.Count);
        });
        
    }
}
