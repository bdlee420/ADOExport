CREATE NONCLUSTERED INDEX WorkItemTags_Tag ON dbo.WorkItemTags ([Tag]) INCLUDE ([WorkItemId])

CREATE NONCLUSTERED INDEX WorkItemTags_WorkItemId ON dbo.WorkItemTags ([WorkItemId]) INCLUDE ([Tag])

CREATE NONCLUSTERED INDEX DevCapacity_IterationAdoId ON dbo.DevCapacity ([IterationAdoId]) INCLUDE ([EmployeeAdoId], [IsDev])

CREATE NONCLUSTERED INDEX DevCapacity_IsDev ON dbo.DevCapacity ([IsDev]) INCLUDE ([IterationAdoId], [TeamAdoId], [Days])

CREATE NONCLUSTERED INDEX DevCapacity_TeamAdoId_IsDev ON dbo.DevCapacity ([TeamAdoId], [IsDev]) INCLUDE ([Days])

CREATE NONCLUSTERED INDEX WorkItems_IterationId ON dbo.WorkItems ([IterationId]) INCLUDE ([EmployeeAdoId], [Estimate], [AreaAdoId])

CREATE NONCLUSTERED INDEX WorkItems_ParentType ON dbo.WorkItems ([ParentType]) INCLUDE ([WorkItemId])

CREATE NONCLUSTERED INDEX WorkItemsPlannedDone_IterationId ON dbo.WorkItemsPlannedDone ([IterationId]) INCLUDE ([WorkItemId], [AreaAdoId], [IsDone], [IsPlanned], [IsDeleted], [IsRemovedFromSprint])
