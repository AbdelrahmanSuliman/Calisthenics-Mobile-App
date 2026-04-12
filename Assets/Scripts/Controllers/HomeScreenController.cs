using System;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;




public class HomeScreenController : MonoBehaviour
{
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;

    private UserModel _currentUser;
    private List<SkillPathModel> _allSkillPaths = new List<SkillPathModel>();
    private VisualElement exercisesPopup;
    
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

        var uiDoc = GetComponent<UIDocument>();

        var root = uiDoc.rootVisualElement;
        var pushNode = root.Q<Button>("PushNode");
        var pullNode = root.Q<Button>("PullNode");
        var legsNode = root.Q<Button>("LegsNode");

        var allNodes = root.Query<Button>(className: "tree-node");

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreatePopup()
    {
        exercisesPopup.style.height = 100;
        exercisesPopup.style.width = 100;
        exercisesPopup.style.backgroundColor = new StyleColor(new Color(0.1f, 0.6f, 1f));
        exercisesPopup.style.display = DisplayStyle.Flex;
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
