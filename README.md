![Poster](./assets/design/poster.png)
# 📺 Max and The Docker Orchestration Quest

Project Goal: A Serious Game designed to teach fundamental Container Orchestration (Docker) concepts by transforming deployment tasks into a fun, physics-based 2D platformer. The game client (Unity) triggers real Docker commands via a Node.js API.

Project Architecture: Monorepo using Unity (Client), Node.js/TypeScript (API), and React.js (Web Dashboard) managed by Yarn Workspaces.

📺 Project Setup (Local Development)

This guide walks you through setting up the Node.js backend and the React frontend components for development.

📺 Prerequisites

You must have the following software installed on your local machine:

- Git
- Node.js (LTS version recommended)
- Yarn (We use Yarn Workspaces for monorepo management)
- Docker Desktop (Required for local database and running deployment targets)

Step 1: Clone the Repository

Clone the project from GitHub and navigate into the root directory.

# Clone using the official repo name
git clone git@github.com:khaledrakhisi/max-and-the-orchestration-quest.git max-and-the-orchestration-quest
cd max-and-the-orchestration-quest

Step 2: Install Dependencies and Configure Workspaces

This step installs all required Node.js and React dependencies across the entire monorepo using Yarn Workspaces.

# 1. Install all dependencies across the Backend and Frontend workspaces
yarn install

# 2. Create the necessary environment file for the backend (CRITICAL: DO NOT COMMIT THIS)
cp docker/.env.example apps/backend/.env


(You may need to edit apps/backend/.env to specify your MongoDB connection string or custom port.)

Step 3: Run the Backend API (Node.js/TypeScript)

The backend server manages game logic and securely executes Docker commands. This uses the dev script from the apps/Backend/package.json.

# Execute the 'dev' script defined in the root package.json for the backend workspace
yarn workspace backend dev


Expected Output: The server should start on port 3000 (or the port specified in .env). Look for the confirmation: Server listening on port 3000.

Step 4: Run the Frontend Dashboard (React.js)

The frontend provides the administrative dashboard and leaderboards (React application). This uses the standard start script from the apps/frontend/package.json.

# Execute the 'start' script defined for the Frontend workspace
yarn workspace frontend start


Expected Output: The React application will usually start on http://localhost:3001 (or another available port). This dashboard connects to the API running on port 3000.

🛑 Docker and Unity Setup (Advanced)

Docker / Database Setup

To run the MongoDB database service locally for development:

# Start the MongoDB container defined in the docker-compose.yml
docker compose up -d mongodb


Unity Game Client

The Unity project is located in apps/unity/.

Open Unity Hub and add the apps/unity folder as a project.

Open the project in Unity Editor (Version 6.1 or higher recommended).

Ensure your C# networking code is configured to send requests to your local backend URL: http://localhost:3000.

🤝 Team Roles

Khaled & Velvin: Unity (Game Client)

Shiva: Docker

Nafeesa: UI/UX (React Frontend, Assets)

Joani: Backend (Node.js/TypeScript)
