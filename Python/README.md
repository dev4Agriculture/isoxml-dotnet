# ISOXML TimeLog and Task Management

This Python project demonstrates the core concepts for managing TimeLogs and Tasks in an ISOXML system. It provides a foundation for implementing the `SplitTaskSet` functionality.

## Project Structure

```
Python/
├── models.py          # Core data models (TimeLog, Task classes)
├── utils.py           # Utility functions and sample data creation
├── test_cases.py      # Comprehensive test suite
├── main.py            # Main demonstration script
└── README.md          # This file
```

## Core Classes

### TimeLog Class
- **Purpose**: Represents a single time log entry
- **Properties**: `id`, `start_time`, `end_time`
- **Methods**: `duration_minutes()`

### Task Class
- **Purpose**: Represents a task containing multiple TimeLogs
- **Properties**: `id`, `time_logs` (list of TimeLog objects)
- **Key Methods**:
  - `get_start_time()`: Returns minimum start time across all TimeLogs
  - `get_end_time()`: Returns maximum end time across all TimeLogs
  - `is_in_active_work(timestamp)`: Checks if timestamp falls within any TimeLog's range
  - `get_duration_minutes()`: Calculates total task duration
  - `add_time_log()`: Adds a TimeLog to the task

## Sample Data

The project includes sample data based on your table:

| TLGNo | TaskNo | TLGStart | TLGEnd | Notes |
|-------|--------|----------|---------|-------|
| TLG00001 | 1 | 08:00:00 | 08:45:00 | Task 1 starts |
| TLG00002 | 3 | 08:45:00 | 09:30:00 | No gap - Different task (Task 3) |
| TLG00003 | 1 | 09:30:00 | 10:15:00 | No gap - Different task (Task 1) |
| TLG00004 | 2 | 10:45:00 | 11:30:00 | Gap: 30 min - Different task (Task 2) |
| TLG00005 | 1 | 11:30:00 | 12:15:00 | No gap - Different task (Task 1 again) |
| TLG00006 | 4 | 12:15:00 | 13:00:00 | No gap - Different Task (Task 4) |
| TLG00007 | 4 | 13:30:00 | 14:15:00 | Gap: 30 min - Same task (Task 4) |
| TLG00008 | 2 | 14:15:00 | 15:00:00 | No gap - Different task (Task 2 again) |
| TLG00009 | 2 | 15:05:00 | 15:45:00 | Gap 5 min - Same task (Task 2) |
| TLG00010 | 3 | 16:00:00 | 16:45:00 | Gap: 15 min - Different task (Task 3 again) |

## Usage Examples

### Basic Usage
```python
from models import TimeLog, Task
from utils import time

# Create a TimeLog
tlg = TimeLog("TLG00001", time(8, 0), time(8, 45))

# Create a Task and add TimeLogs
task = Task(1)
task.add_time_log(tlg)

# Check if a timestamp is within active work
is_active = task.is_in_active_work(time(8, 30))  # Returns True
```

### Working with Sample Data
```python
from utils import create_sample_tasks

# Get sample tasks and time logs
task_objects, time_logs = create_sample_tasks()

# Access specific task
task_1 = task_objects[1]
print(f"Task 1: {task_1}")
print(f"Start: {task_1.get_start_time()}")
print(f"End: {task_1.get_end_time()}")
```

## Running the Project

### Prerequisites
- Python 3.6 or higher
- No external dependencies required

### Quick Start
1. **Run the main demonstration**:
   ```bash
   python main.py
   ```

2. **Run the test suite**:
   ```bash
   python test_cases.py
   ```

3. **Run individual components**:
   ```bash
   python -c "from utils import create_sample_tasks; print(create_sample_tasks())"
   ```

## Key Features

### Task Time Range Calculation
- **Start Time**: Minimum of all TimeLog start times
- **End Time**: Maximum of all TimeLog end times
- **Duration**: Total span from start to end

### Active Work Detection
The `is_in_active_work(timestamp)` method checks if a given timestamp falls within any TimeLog's time range within a task.

### Gap Handling
The system handles gaps between TimeLogs naturally:
- Gaps within tasks are preserved
- Task boundaries are calculated from actual TimeLog data
- No assumptions about continuous time coverage

## Future Development

This foundation can be extended to implement:
- **SplitTaskSet functionality**: Reorganizing tasks based on new criteria
- **Task merging**: Combining related tasks
- **Time-based filtering**: Filtering tasks by time ranges
- **Conflict detection**: Identifying overlapping TimeLogs
- **Export/Import**: Converting to/from ISOXML format

## Testing

The test suite covers:
- Basic class creation and functionality
- TimeLog and Task operations
- IsInActiveWork function accuracy
- Sample data validation
- Edge cases (empty tasks, zero duration, etc.)

Run tests to verify everything works correctly:
```bash
python test_cases.py
```

## Notes for C# Conversion

When converting to C#:
- Replace Python's `datetime` with C#'s `DateTime`
- Convert Python lists to C# `List<T>` or `IEnumerable<T>`
- Replace Python's optional types with C# nullable types
- Convert Python string formatting to C# string interpolation
- Consider using C# properties instead of Python's direct attribute access
