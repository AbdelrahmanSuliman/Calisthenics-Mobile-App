using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using Mono.Cecil;


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
        LoadUserData();
        LoadExercises();
        BuildRoadMapPopup();

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
        if (_allSkillPaths.Count == 0) return;
        
        //nice way of getting the skill path we need
        var selectedPath = _allSkillPaths.FirstOrDefault(p => p.PathType == skillPath);
        
        _exerciseContainer.Clear();

        _db.Collection("workout_logs")
            .WhereEqualTo("UserId", _auth.CurrentUser.UserId)
            .GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted) return;

                //this will keep track of each exercise's highest reps
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
                    //if we find no reps we just automatically go with 0 
                    int bestReps = highestRepsMap.GetValueOrDefault(exercise.Id, 0);
                    
                    bool isCompleted = bestReps >= 8;
                    bool isInProgress = !isCompleted && previousCompleted;

                    VisualElement card = BuildExerciseCard(exercise, isCompleted, isInProgress, bestReps);
                    _exerciseContainer.Add(card);

                    previousCompleted = isCompleted;
                }

            });
        
       
        _activePopup.style.display = DisplayStyle.Flex;
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
