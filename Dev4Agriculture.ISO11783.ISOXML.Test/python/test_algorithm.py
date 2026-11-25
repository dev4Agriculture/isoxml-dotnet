#!/usr/bin/env python3
"""
Simple test cases for the SplitTaskSet algorithm.
"""

from algorithm import split_task_set

def test_simple_case():
    """Test with a simple case: 2 tasks, 3 TimeLogs, 2 split points"""
    print("Testing Simple Case...")
    print("=" * 40)
    
    # Simple input: 2 tasks with 3 TimeLogs
    input_data = {
        1: [
            ("TLG001", 8, 0, 10, 0),    # 8:00 - 10:00
        ],
        2: [
            ("TLG002", 10, 0, 12, 0),   # 10:00 - 12:00
            ("TLG003", 14, 0, 16, 0),   # 14:00 - 16:00
        ]
    }
    
    # Split points: Task 5 starts at 9:00, Task 6 starts at 11:00
    split_points = [
        (5, 9, 0, "Task 5 starts"),
        (6, 11, 0, "Task 6 starts")
    ]
    
    print("Input Data:")
    for task_id, timelogs in input_data.items():
        print(f"  Task {task_id}:")
        for tlg_id, start_h, start_m, end_h, end_m in timelogs:
            print(f"    {tlg_id}: {start_h:02d}:{start_m:02d} - {end_h:02d}:{end_m:02d}")
    
    print("\nSplit Points:")
    for task_id, hour, minute, description in split_points:
        print(f"  Task {task_id} starts at {hour:02d}:{minute:02d} - {description}")
    
    print("\nRunning Algorithm...")
    result = split_task_set(input_data, split_points)
    
    print("\nResult:")
    for task_id, timelogs in result.items():
        print(f"  Task {task_id}:")
        for tlg_id, start_h, start_m, end_h, end_m, tlg_type in timelogs:
            print(f"    {tlg_id}: {start_h:02d}:{start_m:02d} - {end_h:02d}:{end_m:02d} ({tlg_type})")
    
    print(f"\nTotal TimeLogs created: {sum(len(timelogs) for timelogs in result.values())}")
    return result

def test_edge_case():
    """Test edge case: no split points"""
    print("\n\nTesting Edge Case: No Split Points...")
    print("=" * 40)
    
    input_data = {
        1: [
            ("TLG001", 8, 0, 10, 0),
        ]
    }
    
    split_points = []  # No split points
    
    print("Input Data:")
    for task_id, timelogs in input_data.items():
        print(f"  Task {task_id}:")
        for tlg_id, start_h, start_m, end_h, end_m in timelogs:
            print(f"    {tlg_id}: {start_h:02d}:{start_m:02d} - {end_h:02d}:{end_m:02d}")
    
    print("\nSplit Points: None")
    
    print("\nRunning Algorithm...")
    result = split_task_set(input_data, split_points)
    
    print("\nResult:")
    if result:
        for task_id, timelogs in result.items():
            print(f"  Task {task_id}:")
            for tlg_id, start_h, start_m, end_h, end_m, tlg_type in timelogs:
                print(f"    {tlg_id}: {start_h:02d}:{start_m:02d} - {end_h:02d}:{end_m:02d} ({tlg_type})")
    else:
        print("  No tasks created (empty result)")
    
    print(f"\nTotal TimeLogs created: {sum(len(timelogs) for timelogs in result.values())}")
    return result

if __name__ == "__main__":
    print("SplitTaskSet Algorithm Test Cases")
    print("=" * 50)
    
    # Run test cases
    test_simple_case()
    test_edge_case()
    
    print("\n" + "=" * 50)
    print("All tests completed!")
