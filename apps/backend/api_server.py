from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, EmailStr, Field
from datetime import timedelta, datetime
from repositories.user_repository import UserRepository
from repositories.badge_repository import BadgeRepository
from repositories.mission_repository import MissionRepository
from domain.authentication import hash_password, verify_password, create_access_token, normalize_password
from domain.badges import BadgeDomain
from domain.missions import MissionDomain
from models.user import User
from models.badge import BadgeStatus
from models.mission import MissionStatus
from bson.errors import InvalidId
import os


# Load environment variables
load_dotenv()

# Configuration
CONNECTION_STRING = os.getenv("MONGO_URI")
DATABASE_NAME = "max"
SECRET_KEY = os.getenv("SECRET_KEY")
ALGORITHM = os.getenv("ALGORITHM", "HS256")
ACCESS_TOKEN_EXPIRE_MINUTES = int(os.getenv("ACCESS_TOKEN_EXPIRE_MINUTES", 30))
PRIMARY_USER_ID = os.getenv("PRIMARY_USER_ID", "693843f9a8cdbf214cd36a62")
PRIMARY_USER_EMAIL = os.getenv("PRIMARY_USER_EMAIL", "player1@example.com")

# Initialize FastAPI app
app = FastAPI(title="Max Authentication API", version="1.0.0")

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify your frontend URL
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize repository
user_repo = UserRepository(CONNECTION_STRING, DATABASE_NAME)
badge_repo = BadgeRepository(CONNECTION_STRING, DATABASE_NAME)
mission_repo = MissionRepository(CONNECTION_STRING, DATABASE_NAME)
badge_domain = BadgeDomain(badge_repo, user_repo, primary_user_id=PRIMARY_USER_ID)
mission_domain = MissionDomain(mission_repo, user_repo, primary_user_id=PRIMARY_USER_ID)


# Pydantic models for requests/responses
class RegisterRequest(BaseModel):
    username: str = Field(..., min_length=3, max_length=50)
    email: EmailStr
    password: str = Field(..., min_length=8, max_length=100)


class LoginRequest(BaseModel):
    email: EmailStr
    password: str


class TokenResponse(BaseModel):
    access_token: str
    token_type: str
    user: dict


class UserResponse(BaseModel):
    id: str
    username: str
    email: str
    totalXP: int
    level: int


class UserCreateRequest(BaseModel):
    username: str = Field(..., min_length=3, max_length=50)
    email: EmailStr
    password: str = Field(..., min_length=8, max_length=100)
    totalXP: int = 0
    level: int = 1


class UserUpdateRequest(BaseModel):
    username: str | None = Field(default=None, min_length=3, max_length=50)
    email: EmailStr | None = None
    password: str | None = Field(default=None, min_length=8, max_length=100)
    totalXP: int | None = None
    level: int | None = None


class BadgeResponse(BaseModel):
    id: str
    userId: str
    badgeId: str
    badgeName: str
    status: BadgeStatus
    achievedAt: datetime | None = None
    badgeXP: int = 0


class BadgeAchieveResponse(BaseModel):
    badge: BadgeResponse
    userTotalXP: int


class MissionResponse(BaseModel):
    id: str
    userId: str
    missionId: str
    missionName: str
    status: MissionStatus
    startedAt: datetime | None = None
    completedAt: datetime | None = None
    missionXP: int


class MissionCompleteResponse(BaseModel):
    mission: MissionResponse
    userTotalXP: int


def _serialize_user(user) -> dict:
    return {
        "id": str(user.id),
        "username": user.username,
        "email": user.email,
        "totalXP": user.totalXP,
        "level": user.level,
    }


def _serialize_badge(badge) -> dict:
    return {
        "id": str(badge.id),
        "userId": str(badge.userId),
        "badgeId": badge.badgeId,
        "badgeName": badge.badgeName,
        "status": badge.status,
        "achievedAt": badge.achievedAt,
        "badgeXP": badge.badgeXP,
    }


