#!/usr/bin/env python3
"""
Main entry point for the ISOXML TimeLog and Task demonstration.
This file shows how to use the TimeLog and Task classes with sample data.
"""

from models import TimeLog, Task
from utils import time, create_sample_tasks, create_sample_data
from test_cases import run_all_tests

def demonstrate_basic_usage():
    """Demonstrate basic usage of TimeLog and Task classes"""
    print("=" * 60)
    print("BASIC USAGE DEMONSTRATION")
    print("=" * 60)
    print()
    
    # Create a simple TimeLog
    print("1. Creating a TimeLog:")
    tlg = TimeLog("TLG00001", time(8, 0), time(8, 45))
    print(f"   {tlg}")
    print(f"   Duration: {tlg.duration_minutes()} minutes")
    print()
    
    # Create a Task and add TimeLogs
    print("2. Creating a Task and adding TimeLogs:")
    task = Task(1)
    task.add_time_log(tlg)
    task.add_time_log(TimeLog("TLG00002", time(9, 30), time(10, 15)))
    print(f"   {task}")
    print()
    
    # Test IsInActiveWork
    print("3. Testing IsInActiveWork function:")
    test_times = [time(8, 30), time(9, 0), time(10, 0)]
    for ts in test_times:
        is_active = task.is_in_active_work(ts)
        print(f"   Time {ts.strftime('%H:%M:%S')}: {'Active' if is_active else 'Inactive'}")
    print()

def demonstrate_sample_data():
    """Demonstrate the sample data from the user's table"""
    print("=" * 60)
    print("SAMPLE DATA DEMONSTRATION")
    print("=" * 60)
    print()
    
    # Create sample data
    task_objects, time_logs = create_sample_tasks()
    
    # Display all TimeLogs
    print("All TimeLogs:")
    for i, tlg in enumerate(time_logs):
        print(f"  {i+1:2d}. {tlg}")
    print()
    
    # Display all Tasks
    print("All Tasks:")
    for task_id, task in task_objects.items():
        print(f"  {task}")
    print()
    
    # Show task distribution
    print("Task Distribution:")
    for task_id, task in task_objects.items():
        tlg_ids = [tlg.id for tlg in task.time_logs]
        print(f"  Task {task_id}: {', '.join(tlg_ids)}")
    print()

def demonstrate_timestamp_analysis():
    """Demonstrate timestamp analysis across all tasks"""
    print("=" * 60)
    print("TIMESTAMP ANALYSIS")
    print("=" * 60)
    print()
    
    task_objects, _ = create_sample_tasks()
    
    # Test various timestamps
    test_timestamps = [
        (time(8, 30), "Morning work"),
        (time(10, 0), "Mid-morning"),
        (time(12, 0), "Lunch time"),
        (time(14, 0), "Afternoon"),
        (time(15, 30), "Late afternoon"),
        (time(16, 30), "End of day")
    ]
    
    print("Timestamp Analysis:")
    print("Time     | Active Tasks | Description")
    print("-" * 50)
    
    for ts, desc in test_timestamps:
        active_tasks = []
        for task_id, task in task_objects.items():
            if task.is_in_active_work(ts):
                active_tasks.append(str(task_id))
        
        active_str = ", ".join(active_tasks) if active_tasks else "None"
        print(f"{ts.strftime('%H:%M:%S')} | {active_str:11} | {desc}")
    
    print()

def main():
    """Main function"""
    print("ISOXML TimeLog and Task Demonstration")
    print("=====================================")
    print()
    
    # Run demonstrations
    demonstrate_basic_usage()
    demonstrate_sample_data()
    demonstrate_timestamp_analysis()
    
    # Ask if user wants to run tests
    print("=" * 60)
    response = input("Would you like to run the full test suite? (y/n): ").lower().strip()
    
    if response in ['y', 'yes']:
        print()
        run_all_tests()
    else:
        print("Skipping tests. You can run them later with: python test_cases.py")
    
    print("\nDemonstration completed!")

if __name__ == "__main__":
    main()



