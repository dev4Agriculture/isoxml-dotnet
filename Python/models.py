from datetime import datetime
from typing import List, Optional

class TimeLog:
    """Represents a single TimeLog entry with start and end times"""
    
    def __init__(self, id: str, start_time: datetime, end_time: datetime):
        self.id = id
        self.start_time = start_time
        self.end_time = end_time
    
    def __str__(self):
        return f"TimeLog({self.id}: {self.start_time.strftime('%H:%M:%S')} - {self.end_time.strftime('%H:%M:%S')})"
    
    def __repr__(self):
        return self.__str__()
    
    def duration_minutes(self) -> int:
        """Get the duration of this TimeLog in minutes"""
        return int((self.end_time - self.start_time).total_seconds() / 60)

class Task:
    """Represents a Task containing multiple TimeLogs"""
    
    def __init__(self, id: int, time_logs: List[TimeLog] = None):
        self.id = id
        self.time_logs = time_logs if time_logs else []
    
    def add_time_log(self, time_log: TimeLog):
        """Add a TimeLog to this Task"""
        self.time_logs.append(time_log)
    
    def get_start_time(self) -> Optional[datetime]:
        """Get the start time of the task (minimum of all TimeLog start times)"""
        if not self.time_logs:
            return None
        return min(tlg.start_time for tlg in self.time_logs)
    
    def get_end_time(self) -> Optional[datetime]:
        """Get the end time of the task (maximum of all TimeLog end times)"""
        if not self.time_logs:
            return None
        return max(tlg.end_time for tlg in self.time_logs)
    
    def is_in_active_work(self, timestamp: datetime) -> bool:
        """Check if a specific timestamp is within the time range of any TimeLog within this Task"""
        for time_log in self.time_logs:
            if time_log.start_time <= timestamp <= time_log.end_time:
                return True
        return False
    
    def get_duration_minutes(self) -> Optional[int]:
        """Get the total duration of the task in minutes"""
        start = self.get_start_time()
        end = self.get_end_time()
        if start and end:
            return int((end - start).total_seconds() / 60)
        return None
    
    def get_time_log_count(self) -> int:
        """Get the number of TimeLogs in this Task"""
        return len(self.time_logs)
    
    def __str__(self):
        start = self.get_start_time()
        end = self.get_end_time()
        duration = self.get_duration_minutes()
        
        if start and end:
            return f"Task {self.id}: {start.strftime('%H:%M:%S')} - {end.strftime('%H:%M:%S')} ({duration} min, {len(self.time_logs)} TimeLogs)"
        else:
            return f"Task {self.id}: No TimeLogs"
    
    def __repr__(self):
        return self.__str__()
