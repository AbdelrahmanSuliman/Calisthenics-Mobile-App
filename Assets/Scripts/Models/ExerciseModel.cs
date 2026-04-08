using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ExerciseModel
{
    [FirestoreProperty]
    public string Id { get; set; } 

    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public string Description { get; set; }

    [FirestoreProperty]
    public string GifUrl { get; set; }

    //The order of the exercise within the roadmap
    [FirestoreProperty]
    public int Order { get; set; }

    public ExerciseModel()
    {
    }

    public ExerciseModel(string id, string name, string description, string gifUrl, int order)
    {
        Id = id;
        Name = name;
        Description = description;
        GifUrl = gifUrl;
        Order = order;
    }
}