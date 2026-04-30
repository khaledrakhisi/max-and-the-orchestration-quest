from typing import List

from models.badge import Badge
from repositories.badge_repository import BadgeRepository
from repositories.user_repository import UserRepository


class BadgeDomain:
    def __init__(
        self,
        badge_repo: BadgeRepository,
        user_repo: UserRepository,
        primary_user_id: str | None = None,
    ):
        self.badge_repo = badge_repo
        self.user_repo = user_repo
        self.primary_user_id = primary_user_id

    def get_all_badges(self) -> List[Badge]:
        badges = self.badge_repo.get_all(self.primary_user_id)
        if badges or not self.primary_user_id:
            return badges
        return self.badge_repo.get_all()

    def _get_badge_with_scope(self, badge_id: str) -> tuple[Badge | None, str | None]:
        badge = self.badge_repo.get_by_id(badge_id, self.primary_user_id)
        if badge or not self.primary_user_id:
            return badge, self.primary_user_id

        fallback_badge = self.badge_repo.get_by_id(badge_id)
        return fallback_badge, None

    def achieve_badge(self, badge_id: str) -> tuple[Badge, int]:
        badge, scope_user_id = self._get_badge_with_scope(badge_id)
        if not badge:
            raise ValueError("Badge not found")

        if badge.status == "achieved":
            raise ValueError("Badge is already achieved")

        achieved = self.badge_repo.achieve_badge(badge_id, scope_user_id)
        if not achieved:
            raise RuntimeError("Failed to achieve badge")

        target_user_id = self.primary_user_id or str(achieved.userId)
        user = self.user_repo.get_by_id(target_user_id)
        if not user:
            raise ValueError("Badge user not found")

        updated_total_xp = user.totalXP + achieved.badgeXP
        updated_user = self.user_repo.update_user(str(user.id), {"totalXP": updated_total_xp})
        if not updated_user:
            raise RuntimeError("Failed to update user XP")

        return achieved, updated_user.totalXP
