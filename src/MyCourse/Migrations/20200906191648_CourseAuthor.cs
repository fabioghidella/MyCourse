using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCourse.Migrations;

public partial class CourseAuthor : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The PRAGMA foreign_keys = 0; SQL command temporarily suspends foreign key constraint checks
        // so we can manipulate the Courses table without side effects on dependent tables,
        // such as the Lessons table.
        // Constraint checking is re-enabled with PRAGMA foreign_keys = 1; after all other SQL commands have run.

        // IMPORTANT: the PRAGMA SQL command does not work inside a transaction, so
        // true must be passed as the second argument to migrationBuilder.Sql to suppress the transaction. For example:
        // migrationBuilder.Sql("SQL COMMANDS HERE", suppressTransaction: true);
        migrationBuilder.Sql(@"PRAGMA foreign_keys = 0;

CREATE TABLE sqlitestudio_temp_table AS SELECT *
                                          FROM Courses;

DROP TABLE Courses;

CREATE TABLE Courses (
    Id                    INTEGER NOT NULL
                                  CONSTRAINT PK_Courses PRIMARY KEY AUTOINCREMENT,
    Title                 TEXT,
    Description           TEXT,
    ImagePath             TEXT,
    Author                TEXT,
    Email                 TEXT,
    Rating                REAL    NOT NULL,
    FullPrice_Amount      REAL,
    FullPrice_Currency    TEXT,
    CurrentPrice_Amount   REAL,
    CurrentPrice_Currency TEXT,
    RowVersion            TEXT,
    Status                TEXT    NOT NULL
                                  DEFAULT 'Deleted',
    AuthorId              TEXT    REFERENCES AspNetUsers (Id) 
);

INSERT INTO Courses (
                        Id,
                        Title,
                        Description,
                        ImagePath,
                        Author,
                        Email,
                        Rating,
                        FullPrice_Amount,
                        FullPrice_Currency,
                        CurrentPrice_Amount,
                        CurrentPrice_Currency,
                        RowVersion,
                        Status
                    )
                    SELECT Id,
                           Title,
                           Description,
                           ImagePath,
                           Author,
                           Email,
                           Rating,
                           FullPrice_Amount,
                           FullPrice_Currency,
                           CurrentPrice_Amount,
                           CurrentPrice_Currency,
                           RowVersion,
                           Status
                      FROM sqlitestudio_temp_table;

DROP TABLE sqlitestudio_temp_table;

CREATE UNIQUE INDEX IX_Courses_Title ON Courses (
    'Title'
);

CREATE TRIGGER CoursesSetRowVersionOnInsert
         AFTER INSERT
            ON Courses
BEGIN
    UPDATE Courses
       SET RowVersion = CURRENT_TIMESTAMP
     WHERE Id = NEW.Id;
END;

CREATE TRIGGER CoursesSetRowVersionOnUpdate
         AFTER UPDATE
            ON Courses
          WHEN NEW.RowVersion <= OLD.RowVersion
BEGIN
    UPDATE Courses
       SET RowVersion = CURRENT_TIMESTAMP
     WHERE Id = NEW.Id;
END;

PRAGMA foreign_keys = 1;
", suppressTransaction: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"PRAGMA foreign_keys = 0;

CREATE TABLE sqlitestudio_temp_table AS SELECT *
                                          FROM Courses;

DROP TABLE Courses;

CREATE TABLE Courses (
    Id                    INTEGER NOT NULL
                                  CONSTRAINT PK_Courses PRIMARY KEY AUTOINCREMENT,
    Title                 TEXT,
    Description           TEXT,
    ImagePath             TEXT,
    Author                TEXT,
    Email                 TEXT,
    Rating                REAL    NOT NULL,
    FullPrice_Amount      REAL,
    FullPrice_Currency    TEXT,
    CurrentPrice_Amount   REAL,
    CurrentPrice_Currency TEXT,
    RowVersion            TEXT,
    Status                TEXT    NOT NULL
                                  DEFAULT 'Deleted'
);

INSERT INTO Courses (
                        Id,
                        Title,
                        Description,
                        ImagePath,
                        Author,
                        Email,
                        Rating,
                        FullPrice_Amount,
                        FullPrice_Currency,
                        CurrentPrice_Amount,
                        CurrentPrice_Currency,
                        RowVersion,
                        Status
                    )
                    SELECT Id,
                           Title,
                           Description,
                           ImagePath,
                           Author,
                           Email,
                           Rating,
                           FullPrice_Amount,
                           FullPrice_Currency,
                           CurrentPrice_Amount,
                           CurrentPrice_Currency,
                           RowVersion,
                           Status
                      FROM sqlitestudio_temp_table;

DROP TABLE sqlitestudio_temp_table;

CREATE UNIQUE INDEX IX_Courses_Title ON Courses (
    'Title'
);

CREATE TRIGGER CoursesSetRowVersionOnInsert
         AFTER INSERT
            ON Courses
BEGIN
    UPDATE Courses
       SET RowVersion = CURRENT_TIMESTAMP
     WHERE Id = NEW.Id;
END;

CREATE TRIGGER CoursesSetRowVersionOnUpdate
         AFTER UPDATE
            ON Courses
          WHEN NEW.RowVersion <= OLD.RowVersion
BEGIN
    UPDATE Courses
       SET RowVersion = CURRENT_TIMESTAMP
     WHERE Id = NEW.Id;
END;

PRAGMA foreign_keys = 1;
", suppressTransaction: true);
    }
}
