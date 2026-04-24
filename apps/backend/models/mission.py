from datetime import datetime
from typing import Optional, Annotated, Literal
from pydantic import BaseModel, Field, ConfigDict, BeforeValidator


PyObjectId = Annotated[str, BeforeValidator(str)]
MissionStatus = Literal["not_started", "in_progress", "completed"]


class Mission(BaseModel):
    id: Optional[PyObjectId] = Field(alias="_id", default=None)
    userId: PyObjectId
    missionId: str
    missionName: str
    status: MissionStatus = "not_started"
    startedAt: Optional[datetime] = None
    completedAt: Optional[datetime] = None
    missionXP: int = 0

    model_config = ConfigDict(populate_by_name=True, arbitrary_types_allowed=True)
