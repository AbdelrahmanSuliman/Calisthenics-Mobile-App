using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class UserModel
{
    [FirestoreProperty]
    public string Username { get; set; }
    
    [FirestoreProperty]
    public List<RoadmapProgressModel> CurrentRoadmaps { get; set; } = new List<RoadmapProgressModel>();
    
    public UserModel() { } 

    public UserModel(string username)
    {
        this.Username = username;
        this.CurrentRoadmaps =  new List<RoadmapProgressModel>();
    }
}
