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
    private UIManager _uiManager;

    private readonly List<SkillPathModel> _allSkillPaths = new List<SkillPathModel>();
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

        _uiManager = GetComponent<UIManager>();
        // preload all data we need
        LoadExercises();
        
        
        //make sure UI components are built beforehand
        BuildRoadMapPopup();
        BuildLogsPopup();

        var uiDoc = GetComponent<UIDocument>();

        var root = uiDoc.rootVisualElement;
        var pushNode = root.Q<Button>("PushNode");
        var pullNode = root.Q<Button>("PullNode");
        var legsNode = root.Q<Button>("LegsNode");

        var signOutBtn = root.Q<Button>("SignOutButton");

        signOutBtn.clicked += () =>
        {
            _auth.SignOut();
            _currentOpenPath = "";
            _activePopup.style.display = DisplayStyle.None;
            _logsPopup.style.display = DisplayStyle.None;
            _uiManager.OpenSignupPage();
        };

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
    
    var loadingLabel = new Label("Loading your progress...")
    {
        style =
        {
            fontSize = 45,
            color = Color.white,
            unityTextAlign = TextAnchor.MiddleCenter,
            marginTop = 50
        }
    };
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

            var highestRepsMap = new Dictionary<string, int>();

            foreach (var doc in task.Result.Documents)
            {
                var log = doc.ConvertTo<WorkoutLogModel>();
                if (!highestRepsMap.ContainsKey(log.ExerciseId) || log.Reps > highestRepsMap[log.ExerciseId])
                {
                    highestRepsMap[log.ExerciseId] = log.Reps;
                }
            }
            
            var sortedExercises = selectedPath.Exercises.OrderBy(e => e.Order).ToList();
            var previousCompleted = true;
            
            foreach (var exercise in sortedExercises)
            {
                int bestReps = highestRepsMap.GetValueOrDefault(exercise.Id, 0);
                
                bool isCompleted = bestReps >= 8;
                bool isInProgress = !isCompleted && previousCompleted;

                VisualElement card = BuildExerciseCard(exercise, isInProgress, isCompleted, bestReps);
                _exerciseContainer.Add(card);

                //make sure we update the previous completed if current exercise completed (for the next exercise)
                previousCompleted = isCompleted;
            }
        });
}

 
    //this runs only once and makes sure the roadmap popup is built
    private void BuildRoadMapPopup()
    {
        _activePopup = new VisualElement
        {
            style =
            {
                position = Position.Absolute,
                width = Length.Percent(100),
                height = Length.Percent(100),
                backgroundColor = new Color(0, 0, 0, 0.92f),
                justifyContent = Justify.Center,
                alignItems = Align.Center
            }
        };

        var window = new VisualElement
        {
            style =
            {
                width = Length.Percent(95),
                height = Length.Percent(90),
                backgroundColor = new Color(0.12f, 0.12f, 0.12f),
                borderTopLeftRadius = 40,
                borderTopRightRadius = 40,
                paddingTop = 60,
                paddingBottom = 40,
                paddingLeft = 40,
                paddingRight = 40
            }
        };

        _activePopup.Add(window);

        var header = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween,
                alignItems = Align.Center,
                marginBottom = 40
            }
        };
        window.Add(header);

        _popupTitleLabel = new Label($"Skill Path")
        {
            style =
            {
                fontSize = 80,
                unityFontStyleAndWeight = FontStyle.Bold,
                color = Color.white
            }
        };
        header.Add(_popupTitleLabel);

        var closeButton = new Button(() =>
        {
            _activePopup.style.display = DisplayStyle.None;
        })
        {
            text = "X",
            style =
            {
                width = 120,
                height = 120,
                fontSize = 50,
                backgroundColor = new Color(0.9f, 0.2f, 0.2f),
                color = Color.white,
                borderTopLeftRadius = 60,
                borderTopRightRadius = 60,
                borderBottomLeftRadius = 60,
                borderBottomRightRadius = 60
            }
        };

        header.Add(closeButton);

        _exerciseContainer = new ScrollView();
        _exerciseContainer.style.flexGrow = 1;
        window.Add(_exerciseContainer);

        _activePopup.style.display = DisplayStyle.None;
        GetComponent<UIDocument>().rootVisualElement.Add(_activePopup);
    }

    private void BuildLogsPopup()
    {
        _logsPopup = new VisualElement
        {
            style =
            {
                position = Position.Absolute,
                width = Length.Percent(100),
                height = Length.Percent(100),
                backgroundColor = new Color(0, 0, 0, 0.92f),
                justifyContent = Justify.Center,
                alignItems = Align.Center
            }
        };


        var window = new VisualElement
        {
            style =
            {
                width = Length.Percent(95),
                height = Length.Percent(90),
                backgroundColor = new Color(0.12f, 0.12f, 0.12f),
                borderTopLeftRadius = 40,
                borderTopRightRadius = 40,
                paddingTop = 60,
                paddingBottom = 40,
                paddingLeft = 40,
                paddingRight = 40
            }
        };

        _logsPopup.Add(window);
        
        var header = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween,
                alignItems = Align.Center,
                marginBottom = 40
            }
        };
        window.Add(header);
        
        _logsTitleLabel = new Label($"Skill Path")
        {
            style =
            {
                fontSize = 80,
                unityFontStyleAndWeight = FontStyle.Bold,
                color = Color.white
            }
        };
        header.Add(_logsTitleLabel);
        
        var closeButton = new Button(() => {
            _logsPopup.style.display = DisplayStyle.None;
        })
        {
            text = "X",
            style =
            {
                width = 120,
                height = 120,
                fontSize = 50,
                backgroundColor = new Color(0.9f, 0.2f, 0.2f),
                color = Color.white,
                borderTopLeftRadius = 60,
                borderTopRightRadius = 60,
                borderBottomLeftRadius = 60,
                borderBottomRightRadius = 60
            }
        };

        header.Add(closeButton);
        
        _logsContainer = new ScrollView
        {
            style =
            {
                flexGrow = 1
            }
        };
        window.Add(_logsContainer);
        
        _logsPopup.style.display = DisplayStyle.None;
        GetComponent<UIDocument>().rootVisualElement.Add(_logsPopup);
    }
    

    private VisualElement BuildExerciseCard(ExerciseModel exercise, bool isInProgress, bool isCompleted, int bestReps)
    {
        var card = new VisualElement
        {
            style =
            {
                height = StyleKeyword.Auto,
                width = Length.Percent(100),
                flexDirection = FlexDirection.Row,
                paddingTop = 40,
                paddingBottom = 40,
                paddingLeft = 30,
                paddingRight = 30,
                marginBottom = 30
            }
        };

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
        
        var textColumn = new VisualElement
        {
            style =
            {
                flexGrow = 1,
                marginRight = 20
            }
        };

        var nameLabel = new Label(exercise.Name)
        {
            style =
            {
                fontSize = 55,
                unityFontStyleAndWeight = FontStyle.Bold,
                color = Color.white,
                marginBottom = 10
            }
        };
        textColumn.Add(nameLabel);

        var descLabel = new Label(exercise.Description)
        {
            style =
            {
                whiteSpace = WhiteSpace.Normal,
                fontSize = 35,
                color = new Color(0.85f, 0.85f, 0.85f)
            }
        };
        textColumn.Add(descLabel);
        
        card.Add(textColumn);
        
        if (isCompleted || isInProgress)
        {
            var bestRepsLabel = new Label($"Best: {bestReps} Reps")
            {
                style =
                {
                    fontSize = 35,
                    color = new Color(0.9f, 0.9f, 0.3f),
                    marginTop = 15
                }
            };
            textColumn.Add(bestRepsLabel);
            
            
            //make sure only the completed or in progress exercises are tappable
            card.RegisterCallback<ClickEvent>(evt => OpenLogsHistory(exercise));
        }
        else
        {
            var lockedLabel = new Label($"Complete previous exercises to unlock")
            {
                style =
                {
                    fontSize = 35,
                    color = Color.gray,
                    marginTop = 15
                }
            };
            textColumn.Add(lockedLabel);
        }

        var gifBox = new VisualElement
        {
            style =
            {
                width = 200,
                height = 200,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f),
                justifyContent = Justify.Center,
                alignItems = Align.Center,
                borderTopLeftRadius = 20,
                borderTopRightRadius = 20,
                borderBottomLeftRadius = 20,
                borderBottomRightRadius = 20
            }
        };


        //this makes sure that the image fills the background entirely as well as making sure the image is centered
        if (!string.IsNullOrEmpty(exercise.GifUrl))
        {
            var frames = Resources.LoadAll<Texture2D>(exercise.GifUrl);
            if (frames != null && frames.Length != 0)
            {
                int currentFrame = 0;
                gifBox.style.backgroundImage = new StyleBackground(frames[0]);
                gifBox.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                gifBox.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                gifBox.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);

                gifBox.schedule.Execute(() =>
                {
                    //the % makes sure we loop back to the first frame
                    currentFrame = (currentFrame + 1) % frames.Length;
                    gifBox.style.backgroundImage = new StyleBackground(frames[currentFrame]);
                }).Every(200);
            }
        }
        card.Add(gifBox);
        
        return card;
    }

    private void OpenLogsHistory(ExerciseModel exercise)
    {
        _logsContainer.Clear();

        var loadingLabel = new Label("Fetching logs...")
        {
            style =
            {
                fontSize = 40,
                color = Color.gray
            }
        };
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
        var row = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween,
                backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                paddingBottom = 20,
                paddingLeft = 20,
                paddingRight = 20,
                paddingTop = 20,
                marginBottom = 15,
                borderTopLeftRadius = 15,
                borderTopRightRadius = 15,
                borderBottomLeftRadius = 15,
                borderBottomRightRadius = 15
            }
        };

        DateTime date = log.Date.ToDateTime();
        
        var dateLabel = new Label(date.ToString("MMM dd, yyyy - HH:mm"))
        {
            style =
            {
                fontSize = 35,
                color = Color.white
            }
        };
        row.Add(dateLabel);

        var repsLabel = new Label($"{log.Reps} Reps")
        {
            style =
            {
                fontSize = 40,
                unityFontStyleAndWeight = FontStyle.Bold,
                color = log.Reps >= 8 ? new Color(0.3f, 0.8f, 0.3f) : Color.white
            }
        };
        row.Add(repsLabel);

        return row;
    }
    
    private VisualElement BuildNewLogInputArea(ExerciseModel exercise)
    {
        var inputContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.Center,
                marginTop = 40,
                paddingBottom = 20,
                paddingLeft = 20,
                paddingRight = 20,
                paddingTop = 20,
                backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                borderTopLeftRadius = 20,
                borderTopRightRadius = 20,
                borderBottomLeftRadius = 20,
                borderBottomRightRadius = 20
            }
        };

        var repsInput = new TextField
        {
            style =
            {
                width = 200,
                fontSize = 50,
                backgroundColor = Color.white,
                color = Color.black
            }
        };
        inputContainer.Add(repsInput);

        var logBtn = new Button
        {
            text = "Save Logs",
            style =
            {
                fontSize = 40,
                marginLeft = 20,
                paddingLeft = 20,
                paddingRight = 20,
                backgroundColor = new Color(0.2f, 0.6f, 0.2f)
            }
        };

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
