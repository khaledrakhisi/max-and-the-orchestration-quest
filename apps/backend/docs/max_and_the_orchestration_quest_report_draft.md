# Max and the Orchestration Quest

**Written Summary for the Gamification Innovation Lab (GIL)**

**Project title:** Max and the Orchestration Quest  
**Course:** Gamification Innovation Lab  
**Submission:** April 2026  
**Team members:** Khaled, Velvin, Shiva, Nafeesa, Joani  

**Suggested contribution mapping for presentation and report**

- Khaled and Velvin: Unity game client and gameplay implementation
- Shiva: Docker integration concepts and real-time container interaction
- Nafeesa: UI/UX, visual presentation, and frontend-facing materials
- Joani: Backend API service, data persistence, authentication, and mission logic

---

## Abstract

Max and the Orchestration Quest is a serious game designed to introduce beginners to core Docker concepts through an interactive Unity-based game experience. Instead of teaching container technology only through command-line documentation, the project transforms fundamental operations such as listing images, pulling an image, creating a container, starting a container, and observing container resource usage into game tasks that can be triggered inside a playable environment. The aim of the project was to reduce the initial barrier to learning Docker by combining technical actions with game mechanics, visual feedback, and guided progression.

The implemented system uses a split backend architecture. A dedicated WebSocket server provides real-time communication between the Unity client and Docker so that time-sensitive events such as image pulling progress and container statistics can be streamed back to the game immediately. A separate HTTP API server handles the parts of the application that are better represented as structured request-response workflows, including authentication, user management, mission state, experience points, and persistent storage in MongoDB. This separation keeps responsibilities clear and makes the overall design easier to extend.

The project demonstrates that gamification can make technically complex topics more approachable without removing the underlying concepts. The final prototype successfully connects Unity to backend services, performs live Docker operations, stores player-related data, and tracks mission completion and experience points. At the same time, the work revealed practical limits, especially regarding multiplayer support, deployment complexity, and the challenge of integrating infrastructure-oriented tools into a game environment. Overall, the project provides a functional proof of concept for teaching Docker basics through a playful and technically grounded application.

## 1. Introduction and Task Description

Docker has become a standard tool in modern software engineering because it allows developers to package applications together with their dependencies and run them in isolated, reproducible environments. In professional practice, Docker is used for local development, testing, deployment, and the preparation of portable workloads. Despite its importance, Docker can be difficult for beginners to approach. The first encounter often involves unfamiliar terminology such as images, containers, registries, exposed ports, and resource limits. In addition, many learning resources assume comfort with command-line interaction, which can discourage users who are still trying to build an intuitive understanding of the platform.

This project addresses that learning challenge by transforming Docker basics into a game-based learning experience. The central idea behind Max and the Orchestration Quest is that the player learns by interacting with Docker-related tasks inside a Unity environment rather than by reading instructions alone. Instead of typing every command manually, the learner performs actions inside the game world and receives feedback from backend services that execute or simulate real Docker operations. In this way, the game aims to make container concepts less abstract and more memorable.

The project was developed as a multidisciplinary team effort combining game development, backend engineering, Docker integration, and interface design. The final prototype includes a Unity game client, a real-time backend service for Docker communication, and an API service for user and mission data. MongoDB is used to store persistent player information such as mission progress and experience points. The broader objective was not to create a complete production learning platform, but to produce a working prototype that demonstrates how gamification can support the teaching of technical infrastructure concepts.

From a task perspective, the project had three main goals. First, it needed to represent Docker concepts through concrete in-game interactions. Second, it needed a backend capable of communicating with both the game and the Docker environment. Third, it needed a data layer to keep track of player progress so that the game could evolve beyond a one-time demonstration and toward a structured learning system with missions and rewards. These three goals shaped the architecture of the final system.

An important aspect of the project is that it combines two different kinds of interaction. Some actions, such as listing images or monitoring running containers, require immediate or continuous feedback and therefore fit a real-time communication model. Other actions, such as registering a user, logging in, retrieving mission data, or marking a mission as complete, are inherently transactional and benefit from a standard HTTP API. The final implementation reflects this distinction by separating the backend into two services with different communication patterns.

