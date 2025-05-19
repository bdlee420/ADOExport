CREATE PROCEDURE [dbo].[Reporting_Tracked_By_Iteration]
	@Tags varchar(1000) = null,
	@IterationNames varchar(1000) = null,
	@IterationQuarters varchar(1000) = null,
	@StartDate datetime = null,
	@Teams varchar(1000) = null,
	@ParentTypes varchar(1000) = null,
	@WorkItemTypes varchar(1000) = null,
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
	CREATE TABLE #WorkItemTagIds (WorkItemId int)
	CREATE TABLE #Teams (Team varchar(200))
	
	IF @Tags is not null
		BEGIN
			WITH SplitStrings AS (
				SELECT value as TagName
				FROM STRING_SPLIT(@Tags, ',')
			)
			SELECT TagName INTO #TagStrings FROM SplitStrings

			INSERT INTO #WorkItemTagIds (WorkItemId)
			SELECT WorkItemId
			FROM WorkItemTags w
			JOIN #TagStrings ts on w.Tag = ts.TagName
		END	
	
	
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


	IF @ParentTypes is not null
		BEGIN
			WITH SplitStrings AS (
				SELECT value
				FROM STRING_SPLIT(@ParentTypes, ',')
			)
			SELECT value INTO #ParentTypesStrings FROM SplitStrings

			INSERT INTO #WorkItemTagIds (WorkItemId)
			SELECT WorkItemId
			FROM WorkItems w
			JOIN #ParentTypesStrings pt on pt.value = w.ParentType
		END	

	IF @WorkItemTypes is not null
		BEGIN
			WITH SplitStrings AS (
				SELECT value
				FROM STRING_SPLIT(@WorkItemTypes, ',')
			)
			SELECT value INTO #WorkItemTypeStrings FROM SplitStrings

			INSERT INTO #WorkItemTagIds (WorkItemId)
			SELECT WorkItemId
			FROM WorkItems w
			JOIN #WorkItemTypeStrings pt on pt.value = w.WorkItemType
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
	JOIN #Iterations i2 on i2.Id = i.Id
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
	JOIN #WorkItemTagIds wt on wt.WorkItemId = w.WorkItemId
	WHERE w.AreaAdoId = a.Id and i2.Name = it.Name) as IsComplianceDev,

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
	JOIN #WorkItemTagIds wt on wt.WorkItemId = w.WorkItemId
	WHERE w.AreaAdoId = a.Id and i2.Name = it.Name) as IsComplianceQA,

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
	FORMAT(ISNULL(IsComplianceDev,0), 'N2') as 'Dev Tracked (days)',
	FORMAT(TotalDev, 'N2') as 'Dev Total (days)',
	FORMAT((ISNULL(IsComplianceDev,0) / (case when TotalDev = 0 then 1 else TotalDev end)) * 100, 'N2') + '%' as 'Dev Tracked %',

	FORMAT(ISNULL(IsComplianceQA,0), 'N2') as 'QA Tracked (days)',
	FORMAT(TotalQA, 'N2') as 'QA Total (days)',
	FORMAT((ISNULL(IsComplianceQA,0) / (case when TotalQA = 0 then 1 else TotalQA end)) * 100, 'N2') + '%' as 'QA Tracked %',

	FORMAT(ISNULL(IsComplianceDev,0)+ISNULL(IsComplianceQA,0), 'N2') as 'Tracked (days)',
	FORMAT(TotalDev+TotalQA, 'N2') as 'Total (days)',
	FORMAT(((ISNULL(IsComplianceDev,0)+ISNULL(IsComplianceQA,0)) / (case when TotalDev + TotalQA = 0 then 1 else TotalDev + TotalQA end)) * 100, 'N2') + '%' as 'Tracked %',
	((ISNULL(IsComplianceDev,0)+ISNULL(IsComplianceQA,0)) / (case when TotalDev + TotalQA = 0 then 1 else TotalDev + TotalQA end)) * 100 as 'Tracked'

	INTO #FinalResults
	FROM #Results r
	JOIN #TeamCapacityQA tq
	ON tq.Name = r.name and tq.Iteration = r.Iteration
	JOIN #TeamCapacityDev td
	ON td.Name = r.name  and td.Iteration = r.Iteration
	--ORDER BY TotalDev/td.Days desc
	--ORDER BY IsCompliance / Total desc

	if @Advanced = 1
		select * from #FinalResults

	DECLARE @Type varchar(200)
	IF @ParentTypes is not null
		SET @Type = @ParentTypes
	IF @WorkItemTypes is not null
		SET @Type = @WorkItemTypes
	IF @Tags is not null
		SET @Type = @Tags

	SELECT @Type as Type, Iteration, Team, [Tracked (days)], [Total (days)], [Tracked %]
	FROM #FinalResults
	ORDER BY iteration desc, Tracked desc
END
GO