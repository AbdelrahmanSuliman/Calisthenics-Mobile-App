using System;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UIElements;
using Firebase;
public class AuthController : MonoBehaviour
{
    private UIManager _uiManager;
    private FirebaseAuth _auth;
    private FirebaseFirestore _db;

    private UIDocument _uiDoc;


    //make sure to initialize these only once 
    private void Awake()
    {
        _auth = FirebaseManager.Instance.Auth;
        _db = FirebaseManager.Instance.Db;
        _uiManager = GetComponent<UIManager>();
        _uiDoc = GetComponent<UIDocument>();
    }

    //initialize on page load because we need it to resubscribe to events
    private void OnEnable()
    {

        var root = _uiDoc.rootVisualElement;
        
        //access buttons
        var signupBtn = root.Q<Button>("SignUpConfirm");
        var redirectBtnSignup = root.Q<Button>("RedirectButton");
        var loginBtn = root.Q<Button>("LoginConfirm");
        var redirectBtnLogin = root.Q<Button>("RedirectButtonLogin");
        
        
        //access error messages
        var signupErrorMessage = root.Q<Label>("SignupErrorMessage");
        var loginErrorMessage = root.Q<Label>("LoginErrorMessage");
        
        
        if (signupErrorMessage != null) signupErrorMessage.pickingMode = PickingMode.Ignore;
        if (loginErrorMessage != null) loginErrorMessage.pickingMode = PickingMode.Ignore;

        //Add logic to buttons
        signupBtn.clicked += HandleSignup;

        loginBtn.clicked += HandleLogin;

        redirectBtnSignup.clicked += OnRedirectToLogin;
        
        if (redirectBtnLogin != null)
        {
            ClearTextFields();
            redirectBtnLogin.clicked += () => _uiManager.OpenSignupPage(); 
        }

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
                if (loginTask.Exception?.Flatten().GetBaseException() is FirebaseException firebaseEx)
                {
                    var errorCode = (AuthError)firebaseEx.ErrorCode;
                    errorMessage.text = ErrorCodeMapper(errorCode);
                    Debug.LogError($"Login Failed: {firebaseEx.Message}");
                    return;
                }
                errorMessage.text = "Unknown error occured";
                return;
            }

            FirebaseUser returningUser = loginTask.Result.User;
            Debug.Log("Logged in User:" + returningUser.Email);

            errorMessage.text = "";
            _uiManager.OpenHomePage();
            
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
                if (authTask.Exception?.Flatten().GetBaseException() is FirebaseException firebaseEx)
                {
                    var errorCode = (AuthError)firebaseEx.ErrorCode;
                    errorMessage.text = ErrorCodeMapper(errorCode);
                    Debug.LogError($"Auth Failed: {firebaseEx.Message}");
                    return;
                }

                errorMessage.text = "Unknown error occured";
                return;
            }
            
            var newUser = authTask.Result.User;
            
            var newUserProfile = new UserModel(username);
            
            _db.Collection("users").Document(newUser.UserId).SetAsync(newUserProfile).ContinueWithOnMainThread(dbTask =>
            {
                if (dbTask.IsFaulted)
                {
                    Debug.LogError("Database Failed: " + dbTask.Exception);
                    return;
                }
                
                errorMessage.text = "";

                _uiManager.OpenExerciseSelectionPageAndLoadExercises();
                Debug.Log($"User {username} registered successfully!");
                
            });
            

        });
    }

    private string ErrorCodeMapper(AuthError err)
    {
        switch (err)
        {
            case AuthError.EmailAlreadyInUse:
                return "Email already in use";
            case AuthError.InvalidEmail:
                return "Invalid email format";
            case AuthError.WeakPassword:
                return "Weak Password";
            case AuthError.UserNotFound:
                return "No account found for this email";
            case AuthError.WrongPassword:
                return "Wrong password";
            case AuthError.InvalidCredential:
                return "Invalid email or password";
            case AuthError.NetworkRequestFailed:
                return "Network error";
            case AuthError.TooManyRequests:
                return "Too many failed attempts, try again later";
            default:
                return "Authentication failed, please try again";
        }
    }

    private void ClearTextFields()
    {
        var root = _uiDoc.rootVisualElement;

        root.Query<TextField>().ForEach(field => field.value = "");
        root.Q<Label>("SignupErrorMessage").text = "";
        root.Q<Label>("LoginErrorMessage").text = "";
    }
    
    private void HandleSignup() 
    {
        var root = _uiDoc.rootVisualElement;
        var email = root.Q<TextField>("SignupEmailInput").value;
        var pass = root.Q<TextField>("SignupPasswordInput").value;
        var user = root.Q<TextField>("SignupUsernameInput").value;
        var error = root.Q<Label>("SignupErrorMessage");

        SignUp(email, pass, user, error);
    }

    private void HandleLogin()
    {
        var root = _uiDoc.rootVisualElement;
        var email = root.Q<TextField>("LoginEmailInput").value;
        var pass = root.Q<TextField>("LoginPasswordInput").value;
        var error = root.Q<Label>("LoginErrorMessage");

        LogIn(email, pass, error);
    }
    
    private void OnRedirectToLogin()
    {
        ClearTextFields();
        _uiManager.OpenLoginPage();
    }

    private void OnDisable()
    {
        var root = _uiDoc.rootVisualElement;
        if (root == null) return;

        var signupBtn = root.Q<Button>("SignUpConfirm");
        if (signupBtn != null) signupBtn.clicked -= HandleSignup;

        var loginBtn = root.Q<Button>("LoginConfirm");
        if (loginBtn != null) loginBtn.clicked -= HandleLogin;
    }

}
