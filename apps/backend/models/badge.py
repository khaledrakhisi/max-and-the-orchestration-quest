from datetime import datetime
from typing import Optional, Annotated, Literal

from pydantic import BaseModel, Field, ConfigDict, BeforeValidator


PyObjectId = Annotated[str, BeforeValidator(str)]
BadgeStatus = Literal["not_achieved", "achieved"]


class Badge(BaseModel):
    id: Optional[PyObjectId] = Field(alias="_id", default=None)
    userId: PyObjectId
    badgeId: str
    badgeName: str
    status: BadgeStatus = "not_achieved"
    achievedAt: Optional[datetime] = None
    badgeXP: int = 0

    model_config = ConfigDict(populate_by_name=True, arbitrary_types_allowed=True)
