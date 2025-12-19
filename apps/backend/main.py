from dotenv import load_dotenv
from repositories.database import Database
from repositories.user_repository import UserRepository
import os
import asyncio 

async def main():
    load_dotenv()
    CONNECTION_STRING = os.getenv("MONGO_URI")
    DATABASE_NAME = "max"

    
    db = await Database.connect_db(CONNECTION_STRING, DATABASE_NAME)
    user_repo = UserRepository(db)
    users = await user_repo.get_all()
    print(f"Found {len(users)} users")
    await Database.close_db()

if __name__ == "__main__":
    asyncio.run(main())

#   LUpONY5mGrC1FzSs