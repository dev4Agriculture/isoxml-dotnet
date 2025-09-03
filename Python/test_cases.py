from models import TimeLog, Task
from utils import time, create_sample_tasks, create_sample_data

def test_time_log_creation():
    """Test TimeLog creation and basic functionality"""
    print("=== Testing TimeLog Creation ===")
    
    tlg = TimeLog("TLG00001", time(8, 0), time(8, 45))
    print(f"Created: {tlg}")
    print(f"Duration: {tlg.duration_minutes()} minutes")
    print(f"Start: {tlg.start_time.strftime('%H:%M:%S')}")
    print(f"End: {tlg.end_time.strftime('%H:%M:%S')}")
    print()

def test_task_creation():
    """Test Task creation and basic functionality"""
    print("=== Testing Task Creation ===")
    
    task = Task(1)
    print(f"Empty task: {task}")
    
    # Add TimeLogs
    tlg1 = TimeLog("TLG00001", time(8, 0), time(8, 45))
    tlg2 = TimeLog("TLG00002", time(9, 0), time(9, 30))
    
    task.add_time_log(tlg1)
    task.add_time_log(tlg2)
    
    print(f"Task with TimeLogs: {task}")
    print(f"Start time: {task.get_start_time().strftime('%H:%M:%S')}")
    print(f"End time: {task.get_end_time().strftime('%H:%M:%S')}")
    print(f"Duration: {task.get_duration_minutes()} minutes")
    print(f"TimeLog count: {task.get_time_log_count()}")
    print()

def test_is_in_active_work():
    """Test the IsInActiveWork function"""
    print("=== Testing IsInActiveWork Function ===")
    
    # Create a task with a TimeLog from 08:00 to 08:45
    task = Task(1)
    task.add_time_log(TimeLog("TLG00001", time(8, 0), time(8, 45)))
    
    # Test various timestamps
    test_timestamps = [
        (time(7, 59), False, "Before start"),
        (time(8, 0), True, "At start"),
        (time(8, 22), True, "Middle"),
        (time(8, 45), True, "At end"),
        (time(8, 46), False, "After end")
    ]
    
    print("Timestamp | IsActive | Description")
    print("-" * 40)
    for ts, expected, desc in test_timestamps:
        result = task.is_in_active_work(ts)
        status = "✓" if result == expected else "✗"
        print(f"{ts.strftime('%H:%M:%S')} |   {result}     | {desc} {status}")
    print()

def test_sample_data():
    """Test with the user's sample data"""
    print("=== Testing Sample Data ===")
    
    task_objects, time_logs = create_sample_tasks()
    
    # Display all tasks
    for task_id, task in task_objects.items():
        print(f"{task}")
    
    print()
    
    # Test IsInActiveWork for various timestamps
    test_timestamps = [
        time(8, 30),   # Should be in Task 1 (TLG00001)
        time(9, 0),    # Should be in Task 3 (TLG00002)
        time(10, 0),   # Should be in Task 1 (TLG00003)
        time(11, 0),   # Should be in Task 2 (TLG00004)
        time(12, 0),   # Should be in Task 1 (TLG00005)
        time(13, 0),   # Should be in Task 4 (TLG00006)
        time(14, 0),   # Should be in Task 4 (TLG00007)
        time(15, 30),  # Should be in Task 2 (TLG00009)
        time(16, 30),  # Should be in Task 3 (TLG00010)
    ]
    
    print("Testing IsInActiveWork function with sample data:")
    print("Timestamp | Task 1 | Task 2 | Task 3 | Task 4")
    print("-" * 55)
    
    for ts in test_timestamps:
        task1_active = task_objects[1].is_in_active_work(ts)
        task2_active = task_objects[2].is_in_active_work(ts)
        task3_active = task_objects[3].is_in_active_work(ts)
        task4_active = task_objects[4].is_in_active_work(ts)
        
        print(f"{ts.strftime('%H:%M:%S')} |   {task1_active}   |   {task2_active}   |   {task3_active}   |   {task4_active}")
    
    print()

def test_edge_cases():
    """Test edge cases and error handling"""
    print("=== Testing Edge Cases ===")
    
    # Empty task
    empty_task = Task(99)
    print(f"Empty task: {empty_task}")
    print(f"Start time: {empty_task.get_start_time()}")
    print(f"End time: {empty_task.get_end_time()}")
    print(f"Duration: {empty_task.get_duration_minutes()}")
    print(f"IsInActiveWork at any time: {empty_task.is_in_active_work(time(12, 0))}")
    print()
    
    # Single TimeLog task
    single_task = Task(100)
    single_task.add_time_log(TimeLog("TLG00099", time(12, 0), time(12, 0)))  # Zero duration
    print(f"Single TimeLog task: {single_task}")
    print(f"Duration: {single_task.get_duration_minutes()} minutes")
    print(f"IsInActiveWork at start: {single_task.is_in_active_work(time(12, 0))}")
    print()

def run_all_tests():
    """Run all test cases"""
    print("=" * 60)
    print("RUNNING ALL TESTS")
    print("=" * 60)
    print()
    
    test_time_log_creation()
    test_task_creation()
    test_is_in_active_work()
    test_sample_data()
    test_edge_cases()
    
    print("=" * 60)
    print("ALL TESTS COMPLETED")
    print("=" * 60)

if __name__ == "__main__":
    run_all_tests()



