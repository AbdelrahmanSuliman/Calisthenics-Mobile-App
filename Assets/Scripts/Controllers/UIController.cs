using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private VisualElement _signupScreen;
    private VisualElement _homeScreen;
    private VisualElement _loginScreen;
    private VisualElement _exerciseSelectionScreen;
    private ExerciseSelectionController _exerciseSelectionController;

    private readonly List<VisualElement> _allScreens = new List<VisualElement>();

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;

        _signupScreen = root.Q<VisualElement>("SignupMenu");
        _homeScreen = root.Q<VisualElement>("HomeScreen");
        _loginScreen = root.Q<VisualElement>("LoginScreen");
        _exerciseSelectionScreen = root.Q<VisualElement>("ExerciseSelectionScreen");
        
        _exerciseSelectionController = FindFirstObjectByType<ExerciseSelectionController>();
        
        if (_signupScreen != null) _allScreens.Add(_signupScreen);
        if (_homeScreen != null) _allScreens.Add(_homeScreen);
        if(_loginScreen != null) _allScreens.Add(_loginScreen);
        if(_exerciseSelectionScreen != null) _allScreens.Add(_exerciseSelectionScreen);
        
        ShowScreen(_signupScreen);
    }

    private void ShowScreen(VisualElement screenToShow)
    {
        foreach(var screen in _allScreens)
        {
            screen.style.display = DisplayStyle.None;
        }

        screenToShow.style.display = DisplayStyle.Flex;
    }
    
    public void OpenHomePage()
    {
        ShowScreen(_homeScreen);
    }

    public void OpenLoginPage()
    {
        ShowScreen(_loginScreen);
    }

    public void OpenSignupPage()
    {
        ShowScreen(_signupScreen);
    }

    public void OpenExerciseSelectionPageAndLoadExercises()
    {
        ShowScreen(_exerciseSelectionScreen);
        _exerciseSelectionController.FetchAvailableExercises();
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
