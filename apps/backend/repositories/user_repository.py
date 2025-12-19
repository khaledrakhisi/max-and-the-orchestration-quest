from typing import List, Optional
from pymongo import MongoClient
from bson import ObjectId
from models.user import User, Badge


class UserRepository:
    def __init__(self, mongo_uri: str, db_name: str):
        self.client = MongoClient(mongo_uri)
        self.db = self.client[db_name]
        self.collection = self.db["users"]

    # -------------------------
    # READ
    # -------------------------

    def get_all(self) -> List[User]:
        users = self.collection.find()
        return [User(**user) for user in users]

    def get_by_id(self, user_id: str) -> Optional[User]:
        user = self.collection.find_one({"_id": ObjectId(user_id)})
        return User(**user) if user else None

    def get_by_username(self, username: str) -> Optional[User]:
        user = self.collection.find_one({"username": username})
        return User(**user) if user else None

    # -------------------------
    # CREATE
    # -------------------------

    def insert(self, user: User) -> User:
        user_dict = user.model_dump(by_alias=True, exclude={"id"})
        result = self.collection.insert_one(user_dict)
        user.id = str(result.inserted_id)
        return user

    # -------------------------
    # UPDATE
    # -------------------------

    def add_badge(self, user_id: str, badge: Badge) -> bool:
        result = self.collection.update_one(
            {"_id": ObjectId(user_id)},
            {"$push": {"badges": badge.model_dump()}}
        )
        return result.modified_count == 1

    def add_xp(self, user_id: str, xp: int) -> bool:
        result = self.collection.update_one(
            {"_id": ObjectId(user_id)},
            {
                "$inc": {"totalXP": xp},
                "$set": {"level": self._calculate_level(xp)}
            }
        )
        return result.modified_count == 1

    def change_password(self, user_id: str, new_password: str) -> bool:
        result = self.collection.update_one(
            {"_id": ObjectId(user_id)},
            {"$set": {"password": new_password}}
        )
        return result.modified_count == 1

    def _calculate_level(self, xp: int) -> int:
        # Example leveling logic
        return max(1, xp // 1000)