The current version of the system should therefore be understood as a technically grounded educational prototype. It does not aim to replace official Docker documentation. Instead, it acts as an entry point that makes core ideas easier to explore before students move on to more advanced and formal learning materials. In that sense, the project sits at the intersection of serious games, software engineering education, and interactive systems design.

## 2. State of the Art / Used Technologies

The technology stack for the project was selected based on the different responsibilities within the system. Because the project combines an interactive game, backend logic, real-time events, and persistent data, no single tool could reasonably cover all requirements. The chosen technologies therefore reflect a modular architecture.

### 2.1 Unity

Unity was used as the main client technology for the game. Its role in the project was not just visual presentation but also interaction management. The game world provides the player with an environment in which technical tasks are translated into actions such as selecting an image, downloading it, or triggering container-related events through interactable objects. Unity is well suited for this because it supports scene-based design, object interaction, animation, and event-driven scripting. In this project, Unity acts as the layer that turns technical learning into embodied gameplay.

Another important reason for choosing Unity is that it allows technical events to be represented visually. A pulled image, a running container, or a machine state change can be made visible through animations, feedback text, and object behavior. This supports the educational goal of the project: making infrastructure concepts easier to understand by connecting them to direct player actions and visible consequences.

### 2.2 React

React was part of the broader project ecosystem for web-facing interface components. In the context of this report, React is relevant as a supporting technology rather than the main focus of the final prototype. React is a component-based JavaScript library for user interfaces, which makes it a suitable choice when an application needs reusable interface elements, dashboards, or administrative views. In a larger version of the project, React can support features such as leaderboards, user-facing dashboards, or additional web tools around the core game experience.

Although the playable experience itself is centered on Unity, React remains part of the technological context of the project because it represents the direction for complementary interface development beyond the game client. This is especially relevant if the system is extended toward account management, content administration, or analytics dashboards.

### 2.3 Docker and Docker Engine

Docker is the technical domain around which the learning experience is built. The project focuses on core beginner concepts such as Docker images, containers, and registries. Docker Engine follows a client-server architecture in which a client submits requests and the Docker daemon performs actions on the host system. This architectural model is directly relevant to the project because the backend service must act as a bridge between the game and the Docker environment.

Docker was chosen not only because of its importance in software development, but also because its operations map well to mission-based gameplay. Pulling an image, creating a container, starting a container, and monitoring resource consumption are discrete actions that can be represented as player goals. This makes Docker particularly suitable for a gamified educational prototype.

### 2.4 WebSockets

The project uses WebSockets for real-time communication between the Unity client and the Docker-facing backend service. WebSockets enable bidirectional communication over a persistent connection, which is a better fit than plain HTTP for events that evolve over time. In this project, that includes streaming container statistics and providing intermediate progress updates during long-running operations such as pulling an image.

The use of WebSockets is especially justified by the gameplay context. A game client benefits from low-latency updates and continuous feedback. When the player triggers a Docker-related action, it is more natural for the server to send asynchronous updates instead of forcing the client to repeatedly poll for status changes.

### 2.5 FastAPI

The HTTP API service was implemented with FastAPI. FastAPI is a Python framework for building APIs with strong support for type hints, data validation, and clear endpoint definitions. It fits the project well because the API server primarily handles structured business operations such as registration, login, mission retrieval, and progress updates. These are not real-time streaming problems; they are request-response problems with validation and persistence requirements.

FastAPI also supports automatic request parsing and response modeling, which helped keep the backend implementation readable and maintainable. In the current implementation, it serves as the foundation for the routes defined in `api_server.py` and provides a clean interface between the game or future clients and the persistence layer.

### 2.6 MongoDB

MongoDB was used as the database layer for storing user and mission data. The document-oriented model was a good fit because the application data naturally consists of user profiles, badges, mission entries, timestamps, and progress states that can be represented in JSON-like documents. This flexible structure was especially useful during development because the project evolved over time and the data model needed to adapt without the overhead of a rigid relational schema.

The database currently stores information such as users, total experience points, levels, badges, mission identifiers, mission status, and timestamps for mission start and completion. This persistence layer is necessary because the project is not only a live demonstration of Docker commands, but also a progression-based learning experience.

### 2.7 JWT, Password Hashing, and Validation

