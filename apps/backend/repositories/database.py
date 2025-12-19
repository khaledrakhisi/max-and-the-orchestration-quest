from motor.motor_asyncio import AsyncIOMotorClient
from typing import Optional

class Database:
    client: Optional[AsyncIOMotorClient] = None
    
    @classmethod
    async def connect_db(cls, connection_string: str, database_name: str):
        """
        Connect to MongoDB using connection string.
        
        Args:
            connection_string: MongoDB connection string (e.g., from MongoDB Atlas)
            database_name: Name of the database to use
        """
        cls.client = AsyncIOMotorClient(connection_string)
        # Test the connection
        await cls.client.admin.command('ping')
        print(f"Connected to MongoDB database: {database_name}")
        return cls.client[database_name]
    
    @classmethod
    async def close_db(cls):
        """Close the MongoDB connection."""
        if cls.client:
            cls.client.close()
            print("MongoDB connection closed")