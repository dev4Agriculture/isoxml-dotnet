from datetime import datetime, date

def time(hour: int, minute: int, second: int = 0) -> datetime:
    """Create a datetime object for today with the specified time"""
    today = date.today()
    return datetime.combine(today, datetime.min.time().replace(hour=hour, minute=minute, second=second))

def format_time_range(start_time: datetime, end_time: datetime) -> str:
    """Format a time range in HH:MM:SS format"""
    return f"{start_time.strftime('%H:%M:%S')} - {end_time.strftime('%H:%M:%S')}"

def minutes_between(start_time: datetime, end_time: datetime) -> int:
    """Calculate minutes between two datetime objects"""
    return int((end_time - start_time).total_seconds() / 60)

def create_sample_data():
    """Create sample TimeLog data based on the user's table"""
    time_logs_data = [
        ("TLG00001", 8, 0, 8, 45),
        ("TLG00002", 8, 45, 9, 30),
        ("TLG00003", 9, 30, 10, 15),
        ("TLG00004", 10, 45, 11, 30),
        ("TLG00005", 11, 30, 12, 15),
        ("TLG00006", 12, 15, 13, 0),
        ("TLG00007", 13, 30, 14, 15),
        ("TLG00008", 14, 15, 15, 0),
        ("TLG00009", 15, 5, 15, 45),
        ("TLG00010", 16, 0, 16, 45)
    ]
    
    from models import TimeLog
    time_logs = []
    for tlg_id, start_h, start_m, end_h, end_m in time_logs_data:
        tlg = TimeLog(tlg_id, time(start_h, start_m), time(end_h, end_m))
        time_logs.append(tlg)
    
    return time_logs

def create_sample_tasks():
    """Create sample Task objects with assigned TimeLogs"""
    time_logs = create_sample_data()
    
    # Task assignments based on user's table
    tasks = {
        1: [0, 2, 4],    # TLG00001, TLG00003, TLG00005
        2: [3, 7, 8],    # TLG00004, TLG00008, TLG00009
        3: [1, 9],       # TLG00002, TLG00010
        4: [5, 6]        # TLG00006, TLG00007
    }
    
    from models import Task
    task_objects = {}
    for task_id, tlg_indices in tasks.items():
        task = Task(task_id)
        for idx in tlg_indices:
            task.add_time_log(time_logs[idx])
        task_objects[task_id] = task
    
    return task_objects, time_logs
