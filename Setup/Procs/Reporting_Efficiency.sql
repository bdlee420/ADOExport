CREATE PROCEDURE [dbo].[Reporting_Efficiency]
	@IterationNames varchar(1000) = null,
	@IterationQuarters varchar(1000) = null,
	@Teams varchar(1000) = null,
	@Advanced bit = 0
AS
BEGIN

	SET NOCOUNT ON;

    DROP TABLE IF EXISTS #Results
	DROP TABLE IF EXISTS #Iterations
	DROP TABLE IF EXISTS #TeamCapacityDev
	DROP TABLE IF EXISTS #TeamCapacityQA
	DROP TABLE IF EXISTS #EmployeeActivity
	DROP TABLE IF EXISTS #FinalResults
	DROP TABLE IF EXISTS #WorkItemTagIds
	DROP TABLE IF EXISTS #Teams
	DROP TABLE IF EXISTS #TeamsStrings

	CREATE TABLE #Iterations (Name varchar(200))
	CREATE TABLE #Teams (Team varchar(200))	
	
	IF @IterationNames is not null
		BEGIN
			WITH SplitStrings AS (
				SELECT value
				FROM STRING_SPLIT(@IterationNames, ',')
			)
			SELECT value INTO #IterationStrings FROM SplitStrings

			INSERT INTO #Iterations (Name)
			SELECT i.Name
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

			INSERT INTO #Iterations (Name)
			SELECT i.YearQuarter as Name
			FROM Iterations i
			JOIN #IterationQuartersStrings ist on ist.value = i.YearQuarter
		END	

	IF @Teams is not null
		BEGIN
			WITH SplitStrings AS (
				SELECT value as TeamName
				FROM STRING_SPLIT(@Teams, ',')
			)
			SELECT TeamName INTO #TeamsStrings FROM SplitStrings

			INSERT INTO #Teams (Team)
			SELECT Name as Team
			FROM Teams t
			JOIN #TeamsStrings ts on t.Name = ts.TeamName
		END
	ELSE
		BEGIN
			INSERT INTO #Teams (Team)
			SELECT Name as Team
			FROM Teams
		END
	
	select distinct EmployeeAdoId, IsDev
	into #EmployeeActivity
	from DevCapacity dc
	JOIN Iterations i on i.Id = dc.IterationAdoId
	JOIN #Iterations i2 on i2.Name = i.Name OR i2.Name = i.YearQuarter
	order by EmployeeAdoId

	select 
	distinct
	i2.Name as Iteration,
	t.name,

	(select sum(w.Estimate) 
	from WorkItems w 
	JOIN Iterations i on w.IterationId = i.Id 
	JOIN #Iterations it on it.Name = i.Name OR it.Name = i.YearQuarter
	JOIN #EmployeeActivity e on e.EmployeeAdoId = w.EmployeeAdoId and e.IsDev = 1
	WHERE w.AreaAdoId = a.Id and i2.Name = it.Name) as TotalDev,

	(select sum(w.Estimate) 
	from WorkItems w 
	JOIN Iterations i on w.IterationId = i.Id 
	JOIN #Iterations it on it.Name = i.Name OR it.Name = i.YearQuarter
	JOIN #EmployeeActivity e on e.EmployeeAdoId = w.EmployeeAdoId and e.IsDev = 0
	WHERE w.AreaAdoId = a.Id and i2.Name = it.Name) as TotalQA

	INTO #Results
	FROM Teams t
	JOIN #Teams t2 on t2.Team = t.Name
	CROSS JOIN #Iterations i2 
	join Areas a on t.areaId = a.id

	select i2.Name as Iteration, t.Name, sum(Days) as Days
	into #TeamCapacityDev
	from DevCapacity dc
	join teams t on dc.TeamAdoId = t.id
	JOIN #Teams t2 on t2.Team = t.Name
	JOIN Iterations i on dc.IterationAdoId = i.Id 
	JOIN #Iterations i2 on i2.Name = i.Name OR i2.Name = i.YearQuarter
	WHERE dc.IsDev = 1
	group by t.name, i2.Name

	select i2.Name as Iteration, t.Name, sum(Days) as Days
	into #TeamCapacityQA
	from DevCapacity dc
	join teams t on dc.TeamAdoId = t.id
	JOIN #Teams t2 on t2.Team = t.Name
	JOIN Iterations i on dc.IterationAdoId = i.Id 
	JOIN #Iterations i2 on i2.Name = i.Name OR i2.Name = i.YearQuarter
	WHERE dc.IsDev = 0
	group by t.name, i2.Name


	SELECT
	r.Iteration,
	r.name as Team,
	FORMAT(TotalDev, 'N2') as 'Dev Total (days)',
	FORMAT(TotalDev/td.Days, 'N2') as 'Dev Efficiency',

	FORMAT(TotalQA, 'N2') as 'QA Total (days)',
	FORMAT(TotalQA/tq.Days, 'N2') as 'QA Efficiency',

	FORMAT(TotalDev+TotalQA, 'N2') as 'Total (days)',
	FORMAT((TotalDev+TotalQA)/(tq.Days+td.Days), 'N2') as 'Efficiency'

	INTO #FinalResults
	FROM #Results r
	JOIN #TeamCapacityQA tq
	ON tq.Name = r.name and tq.Iteration = r.Iteration
	JOIN #TeamCapacityDev td
	ON td.Name = r.name  and td.Iteration = r.Iteration

	if @Advanced = 1
		select * from #FinalResults

	SELECT Iteration, Team, [Total (days)], Efficiency, [Dev Efficiency], [QA Efficiency]
	FROM #FinalResults
	ORDER BY iteration desc, Efficiency desc
END
GO

