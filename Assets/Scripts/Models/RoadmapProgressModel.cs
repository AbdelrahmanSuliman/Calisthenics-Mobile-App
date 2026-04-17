using UnityEngine;
using Firebase.Firestore;

[FirestoreData]
public class RoadmapProgressModel 
{

        [FirestoreProperty]
        public string SkillPathId { get; set; } 
    
        [FirestoreProperty]
        public string CurrentExerciseId { get; set; }

        public RoadmapProgressModel() { } 

        public RoadmapProgressModel(string pathId, string exerciseId)
        {
            SkillPathId = pathId;
            CurrentExerciseId = exerciseId;
        }

}
