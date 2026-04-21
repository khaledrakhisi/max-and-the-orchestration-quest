import logging
import queue
import threading
from datetime import datetime, timezone

from pymongo import MongoClient
from pymongo.errors import PyMongoError


class MongoDBHandler(logging.Handler):
    """
    Thread-safe, non-blocking logging handler that writes to MongoDB.
    Uses an internal queue + background thread so log calls never
    block the asyncio event loop.
    """

    def __init__(self, mongo_uri: str, db_name: str = "max", collection: str = "logs"):
        super().__init__()
        self._client = MongoClient(mongo_uri, serverSelectionTimeoutMS=5000)
        self._col = self._client[db_name][collection]

        # Ensure useful indexes exist (idempotent)
        self._col.create_index("timestamp")
        self._col.create_index("level")
        self._col.create_index("client_id")

        self._queue: queue.Queue = queue.Queue(maxsize=1000)
        self._stop_event = threading.Event()

        self._worker = threading.Thread(target=self._flush_loop, daemon=True, name="mongo-log-worker")
        self._worker.start()

    # ------------------------------------------------------------------
    # logging.Handler interface
    # ------------------------------------------------------------------

    def emit(self, record: logging.LogRecord):
        try:
            doc = self._build_doc(record)
            self._queue.put_nowait(doc)
        except queue.Full:
            # Drop rather than block — game server availability > log completeness
            pass

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    def _build_doc(self, record: logging.LogRecord) -> dict:
        doc = {
            "timestamp": datetime.fromtimestamp(record.created, tz=timezone.utc),
            "level": record.levelname,
            "logger": record.name,
            "message": record.getMessage(),
            "module": record.module,
            "funcName": record.funcName,
            "lineno": record.lineno,
        }

        # Attach extra fields (client_id, container, image, etc.)
        standard_keys = logging.LogRecord.__dict__.keys() | {
            "message", "asctime", "args", "msg",
        }
        for key, value in record.__dict__.items():
            if key not in standard_keys and not key.startswith("_"):
                doc[key] = value

        if record.exc_info:
            doc["exception"] = self.formatException(record.exc_info)

        return doc

    def _flush_loop(self):
        """Background thread: drains the queue into MongoDB in small batches."""
        batch = []
        while not self._stop_event.is_set() or not self._queue.empty():
            try:
                doc = self._queue.get(timeout=0.5)
                batch.append(doc)
                # Flush in batches of up to 50
                if len(batch) >= 50:
                    self._insert_batch(batch)
                    batch = []
            except queue.Empty:
                if batch:
                    self._insert_batch(batch)
                    batch = []

    def _insert_batch(self, batch: list):
        try:
            self._col.insert_many(batch, ordered=False)
        except PyMongoError as e:
            # Last-resort stderr fallback so we don't silently eat errors
            print(f"[MongoDBHandler] Failed to write log batch: {e}", flush=True)

    def close(self):
        self._stop_event.set()
        self._worker.join(timeout=5)
        self._client.close()
        super().close()


# ------------------------------------------------------------------
# Public factory — call once at startup
# ------------------------------------------------------------------

def setup_logging(mongo_uri: str) -> logging.Logger:
    """
    Configures root logging with:
      - A StreamHandler for the console
      - A MongoDBHandler for persistent storage

    Returns the application-level logger.
    """
    # Silence third-party loggers (websockets, urllib3, docker, etc.)
    logging.getLogger().setLevel(logging.WARNING)

    app_log = logging.getLogger("docker_game")
    app_log.setLevel(logging.INFO)

    # --- Console handler (human-readable, INFO and above) ---
    console = logging.StreamHandler()
    console.setLevel(logging.INFO)
    console.setFormatter(logging.Formatter(
        fmt="%(asctime)s [%(levelname)-8s] %(name)s — %(message)s",
        datefmt="%H:%M:%S",
    ))

    # --- MongoDB handler ---
    mongo = MongoDBHandler(mongo_uri)
    mongo.setLevel(logging.INFO)

    app_log.addHandler(console)
    app_log.addHandler(mongo)
    app_log.propagate = False

    app_log.info("Logging initialised — writing to MongoDB")
    return app_log