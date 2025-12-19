from typing import List, Optional
from motor.motor_asyncio import AsyncIOMotorClient, AsyncIOMotorDatabase
from bson import ObjectId
from models.user import User, Badge

class UserRepository:
    def __init__(self, db: AsyncIOMotorDatabase):
        """
        Initialize the repository with a database connection.
        
        Args:
            db: AsyncIOMotorDatabase instance
        """
        self.collection = db["users"]
    
    async def get_all(self) -> List[User]:
        users = []
        cursor = self.collection.find({})
        async for document in cursor:
            users.append(User(**document))
        return users
    
    async def get_by_id(self, user_id: str) -> Optional[User]:
        try:
            document = await self.collection.find_one({"_id": ObjectId(user_id)})
            if document:
                return User(**document)
            return None
        except Exception:
            return None
    
    async def get_by_username(self, username: str) -> Optional[User]:
        document = await self.collection.find_one({"username": username})
        if document:
            return User(**document)
        return None
    
    async def insert(self, user: User) -> str:
        user_dict = user.model_dump(by_alias=True, exclude=["id"])
        result = await self.collection.insert_one(user_dict)
        return str(result.inserted_id)
    
    async def add_badge(self, user_id: str, badge: Badge) -> bool:
        try:
            badge_dict = badge.model_dump()
            result = await self.collection.update_one(
                {"_id": ObjectId(user_id)},
                {"$push": {"badges": badge_dict}}
            )
            return result.modified_count > 0
        except Exception:
            return False
    
    async def add_xp(self, user_id: str, xp_amount: int) -> bool:
        try:
            # Get current user to calculate new level
            user = await self.get_by_id(user_id)
            if not user:
                return False
            
            new_total_xp = user.totalXP + xp_amount
            # Simple level calculation: level = 1 + (totalXP // 100)
            new_level = 1 + (new_total_xp // 100)
            
            result = await self.collection.update_one(
                {"_id": ObjectId(user_id)},
                {
                    "$inc": {"totalXP": xp_amount},
                    "$set": {"level": new_level}
                }
            )
            return result.modified_count > 0
        except Exception:
            return False
    
    async def change_password(self, user_id: str, new_password: str) -> bool:
        try:
            result = await self.collection.update_one(
                {"_id": ObjectId(user_id)},
                {"$set": {"password": new_password}}
            )
            return result.modified_count > 0
        except Exception:
            return False


# Example usage:
"""
from motor.motor_asyncio import AsyncIOMotorClient

# Create database connection
client = AsyncIOMotorClient("mongodb://localhost:27017")
db = client["your_database_name"]

# Initialize repository
user_repo = UserRepository(db)

# Use the repository
user = await user_repo.get_by_username("john_doe")
success = await user_repo.add_xp(user.id, 50)
"""