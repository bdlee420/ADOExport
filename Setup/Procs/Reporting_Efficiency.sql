ALTER PROCEDURE [dbo].[Reporting_Efficiency]
	@IterationNames varchar(1000) = null,
	@IterationQuarters varchar(1000) = null,
	@StartDate datetime = null,
	@Teams varchar(1000) = null,
	@Advanced bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
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

	CREATE TABLE #Iterations (Id int, Name varchar(200))
	CREATE TABLE #Teams (Team varchar(200))	
	
	IF @IterationNames is not null
		BEGIN
			WITH SplitStrings AS (
				SELECT value
				FROM STRING_SPLIT(@IterationNames, ',')
			)
			SELECT value INTO #IterationStrings FROM SplitStrings

			INSERT INTO #Iterations (Id, Name)
			SELECT i.Id, i.Name
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

			INSERT INTO #Iterations (Id, Name)
			SELECT i.Id, i.YearQuarter as Name
			FROM Iterations i
			JOIN #IterationQuartersStrings ist on ist.value = i.YearQuarter
		END	

	IF @StartDate is not null
		BEGIN	
			INSERT INTO #Iterations (Id, Name)
			SELECT i.Id, 'Since ' + FORMAT(@StartDate, 'MMM dd, yyyy') as Name
			FROM Iterations i
			WHERE i.StartDate >= @StartDate
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
	JOIN #Iterations i2 on i2.id = i.Id
	order by EmployeeAdoId

	select 
	distinct
	i2.Name as Iteration,
	t.name,

	(select sum(w.Estimate) 
	from WorkItems w 
	JOIN Iterations i on w.IterationId = i.Id 
	JOIN #Iterations it on it.Id = i.Id
	JOIN #EmployeeActivity e on e.EmployeeAdoId = w.EmployeeAdoId and e.IsDev = 1
	WHERE w.AreaAdoId = a.Id and i2.Name = it.Name) as TotalDev,

	(select sum(w.Estimate) 
	from WorkItems w 
	JOIN Iterations i on w.IterationId = i.Id 
	JOIN #Iterations it on it.Id = i.Id
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
	JOIN #Iterations i2 on i2.Id = i.Id
	WHERE dc.IsDev = 1
	group by t.name, i2.Name

	select i2.Name as Iteration, t.Name, sum(Days) as Days
	into #TeamCapacityQA
	from DevCapacity dc
	join teams t on dc.TeamAdoId = t.id
	JOIN #Teams t2 on t2.Team = t.Name
	JOIN Iterations i on dc.IterationAdoId = i.Id 
	JOIN #Iterations i2 on i2.Id = i.Id
	WHERE dc.IsDev = 0
	group by t.name, i2.Name

	SELECT
	r.Iteration,
	r.name as Team,
	FORMAT(TotalDev, 'N2') as 'Dev Total (days)',
	FORMAT(TotalDev/td.Days, 'N2') as 'Dev Efficiency',

	FORMAT(TotalQA, 'N2') as 'QA Total (days)',
	FORMAT(TotalQA/tq.Days, 'N2') as 'QA Efficiency',

	FORMAT(ISNULL(tq.Days,0)+ISNULL(td.Days,0), 'N2') as 'Total',
	FORMAT(ISNULL(TotalDev,0)+ISNULL(TotalQA,0), 'N2') as 'Velocity',	
	case when ISNULL(tq.Days,0) = 0 OR ISNULL(td.Days,0) = 0 then 0.0 else FORMAT((ISNULL(TotalDev,0)+ISNULL(TotalQA,0))/(ISNULL(tq.Days,0)+ISNULL(td.Days,0)), 'N2') end as 'Efficiency'

	INTO #FinalResults
	FROM #Results r
	LEFT JOIN #TeamCapacityQA tq
	ON tq.Name = r.name and tq.Iteration = r.Iteration
	LEFT JOIN #TeamCapacityDev td
	ON td.Name = r.name  and td.Iteration = r.Iteration
	--ORDER BY TotalDev/td.Days desc
	--ORDER BY IsCompliance / Total desc

	select * from #FinalResults

	if @Advanced = 1
		select * from #FinalResults

	SELECT Iteration, Team, Velocity, Total, Efficiency, [Dev Efficiency], [QA Efficiency]
	FROM #FinalResults
	ORDER BY iteration desc, Efficiency desc
END
