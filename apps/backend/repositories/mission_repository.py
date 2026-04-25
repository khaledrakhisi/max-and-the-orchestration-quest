from datetime import datetime, timezone
from typing import List, Optional

from bson import ObjectId
from pymongo import MongoClient, ReturnDocument

from models.mission import Mission


class MissionRepository:
    def __init__(self, mongo_uri: str, db_name: str):
        self.client = MongoClient(mongo_uri)
        self.db = self.client[db_name]
        self.collection = self.db["missions"]

    def _user_query(self, user_id: Optional[str]) -> dict:
        if not user_id:
            return {}
        if ObjectId.is_valid(user_id):
            return {"userId": ObjectId(user_id)}
        return {"userId": None}

    def get_all(self, user_id: Optional[str] = None) -> List[Mission]:
        missions = self.collection.find(self._user_query(user_id))
        return [Mission(**mission) for mission in missions]

    def get_by_id(self, mission_id: str, user_id: Optional[str] = None) -> Optional[Mission]:
        query = {"missionId": mission_id, **self._user_query(user_id)}
        mission = self.collection.find_one(query)
        if mission:
            return Mission(**mission)

        if ObjectId.is_valid(mission_id):
            by_object_id = self.collection.find_one({"_id": ObjectId(mission_id), **self._user_query(user_id)})
            return Mission(**by_object_id) if by_object_id else None

        return None

    def start_mission(self, mission_id: str, user_id: Optional[str] = None) -> Optional[Mission]:
        started = self.collection.find_one_and_update(
            {"missionId": mission_id, "status": {"$ne": "completed"}, **self._user_query(user_id)},
            {"$set": {"status": "in_progress", "startedAt": datetime.now(timezone.utc)}},
            return_document=ReturnDocument.AFTER,
        )
        return Mission(**started) if started else None

    def complete_mission(self, mission_id: str, user_id: Optional[str] = None) -> Optional[Mission]:
        completed = self.collection.find_one_and_update(
            {"missionId": mission_id, "status": {"$ne": "completed"}, **self._user_query(user_id)},
            {"$set": {"status": "completed", "completedAt": datetime.now(timezone.utc)}},
            return_document=ReturnDocument.AFTER,
        )
        return Mission(**completed) if completed else None
