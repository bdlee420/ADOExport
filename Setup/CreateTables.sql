drop table if exists WorkItemsPlannedDone
CREATE TABLE WorkItemsPlannedDone (
	WorkItemId int,
	EmployeeAdoId VARCHAR(255),
	IterationId int,
	AreaAdoId int,
	IsDone bit,
	IsPlanned bit,
	IsDeleted bit
)

drop table if exists Teams
CREATE TABLE Teams (
	Id VARCHAR(255),
	Name VARCHAR(255),
	AreaId int	
)

drop table if exists Areas
CREATE TABLE Areas (
	Id int,
	Name VARCHAR(255)
)

drop table if exists Employees

CREATE TABLE Employees (
    EmployeeAdoId VARCHAR(255),
    Name VARCHAR(255),
	TeamName  VARCHAR(255),
	IsLead bit,
	IsFTE bit,
	Activity varchar(10) NULL,
	BCE decimal(28,12),
	Rating tinyint
)

DROP TABLE IF EXISTS WorkItemTags

CREATE TABLE WorkItemTags (
	WorkItemId int NOT NULL,
    Tag VARCHAR(255)
)


DROP TABLE IF EXISTS WorkItems

CREATE TABLE WorkItems (
	WorkItemId int NOT NULL,
    EmployeeAdoId VARCHAR(255),
    IterationPath VARCHAR(255),
	IterationId int,
    WorkItemType VARCHAR(20),
    Estimate DECIMAL(28,12),
	Remaining DECIMAL(28,12),
    AreaAdoId int,
	ParentType VARCHAR(50),
	IsDone bit,
	Activity varchar(50)
)

CREATE UNIQUE INDEX IX_Unique_WorkItemId
ON [dbo].[WorkItems] ([WorkItemId]);

drop table if exists DevCapacity

CREATE TABLE DevCapacity (
    EmployeeAdoId VARCHAR(255),
    IterationAdoId int,
	IterationAdoIdentifier VARCHAR(255),
	TeamAdoId VARCHAR(255),
	Days int,
	IsDev bit
)

CREATE UNIQUE INDEX IX_Unique_DevCapacity
ON [dbo].[DevCapacity] ([EmployeeAdoId], [IterationAdoIdentifier], [TeamAdoId]);



drop table if exists Iterations

CREATE TABLE Iterations (
    Id int,
    Identifier VARCHAR(255),
	Name  VARCHAR(255),
	Path VARCHAR(255),
	StartDate datetime,
	EndDate datetime,
	YearQuarter VARCHAR(255)
)

DROP TABLE IF EXISTS Projects

CREATE TABLE Projects
(
	Timestamp datetime, 
	Tag varchar(200), 
	TotalCapacity decimal(4,2), 
	DevDedication decimal(4,2), 
	Remaining decimal(4,2), 
	StartDate datetime, 
	TargetDate datetime
)