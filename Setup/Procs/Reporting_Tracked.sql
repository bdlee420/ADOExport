CREATE PROCEDURE [dbo].[Reporting_Tracked]
	@Tags varchar(1000) = null,
	@IterationNames varchar(1000) = null,
	@IterationQuarters varchar(1000) = null,
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
	DROP TABLE IF EXISTS #WorkItemTagIds2

	CREATE TABLE #Iterations (Name varchar(200))
	CREATE TABLE #WorkItemTagIds (WorkItemId int)	
	
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
	
	select distinct EmployeeAdoId, IsDev
	into #EmployeeActivity
	from DevCapacity dc
	JOIN Iterations i on i.Id = dc.IterationAdoId
	JOIN #Iterations i2 on i2.Name = i.Name
	order by EmployeeAdoId

	select 
	t.name,

	(select sum(w.Estimate) 
	from WorkItems w 
	JOIN Iterations i on w.IterationId = i.Id 
	JOIN #Iterations TI on TI.Name = i.Name 
	JOIN #EmployeeActivity e on e.EmployeeAdoId = w.EmployeeAdoId and e.IsDev = 1
	JOIN #WorkItemTagIds wt on wt.WorkItemId = w.WorkItemId
	WHERE w.AreaAdoId = a.Id) as IsComplianceDev,

	(select sum(w.Estimate) 
	from WorkItems w 
	JOIN Iterations i on w.IterationId = i.Id 
	JOIN #Iterations TI on TI.Name = i.Name 
	JOIN #EmployeeActivity e on e.EmployeeAdoId = w.EmployeeAdoId and e.IsDev = 1
	WHERE w.AreaAdoId = a.Id) as TotalDev,

	(select sum(w.Estimate) 
	from WorkItems w 
	JOIN Iterations i on w.IterationId = i.Id 
	JOIN #Iterations TI on TI.Name = i.Name 
	JOIN #EmployeeActivity e on e.EmployeeAdoId = w.EmployeeAdoId and e.IsDev = 0
	JOIN #WorkItemTagIds wt on wt.WorkItemId = w.WorkItemId
	WHERE w.AreaAdoId = a.Id) as IsComplianceQA,

	(select sum(w.Estimate) 
	from WorkItems w 
	JOIN Iterations i on w.IterationId = i.Id 
	JOIN #Iterations TI on TI.Name = i.Name 
	JOIN #EmployeeActivity e on e.EmployeeAdoId = w.EmployeeAdoId and e.IsDev = 0
	WHERE w.AreaAdoId = a.Id) as TotalQA

	INTO #Results
	FROM Teams t
	join Areas a on t.areaId = a.id

	select t.Name, sum(Days) as Days
	into #TeamCapacityDev
	from DevCapacity dc
	join teams t on dc.TeamAdoId = t.id
	JOIN Iterations i on dc.IterationAdoId = i.Id 
	JOIN #Iterations TI on TI.Name = i.Name 
	WHERE dc.IsDev = 1
	group by t.name

	select t.Name, sum(Days) as Days
	into #TeamCapacityQA
	from DevCapacity dc
	join teams t on dc.TeamAdoId = t.id
	JOIN Iterations i on dc.IterationAdoId = i.Id 
	JOIN #Iterations TI on TI.Name = i.Name 
	WHERE dc.IsDev = 0
	group by t.name

	SELECT
	r.name as Team,
	FORMAT(ISNULL(IsComplianceDev,0), 'N2') as 'Dev Tracked (days)',
	FORMAT(TotalDev, 'N2') as 'Dev Total (days)',
	FORMAT((ISNULL(IsComplianceDev,0) / TotalDev) * 100, 'N2') + '%' as 'Dev Tracked %',

	FORMAT(ISNULL(IsComplianceQA,0), 'N2') as 'QA Tracked (days)',
	FORMAT(TotalQA, 'N2') as 'QA Total (days)',
	FORMAT((ISNULL(IsComplianceQA,0) / TotalQA) * 100, 'N2') + '%' as 'QA Tracked %',

	FORMAT(ISNULL(IsComplianceDev,0)+ISNULL(IsComplianceQA,0), 'N2') as 'Tracked (days)',
	FORMAT(TotalDev+TotalQA, 'N2') as 'Total (days)',
	FORMAT(((ISNULL(IsComplianceDev,0)+ISNULL(IsComplianceQA,0)) / (TotalDev+TotalQA)) * 100, 'N2') + '%' as 'Tracked %',
	((ISNULL(IsComplianceDev,0)+ISNULL(IsComplianceQA,0)) / (TotalDev+TotalQA)) * 100 as 'Tracked'

	INTO #FinalResults
	FROM #Results r
	JOIN #TeamCapacityQA tq
	ON tq.Name = r.name 
	JOIN #TeamCapacityDev td
	ON td.Name = r.name 

	IF @Advanced = 1 
		select * from #FinalResults

	DECLARE @Type varchar(200)
	IF @ParentTypes is not null
		SET @Type = @ParentTypes
	IF @WorkItemTypes is not null
		SET @Type = @WorkItemTypes
	IF @Tags is not null
		SET @Type = @Tags

	SELECT @Type as Type, Team, [Tracked (days)], [Total (days)], [Tracked %]
	FROM #FinalResults
	ORDER BY Tracked desc
END
GO

