# Initial data from the user's table
input_data = {
    1:[
        ("TLG00001", 8, 0, 8, 45),
        ("TLG00003", 9, 30, 10, 15),
        ("TLG00005", 11, 30, 12, 15)
    ],
    2:[
        ("TLG00004", 10, 45, 11, 30),
        ("TLG00008", 14, 15, 15, 0),
        ("TLG00009", 15, 5, 15, 45)
    ],
    3:[
        ("TLG00002", 8, 45, 9, 30),
        ("TLG00010", 16, 0, 16, 45)
    ],
    4:[
        ("TLG00006", 12, 15, 13, 0),
        ("TLG00007", 13, 30, 14, 15)
    ]
}

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

output_data = {
        5: [  # Task 5 TLGs
            ("TLG00001_1", 8, 0, 8, 15, "split"),       # Split from TLG00001
            ("TLG00004_2", 11, 0, 11, 30, "split"),     # Split from TLG00004
            ("TLG00005_1", 11, 30, 12, 0, "split"), # First half of TLG00005 split
            ("TLG00007_2", 13, 45, 14, 15, "split"), # Second half of TLG00007 split
            ("TLG00008_1", 14, 15, 14, 30, "split"), # Second half of TLG00008 split
        ],
        6: [  # Task 6 TLGs
            ("TLG00001_3", 8, 30, 8, 45, "split"),      # Split from TLG00001
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