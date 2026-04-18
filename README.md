# Calisthenics Mobile App

A comprehensive mobile application built with Unity designed to help fitness enthusiasts learn, track, and master calisthenics skills. The app provides structured exercise roadmaps, progress tracking, and visual guides for various bodyweight movements.

## Features

* **User Authentication:** Secure signup and login functionality utilizing Firebase.
* **Structured Skill Paths:** Step-by-step progressions for advanced calisthenics moves (e.g., Muscle-Ups, Pistol Squats, One-Arm Push-Ups).
* **Exercise Library:** A categorized database of exercises (Push, Pull, Legs) complete with visual demonstrations.
* **Progress Tracking:** Log workouts and monitor your journey through different skill roadmaps.
* **Modern UI:** Built using Unity's UI Toolkit and TextMesh Pro for a crisp, responsive, and scalable mobile experience.

## Tech Stack

* **Engine:** Unity (Configured for Mobile: Android & iOS)
* **Language:** C#
* **Backend:** Firebase (Authentication, Realtime Database/Firestore)
* **UI Framework:** Unity UI Toolkit, TextMesh Pro
* **Input Handling:** Unity New Input System

## Project Architecture

The codebase follows a modular structure to separate data, logic, and presentation:

* **`Assets/Scripts/Controllers/`**: Manages application logic, routing, and UI interactions (`AuthController`, `HomeScreenController`, `ExerciseSelectionController`, `UIController`).
* **`Assets/Scripts/Models/`**: Contains the data structures representing application state (`UserModel`, `ExerciseModel`, `SkillPathModel`, `RoadmapProgressModel`, `WorkoutLogModel`).
* **`Assets/Scripts/Utility/`**: Houses helper classes, including backend integration (`FirebaseManager`) and initial data population (`DatabaseSeeder`).
* **`Assets/Resources/Exercise GIFs/`**: Categorized visual assets demonstrating proper form for various exercises (Push, Pull, Legs).
* **`Assets/UI Toolkit/`**: Contains the UXML and USS files defining the application's layout and styling.

## Getting Started

### Prerequisites

* Unity Hub and Unity Editor (Check `ProjectSettings/ProjectVersion.txt` for the exact required version).
* A Google Firebase account.
* Android Build Support and/or iOS Build Support modules installed in your Unity Editor.

### Installation

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/abdelrahmansuliman/calisthenics-mobile-app.git](https://github.com/abdelrahmansuliman/calisthenics-mobile-app.git)
