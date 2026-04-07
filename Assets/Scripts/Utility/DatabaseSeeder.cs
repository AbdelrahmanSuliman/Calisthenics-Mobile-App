using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

public class DatabaseSeeder : MonoBehaviour
{
    private FirebaseFirestore _db;
    private string baseUrl = "Exercise GIFs";
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                _db = FirebaseFirestore.DefaultInstance;
                
                //SeedExercises(); 
            } else {
                Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void SeedExercises()
    {
        Debug.Log("Starting database seeding");

        List<ExerciseModel> exercisesToSeed = new List<ExerciseModel>
        {

            new ExerciseModel { 
                Id = "pushup_01", 
                Name = "Standard Pushup", 
                Description = "In a plank position with elbows in and straight, lower your body by bending your elbows to a comfortable depth keeping your elbows close to your body. Then raise your body back up by straightening your arms. ",
                GifUrl = $"{baseUrl}/Push/push up",
                Order = 1,
                PrerequisiteIds = new List<string>(), 
                SkillPath = "Push"
            },
            new ExerciseModel { 
                Id = "archer_pushup_01", 
                Name = "Archer Pushup", 
                Description = "\nStart in a regular pushup position, then as you lower down, shift your weight to one arm while extending the other arm straight out to the side. Push back up primarily using the bent arm while the straight arm provides minimal support, then alternate sides on subsequent reps.",
                GifUrl = $"{baseUrl}/Push/archer pushup",
                Order = 2,
                PrerequisiteIds = new List<string> { "pushup_01" }, 
                SkillPath = "Push"
            },
            new ExerciseModel { 
                Id = "one_arm_pushup_01", 
                Name = "One Arm Pushup", 
                Description = "Start in a pushup position but place one hand behind your back or on your hip, with feet spread wider than normal for stability. Lower your body using only the supporting arm, then\npush back up to the starting position while maintaining a straight body line throughout the movement.",
                GifUrl = $"{baseUrl}/Push/one arm pushup",
                Order = 3,
                PrerequisiteIds = new List<string> { "inclined_pushup_01" }, 
                SkillPath = "Push"
            },


            new ExerciseModel { 
                Id = "assisted_pullup_01", 
                Name = "Assisted Pullup", 
                Description = "Hang from a pull-up bar with assistance from a resistance band, partner, or your feet on a chair/box to reduce your body weight. Pull yourself up until your chin clears the bar, using th assistance to help complete the movement, then lower back down with control. Alternatively doing pull up negatives (jumping to top of pull up movement then lowering yourself as slow a you can) works as well to build up to pull ups.",
                GifUrl = $"{baseUrl}/Pull/assisted pull up",
                Order = 1,
                PrerequisiteIds = new List<string>(), 
                SkillPath = "Pull"
            },
            new ExerciseModel { 
                Id = "pullup_01", 
                Name = "Standard Pullup", 
                Description = "Hang from a pull-up bar with an overhand grip, arms fully extended and feet off the ground. Pull your body up until your chin clears the bar, then lower yourself back down with control to the dead hang position.",
                GifUrl = $"{baseUrl}/Pull/pull up",
                Order = 2,
                PrerequisiteIds = new List<string> { "assisted_pullup_01" }, 
                SkillPath = "Pull"
            },
            new ExerciseModel { 
                Id = "muscleup_01", 
                Name = "Muscle Up", 
                Description = "Start hanging from a pull-up bar, then explosively pull yourself up and transition your elbows over the bar by leaning forward and pushing down. Complete the movement by pressing yourself up above the bar until your arms are straight, essentially combining a pull-up with a dip in one fluid motion. Difficulty can be increased by using gymnastics rings.",
                GifUrl = $"{baseUrl}/Pull/muscle up",
                Order = 3,
                PrerequisiteIds = new List<string> { "pullup_01" }, 
                SkillPath = "Pull"
            },


            new ExerciseModel { 
                Id = "chair_squat_01", 
                Name = "Chair Squat", 
                Description = "Stand facing away to the chair and with straight posture, bend your legs until seated in the chair, then stand back up.",
                GifUrl = $"{baseUrl}/Legs/chair squat",
                Order = 1,
                PrerequisiteIds = new List<string>(), 
                SkillPath = "Legs"
            },
            new ExerciseModel { 
                Id = "floor_squat_01", 
                Name = "Floor Squat", 
                Description = "Lower your body by bending your knees (with straight back) to a comfortable depth, then raise your body back up by straightening your legs.",
                GifUrl = $"{baseUrl}/Legs/squat",
                Order = 2,
                PrerequisiteIds = new List<string> { "chair_squat_01" }, 
                SkillPath = "Legs"
            },
            new ExerciseModel { 
                Id = "pistol_squat_01", 
                Name = "Pistol Squat", 
                Description = "Stand on one leg with the other leg extended straight out in front of you, then squat down on the standing leg until your thigh is parallel to the ground or as low as possible. Push through your heel to return to standing while keeping the extended leg off the ground throughout the entire movement.",
                GifUrl = $"{baseUrl}/Legs/pistol squat",
                Order = 3,
                PrerequisiteIds = new List<string> { "floor_squat_01" }, 
                SkillPath = "Legs"
            }
        };

        foreach (var exercise in exercisesToSeed)
        {
            _db.Collection("exercises").Document(exercise.Id).SetAsync(exercise).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted) Debug.LogError($"Failed to seed {exercise.Name}");
                else Debug.Log($"Successfully seeded: {exercise.Name}");
            });
        }
    }
}