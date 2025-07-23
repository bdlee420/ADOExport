ALTER PROCEDURE [dbo].[Reporting_Projects]
AS
BEGIN
	
	DROP TABLE IF EXISTS #Remaining
	DROP TABLE IF EXISTS #FinalResults		
	DROP TABLE IF EXISTS #Projects2

	DECLARE @DaysPerWeek decimal(4,2) = 4
	DECLARE @Buffer decimal(4,2) = 1.15

	select p.Team, p.Tag, SUM(w.Remaining) as Remaining
	INTO #Remaining
	FROM #Projects p
	JOIN WorkItemTags wt on wt.Tag = p.Tag
	JOIN WorkItems w on w.WorkItemId = wt.WorkItemId
	WHERE EXISTS (SELECT value FROM STRING_SPLIT(p.Activity, '|') WHERE value = w.Activity)
	GROUP BY p.Team, p.Tag

	SELECT *, CASE 
	WHEN StartDate > GetDate() THEN StartDate ELSE GetDate() end as ActualStart
	INTO #projects2
	FROM #projects

	select 
	p.Team,
	p.Tag, 
	p.TotalCapacity,
	p.DevDedication,
	FORMAT(r.Remaining, 'N1') as Remaining, 
	FORMAT(p.StartDate, 'MMMM dd yyyy') as StartDate, 
	FORMAT(DATEADD(d, @Buffer*((r.Remaining/(p.TotalCapacity*p.DevDedication))/@DaysPerWeek*7) + p.QADays, p.ActualStart), 'MMMM dd yyyy') as TargetDate,
	FORMAT(p.DueDate, 'MMMM dd yyyy') as DueDate
	INTO #FinalResults
	FROM #Projects2 p
	JOIN #Remaining r on r.Tag = p.Tag

	INSERT INTO Projects (Team, TimeStamp, Tag, TotalCapacity, DevDedication, Remaining, StartDate, TargetDate, DueDate)
	SELECT Team, GetDate(), Tag, TotalCapacity, DevDedication, Remaining, StartDate, TargetDate, DueDate
	FROM #FinalResults

	select * from #FinalResults order by Team, Tag, DueDate
END
