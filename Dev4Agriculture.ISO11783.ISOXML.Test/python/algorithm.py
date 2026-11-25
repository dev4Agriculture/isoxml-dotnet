#!/usr/bin/env python3
"""
Algorithm for splitting TimeLogs across tasks based on split points.
"""

from datetime import datetime, time
from typing import Dict, List, Tuple, Optional

def time_to_minutes(hour: int, minute: int) -> int:
    """Convert hour and minute to minutes since midnight"""
    return hour * 60 + minute

def minutes_to_time(minutes: int) -> Tuple[int, int]:
    """Convert minutes since midnight to hour and minute"""
    return minutes // 60, minutes % 60

class TimeLog:
    """Represents a TimeLog with start and end times"""
    def __init__(self, tlg_id: str, start_h: int, start_m: int, end_h: int, end_m: int):
        self.id = tlg_id
        self.start_h = start_h
        self.start_m = start_m
        self.end_h = end_h
        self.end_m = end_m
        self.start_minutes = time_to_minutes(start_h, start_m)
        self.end_minutes = time_to_minutes(end_h, end_m)
    
    def contains_time(self, hour: int, minute: int) -> bool:
        """Check if this TimeLog contains the specified time"""
        time_minutes = time_to_minutes(hour, minute)
        return self.start_minutes <= time_minutes < self.end_minutes
    
    def split_at(self, split_hour: int, split_minute: int) -> Tuple['TimeLog', 'TimeLog']:
        """Split this TimeLog at the specified time, returning (before, after)"""
        split_minutes = time_to_minutes(split_hour, split_minute)
        
        if split_minutes <= self.start_minutes or split_minutes >= self.end_minutes:
            raise ValueError(f"Split time {split_hour:02d}:{split_minute:02d} is outside TimeLog range")
        
        # Create first part (before split)
        first_start_h, first_start_m = minutes_to_time(self.start_minutes)
        first_end_h, first_end_m = minutes_to_time(split_minutes)
        first_part = TimeLog(f"{self.id}_1", first_start_h, first_start_m, first_end_h, first_end_m)
        
        # Create second part (after split)
        second_start_h, second_start_m = minutes_to_time(split_minutes)
        second_end_h, second_end_m = minutes_to_time(self.end_minutes)
        second_part = TimeLog(f"{self.id}_2", second_start_h, second_start_m, second_end_h, second_end_m)
        
        return first_part, second_part
    
    def __repr__(self):
        return f"TimeLog({self.id}, {self.start_h:02d}:{self.start_m:02d}-{self.end_h:02d}:{self.end_m:02d})"

