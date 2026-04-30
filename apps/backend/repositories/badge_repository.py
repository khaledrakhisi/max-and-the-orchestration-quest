from datetime import datetime, timezone
from typing import List, Optional

from bson import ObjectId
from pymongo import MongoClient, ReturnDocument

from models.badge import Badge


class BadgeRepository:
    def __init__(self, mongo_uri: str, db_name: str):
        self.client = MongoClient(mongo_uri)
        self.db = self.client[db_name]
        self.collection = self.db["badges"]

    def _user_query(self, user_id: Optional[str]) -> dict:
        if not user_id:
            return {}
        if ObjectId.is_valid(user_id):
            return {"userId": ObjectId(user_id)}
        return {"userId": None}

    def get_all(self, user_id: Optional[str] = None) -> List[Badge]:
        badges = self.collection.find(self._user_query(user_id))
        return [Badge(**badge) for badge in badges]

    def get_by_id(self, badge_id: str, user_id: Optional[str] = None) -> Optional[Badge]:
        badge = self.collection.find_one({"badgeId": badge_id, **self._user_query(user_id)})
        if badge:
            return Badge(**badge)

        if ObjectId.is_valid(badge_id):
            by_object_id = self.collection.find_one({"_id": ObjectId(badge_id), **self._user_query(user_id)})
            return Badge(**by_object_id) if by_object_id else None

        return None

    def achieve_badge(self, badge_id: str, user_id: Optional[str] = None) -> Optional[Badge]:
        achieved = self.collection.find_one_and_update(
            {"badgeId": badge_id, "status": {"$ne": "achieved"}, **self._user_query(user_id)},
            {"$set": {"status": "achieved", "achievedAt": datetime.now(timezone.utc)}},
            return_document=ReturnDocument.AFTER,
        )
        return Badge(**achieved) if achieved else None
