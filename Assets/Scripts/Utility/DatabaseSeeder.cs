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
        _db = FirebaseFirestore.DefaultInstance;
        //SeedRoadmaps();
    }

    private void SeedRoadmaps()
    {
        Debug.Log("Starting roadmap seeding");

        SkillPathModel pullPath = new SkillPathModel("pull_progression", "PULL", new List<ExerciseModel>
        {
            new ExerciseModel(
                "assisted_pullup_01", 
                "Assisted Pullup", 
                "Hang from a pull-up bar with assistance from a resistance band, partner, or your feet on a chair/box to reduce your body weight. Pull yourself up until your chin clears the bar, using the assistance to help complete the movement, then lower back down with control.", 
                $"{baseUrl}/Pull/assisted pull up", 
                1),
            new ExerciseModel(
                "pullup_01", 
                "Standard Pullup", 
                "Hang from a pull-up bar with an overhand grip, arms fully extended and feet off the ground. Pull your body up until your chin clears the bar, then lower yourself back down with control to the dead hang position.", 
                $"{baseUrl}/Pull/pull up", 
                2),
            new ExerciseModel(
                "muscleup_01", 
                "Muscle Up", 
                "Start hanging from a pull-up bar, then explosively pull yourself up and transition your elbows over the bar by leaning forward and pushing down. Complete the movement by pressing yourself up above the bar until your arms are straight.", 
                $"{baseUrl}/Pull/muscle up", 
                3)
        });

        SkillPathModel pushPath = new SkillPathModel("push_progression", "PUSH", new List<ExerciseModel>
        {
            new ExerciseModel(
                "pushup_01", 
                "Standard Pushup", 
                "In a plank position with elbows in and straight, lower your body by bending your elbows to a comfortable depth keeping your elbows close to your body. Then raise your body back up by straightening your arms.", 
                $"{baseUrl}/Push/push up", 
                1),
            new ExerciseModel(
                "archer_pushup_01", 
                "Archer Pushup", 
                "Start in a regular pushup position, then as you lower down, shift your weight to one arm while extending the other arm straight out to the side. Push back up primarily using the bent arm.", 
                $"{baseUrl}/Push/archer pushup", 
                2),
            new ExerciseModel(
                "one_arm_pushup_01", 
                "One Arm Pushup", 
                "Start in a pushup position but place one hand behind your back or on your hip, with feet spread wider than normal for stability. Lower your body using only the supporting arm, then push back up to the starting position.", 
                $"{baseUrl}/Push/one arm pushup", 
                3)
        });

        SkillPathModel legsPath = new SkillPathModel("legs_progression", "LEGS", new List<ExerciseModel>
        {
            new ExerciseModel(
                "chair_squat_01", 
                "Chair Squat", 
                "Stand facing away from a chair with straight posture. Bend your legs until seated in the chair, then stand back up to the starting position.", 
                $"{baseUrl}/Legs/chair squat", 
                1),
            new ExerciseModel(
                "floor_squat_01", 
                "Floor Squat", 
                "Lower your body by bending your knees (with a straight back) to a comfortable depth, then raise your body back up by straightening your legs.", 
                $"{baseUrl}/Legs/squat", 
                2),
            new ExerciseModel(
                "pistol_squat_01", 
                "Pistol Squat", 
                "Stand on one leg with the other leg extended straight out in front of you. Squat down on the standing leg until your thigh is parallel to the ground, then push through your heel to return to standing.", 
                $"{baseUrl}/Legs/pistol squat", 
                3)
        });
      

        UploadRoadmap(pullPath);
        UploadRoadmap(pushPath);
        UploadRoadmap(legsPath);
    }

    private void UploadRoadmap(SkillPathModel roadmap)
    {
        _db.Collection("SkillRoadmaps").Document(roadmap.Id).SetAsync(roadmap).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted) Debug.LogError($"Failed to seed {roadmap.PathType}");
            else Debug.Log($"Successfully seeded {roadmap.PathType} roadmap");
        });
    }
}