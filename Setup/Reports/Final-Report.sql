DECLARE @Teams varchar(2000)				= null
DECLARE @Tags varchar(2000)					= null
DECLARE @IterationNames varchar(2000)		= null
DECLARE @IterationQuarters varchar(2000)	= null
DECLARE @ParentTypes varchar(2000)			= null --Bug or User Story
DECLARE @WorkItemTypes varchar(2000)		= null --Task or Defect

--Efficiency
EXEC Reporting_Efficiency
@IterationNames =		@IterationNames,
@IterationQuarters =	@IterationQuarters,
@Teams =				@Teams,
@Advanced =				0

--Tags
EXEC Reporting_Tracked_By_Iteration
@IterationNames =		@IterationNames,
@IterationQuarters =	@IterationQuarters,
@Teams =				@Teams,
@Tags =					@Tags, 
@Advanced =				0

--Bugs
EXEC Reporting_Tracked_By_Iteration
@IterationNames =		@IterationNames,
@IterationQuarters =	@IterationQuarters,
@Teams =				@Teams,
@ParentTypes =			'Bug',
@Advanced =				0

--Defects
EXEC Reporting_Tracked_By_Iteration
@IterationNames =		@IterationNames,
@IterationQuarters =	@IterationQuarters,
@Teams =				@Teams,
@WorkItemTypes =		'Defect',
@Advanced =				0

--Planned/Done
EXEC Reporting_Planned_Done
@IterationNames =		@IterationNames,
@IterationQuarters =	@IterationQuarters,
@Teams =				@Teams,
@Advanced =				1

--ALL TEAMS and ALL ITERATIONS - Tag %
EXEC Reporting_Tracked
@IterationNames =		@IterationNames,
@IterationQuarters =	@IterationQuarters,
@Tags =					@Tags, 
@Advanced =				0

IF @Teams is not null
	BEGIN
		--ALL TEAMS - Efficiency
		EXEC Reporting_Efficiency
		@IterationNames =		@IterationNames,
		@IterationQuarters =	@IterationQuarters,
		@Advanced =				0
	END