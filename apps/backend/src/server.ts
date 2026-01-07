// Load environment variables (like PORT) from a .env file
import * as dotenv from "dotenv";
dotenv.config();

import express, { Request, Response } from "express";
import * as http from "http";
import { Server as SocketIoServer, Socket } from "socket.io";
import Docker from "dockerode";

const app = express();
const server = http.createServer(app);
const PORT = process.env.PORT || 3000;

// --- 1. Middleware ---
app.use(express.json());

// --- 2. Socket.IO Setup (WebSockets) ---
// Initialize Socket.IO server with type information
const io = new SocketIoServer(server, {
  cors: {
    origin: "*", // Allow all origins for development (CHANGE THIS LATER!)
    methods: ["GET", "POST"],
  },
});

io.on("connection", (socket: Socket) => {
  console.log(`Socket client connected: ${socket.id}`);

  // Example: Send a welcome message to Unity/React on connection
  socket.emit("connection_status", {
    message: `Welcome to MaxPi Backend! Your ID: ${socket.id}`,
  });

  socket.on("disconnect", () => {
    console.log(`Socket client disconnected: ${socket.id}`);
  });

  // NOTE: Game deployment commands (docker logic) go here:
  // socket.on('deployment_request', (data) => { ... });
});

// --- 3. Express Routes (HTTP API) ---
// Interface for the deployment request body for type safety
interface DeploymentRequest {
  containerName: string;
  nodeId: string;
}

// Handler for the deployment command from Unity
app.post(
  "/api/deploy",
  (req: Request<{}, {}, DeploymentRequest>, res: Response) => {
    const { containerName, nodeId } = req.body;
    console.log(`[API] Deployment requested: ${containerName} on ${nodeId}`);

    // **TODO:** Implement Docker SDK logic here (The logic runs in the background)

    // Immediately push an asynchronous update via Socket.IO
    io.emit("log_stream", {
      logEntry: `Deployment of ${containerName} initiated...`,
    });

    res.status(202).json({
      message: "Command accepted. Check socket stream for status.",
      command_sent: `docker run -d --name ${containerName} ${containerName}`,
    });
  }
);

app.get("/", (req: Request, res: Response) => {
  res.send("max and Orchestration Backend Running.");
});

// --- 4. Start Server ---
server.listen(PORT, () => {
  console.log(`Server listening on port ${PORT}`);
  console.log(`Node.js environment: ${process.env.NODE_ENV || "development"}`);
});

const docker = new Docker(); // Defaults to /var/run/docker.sock or equivalent
async function pullImage() {
  console.log("Pulling nginx:latest...");

  try {
    // dockerode returns a stream for the pull progress
    await new Promise((resolve, reject) => {
      docker.pull("nginx:latest", (err: any, stream: any) => {
        if (err) return reject(err);

        // You must follow the stream to ensure the pull completes
        docker.modem.followProgress(stream, onFinished, onProgress);

        function onFinished(err: any, output: any) {
          if (err) return reject(err);
          resolve(output);
        }

        function onProgress(event: any) {
          // Optional: Log download progress
          if (event.status) console.log(event.status);
        }
      });
    });

    console.log("Successfully pulled nginx!");
  } catch (error) {
    console.error("Failed to pull image:", error);
  }
}
