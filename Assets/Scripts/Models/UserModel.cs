using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class UserModel
{
    [FirestoreProperty]
    public string Username { get; set; }
    
    //This tracks the progress of the user in any given exercise
    //for example skillpath = pull and value is the current exercise ID like pullup_01
    [FirestoreProperty]
    public Dictionary<string, string> CurrentRoadmaps { get; set; }
    
    public UserModel() { } 

    public UserModel(string username)
    {
        this.Username = username;
        this.CurrentRoadmaps = new Dictionary<string, string>();
    }
}