Authentication-related technologies are another important part of the backend stack. Password handling in the API service uses hashing rather than plain-text storage, which is a basic but essential security principle. Authentication tokens are represented as JSON Web Tokens (JWTs), allowing the API to issue compact credentials after login or registration. Even in the simplified single-primary-user version of the project, this foundation is important because it shows how the system can support account-based progression.

Validation is handled through Pydantic models inside the FastAPI service. This provides structured input parsing and typed response models, which improves both correctness and clarity. For an educational prototype, this is valuable because it reduces avoidable backend errors and makes the interface contract more explicit.

## 3. Methodology

### 3.1 Overall System Architecture

The final system follows a split-service architecture with Unity as the main client. The player interacts with the game world through the Unity application. From there, two backend paths exist depending on the kind of action being performed.

The first path is the Docker WebSocket server. This service is responsible for all operations that require real-time interaction with Docker. It accepts messages from the game, translates them into Docker SDK calls, and sends responses or status updates back to Unity. This includes image listing, image pulling, container creation, container start and stop operations, image and container removal, and the streaming of container statistics.

The second path is the API server. This service is responsible for persistent application logic: user registration, login, user retrieval, mission retrieval, mission state transitions, and the assignment of experience points when a mission is completed. It connects to MongoDB, where user and mission documents are stored.

MongoDB acts as the shared persistence layer for player-related data. It stores the state that must survive beyond a single play session, such as user profiles, total XP, level, badges, and mission progress. This distinction between volatile technical operations and persistent game state is one of the key structural decisions in the project.

The architecture can be summarized as follows:

```text
Player
  ->
Unity Game Client
  -> WebSocket Server -> Docker Engine / Docker Registry
  -> API Server       -> MongoDB
```

This split architecture reflects the difference between two communication styles. Docker-related events are time-sensitive and event-driven, while user and mission management is transactional and state-oriented. Separating them keeps each service easier to reason about and extend.

### 3.2 Game and Learning Design

The educational design of the project is based on the idea that a beginner should not be confronted with Docker only as a list of commands. Instead, the player should encounter the underlying ideas through guided actions embedded in a game world. Missions provide this structure. Each mission corresponds to a learning goal, for example retrieving images, pulling a specific image, or creating a first container.

This mission-oriented design serves several purposes. First, it breaks a potentially intimidating technical topic into smaller, goal-driven steps. Second, it gives the player a sense of progression by connecting completed tasks to rewards such as XP. Third, it provides a natural place for backend persistence, because mission progress is meaningful only if it can be stored and revisited.

From the perspective of gamification, this design turns technical interaction into a sequence of challenges, feedback, and rewards. The educational logic is therefore not separate from the system architecture. The backend is part of the learning design because it tracks which tasks have been started, which have been completed, and how the player profile evolves over time.

### 3.3 Docker WebSocket Server

The Docker-facing service is implemented in `docker_server.py` and uses Python together with the Docker SDK and the `websockets` library. Its main purpose is to connect Unity to Docker in a way that supports asynchronous, low-latency feedback.

The service initializes a Docker client and listens for WebSocket connections on `ws://localhost:8765`. When the Unity client sends a message, the server dispatches that message to a function that performs the corresponding Docker operation. This approach allows the game to send short commands such as `list_images`, `pull_image:<name>`, or `create_container:<image>:<cpu>:<ram>` and receive machine-readable responses.

The main supported operations are:

- listing available Docker images
- listing existing containers
- pulling an image from a registry
- creating a container from a chosen image
- starting a container
- stopping a container
- removing a container
- removing an image
- streaming container statistics to the client

WebSockets were the correct choice here because some of these operations unfold over time. Pulling an image can take several seconds and should expose intermediate progress messages. Container statistics can change continuously and need to be pushed to the client without repeated polling. In a game context, this improves responsiveness and gives the player a clearer sense of cause and effect.

At the same time, the final report should not overemphasize this service. It is an important part of the project, but it is only one part of the system. In the earlier draft, this section dominated the report because it included long raw message examples and large code outlines. In the revised structure, the server is still explained technically, but at the level of architecture, communication style, and main supported commands rather than line-by-line implementation detail.

### 3.4 API Server Design and Implementation

