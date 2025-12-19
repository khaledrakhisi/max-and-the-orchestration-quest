import logging
import logging.handlers
import json
import os
from datetime import datetime
import pymongo

MONGO_URI = "mongodb+srv://joanigaxhi1_db_user:6qKoKOtOXDO00lrT@cluster0.hfqth20.mongodb.net/"


class JsonFormatter(logging.Formatter):
    def format(self, record):
        record_message = record.getMessage()
        if record.exc_info:
            exc = self.formatException(record.exc_info)
        else:
            exc = None

        obj = {
            "ts": datetime.utcfromtimestamp(record.created).isoformat() + "Z",
            "level": record.levelname,
            "logger": record.name,
            "message": record_message,
            "module": record.module,
            "funcName": record.funcName,
            "lineno": record.lineno,
        }
        if exc:
            obj["exc"] = exc
        if hasattr(record, "extra") and isinstance(record.extra, dict):
            obj.update(record.extra)

        return json.dumps(obj)


class MongoHandler(logging.Handler):
    """A simple logging handler that inserts logs into MongoDB.

    It will create a TTL index on `created_at` if `ttl_days` > 0 so old logs
    are removed automatically.
    """

    def __init__(self, mongo_uri=MONGO_URI, db="logs", collection="app_logs", ttl_days=30):
        super().__init__()
        if pymongo is None:
            raise RuntimeError("pymongo is required for MongoHandler")

        self.client = pymongo.MongoClient(mongo_uri)
        self.db = self.client[db]
        self.coll = self.db[collection]
        self.ttl_days = ttl_days
        try:
            if ttl_days and ttl_days > 0:
                # create TTL index on created_at
                self.coll.create_index([("created_at", pymongo.ASCENDING)], expireAfterSeconds=ttl_days * 86400)
        except Exception:
            # be resilient if index creation fails (permissions, etc.)
            pass

    def emit(self, record):
        try:
            msg = self.format(record)
            doc = {
                "created_at": datetime.utcfromtimestamp(record.created),
                "level": record.levelname,
                "logger": record.name,
                "message": record.getMessage(),
                "module": record.module,
                "funcName": record.funcName,
                "lineno": record.lineno,
            }
            if record.exc_info:
                doc["exc"] = self.formatException(record.exc_info)

            # If the formatter produced JSON extra fields, try to include them
            try:
                extra = getattr(record, "extra", None)
                if isinstance(extra, dict):
                    doc.update(extra)
            except Exception:
                pass

            # Also include the raw JSON string for compatibility
            doc["raw"] = msg

            self.coll.insert_one(doc)
        except Exception:
            self.handleError(record)


def setup_logging(name=None, level=logging.INFO, log_dir=None, rotate_mb=5, backup_count=5,
                  mongo_uri=None, mongo_db="logs", mongo_collection="app_logs", mongo_ttl_days=30):
    """Configure root logger with console + file rotation + optional MongoDB handler.

    - `log_dir`: directory to place rotated log files. If None, uses current working dir.
    - `mongo_uri`: if provided, will attach the MongoHandler (requires pymongo).
    """
    root = logging.getLogger()
    root.setLevel(level)

    # avoid duplicate handlers if called multiple times
    if getattr(root, "_configured_by_logging_config", False):
        return

    formatter = JsonFormatter()

    # Console
    ch = logging.StreamHandler()
    ch.setLevel(level)
    ch.setFormatter(formatter)
    root.addHandler(ch)

    # Rotating file
    if not log_dir:
        log_dir = os.getcwd()
    os.makedirs(log_dir, exist_ok=True)
    logfile = os.path.join(log_dir, "app.log")
    fh = logging.handlers.RotatingFileHandler(logfile, maxBytes=rotate_mb * 1024 * 1024, backupCount=backup_count, encoding="utf-8")
    fh.setLevel(level)
    fh.setFormatter(formatter)
    root.addHandler(fh)

    # Optional Mongo handler
    if mongo_uri:
        try:
            mh = MongoHandler(mongo_uri=mongo_uri, db=mongo_db, collection=mongo_collection, ttl_days=mongo_ttl_days)
            mh.setLevel(level)
            mh.setFormatter(formatter)
            root.addHandler(mh)
        except Exception:
            # If Mongo not available, continue without failing
            root.warning("Mongo logging handler not configured (pymongo missing or connection failed)")

    root._configured_by_logging_config = True


def get_logger(name=None):
    return logging.getLogger(name)
