using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;



public class HomeScreenController : MonoBehaviour
{
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;

    private UserModel _currentUser;
    private List<SkillPathModel> _allSkillPaths = new List<SkillPathModel>();
    private VisualElement _activePopup;  
    private VisualElement _exerciseContainer;
    private Label _popupTitleLabel;
    
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
        BuildPopup();

        var uiDoc = GetComponent<UIDocument>();

        var root = uiDoc.rootVisualElement;
        var pushNode = root.Q<Button>("PushNode");
        var pullNode = root.Q<Button>("PullNode");
        var legsNode = root.Q<Button>("LegsNode");

        pushNode.clicked += () => PopulatePopup("PUSH");
        pullNode.clicked += () => PopulatePopup("PULL");
        legsNode.clicked += () => PopulatePopup("LEGS");
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    //this populates the popup with values so the user does not create a new popup everytime the button is clicked :>
    private void PopulatePopup(string skillPath)
    {
        if (_allSkillPaths.Count == 0) return;
        
        //nice way of getting the skill path we need
        var selectedPath = _allSkillPaths.FirstOrDefault(p => p.PathType == skillPath);
        
        _exerciseContainer.Clear();
        
        if (selectedPath != null)
        {
            foreach (var exercise in selectedPath.Exercises)
            {
                VisualElement card = BuildExerciseCard(exercise.Name, exercise.Description, exercise.GifUrl);
                _exerciseContainer.Add(card);
            }
        }

        _activePopup.style.display = DisplayStyle.Flex;
    }
    
    //this runs only once and makes sure the popup is built
    private void BuildPopup()
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

    var closeButton = new Button(() => {
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

private VisualElement BuildExerciseCard(string exName, string exDescription, string exGifUrl)
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
    card.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
    card.style.borderTopLeftRadius = 25;
    card.style.borderTopRightRadius = 25;
    card.style.borderBottomLeftRadius = 25;
    card.style.borderBottomRightRadius = 25;

    var textColumn = new VisualElement();
    textColumn.style.flexGrow = 1;
    textColumn.style.marginRight = 20;
    
    var nameLabel = new Label(exName);
    nameLabel.style.fontSize = 55;
    nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
    nameLabel.style.color = Color.white;
    nameLabel.style.marginBottom = 10;
    textColumn.Add(nameLabel);

    var descLabel = new Label(exDescription);
    descLabel.style.whiteSpace = WhiteSpace.Normal;
    descLabel.style.fontSize = 35; 
    descLabel.style.color = new Color(0.85f, 0.85f, 0.85f);
    textColumn.Add(descLabel);
    
    card.Add(textColumn);

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

    var gifLabel = new Label("GIF");
    gifLabel.style.fontSize = 30;
    gifBox.Add(gifLabel);
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
