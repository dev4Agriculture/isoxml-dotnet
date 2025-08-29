#!/usr/bin/env python3
"""
Timeline Diagram Generator for ISOXML TimeLogs
Creates a visual representation of TimeLogs over time.
"""

import matplotlib.pyplot as plt
import matplotlib.patches as patches
from datetime import datetime, time
import numpy as np

def time_to_minutes(t):
    """Convert time to minutes since midnight"""
    return t.hour * 60 + t.minute

def create_timeline_diagram():
    """Create the timeline diagram"""
    
    # Initial data from the user's table
    time_logs_data = [
        ("TLG00001", 1, 8, 0, 8, 45),
        ("TLG00002", 3, 8, 45, 9, 30),
        ("TLG00003", 1, 9, 30, 10, 15),
        ("TLG00004", 2, 10, 45, 11, 30),
        ("TLG00005", 1, 11, 30, 12, 15),
        ("TLG00006", 4, 12, 15, 13, 0),
        ("TLG00007", 4, 13, 30, 14, 15),
        ("TLG00008", 2, 14, 15, 15, 0),
        ("TLG00009", 2, 15, 5, 15, 45),
        ("TLG00010", 3, 16, 0, 16, 45)
    ]
    
    # Create figure and axis
    fig, ax = plt.subplots(figsize=(15, 10))
    
    # Color scheme for tasks
    task_colors = {
        1: '#FF6B6B',  # Red
        2: '#4ECDC4',  # Teal
        3: '#45B7D1',  # Blue
        4: '#96CEB4'   # Green
    }
    
    # Plot each TimeLog as a horizontal bar
    y_positions = []
    for i, (tlg_id, task_no, start_h, start_m, end_h, end_m) in enumerate(time_logs_data):
        start_time = time(start_h, start_m)
        end_time = time(end_h, end_m)
        
        start_minutes = time_to_minutes(start_time)
        end_minutes = time_to_minutes(end_time)
        duration = end_minutes - start_minutes
        
        # Create rectangle for TimeLog
        rect = patches.Rectangle(
            (start_minutes, i - 0.3), 
            duration, 
            0.6, 
            facecolor=task_colors[task_no],
            edgecolor='black',
            linewidth=1,
            alpha=0.8
        )
        ax.add_patch(rect)
        
        # Add TimeLog ID text
        ax.text(start_minutes + duration/2, i, tlg_id, 
                ha='center', va='center', fontweight='bold', fontsize=9)
        
        # Add task number
        ax.text(start_minutes + duration/2, i + 0.4, f"TSK{task_no}", 
                ha='center', va='center', fontsize=8, color='darkblue')
        
        y_positions.append(i)
    
    # Set up the plot
    ax.set_xlim(7 * 60, 17 * 60)  # 7:00 to 17:00
    ax.set_ylim(-0.5, len(time_logs_data) - 0.5)
    
    # Set x-axis ticks (every hour)
    hour_ticks = range(7, 18)
    ax.set_xticks([h * 60 for h in hour_ticks])
    ax.set_xticklabels([f"{h:02d}:00" for h in hour_ticks])
    
    # Set y-axis labels
    ax.set_yticks(y_positions)
    ax.set_yticklabels([f"TLG{i+1:05d}" for i in range(len(time_logs_data))])
    
    # Add grid
    ax.grid(True, alpha=0.3, axis='x')
    
    # Add labels and title
    ax.set_xlabel('Time', fontsize=12, fontweight='bold')
    ax.set_ylabel('TimeLog Index', fontsize=12, fontweight='bold')
    ax.set_title('ISOXML TimeLog Timeline Diagram (Initial Data)', fontsize=14, fontweight='bold')
    
    # Add legend for task colors
    legend_elements = [patches.Patch(color=color, label=f'Task {task}') 
                      for task, color in task_colors.items()]
    ax.legend(handles=legend_elements, loc='upper right', title='Task Colors')
    
    # Add time annotations for key events
    key_times = [
        (8, 0, "08:00 - Task 1 starts"),
        (8, 45, "08:45 - Task 3 starts"),
        (9, 30, "09:30 - Task 1 resumes"),
        (10, 45, "10:45 - Task 2 starts"),
        (11, 30, "11:30 - Task 1 resumes"),
        (12, 15, "12:15 - Task 4 starts"),
        (13, 30, "13:30 - Task 4 continues"),
        (14, 15, "14:15 - Task 2 resumes"),
        (15, 5, "15:05 - Task 2 continues"),
        (16, 0, "16:00 - Task 3 resumes")
    ]
    
    for hour, minute, label in key_times:
        time_minutes = hour * 60 + minute
        ax.axvline(x=time_minutes, color='red', linestyle='--', alpha=0.5)
        ax.text(time_minutes, len(time_logs_data) - 0.5, label, 
                rotation=90, ha='right', va='top', fontsize=8, color='red')
    
    # Adjust layout
    plt.tight_layout()
    
    return fig