def _serialize_mission(mission) -> dict:
    return {
        "id": str(mission.id),
        "userId": str(mission.userId),
        "missionId": mission.missionId,
        "missionName": mission.missionName,
        "status": mission.status,
        "startedAt": mission.startedAt,
        "completedAt": mission.completedAt,
        "missionXP": mission.missionXP,
    }


def _get_primary_user():
    if PRIMARY_USER_ID:
        try:
            user_by_id = user_repo.get_by_id(PRIMARY_USER_ID)
            if user_by_id:
                return user_by_id
        except InvalidId:
            pass

    user_by_email = user_repo.get_by_email(PRIMARY_USER_EMAIL)
    if user_by_email:
        return user_by_email

    return None


@app.get("/")
async def root():
    """Health check endpoint"""
    return {"message": "Max Authentication API is running.", "status": "healthy"}


@app.post("/register", response_model=TokenResponse, status_code=status.HTTP_201_CREATED)
async def register(request: RegisterRequest):
    """
    Register a new user with username, email, and password.
    
    - **username**: Unique username (3-50 characters)
    - **email**: Valid email address
    - **password**: Strong password (minimum 8 characters)
    
    Returns JWT access token on success.
    """
    # Check if username already exists
    existing_username = user_repo.get_by_username(request.username)
    if existing_username:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Username already registered"
        )
    
    # Check if email already exists
    existing_email = user_repo.get_by_email(request.email)
    if existing_email:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Email already registered"
        )
    
    # Hash the password
    normalized_password = normalize_password(request.password)
    hashed_password = hash_password(normalized_password)
    
    # Create new user
    new_user = User(
        username=request.username,
        email=request.email,
        password=hashed_password
    )
    
    # Insert user into database
    created_user = user_repo.insert(new_user)
    
    # Create access token
    access_token_expires = timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    access_token = create_access_token(
        data={"sub": created_user.email, "user_id": str(created_user.id)},
        expires_delta=access_token_expires
    )
    
    return {
        "access_token": access_token,
        "token_type": "bearer",
        "user": _serialize_user(created_user)
    }


@app.post("/login", response_model=TokenResponse)
async def login(request: LoginRequest):
    """
    Authenticate user with email and password.
    
    - **email**: Registered email address
    - **password**: User's password
    
    Returns JWT access token on success.
    """
    # Find user by email
    user = user_repo.get_by_email(request.email)
    
    if not user:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect email or password",
            headers={"WWW-Authenticate": "Bearer"}
        )
    
    # Verify password
    if not verify_password(request.password, user.password):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect email or password",
            headers={"WWW-Authenticate": "Bearer"}
        )
    
    # Create access token
    access_token_expires = timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    access_token = create_access_token(
        data={"sub": user.email, "user_id": str(user.id)},
        expires_delta=access_token_expires
    )
    
    return {
        "access_token": access_token,
        "token_type": "bearer",
        "user": _serialize_user(user)
    }


@app.get("/users", response_model=list[UserResponse])
async def list_users():
    users = user_repo.get_all()
    return [_serialize_user(user) for user in users]


@app.get("/user", response_model=UserResponse)
async def get_primary_user():
    user = _get_primary_user()
    if not user:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Primary user not found")

    return _serialize_user(user)


@app.get("/users/{user_id}", response_model=UserResponse)
async def get_user(user_id: str):
    try:
        user = user_repo.get_by_id(user_id)
    except InvalidId:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Invalid user id")

    if not user:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="User not found")

    return _serialize_user(user)


@app.post("/users", response_model=UserResponse, status_code=status.HTTP_201_CREATED)
async def create_user(request: UserCreateRequest):
    existing_username = user_repo.get_by_username(request.username)
    if existing_username:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Username already registered")

    existing_email = user_repo.get_by_email(request.email)
    if existing_email:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Email already registered")

    normalized_password = normalize_password(request.password)
    hashed_password = hash_password(normalized_password)

    new_user = User(
        username=request.username,
        email=request.email,
        password=hashed_password,
        totalXP=request.totalXP,
        level=request.level,
    )

    created_user = user_repo.insert(new_user)

    return _serialize_user(created_user)


