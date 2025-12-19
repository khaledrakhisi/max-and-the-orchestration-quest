from dotenv import load_dotenv
from repositories.user_repository import UserRepository
import os
import asyncio 

def main():
    load_dotenv()
    CONNECTION_STRING = os.getenv("MONGO_URI")
    DATABASE_NAME = "max"

    user_repo = UserRepository(CONNECTION_STRING, DATABASE_NAME)
    users = user_repo.get_all()
    print(f"Found {len(users)} users")

if __name__ == "__main__":
    main()