The API service is the part of the backend that I implemented and structured as the main application service excluding the Docker WebSocket server. Its responsibility is fundamentally different from the real-time Docker integration. Instead of streaming events, it manages stable application data and domain logic: users, authentication, missions, and experience points.

The service is implemented with FastAPI in `api_server.py`. FastAPI was chosen because it provides a clean way to declare endpoints, validate request bodies, structure responses, and raise meaningful HTTP errors. This is especially useful for game-related application logic, where account creation, login, mission access, and state updates all depend on predictable request-response contracts.

The API follows a layered structure:

- the route layer in `api_server.py` defines the HTTP endpoints
- the domain layer contains business logic such as authentication and mission rules
- the repository layer handles database operations against MongoDB
- the model layer defines typed structures for users and missions

This separation improves maintainability because each layer has a specific job. Routes translate incoming HTTP requests into application operations. Domain classes encapsulate rules such as when a mission can be started or completed. Repository classes isolate the details of MongoDB queries and updates. Pydantic-backed models make the data exchanged by the application explicit and safer to work with.

The API service also configures Cross-Origin Resource Sharing (CORS), which is important when different clients need to access the server during development. Even though the immediate game client is Unity, the backend is designed as a reusable service that can also serve future web interfaces or testing tools. This makes the system more flexible than a tightly coupled one-off implementation.

Another reason to introduce a separate HTTP API server is conceptual clarity. Not every system interaction benefits from a WebSocket model. Authentication, user retrieval, mission loading, and progress updates are better treated as durable resource operations. They need validation, structured status codes, and database integration rather than continuous streams. REST-style endpoints are therefore a better fit than WebSockets for this part of the platform.

This separation also improves extensibility. If the project later restores full multiplayer support or adds more mission types, the API service can evolve independently from the Docker integration server. Similarly, the WebSocket server can focus on infrastructure-facing tasks without becoming overloaded with account and progression logic.

### 3.5 Authentication, User Data, and Mission Progress

The API service currently supports user registration and login, and it lays the groundwork for multi-user play even though the present version of the game is simplified around a primary user. This was a practical compromise between the original multiplayer ambition and the constraints of the prototype.

The authentication flow starts with registration, where a username, email address, and password are accepted by the API. Passwords are normalized and hashed before they are stored, which means the system does not keep raw passwords in the database. During login, the submitted password is verified against the stored hash. After successful registration or login, the API issues a JWT-based access token that can represent the authenticated user in later interactions.

The user data model stores more than identity information. It also includes total XP, level, badges, and creation timestamps. This is important for the educational design because the player is not only performing isolated technical tasks; the player is building a progression profile. The backend therefore acts as the memory of the learning experience.

Mission progress is handled through dedicated API endpoints and a mission domain layer. The relevant mission operations include:

- retrieving all missions
- retrieving a mission by identifier
- starting a mission
- completing a mission

The completion flow is particularly important because it couples application logic and persistence. When a mission is completed, the system updates the mission status, writes the completion timestamp, and adds the mission's XP value to the player's total XP. This means the report can describe a concrete and meaningful backend workflow rather than a generic CRUD interface.

The current implementation also includes a simplification for the single-player version of the prototype: a primary user can be resolved without requiring the full multiplayer handling envisioned earlier in development. This decision allows the project to remain functional and demonstrable while preserving the architectural basis for user accounts and authenticated progression.

MongoDB stores this data in collections such as `users` and `missions`. This document-oriented structure fits the project well. A mission entry can include identifiers, user references, timestamps, status fields such as `not_started`, `in_progress`, or `completed`, and an XP value. A user entry can include profile data, current XP total, level, badges, and timestamps. Because the report is concerned with applied software engineering rather than database theory, the most important point is that the backend persists learning progress as structured, game-relevant state.

### 3.6 Key Challenges and Design Decisions

One of the main design challenges was deciding how to divide responsibilities across backend services. A single backend process could have handled everything, but that would have mixed real-time Docker communication with persistent application logic. The final split between `docker_server.py` and `api_server.py` produced a cleaner architecture in which each service matches a distinct communication problem.

Another challenge was the tension between the original multiplayer direction and the practical reality of the prototype. The backend was initially designed with user accounts and login in mind, but the current implementation uses a primary-user simplification to keep the system stable and demonstrable. This was a sensible tradeoff because it preserved the structure needed for future multiplayer work without blocking progress on the core learning experience.

