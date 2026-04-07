using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class WorkoutLogModel
{
    [FirestoreProperty] public string ExerciseId { get; set; }
    [FirestoreProperty] public int Reps { get; set; }
    [FirestoreProperty] public Timestamp Date { get; set; }
    [FirestoreProperty] public string UserId { get; set; }

    public WorkoutLogModel() {}

    public WorkoutLogModel(string userId, string exerciseId, int reps)
    {
        UserId = userId;
        ExerciseId = exerciseId;
        Reps = reps;
        Date = Timestamp.GetCurrentTimestamp();
    }
}