@app.patch("/users/{user_id}", response_model=UserResponse)
async def update_user(user_id: str, request: UserUpdateRequest):
    updates = request.model_dump(exclude_unset=True)

    if "username" in updates:
        existing_username = user_repo.get_by_username(updates["username"])
        if existing_username and str(existing_username.id) != user_id:
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Username already registered")

    if "email" in updates:
        existing_email = user_repo.get_by_email(updates["email"])
        if existing_email and str(existing_email.id) != user_id:
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Email already registered")

    if "password" in updates:
        normalized_password = normalize_password(updates["password"])
        updates["password"] = hash_password(normalized_password)

    try:
        updated_user = user_repo.update_user(user_id, updates)
    except InvalidId:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Invalid user id")

    if not updated_user:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="User not found")

    return _serialize_user(updated_user)


@app.delete("/users/{user_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_user(user_id: str):
    try:
        deleted = user_repo.delete_by_id(user_id)
    except InvalidId:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Invalid user id")

    if not deleted:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="User not found")

    return None


@app.get("/badges", response_model=list[BadgeResponse])
async def list_badges():
    badges = badge_domain.get_all_badges()
    return [_serialize_badge(badge) for badge in badges]


@app.post("/badges/{badge_id}/achieve", response_model=BadgeAchieveResponse)
async def achieve_badge(badge_id: str):
    try:
        achieved_badge, user_total_xp = badge_domain.achieve_badge(badge_id)
    except ValueError as exc:
        message = str(exc)
        status_code = status.HTTP_404_NOT_FOUND if "not found" in message.lower() else status.HTTP_400_BAD_REQUEST
        raise HTTPException(status_code=status_code, detail=message)
    except RuntimeError as exc:
        raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail=str(exc))

    return {
        "badge": _serialize_badge(achieved_badge),
        "userTotalXP": user_total_xp,
    }


@app.get("/missions", response_model=list[MissionResponse])
async def list_missions():
    missions = mission_domain.get_all_missions()
    return [_serialize_mission(mission) for mission in missions]


@app.get("/missions/{mission_id}", response_model=MissionResponse)
async def get_mission(mission_id: str):
    mission = mission_domain.get_mission_by_id(mission_id)
    if not mission:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Mission not found")

    return _serialize_mission(mission)


@app.post("/missions/{mission_id}/start", response_model=MissionResponse)
async def start_mission(mission_id: str):
    try:
        mission = mission_domain.start_mission(mission_id)
    except ValueError as exc:
        message = str(exc)
        status_code = status.HTTP_404_NOT_FOUND if "not found" in message.lower() else status.HTTP_400_BAD_REQUEST
        raise HTTPException(status_code=status_code, detail=message)
    except RuntimeError as exc:
        raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail=str(exc))

    return _serialize_mission(mission)


@app.post("/missions/{mission_id}/complete", response_model=MissionCompleteResponse)
async def complete_mission(mission_id: str):
    try:
        completed_mission, user_total_xp = mission_domain.complete_mission(mission_id)
    except ValueError as exc:
        message = str(exc)
        status_code = status.HTTP_404_NOT_FOUND if "not found" in message.lower() else status.HTTP_400_BAD_REQUEST
        raise HTTPException(status_code=status_code, detail=message)
    except RuntimeError as exc:
        raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail=str(exc))

    return {
        "mission": _serialize_mission(completed_mission),
        "userTotalXP": user_total_xp,
    }


if __name__ == "__main__":
    import uvicorn
    # Run the FastAPI application
    port = int(os.getenv("PORT", 3000))
    uvicorn.run(app, host="0.0.0.0", port=port)