A third challenge involved consistency in mission completion and XP assignment. Mission progress is not only a visual feature; it is a part of the educational feedback loop. The backend therefore needed to ensure that completing a mission updates both the mission document and the user's persistent XP total in a predictable way. This makes the game progression meaningful and prepares the project for future expansion.

## 4. Results

The final prototype demonstrates that the combination of a game client, Docker integration, and persistent backend services can produce a coherent learning experience. Several concrete outcomes were achieved.

First, the system successfully connects the Unity client to the Docker-facing WebSocket server. This allows the player to trigger Docker-related actions from within the game and receive feedback without leaving the game environment. Image-related and container-related commands can therefore be experienced as game interactions rather than isolated technical exercises.

Second, the project implements a working API service for player identity and progression. Users can be registered and authenticated, and user-related data can be retrieved and updated through structured HTTP endpoints. This transforms the project from a one-session demonstration into a system that can support persistent learning progress.

Third, mission handling is operational. Missions can be stored, queried, started, and completed. Completion updates player XP, which means the system supports a basic but meaningful reward loop. From a serious game perspective, this is important because it ties technical learning tasks to motivation and progression.

Fourth, the integration between gameplay and backend logic is visible in the overall flow. A player can begin in the game, trigger a Docker-oriented task through the Unity interface, receive real-time responses from the WebSocket server, and rely on the API service to preserve the broader learning state. This is one of the strongest outcomes of the project because it shows that the system is more than a collection of disconnected parts.

The prototype should still be evaluated as a proof of concept rather than a finished product. However, as a proof of concept it succeeds in three important ways:

- it makes Docker concepts interactive
- it demonstrates a clear division of backend responsibilities
- it stores and rewards player progress rather than limiting the experience to transient actions

For the final submitted report, this section should ideally include:

- one architecture diagram showing the relationship between Unity, the two backend services, Docker, and MongoDB
- one screenshot of the game interface during a Docker-related task
- one short example API flow for mission completion
- one short example WebSocket flow for image pulling or stats streaming

These visuals would communicate the achieved functionality more effectively than raw code or long JSON listings.

## 5. Discussion

The project shows that a split-server architecture works well for an educational game that mixes real-time infrastructure operations with persistent user progression. The Docker WebSocket server and the API server solve different problems, and separating them reduced complexity compared to a monolithic design. This separation is one of the strongest architectural outcomes of the project because it aligns communication style with application responsibility.

The revised interpretation of the backend is also more accurate than the earlier report draft. The earlier version focused almost entirely on the WebSocket-Docker middleware and therefore underrepresented the broader backend work required to make the project feel like a progression-based game. In practice, the API service is essential because it manages user identity, mission state, experience points, and persistence. Without that layer, the project would be a live Docker demonstration inside Unity, but not a structured learning platform.

At the same time, several limitations remain. The current version is effectively simplified around one primary user, even though the architectural direction initially aimed at multiplayer support. The system also depends on a local Docker environment and a local MongoDB setup, which limits portability. In addition, while the backend services are clearly separated from each other, the relationship between gameplay systems and infrastructure concepts could be expanded further to create a richer teaching experience.

Another lesson concerns the difficulty of bringing infrastructure-oriented tooling into a game environment. Docker is normally used through the command line, APIs, or deployment pipelines, not through physics-based game interactions. Translating these concepts into gameplay was therefore both the innovative part of the project and one of its hardest engineering tasks. The project had to maintain educational accuracy while also staying understandable and playable.

The work also highlighted the value of persistence in serious games. Missions, XP, and user profiles are not just technical extras. They provide continuity and make the system feel like a learning journey rather than a disconnected sequence of demos. From this perspective, the API service is not merely a support system; it is part of the pedagogical structure of the application.

Overall, the project demonstrates that educational software benefits from architecture choices that mirror educational goals. Real-time systems should support responsiveness and feedback. Persistent systems should support progression, memory, and structure. Max and the Orchestration Quest moved toward that alignment, even though the prototype remains limited in scope.

## 6. Conclusion and Outlook

