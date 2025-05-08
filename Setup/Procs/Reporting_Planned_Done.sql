CREATE PROCEDURE [dbo].[Reporting_Planned_Done]
	@IterationNames varchar(1000) = null,
	@IterationQuarters varchar(1000) = null,
	@Teams varchar(2000) = null,
	@Advanced bit = 1	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    drop table if exists #tmp
	drop table if exists #teams
	drop table if exists #iterations
	drop table if exists #results
	drop table if exists #TEAMS2
	DROP TABLE IF EXISTS #FinalResults

	CREATE TABLE #Iterations (Id int, Name varchar(200), StartDate datetime)
	CREATE TABLE #TEAMS2 (Team varchar(200))

	IF @Teams is not null
		BEGIN
			WITH SplitStrings AS (
				SELECT value as TeamName
				FROM STRING_SPLIT(@Teams, ',')
			)
			SELECT TeamName INTO #TeamsStrings FROM SplitStrings

			INSERT INTO #TEAMS2 (Team)
			SELECT Name as Team
			FROM Teams t
			JOIN #TeamsStrings ts on t.Name = ts.TeamName
		END
	ELSE
		BEGIN
			INSERT INTO #TEAMS2 (Team)
			SELECT Name as Team
			FROM Teams
		END

	IF @IterationNames is not null
		BEGIN
			WITH SplitStrings AS (
				SELECT value
				FROM STRING_SPLIT(@IterationNames, ',')
			)
			SELECT value INTO #IterationStrings FROM SplitStrings

			INSERT INTO #Iterations (Id, Name, StartDate)
			SELECT id, i.Name, StartDate
			FROM Iterations i
			JOIN #IterationStrings ist on ist.value = i.Name
		END	

	IF @IterationQuarters is not null
		BEGIN
			WITH SplitStrings AS (
				SELECT value
				FROM STRING_SPLIT(@IterationQuarters, ',')
			)
			SELECT value INTO #IterationQuartersStrings FROM SplitStrings

			INSERT INTO #Iterations (Id, Name, StartDate)
			SELECT id, i.YearQuarter as Name, StartDate
			FROM Iterations i
			JOIN #IterationQuartersStrings ist on ist.value = i.YearQuarter
		END	


	select w.WorkItemId, w.IsDone, w.AreaAdoId, w.IterationId, w.IsPlanned
	into #tmp
	from WorkItemsPlannedDone w
	join #iterations i on i.Id = w.IterationId

	select 
	t.Name as TeamName, 
	i.name,
	(select cast(count(*) as decimal) from #tmp tm where tm.IsDone = 1 and tm.IsPlanned = 0 and tm.AreaAdoId = t.AreaId and i.Id = tm.IterationId) as UnplannedDone,
	(select cast(count(*) as decimal) from #tmp tm where tm.IsDone = 1 and tm.AreaAdoId = t.AreaId and i.Id = tm.IterationId) as TotalDone,
	(select cast(count(*) as decimal) from #tmp tm where tm.IsDone = 1 and tm.IsPlanned = 1 and tm.AreaAdoId = t.AreaId and i.Id = tm.IterationId) as PlannedDone,
	(select cast(count(*) as decimal) from #tmp tm where tm.IsPlanned = 1 and tm.AreaAdoId = t.AreaId and i.Id = tm.IterationId) as PlannedTotal,
	i.StartDate
	into #results
	from teams t
	JOIN #TEAMS2 t2 on t.Name = t2.team
	cross join #iterations i
	order by t.name, i.StartDate

	select 
	name as Iteration,
	TeamName, 
	PlannedDone,
	PlannedTotal,
	CASE 
		WHEN PlannedTotal = 0 THEN 0
		ELSE (CAST(PlannedDone AS DECIMAL(18, 2)) / CAST(PlannedTotal AS DECIMAL(18, 2))) * 100 
	END as 'PercDone',
	UnplannedDone,
	TotalDone,
	CASE 
		WHEN TotalDone = 0 THEN 0
		ELSE (CAST(UnplannedDone AS DECIMAL(18, 2)) / CAST(TotalDone AS DECIMAL(18, 2))) * 100 
	END as 'PercUnplanned'
	into #FinalResults
	from #results
	order by TeamName, StartDate

	SELECT 
	Iteration,
	TeamName,
	PlannedDone,
	PlannedTotal,
	FORMAT(PercDone, 'N2') + '%' as '% Done',
	UnplannedDone,
	TotalDone,
	FORMAT(PercUnplanned, 'N2') + '%' as '% Unplanned'
	FROM #FinalResults
	ORDER BY Iteration, PercDone desc
END
GO