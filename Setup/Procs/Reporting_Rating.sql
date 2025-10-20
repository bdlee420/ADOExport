ALTER PROCEDURE [dbo].[Reporting_Rating]
	@Team varchar(50) = null,
	@Employee varchar(50) = null,
	@ShowLeads bit = 1,
	@ShowContractors bit = 0,
	@LimitedView bit = 0,
	@FilterTeam varchar(50) = null,
	@Sprints varchar(1000) = null,
	@ShowOutput bit = 1,
	@StartDate datetime = null,
	@EndDate datetime = null,
	@SplitBySprint bit = 0,
	@SplitByQuarter bit = 0
AS
BEGIN
	DROP TABLE IF EXISTS #Sprints
	CREATE TABLE #Sprints (Sprint varchar(200), GroupName varchar(200))

	IF @Sprints is not null
	BEGIN
		WITH SplitStrings AS (
			SELECT value
			FROM STRING_SPLIT(@Sprints, ',')
		)
		SELECT value INTO #IterationStrings FROM SplitStrings

		INSERT INTO #Sprints (Sprint, GroupName)
		SELECT value, case when @SplitBySprint = 1 then value else case when @SplitByQuarter = 1 then i.YearQuarter else 'All' end end
		FROM #IterationStrings ist
		join Iterations i on i.Name = ist.value
	END	

	IF @StartDate is not null AND @EndDate is not null
	BEGIN
		DELETE FROM #Sprints

		INSERT INTO #Sprints (Sprint, GroupName)
		SELECT	Name, case when @SplitBySprint = 1 then Name else case when @SplitByQuarter = 1 then YearQuarter else 'All' end end
		FROM	Iterations
		WHERE	EndDate >= @StartDate AND EndDate <= @EndDate
	END	

	drop table if exists #EmployeesTeam

	select distinct t.Name AS TeamName, dc.TeamAdoId, t.AreaId, dc.EmployeeAdoId, e.Name, e.IsFTE, e.IsLead, dc.IsDev
	into #EmployeesTeam
	from DevCapacity dc
	join Teams t on t.Id = dc.TeamAdoId
	join Iterations i on i.Id = dc.IterationAdoId
	join #Sprints s on s.Sprint = i.Name
	join Employees2 e on e.EmployeeAdoId = dc.EmployeeAdoId
	where dc.IsDev = 1

	---------------END INPUTS---------------------

	drop table if exists #tmp_stats_recent_velocity
	select s.GroupName, a1.EmployeeAdoId, e.TeamAdoId, sum(estimate) as RecentVelocity
	into #tmp_stats_recent_velocity
	from WorkItems a1 
	join Iterations i on i.id = a1.IterationId
	join #Sprints s on s.Sprint = i.Name 
	join #EmployeesTeam e on e.EmployeeAdoId = a1.EmployeeAdoId and e.AreaId = a1.AreaAdoId
	where (e.IsFTE = 1 OR @ShowContractors = 1)
	group by a1.EmployeeAdoId, s.GroupName, e.TeamAdoId
	order by sum(estimate) desc

	drop table if exists #tmp_stats_recent_capacity
	select s.GroupName, dc.EmployeeAdoId, e.TeamAdoId, sum(Days) as RecentCapacity
	into #tmp_stats_recent_capacity
	from DevCapacity dc
	join Iterations i on i.id = dc.IterationAdoId
	join #Sprints s on s.Sprint = i.Name 
	join #EmployeesTeam e on e.EmployeeAdoId = dc.EmployeeAdoId and e.TeamAdoId = dc.TeamAdoId
	where (e.IsFTE = 1 OR @ShowContractors = 1)
	group by dc.EmployeeAdoId, s.GroupName, e.TeamAdoId

	drop table if exists #results
	select tv.GroupName, tv.EmployeeAdoId, tv.TeamAdoId, tv.RecentVelocity, tc.RecentCapacity, (tv.RecentVelocity/tc.RecentCapacity) as Efficiency
	into #results
	from #tmp_stats_recent_velocity tv
	join #tmp_stats_recent_capacity tc
	on tv.EmployeeAdoId = tc.EmployeeAdoId and tv.TeamAdoId = tc.TeamAdoId
	where RecentCapacity > 0
	order by Efficiency desc

	DECLARE @RecentVelocityGoal decimal = 20.0
	DECLARE @RecentEfficiencyGoal decimal(28,12) = 0.40
	DECLARE @YearGoal decimal = 75
	DECLARE @ManagerRatingGoal decimal = 7
	DECLARE @BlueOptimaGoal decimal = 2

	drop table if exists #EmployeeBCE

	SELECT		TOP 1 WITH TIES e.EmployeeAdoId, (case when er.BCE is null then 1.3 else er.bce end) as BCE, er.Rating
	INTO		#EmployeeBCE	
	FROM		#EmployeesTeam e
	LEFT JOIN	EmployeeRatings er on er.Employee = e.Name
	WHERE		er.TimeStamp <= @EndDate
	ORDER BY ROW_NUMBER() OVER (PARTITION BY e.EmployeeAdoId ORDER BY TimeStamp desc)

	drop table if exists #results2

	SELECT 
	r.GroupName, 
	e.EmployeeAdoId,
	r.TeamAdoId,
	(0.30 *((cast(RecentVelocity as decimal) / @RecentVelocityGoal)  +
	(cast(Efficiency as decimal) / @RecentEfficiencyGoal))) as ADORating,
	((ISNULL(e.bce,0)/@BlueOptimaGoal) * 0.50) as BlueOptimaRating,
	((ISNULL(e.rating,0)/@ManagerRatingGoal) * 2) as ManagerRating
	INTO #results2
	FROM #results r
	LEFT JOIN #EmployeeBCE e on e.EmployeeAdoId = r.EmployeeAdoId

	drop table if exists #visibleEmployees

	select 
	e.EmployeeAdoId, 
	e.TeamName, 
	e.Name as Employee
	into #visibleEmployees
	from #EmployeesTeam e 
	where 
	(@Team is null OR e.TeamName = @Team) 
	and (@Employee is null OR e.Name = @Employee) 
	and (e.IsFTE = 1 OR @ShowContractors = 1) 
	and (e.IsLead = 0 OR @ShowLeads = 1) 
	and TeamName is not null and TeamName <> ''

	drop table if exists #finalresults

	select 
	r.GroupName,
	case when ve.EmployeeAdoId is null then '' else e.TeamName end as Team, 
	case when ve.EmployeeAdoId is null then '' else e.Name end as Name, 
	e.IsDev,
	ADORating, 
	BlueOptimaRating, 
	ManagerRating, 
	case when e.IsLead = 0 then 0 else 1.5 end as TeamLeadPoints,
	case when e.IsDev = 1 then 0.3 else 0 end as DevPoints
	into #finalresults
	from #results2 r
	join #EmployeesTeam e on e.EmployeeAdoId = r.EmployeeAdoId
	left join #visibleEmployees ve on ve.EmployeeAdoId = r.EmployeeAdoId


	--------------------------RESULTS--------------------------
	IF @ShowOutput = 1
		BEGIN
			IF @LimitedView = 1
				BEGIN
					INSERT INTO #RatingsResults1
					select 
					r.GroupName,
					ve.EmployeeAdoId,
					case when ve.EmployeeAdoId is null then '' else e.TeamName end as Team, 
					case when ve.EmployeeAdoId is null then '' else e.Name end as Name, 
					FORMAT(r.RecentVelocity, 'N2') as RecentVelocity, 
					FORMAT(r.Efficiency, 'N2') as Efficiency
					from #results r
					join #EmployeesTeam e on e.EmployeeAdoId = r.EmployeeAdoId
					left join #visibleEmployees ve on ve.EmployeeAdoId = e.EmployeeAdoId
					where @FilterTeam is null OR e.TeamName = @FilterTeam
					order by Efficiency desc

					INSERT INTO #RatingsResults2
					select 
					Team,
					Name, 
					IsDev,
					FORMAT(ADORating, 'N2') as ADORating, 
					FORMAT(BlueOptimaRating, 'N2') as BlueOptimaRating, 
					FORMAT((ADORating+BlueOptimaRating+ManagerRating+TeamLeadPoints+DevPoints), 'N2') as TotalRating
					from #finalresults r
					where @FilterTeam is null OR Team = @FilterTeam
					order by TotalRating desc
				END
			ELSE
				BEGIN 

					INSERT INTO #RatingsResults1
					select 					
					r.GroupName,
					ve.EmployeeAdoId,
					case when ve.EmployeeAdoId is null then '' else e.TeamName end as Team, 
					case when ve.EmployeeAdoId is null then '' else e.Name end as Name, 
					FORMAT(r.RecentVelocity, 'N2') as RecentVelocity, 
					FORMAT(r.RecentCapacity, 'N2') as RecentCapacity, 
					FORMAT(r.Efficiency, 'N2') as Efficiency
					from #results r
					join #EmployeesTeam e on e.EmployeeAdoId = r.EmployeeAdoId and e.TeamAdoId = r.TeamAdoId
					left join #visibleEmployees ve on ve.EmployeeAdoId = e.EmployeeAdoId and ve.TeamName = e.TeamName
					where @FilterTeam is null OR e.TeamName = @FilterTeam
					order by Efficiency desc

					INSERT INTO #RatingsResults2
					select 
					r.GroupName,
					Team,
					Name, 
					IsDev,
					FORMAT(ADORating, 'N2') as ADORating, 
					FORMAT(BlueOptimaRating, 'N2') as BlueOptimaRating, 
					FORMAT(ADORating + BlueOptimaRating, 'N2') as MetricRating, 
					FORMAT(ManagerRating, 'N2') as ManagerRating, 		
					TeamLeadPoints,
					DevPoints,
					(ADORating+BlueOptimaRating+ManagerRating+TeamLeadPoints+DevPoints) as TotalRating
					from #finalresults r
					where @FilterTeam is null OR Team = @FilterTeam
					order by TotalRating desc
				END
		END
END