Max and the Orchestration Quest demonstrates a practical approach to teaching Docker basics through a serious game. By combining a Unity client, a Docker-facing WebSocket server, and a separate API server connected to MongoDB, the project turns technical actions into guided, persistent, and interactive learning tasks. The prototype proves that container concepts can be introduced through gameplay without removing their technical foundations.

The backend contribution described in this report is especially important for that outcome. The API service provides structure to the learning experience by managing identity, missions, progress, and reward logic. In combination with the Docker WebSocket server, it helps divide the application into real-time infrastructure interaction on one side and persistent game progression on the other.

There are several clear directions for future work. The most important is the restoration of full multiplayer or multi-user support, which would better match the original ambition of the project. A second direction is the expansion of the mission system so that learning pathways cover more Docker and orchestration concepts. A third direction is improved deployment, for example by preparing the services for cloud or containerized hosting rather than relying purely on local development environments. Additional dashboard or content-management tools could also improve usability for instructors and future teams.

In conclusion, the project achieved its main educational and technical goal: it built a functioning prototype that makes Docker concepts more approachable through an interactive game while also illustrating the value of a carefully structured backend architecture.

## 7. References

[1] Docker Inc. "What is Docker?" Docker Docs. Available at: https://docs.docker.com/engine/docker-overview/

[2] Docker Inc. "Docker Documentation." Available at: https://docs.docker.com/

[3] FastAPI. "FastAPI Documentation." Available at: https://fastapi.tiangolo.com/

[4] Pydantic. "Welcome to Pydantic." Available at: https://docs.pydantic.dev/latest/

[5] MongoDB. "What is MongoDB?" MongoDB Documentation. Available at: https://www.mongodb.com/docs/manual/

[6] Unity. "Unity Documentation." Available at: https://docs.unity.com/

[7] React. "Describing the UI." React Documentation. Available at: https://react.dev/learn/describing-the-ui

[8] Python Software Foundation. "Python Documentation." Available at: https://docs.python.org/

[9] Fette, I., and Melnikov, A. "RFC 6455: The WebSocket Protocol." Internet Engineering Task Force, 2011. Available at: https://datatracker.ietf.org/doc/html/rfc6455

[10] Jones, M., Bradley, J., and Sakimura, N. "RFC 7519: JSON Web Token (JWT)." Internet Engineering Task Force, 2015. Available at: https://datatracker.ietf.org/doc/html/rfc7519

[11] Gamification Innovation Lab. "Presentation & Written Summary – Student Guidelines." Course document provided for the exam preparation and written report.

## 8. Appendix

### Appendix A: API Endpoints

- `GET /`
  Returns a health message for the API server.
- `POST /register`
  Registers a new user and returns an access token.
- `POST /login`
  Authenticates a user and returns an access token.
- `GET /users`
  Returns all users.
- `GET /user`
  Returns the primary user used in the current simplified single-player version.
- `GET /users/{user_id}`
  Returns a specific user by identifier.
- `POST /users`
  Creates a user directly.
- `PATCH /users/{user_id}`
  Updates user data.
- `DELETE /users/{user_id}`
  Deletes a user.
- `GET /missions`
  Returns all mission entries.
- `GET /missions/{mission_id}`
  Returns a mission by identifier.
- `POST /missions/{mission_id}/start`
  Marks a mission as started.
- `POST /missions/{mission_id}/complete`
  Marks a mission as completed and awards XP to the player.

### Appendix B: Main WebSocket Messages

- `list_images`
  Requests the available Docker images.
- `list_containers`
  Requests the available containers and their states.
- `pull_image:<image_name>`
  Pulls an image and sends intermediate progress messages.
- `create_container:<image_name>:<cpu>:<ram>`
  Creates a new container from the selected image.
- `start_container:<container_name>`
  Starts a chosen container.
- `stop_container:<container_name>`
  Stops a running container.
- `remove_container:<container_name>`
  Removes a container.
- `remove_image:<image_name>`
  Removes an image.
- `stats:<container_name>`
  Streams resource information for a running container.

### Appendix C: Suggested Figures for the Final Submission

- Figure 1: Overall system architecture
- Figure 2: Unity interface showing a Docker-related task
- Figure 3: Mission flow from start to completion and XP update
- Figure 4: Example real-time message flow for image pulling or stats streaming

