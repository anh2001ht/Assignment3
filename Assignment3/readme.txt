1. Thay đổi connectionstring ở appsetting.json
2. Chuot phai Assignment3 -> Terminal -> chạy lần lượt
		dotnet ef migrations add InitialCreate
		dotnet ef database update
3. Chạy xong thì đã có db ở sql server
	run query để import data test

	-- Users
INSERT INTO Users VALUES ('john123', 'pass123', 'John Doe', 'john@example.com', 'admin');
INSERT INTO Users VALUES ( 'alice456', 'pass456', 'Alice Smith', 'alice@example.com', 'user');
INSERT INTO Users VALUES ( 'bob789', 'pass789', 'Bob Brown', 'bob@example.com', 'user');

-- Event Categories
INSERT INTO EventCategories VALUES ( 'Technology');
INSERT INTO EventCategories VALUES ( 'Health');
INSERT INTO EventCategories VALUES ( 'Education');

-- Events
INSERT INTO Events VALUES ('Tech Conference 2025', 'Annual tech event', '2025-07-10 09:00', '2025-07-10 17:00', 'Hanoi', 1);
INSERT INTO Events VALUES ( 'Health & Wellness Seminar', 'Tips for a healthy life', '2025-08-05 10:00', '2025-08-05 15:00', 'HCMC', 2);
INSERT INTO Events VALUES ('Education Forum', 'Discussion on modern education', '2025-09-01 13:00', '2025-09-01 17:00', 'Danang', 3);

-- Attendees
INSERT INTO Attendees VALUES ( 1, 2, 'Alice Smith', 'alice@example.com', '2025-06-18 08:00');
INSERT INTO Attendees VALUES ( 1, 3, 'Bob Brown', 'bob@example.com', '2025-06-18 08:15');
INSERT INTO Attendees VALUES ( 2, 2, 'Alice Smith', 'alice@example.com', '2025-06-19 09:00');