def create_combined_diagram():
    """Create combined diagram showing both original TimeLogs and new task assignments"""
    
    # Split points data
    split_points = [
        (5, 7, 45, "Task 5 active"),
        (5, 11, 0, "Task 5 active"),
        (5, 13, 45, "Task 5 active"),
        (6, 8, 30, "Task 6 active"),
        (6, 12, 0, "Task 6 active"),
        (6, 14, 30, "Task 6 active"),
        (7, 8, 15, "Task 7 active"),
        (7, 10, 0, "Task 7 active"),
        (7, 15, 30, "Task 7 active")
    ]
    
    # New task TLG assignments at each specific activation point
    # Format: (task_no, hour, minute): [TLGs created at this point]
    split_point_tlgs = {
        (5, 7, 45): ["TLG00011"],  # Task 5 starts - gets TLG00011 (split from TLG00001)
        (5, 11, 0): ["TLG00012", "TLG00013"],  # Task 5 resumes - gets TLG00012, TLG00013
        (5, 13, 45): ["TLG00014", "TLG00015"],  # Task 5 resumes - gets TLG00014, TLG00015
        (6, 8, 30): ["TLG00016"],  # Task 6 starts - gets TLG00016 (split from TLG00001)
        (6, 12, 0): ["TLG00017", "TLG00018"],  # Task 6 resumes - gets TLG00017, TLG00018
        (6, 14, 30): ["TLG00019", "TLG00020", "TLG00021"],  # Task 6 resumes - gets TLG00019, TLG00020, TLG00021
        (7, 8, 15): ["TLG00022"],  # Task 7 starts - gets TLG00022 (split from TLG00001)
        (7, 10, 0): ["TLG00023"],  # Task 7 resumes - gets TLG00023 (split from TLG00003)
        (7, 15, 30): ["TLG00024"]  # Task 7 resumes - gets TLG00024 (split from TLG00009)
    }
    
    # Create figure with subplots
    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(16, 12), height_ratios=[1, 2])
    
    # Color scheme for new tasks
    new_task_colors = {
        5: '#FF8C42',  # Orange
        6: '#9370DB',  # Medium Purple
        7: '#20B2AA'   # Light Sea Green
    }
    
    # Top subplot: New task assignments and split points
    for task_no, hour, minute, label in split_points:
        time_minutes = hour * 60 + minute
        color = new_task_colors[task_no]
        
        # Add vertical line for split point
        ax1.axvline(x=time_minutes, color=color, linestyle='-', linewidth=3, alpha=0.7)
    
    # Define the actual TLGs for each new task with their time ranges (CORRECTED)
    new_task_tlg_data = {
        5: [  # Task 5 TLGs
            ("TLG00001_1", 8, 0, 8, 15, "split"),       # Split from TLG00001
            ("TLG00004_2", 11, 0, 11, 30, "split"),     # Split from TLG00004
            ("TLG00005_1", 11, 30, 12, 0, "split"), # First half of TLG00005 split
            ("TLG00007_2", 13, 45, 14, 15, "split"), # Second half of TLG00007 split
            ("TLG00008_2", 14, 15, 14, 30, "split"), # Second half of TLG00008 split
        ],
        6: [  # Task 6 TLGs
            ("TLG00016", 8, 30, 8, 45, "split"),      # Split from TLG00001
            ("TLG00002", 8, 45, 9, 30, "preserved"),  # Preserved from original (moved to Task 6)
            ("TLG00003_1", 9, 30, 10, 0, "split"), # First half of TLG00003 split
            ("TLG00005_2", 12, 0, 12, 15, "split"),     # Split from TLG00005
            ("TLG00006", 12, 15, 13, 0, "preserved"),     # Split from TLG00006
            ("TLG00007_1", 13, 30, 13, 45, "split"), # First half of TLG00007 split
            ("TLG00008_2", 14, 30, 15, 0, "split"), # Second half of TLG00008 split
            ("TLG00009_1", 15, 5, 15, 30, "split")  # First half of TLG00009 split
        ],
        7: [  # Task 7 TLGs
            ("TLG00001_2", 8, 15, 8, 30, "split"),      # Split from TLG00001 (corrected end time)
            ("TLG00003_2", 10, 0, 10, 15, "split"),     # Split from TLG00003
            ("TLG00004_1", 10, 45, 11, 00, "split"),     # Split from TLG00003
            ("TLG00009_2", 15, 30, 15, 45, "split"),     # Split from TLG00009
            ("TLG000010", 16, 00, 16, 45, "preserved")     # Split from TLG00009
        ]
    }
    
    # Plot TLG bars for each new task
    for task_no, tlgs in new_task_tlg_data.items():
        y_position = task_no - 0.3  # Adjust Y position for each task
        
        for tlg_id, start_h, start_m, end_h, end_m, tlg_type in tlgs:
            start_minutes = start_h * 60 + start_m
            end_minutes = end_h * 60 + end_m
            duration = end_minutes - start_minutes
            
            # Choose color based on TLG type
            if tlg_type == "preserved":
                face_color = new_task_colors[task_no]
                edge_color = 'darkblue'
                line_width = 2
            else:  # split
                face_color = new_task_colors[task_no]
                edge_color = 'red'
                line_width = 1
            
            # Create rectangle for TLG
            rect = patches.Rectangle(
                (start_minutes, y_position), 
                duration, 
                0.6, 
                facecolor=face_color,
                edgecolor=edge_color,
                linewidth=line_width,
                alpha=0.8
            )
            ax1.add_patch(rect)
            
            # Add TLG ID text
            ax1.text(start_minutes + duration/2, y_position + 0.3, tlg_id, 
                    ha='center', va='center', fontweight='bold', fontsize=8)
            
            # Add type indicator
            type_text = "P" if tlg_type == "preserved" else "S"
            ax1.text(start_minutes + duration/2, y_position + 0.5, type_text, 
                    ha='center', va='center', fontsize=7, color='white', fontweight='bold')
    
    # Set up the top subplot
    ax1.set_xlim(7 * 60, 17 * 60)  # 7:00 to 17:00
    ax1.set_ylim(4.5, 7.5)
    
    # Set x-axis ticks
    hour_ticks = range(7, 18)
    ax1.set_xticks([h * 60 for h in hour_ticks])
    ax1.set_xticklabels([f"{h:02d}:00" for h in hour_ticks])
    
    # Set y-axis
    ax1.set_yticks([5, 6, 7])
    ax1.set_yticklabels(['Task 5', 'Task 6', 'Task 7'])
    
    # Add grid
    ax1.grid(True, alpha=0.3, axis='x')
    
    # Add labels and title for top subplot
    ax1.set_ylabel('New Task', fontsize=12, fontweight='bold')
    ax1.set_title('New Task TLG Timeline (P=Preserved, S=Split)', fontsize=14, fontweight='bold')
    
    # Add legend for top subplot
    legend_elements = [
        patches.Patch(color=new_task_colors[5], label='Task 5'),
        patches.Patch(color=new_task_colors[6], label='Task 6'),
        patches.Patch(color=new_task_colors[7], label='Task 7')
    ]
    ax1.legend(handles=legend_elements, loc='upper right', title='New Task Colors')
    
    # Bottom subplot: Original TimeLogs timeline
    # Initial data from the user's table
    time_logs_data = [
        ("TLG00001", 1, 8, 0, 8, 45),
        ("TLG00002", 3, 8, 45, 9, 30),
        ("TLG00003", 1, 9, 30, 10, 15),
        ("TLG00004", 2, 10, 45, 11, 30),
        ("TLG00005", 1, 11, 30, 12, 15),
        ("TLG00006", 4, 12, 15, 13, 0),
        ("TLG00007", 4, 13, 30, 14, 15),
        ("TLG00008", 2, 14, 15, 15, 0),
        ("TLG00009", 2, 15, 5, 15, 45),
        ("TLG00010", 3, 16, 0, 16, 45)
    ]
    
    # Color scheme for original tasks
    task_colors = {
        1: '#FF6B6B',  # Red
        2: '#4ECDC4',  # Teal
        3: '#45B7D1',  # Blue
        4: '#96CEB4'   # Green
    }
    
    # Plot each TimeLog as a horizontal bar in bottom subplot
    y_positions = []
    for i, (tlg_id, task_no, start_h, start_m, end_h, end_m) in enumerate(time_logs_data):
        start_time = time(start_h, start_m)
        end_time = time(end_h, end_m)
        
        start_minutes = time_to_minutes(start_time)
        end_minutes = time_to_minutes(end_time)
        duration = end_minutes - start_minutes
        
        # Create rectangle for TimeLog
        rect = patches.Rectangle(
            (start_minutes, i - 0.3), 
            duration, 
            0.6, 
            facecolor=task_colors[task_no],
            edgecolor='black',
            linewidth=1,
            alpha=0.8
        )
        ax2.add_patch(rect)
        
        # Add TimeLog ID text
        ax2.text(start_minutes + duration/2, i, tlg_id, 
                ha='center', va='center', fontweight='bold', fontsize=9)
        
        # Add task number
        ax2.text(start_minutes + duration/2, i + 0.4, f"TSK{task_no}", 
                ha='center', va='center', fontsize=8, color='darkblue')
        
        y_positions.append(i)
    
    # Set up the bottom subplot
    ax2.set_xlim(7 * 60, 17 * 60)  # 7:00 to 17:00
    ax2.set_ylim(-0.5, len(time_logs_data) - 0.5)
    
    # Set x-axis ticks (every hour)
    ax2.set_xticks([h * 60 for h in hour_ticks])
    ax2.set_xticklabels([f"{h:02d}:00" for h in hour_ticks])
    
    # Set y-axis labels
    ax2.set_yticks(y_positions)
    ax2.set_yticklabels([f"TLG{i+1:05d}" for i in range(len(time_logs_data))])
    
    # Add grid
    ax2.grid(True, alpha=0.3, axis='x')
    
    # Add labels and title for bottom subplot
    ax2.set_xlabel('Time', fontsize=12, fontweight='bold')
    ax2.set_ylabel('TimeLog Index', fontsize=12, fontweight='bold')
    ax2.set_title('Original TimeLog Timeline', fontsize=14, fontweight='bold')
    
    # Add legend for original tasks
    legend_elements = [patches.Patch(color=color, label=f'Task {task}') 
                      for task, color in task_colors.items()]
    ax2.legend(handles=legend_elements, loc='upper right', title='Original Task Colors')
    
    # Add split point indicators to bottom subplot
    for task_no, hour, minute, label in split_points:
        time_minutes = hour * 60 + minute
        color = new_task_colors[task_no]
        ax2.axvline(x=time_minutes, color=color, linestyle='--', alpha=0.5, linewidth=2)
    
    # Adjust layout
    plt.tight_layout()
    
    return fig

def main():
    """Main function to create and display diagrams"""
    print("Creating ISOXML Timeline Diagrams...")
    
    # Create initial timeline diagram
    fig1 = create_timeline_diagram()
    fig1.savefig('timeline_initial.png', dpi=300, bbox_inches='tight')
    print("✅ Saved: timeline_initial.png")
    
    # Create combined diagram
    fig2 = create_combined_diagram()
    fig2.savefig('timeline_combined.png', dpi=300, bbox_inches='tight')
    print("✅ Saved: timeline_combined.png")
    
    # Show plots
    plt.show()
    
    print("Diagrams created successfully!")

if __name__ == "__main__":
    main()

