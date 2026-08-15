# Vodcast — AI-Powered Virtual Agent

## Description

Vodcast is an AI-powered virtual agent developed as a university project at the University of Costa Rica by a team of 2 developers. Built in Unity using C#, the agent enables natural voice interaction through a 3-stage conversational pipeline: Speech-to-Text input, Gemini LLM response generation, and Text-to-Speech output. Response time was optimized from 6+ seconds to ~3 seconds through asynchronous communication improvements.

## Tech Stack

**Engine:** Unity  
**Language:** C#  
**AI:** Google Gemini API  
**Speech:** Speech-to-Text, Text-to-Speech  
**Tools:** Git, GitHub  

## Features

- Real-time voice interaction with an AI-powered virtual agent
- 3-stage conversational pipeline: Speech-to-Text → Gemini LLM → Text-to-Speech
- Multi-component architecture applying OOP principles (inheritance, interfaces, polymorphism)
- Asynchronous communication between components for optimized response time
- 50% reduction in end-to-end response time (6+ seconds → ~3 seconds)

## Screenshots

![Screenshot](./img/Vodcast%201.png)
![Screenshot](./img/Vodcast%202.png)

## Project Structure

```
Vodcast/
├── Assets/
│   ├── Avatar/           # 3D avatar model, animations and materials
│   ├── Prefabs/          # Reusable Unity prefabs (message bubbles)
│   ├── Scenes/           # Unity scenes
│   ├── Scripts/          # C# scripts
│   │   ├── AgentController.cs   # Main agent logic
│   │   ├── ChatManager.cs       # Chat flow management
│   │   ├── LLMService.cs        # Gemini API integration
│   │   ├── SpeechToText.cs      # Voice input processing
│   │   └── TextToSpeech.cs      # Voice output processing
│   └── TextMesh Pro/     # UI text rendering
├── Packages/             # Unity package dependencies
└── ProjectSettings/      # Unity project configuration
```

## License

This project was developed for academic purposes at the University of Costa Rica.