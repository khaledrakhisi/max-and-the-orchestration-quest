from typing import List

from models.mission import Mission
from repositories.mission_repository import MissionRepository
from repositories.user_repository import UserRepository


class MissionDomain:
    def __init__(
        self,
        mission_repo: MissionRepository,
        user_repo: UserRepository,
        primary_user_id: str | None = None,
    ):
        self.mission_repo = mission_repo
        self.user_repo = user_repo
        self.primary_user_id = primary_user_id

    def get_all_missions(self) -> List[Mission]:
        missions = self.mission_repo.get_all(self.primary_user_id)
        if missions or not self.primary_user_id:
            return missions
        return self.mission_repo.get_all()

    def get_mission_by_id(self, mission_id: str) -> Mission | None:
        mission = self.mission_repo.get_by_id(mission_id, self.primary_user_id)
        if mission or not self.primary_user_id:
            return mission
        return self.mission_repo.get_by_id(mission_id)

    def _get_mission_with_scope(self, mission_id: str) -> tuple[Mission | None, str | None]:
        mission = self.mission_repo.get_by_id(mission_id, self.primary_user_id)
        if mission or not self.primary_user_id:
            return mission, self.primary_user_id

        fallback_mission = self.mission_repo.get_by_id(mission_id)
        return fallback_mission, None

    def start_mission(self, mission_id: str) -> Mission:
        mission, scope_user_id = self._get_mission_with_scope(mission_id)
        if not mission:
            raise ValueError("Mission not found")

        if mission.status == "completed":
            raise ValueError("Mission is already completed")

        if mission.status == "in_progress":
            return mission

        started = self.mission_repo.start_mission(mission_id, scope_user_id)
        if not started:
            raise RuntimeError("Failed to start mission")

        return started

    def complete_mission(self, mission_id: str) -> tuple[Mission, int]:
        mission, scope_user_id = self._get_mission_with_scope(mission_id)
        if not mission:
            raise ValueError("Mission not found")

        if mission.status == "completed":
            raise ValueError("Mission is already completed")

        if mission.status == "not_started":
            raise ValueError("Mission must be started before completion")

        completed = self.mission_repo.complete_mission(mission_id, scope_user_id)
        if not completed:
            raise RuntimeError("Failed to complete mission")

        target_user_id = self.primary_user_id or str(completed.userId)
        user = self.user_repo.get_by_id(target_user_id)
        if not user:
            raise ValueError("Mission user not found")

        updated_total_xp = user.totalXP + completed.missionXP
        updated_user = self.user_repo.update_user(str(user.id), {"totalXP": updated_total_xp})
        if not updated_user:
            raise RuntimeError("Failed to update user XP")

        return completed, updated_user.totalXP
