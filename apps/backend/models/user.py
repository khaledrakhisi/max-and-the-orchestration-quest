from datetime import datetime
from typing import List, Optional, Annotated
from pydantic import BaseModel, Field, ConfigDict, BeforeValidator


# This helper converts MongoDB's ObjectId to a string so Python can handle it easily
PyObjectId = Annotated[str, BeforeValidator(str)]

class Badge(BaseModel):
    badgeId: str
    name: str
    earnedAt: datetime = Field(default_factory=datetime.utcnow)

class User(BaseModel):
    # The 'alias' allows us to use 'id' in Python but '_id' in MongoDB
    id: Optional[PyObjectId] = Field(alias="_id", default=None)
    username: str
    email: str
    password: str
    totalXP: int = 0
    level: int = 1
    badges: List[Badge] = []
    createdAt: datetime = Field(default_factory=datetime.utcnow)

    # This allows the model to work even if MongoDB returns extra fields
    model_config = ConfigDict(populate_by_name=True, arbitrary_types_allowed=True)