def split_task_set(input_data: Dict[int, List[Tuple]], split_points: List[Tuple]) -> Dict[int, List[Tuple]]:
    """
    Split and reorganize tasks based on split points.
    
    Args:
        input_data: Dictionary mapping task_id to list of (tlg_id, start_h, start_m, end_h, end_m) tuples
        split_points: List of (task_id, hour, minute, description) tuples
    
    Returns:
        Dictionary mapping new_task_id to list of (tlg_id, start_h, start_m, end_h, end_m, type) tuples
    """
    # Convert input data to TimeLog objects
    all_timelogs = []
    for task_id, timelogs in input_data.items():
        for tlg_id, start_h, start_m, end_h, end_m in timelogs:
            all_timelogs.append((task_id, TimeLog(tlg_id, start_h, start_m, end_h, end_m)))
    
    # Sort all timelogs by start time
    all_timelogs.sort(key=lambda x: x[1].start_minutes)
    
    # Sort split points by time
    split_points_sorted = sorted(split_points, key=lambda x: time_to_minutes(x[1], x[2]))
    
    # Initialize result structure dynamically based on split points
    result = {}
    for split_task_id, _, _, _ in split_points:
        if split_task_id not in result:
            result[split_task_id] = []
    
    # For each TimeLog, determine which new tasks it belongs to
    for original_task_id, timelog in all_timelogs:
        # Find all split points that affect this TimeLog
        affecting_splits = []
        for split_task_id, split_hour, split_minute, description in split_points_sorted:
            split_minutes = time_to_minutes(split_hour, split_minute)
            if timelog.start_minutes <= split_minutes < timelog.end_minutes:
                affecting_splits.append((split_minutes, split_task_id))
        
        # Sort affecting splits by time
        affecting_splits.sort(key=lambda x: x[0])
        
        if not affecting_splits:
            # This TimeLog is not affected by any split points
            # Find which task is active when this TimeLog starts
            active_task = None
            for split_task_id, split_hour, split_minute, description in split_points_sorted:
                split_minutes = time_to_minutes(split_hour, split_minute)
                if split_minutes <= timelog.start_minutes:
                    active_task = split_task_id
                else:
                    break
            
            if active_task is not None:
                # Check if this TimeLog gets split by a future split point
                split_occurred = False
                for split_task_id, split_hour, split_minute, description in split_points_sorted:
                    if split_task_id != active_task:
                        split_minutes = time_to_minutes(split_hour, split_minute)
                        if timelog.start_minutes <= split_minutes < timelog.end_minutes:
                            # This TimeLog gets split
                            split_occurred = True
                            end_h, end_m = minutes_to_time(split_minutes)
                            tlg_id = f"{timelog.id}_1"
                            tlg_type = "split"
                            result[active_task].append((tlg_id, timelog.start_h, timelog.start_m, end_h, end_m, tlg_type))
                            break
                
                if not split_occurred:
                    tlg_id = timelog.id
                    tlg_type = "preserved"
                    result[active_task].append((tlg_id, timelog.start_h, timelog.start_m, timelog.end_h, timelog.end_m, tlg_type))
        else:
            # This TimeLog is affected by split points
            # Create segments for each split
            current_start = timelog.start_minutes
            segment_count = 0
            
            for split_minutes, split_task_id in affecting_splits:
                if current_start < split_minutes:
                    # Create segment from current_start to split_minutes
                    start_h, start_m = minutes_to_time(current_start)
                    end_h, end_m = minutes_to_time(split_minutes)
                    
                    # Find which task is active for this segment
                    active_task = None
                    for sp_task_id, sp_hour, sp_minute, description in split_points_sorted:
                        sp_minutes = time_to_minutes(sp_hour, sp_minute)
                        if sp_minutes <= current_start:
                            active_task = sp_task_id
                        else:
                            break
                    
                    if active_task is not None:
                        segment_count += 1
                        if segment_count == 1:
                            tlg_id = f"{timelog.id}_1"
                        else:
                            tlg_id = f"{timelog.id}_2"
                        
                        tlg_type = "split"
                        result[active_task].append((tlg_id, start_h, start_m, end_h, end_m, tlg_type))
                    
                    current_start = split_minutes
            
            # Create final segment if there's remaining time
            if current_start < timelog.end_minutes:
                start_h, start_m = minutes_to_time(current_start)
                end_h, end_m = minutes_to_time(timelog.end_minutes)
                
                # Find which task is active for this final segment
                active_task = None
                for sp_task_id, sp_hour, sp_minute, description in split_points_sorted:
                    sp_minutes = time_to_minutes(sp_hour, sp_minute)
                    if sp_minutes <= current_start:
                        active_task = sp_task_id
                    else:
                        break
                
                if active_task is not None:
                    segment_count += 1
                    if segment_count == 1:
                        tlg_id = f"{timelog.id}_1"
                    else:
                        tlg_id = f"{timelog.id}_2"
                    
                    tlg_type = "split"
                    result[active_task].append((tlg_id, start_h, start_m, end_h, end_m, tlg_type))
    
    # Sort each task's TimeLogs by start time
    for task_id in result:
        result[task_id].sort(key=lambda x: time_to_minutes(x[1], x[2]))
    
    return result

def test_algorithm():
    """Test the algorithm with the provided data"""
    from data import input_data, split_points, output_data
    
    print("Testing SplitTaskSet Algorithm...")
    print("=" * 50)
    
    print("Input Data:")
    for task_id, timelogs in input_data.items():
        print(f"  Task {task_id}:")
        for tlg_id, start_h, start_m, end_h, end_m in timelogs:
            print(f"    {tlg_id}: {start_h:02d}:{start_m:02d} - {end_h:02d}:{end_m:02d}")
    
    print("\nSplit Points:")
    for task_id, hour, minute, description in split_points:
        print(f"  Task {task_id} starts at {hour:02d}:{minute:02d} - {description}")
    
    print("\nExpected Output:")
    for task_id, timelogs in output_data.items():
        print(f"  Task {task_id}:")
        for tlg_id, start_h, start_m, end_h, end_m, tlg_type in timelogs:
            print(f"    {tlg_id}: {start_h:02d}:{start_m:02d} - {end_h:02d}:{end_m:02d} ({tlg_type})")
    
    print("\nRunning Algorithm...")
    result = split_task_set(input_data, split_points)
    
    print("\nAlgorithm Result:")
    for task_id, timelogs in result.items():
        print(f"  Task {task_id}:")
        for tlg_id, start_h, start_m, end_h, end_m, tlg_type in timelogs:
            print(f"    {tlg_id}: {start_h:02d}:{start_m:02d} - {end_h:02d}:{end_m:02d} ({tlg_type})")
    
    print("\nValidation:")
    # Simple validation - check if we have the same number of TimeLogs
    expected_count = sum(len(timelogs) for timelogs in output_data.values())
    actual_count = sum(len(timelogs) for timelogs in result.values())
    
    print(f"  Expected TimeLogs: {expected_count}")
    print(f"  Actual TimeLogs: {actual_count}")
    print(f"  Match: {'✅' if expected_count == actual_count else '❌'}")
    
    return result

if __name__ == "__main__":
    test_algorithm()
