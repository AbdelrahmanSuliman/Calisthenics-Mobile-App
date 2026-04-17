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
        var redirectBtnSignup = root.Q<Button>("RedirectButton");
        var loginBtn = root.Q<Button>("LoginConfirm");
        var redirectBtnLogin = root.Q<Button>("RedirectButtonLogin");
        
        
        //access error messages
        var signupErrorMessage = root.Q<Label>("SignupErrorMessage");
        var loginErrorMessage = root.Q<Label>("LoginErrorMessage");
        
        
        if (signupErrorMessage != null) signupErrorMessage.pickingMode = PickingMode.Ignore;
        if (loginErrorMessage != null) loginErrorMessage.pickingMode = PickingMode.Ignore;

        //Add logic to buttons
        signupBtn.clicked += () =>
        {
            SignUp(signupEmailInput.value, signupPasswordInput.value, signupUsernameInput.value, signupErrorMessage);
        };

        loginBtn.clicked += () =>
        {
            LogIn(loginEmailInput.value, loginPasswordInput.value, loginErrorMessage);
        };

        redirectBtnSignup.clicked += () =>
        {
            _uiManager.OpenLoginPage();
        };
        
        if (redirectBtnLogin != null)
        {
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
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
