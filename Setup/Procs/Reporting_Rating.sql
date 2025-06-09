ALTER PROCEDURE [dbo].[Reporting_Rating]
	@Team varchar(50) = null,
	@Employee varchar(50) = null,
	@ShowLeads bit = 1,
	@ShowContractors bit = 0,
	@LimitedView bit = 0,
	@FilterTeam varchar(50) = null,
	@Sprints varchar(1000) = null,
	@ShowOutput bit = 1,
	@BCETimeStamp datetime = null
AS
BEGIN
	IF @BCETimeStamp is null
		BEGIN
			SET @BCETimeStamp = GetDate()
		END

	DROP TABLE IF EXISTS #Sprints
	CREATE TABLE #Sprints (Sprint varchar(200))

	IF @Sprints is not null
	BEGIN
		WITH SplitStrings AS (
			SELECT value
			FROM STRING_SPLIT(@Sprints, ',')
		)
		SELECT value INTO #IterationStrings FROM SplitStrings

		INSERT INTO #Sprints (Sprint)
		SELECT value
		FROM #IterationStrings
	END	

	drop table if exists #tmp_stats_year
	select e.EmployeeAdoId,
	(select sum(estimate) from WorkItems a1 join iterations i on i.id=a1.IterationId where a1.EmployeeAdoId = e.EmployeeAdoId and i.YearQuarter = '2024-Q2') as '2024-Q2',
	(select sum(estimate) from WorkItems a1 join iterations i on i.id=a1.IterationId where a1.EmployeeAdoId = e.EmployeeAdoId and i.YearQuarter = '2024-Q3') as '2024-Q3',
	(select sum(estimate) from WorkItems a1 join iterations i on i.id=a1.IterationId where a1.EmployeeAdoId = e.EmployeeAdoId and i.YearQuarter = '2024-Q4') as '2024-Q4',
	(select sum(estimate) from WorkItems a1 join iterations i on i.id=a1.IterationId where a1.EmployeeAdoId = e.EmployeeAdoId and i.YearQuarter = '2025-Q1') as '2025-Q1',
	(select sum(estimate) from WorkItems a1 join iterations i on i.id=a1.IterationId where a1.EmployeeAdoId = e.EmployeeAdoId and i.YearQuarter in ('2024-Q2','2024-Q3','2024-Q4','2025-Q1')) as 'Total'
	into #tmp_stats_year
	FROM Employees e
	where (e.IsFTE = 1 OR @ShowContractors = 1)

	---------------END INPUTS---------------------

	drop table if exists #tmp_stats_recent_velocity
	select a1.EmployeeAdoId, sum(estimate) as RecentVelocity
	into #tmp_stats_recent_velocity
	from WorkItems a1 
	join Iterations i on i.id = a1.IterationId
	join #Sprints s on s.Sprint = i.Name 
	join Employees e on e.EmployeeAdoId = a1.EmployeeAdoId
	where (e.IsFTE = 1 OR @ShowContractors = 1)
	group by a1.EmployeeAdoId
	order by sum(estimate) desc

	drop table if exists #tmp_stats_recent_capacity
	select dc.EmployeeAdoId, sum(Days) as RecentCapacity
	into #tmp_stats_recent_capacity
	from DevCapacity dc
	join Iterations i on i.id = dc.IterationAdoId
	join #Sprints s on s.Sprint = i.Name 
	join Employees e on e.EmployeeAdoId = dc.EmployeeAdoId
	where (e.IsFTE = 1 OR @ShowContractors = 1)
	group by dc.EmployeeAdoId

	drop table if exists #results
	select tv.EmployeeAdoId, tv.RecentVelocity, tc.RecentCapacity, (tv.RecentVelocity/tc.RecentCapacity) as Efficiency, ty.Total
	into #results
	from #tmp_stats_recent_velocity tv
	join #tmp_stats_recent_capacity tc
	on tv.EmployeeAdoId = tc.EmployeeAdoId 
	join #tmp_stats_year ty
	on ty.EmployeeAdoId = tv.EmployeeAdoId
	where RecentCapacity > 0
	order by Efficiency desc

	DECLARE @RecentVelocityGoal decimal = 20.0
	DECLARE @RecentEfficiencyGoal decimal(28,12) = 0.40
	DECLARE @YearGoal decimal = 75
	DECLARE @ManagerRatingGoal decimal = 7
	DECLARE @BlueOptimaGoal decimal = 2

	drop table if exists #EmployeeBCE

	SELECT		TOP 1 WITH TIES EmployeeAdoId, (case when b.BCE is null then 1.3 else b.bce end) as BCE, Rating
	INTO		#EmployeeBCE	
	FROM		Employees e
	LEFT JOIN	BlueOptima b on b.Employee = e.Name
	WHERE		b.TimeStamp <= @BCETimeStamp
	ORDER BY ROW_NUMBER() OVER (PARTITION BY EmployeeAdoId ORDER BY TimeStamp desc)

	drop table if exists #results2
	select e.EmployeeAdoId,
	(0.30 *((cast(RecentVelocity as decimal) / @RecentVelocityGoal)  +
	(cast(Efficiency as decimal) / @RecentEfficiencyGoal) + 
	(cast(Total as decimal) / @YearGoal))) as ADORating,
	((e.bce/@BlueOptimaGoal) * 0.50) as BlueOptimaRating,
	((e.rating/@ManagerRatingGoal) * 2) as ManagerRating
	into #results2
	from #results r
	join #EmployeeBCE e on e.EmployeeAdoId = r.EmployeeAdoId

	drop table if exists #visibleEmployees
	select e.EmployeeAdoId, e.TeamName, e.Name as Employee
	into #visibleEmployees
	from Employees e 
	where 
	(@Team is null OR e.TeamName = @Team) 
	and (@Employee is null OR e.Name = @Employee) 
	and (e.IsFTE = 1 OR @ShowContractors = 1) 
	and (e.IsLead = 0 OR @ShowLeads = 1) 
	and TeamName is not null and TeamName <> ''

	drop table if exists #finalresults

	select 
	case when ve.EmployeeAdoId is null then '' else e.TeamName end as Team, 
	case when ve.EmployeeAdoId is null then '' else e.Name end as Name, 
	e.activity,
	ADORating, 
	BlueOptimaRating, 
	ManagerRating, 
	case when e.IsLead = 0 then 0 else 1.5 end as TeamLeadPoints,
	case when e.Activity = 'dev' then 0.3 else 0 end as DevPoints
	into #finalresults
	from #results2 r
	join Employees e on e.EmployeeAdoId = r.EmployeeAdoId
	left join #visibleEmployees ve on ve.EmployeeAdoId = r.EmployeeAdoId


	--------------------------RESULTS--------------------------
	IF @ShowOutput = 1
		BEGIN
			IF @LimitedView = 1
				BEGIN
					select 
					case when ve.EmployeeAdoId is null then '' else e.TeamName end as Team, 
					case when ve.EmployeeAdoId is null then '' else e.Name end as Name, 
					FORMAT(r.RecentVelocity, 'N2') as RecentVelocity, 
					FORMAT(r.Efficiency, 'N2') as Efficiency, 
					FORMAT(r.Total, 'N2') as Total
					from #results r
					join Employees e on e.EmployeeAdoId = r.EmployeeAdoId
					left join #visibleEmployees ve on ve.EmployeeAdoId = e.EmployeeAdoId
					where @FilterTeam is null OR e.TeamName = @FilterTeam
					order by Efficiency desc

					select 
					Team,
					Name, 
					Activity,
					FORMAT(ADORating, 'N2') as ADORating, 
					FORMAT(BlueOptimaRating, 'N2') as BlueOptimaRating, 
					FORMAT((ADORating+BlueOptimaRating+ManagerRating+TeamLeadPoints+DevPoints), 'N2') as TotalRating
					from #finalresults r
					where @FilterTeam is null OR Team = @FilterTeam
					order by TotalRating desc
				END
			ELSE
				BEGIN 
					select 
					case when ve.EmployeeAdoId is null then '' else e.TeamName end as Team, 
					case when ve.EmployeeAdoId is null then '' else e.Name end as Name, 
					FORMAT(r.RecentVelocity, 'N2') as RecentVelocity, 
					FORMAT(r.RecentCapacity, 'N2') as RecentCapacity, 
					FORMAT(r.Efficiency, 'N2') as Efficiency, 
					FORMAT(r.Total, 'N2') as Total		
					from #results r
					join Employees e on e.EmployeeAdoId = r.EmployeeAdoId
					left join #visibleEmployees ve on ve.EmployeeAdoId = e.EmployeeAdoId
					where @FilterTeam is null OR e.TeamName = @FilterTeam
					order by Efficiency desc

					select 
					Team,
					Name, 
					Activity,
					FORMAT(ADORating, 'N2') as ADORating, 
					FORMAT(BlueOptimaRating, 'N2') as BlueOptimaRating, 
					FORMAT(ADORating + BlueOptimaRating, 'N2') as MetricRating, 
					FORMAT(ManagerRating, 'N2') as ManagerRating, 		
					TeamLeadPoints,
					DevPoints,
					(ADORating+BlueOptimaRating+ManagerRating+TeamLeadPoints+DevPoints) as TotalRating
					from #finalresults r
					where @FilterTeam is null OR Team = @FilterTeam
					order by MetricRating desc
				END
		END
END
