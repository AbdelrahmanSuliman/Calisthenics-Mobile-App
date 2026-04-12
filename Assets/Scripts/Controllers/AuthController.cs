using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UIElements;

public class AuthController : MonoBehaviour
{
    private UIManager _uiManager;
    private FirebaseAuth _auth;
    private FirebaseFirestore _db;
        
    private void OnEnable()
    {
        
        //initialize firebase
        _auth = FirebaseManager.Instance.Auth;
        _db = FirebaseManager.Instance.Db;

        //grab the UI manager component
        _uiManager = GetComponent<UIManager>();
        var uiDoc = GetComponent<UIDocument>();

        var root = uiDoc.rootVisualElement;
        
        //grab our credentials
        var signupUsernameInput = root.Q<TextField>("SignupUsernameInput");
        var signupEmailInput = root.Q<TextField>("SignupEmailInput");
        var signupPasswordInput = root.Q<TextField>("SignupPasswordInput");

        var loginEmailInput = root.Q<TextField>("LoginEmailInput");
        var loginPasswordInput = root.Q<TextField>("LoginPasswordInput");
        
        //access buttons
        var signupBtn = root.Q<Button>("SignUpConfirm");
        var redirectBtn = root.Q<Button>("RedirectButton");
        var loginBtn = root.Q<Button>("LoginConfirm");
        
        
        //access error messages
        var signupErrorMessage = root.Q<Label>("SignupErrorMessage");
        var loginErrorMessage = root.Q<Label>("LoginErrorMessage");
        

        //Add logic to buttons
        signupBtn.clicked += () =>
        {
            SignUp(signupEmailInput.value, signupPasswordInput.value, signupUsernameInput.value, signupErrorMessage);
        };

        loginBtn.clicked += () =>
        {
            LogIn(loginEmailInput.value, loginPasswordInput.value, loginErrorMessage);
        };

        redirectBtn.clicked += () =>
        {
            _uiManager.OpenLoginPage();
        };

    }

    private void LogIn(string email, string password, Label errorMessage)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            errorMessage.text = "Email and password are required";
            Debug.Log("Email and password required");
            return;
        }

        _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(loginTask =>
        {
            if (loginTask.IsFaulted || loginTask.IsCanceled)
            {
                errorMessage.text = "Invalid email or password";
                Debug.LogError("Login Failed: " + loginTask.Exception);
                return;
            }

            FirebaseUser returningUser = loginTask.Result.User;
            Debug.Log("Logged in User:" + returningUser.Email);
            _uiManager.OpenHomePage();
            

            errorMessage.text = "";
        });
    }
    private void SignUp(string email, string password, string username, Label errorMessage)
    {
        email = email.Trim();
        password = password.Trim();
        username = username.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
        {
            errorMessage.text = "All fields are required.";
            Debug.LogError("Missing fields.");
            return;
        }

        if (password.Length < 6)
        {
            errorMessage.text = "Password must be at least 6 characters long.";
            Debug.LogWarning("Password too short.");
            return;
        }
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            errorMessage.text = "Email and password are required";
            Debug.LogError("Email and password required");
            return;
        }

        _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(authTask =>
        {
            if (authTask.IsFaulted || authTask.IsCanceled)
            {
                errorMessage.text = "Auth Failed";
                Debug.LogError("Auth Failed: " + authTask.Exception);
                return;
            }

            FirebaseUser newUser = authTask.Result.User;
            
            UserModel newUserProfile = new UserModel(username);
            
            _db.Collection("users").Document(newUser.UserId).SetAsync(newUserProfile).ContinueWithOnMainThread(dbTask =>
            {
                if (dbTask.IsFaulted)
                {
                    Debug.LogError("Database Failed: " + dbTask.Exception);
                    return;
                }

                _uiManager.OpenExerciseSelectionPage();
                Debug.Log($"User {username} registered successfully!");
                
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
