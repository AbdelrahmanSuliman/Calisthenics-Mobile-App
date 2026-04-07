using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ExerciseModel
{
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
    
    //All exercise IDs that come before it
    [FirestoreProperty]
    public List<string> PrerequisiteIds { get; set; }

    [FirestoreProperty]
    public string SkillPath { get; set; }

    public override string ToString()
    {
        string prereqs = (PrerequisiteIds != null && PrerequisiteIds.Count > 0)
            ? string.Join(", ", PrerequisiteIds)
            : "None";

        return $"[EXERCISE: {Name}]\n" +
               $"- ID: {Id}\n" +
               $"- Path: {SkillPath} (Order: {Order})\n" +
               $"- Description: {Description}\n" +
               $"- GIF: {GifUrl}\n" +
               $"- Prerequisites: {prereqs}\n" +
               $"------------------------------";
    }


    public void division(float num1, float num2)
    {
        Debug.Log(num1/num2);
    }
    
    
}