CREATE PROCEDURE [dbo].[Reporting_Projects]
AS
BEGIN
	
	DROP TABLE IF EXISTS #Remaining
	DROP TABLE IF EXISTS #FinalResults		

	DECLARE @DaysPerWeek decimal(4,2) = 4
	DECLARE @Buffer decimal(4,2) = 1.15

	select p.Tag, SUM(w.Remaining) as Remaining
	INTO #Remaining
	FROM #Projects p
	JOIN WorkItemTags wt on wt.Tag = p.Tag
	JOIN WorkItems w on w.WorkItemId = wt.WorkItemId
	WHERE w.Activity = p.Activity
	GROUP BY p.Tag


	select p.Tag, 
	p.Capacity,
	FORMAT(r.Remaining, 'N1') as Remaining, 
	FORMAT(p.StartDate, 'MMMM dd yyyy') as StartDate, 
	FORMAT(DATEADD(d, @Buffer*((r.Remaining/p.Capacity)/@DaysPerWeek*7) + p.QADays, p.StartDate), 'MMMM dd yyyy') as TargetDate
	INTO #FinalResults
	FROM #Projects p
	JOIN #Remaining r on r.Tag = p.Tag

	INSERT INTO Projects (TimeStamp, Tag, Capacity, Remaining, StartDate, TargetDate)
	SELECT GetDate(), Tag, Capacity, Remaining, StartDate, TargetDate
	FROM #FinalResults

	select * from #FinalResults
END
GO

