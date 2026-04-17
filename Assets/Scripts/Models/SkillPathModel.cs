using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;


[FirestoreData]
public class SkillPathModel
{
    [FirestoreDocumentId]
    public string Id { get; set; }
    
    [FirestoreProperty]
    public string PathType { get; set; } 
    
    [FirestoreProperty]
    public List<ExerciseModel> Exercises { get; set; }
    
    SkillPathModel(){}

    public SkillPathModel(string id, string pathType, List<ExerciseModel> exercises)
    {
        Id = id;
        PathType = pathType;
        Exercises = exercises;
    }
}
