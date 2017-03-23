DROP TABLE #bowlersAffected
DROP TABLE #normalizedResults
DROP TABLE #resultsWithTotals

--affected bowlers
select SeasonYear, BowlerId, Name, count(Division) as DivisionCount
into #bowlersAffected
from (select SeasonYear, BowlerId, Name, Division, count(1) as z
	from (select SeasonYear, BowlerId, Name, Division from TournamentResultsScratch) x
		group by SeasonYear, BowlerId, Name, Division
		having count(1) > 2) as x
group by SeasonYear, BowlerId, Name
having count(Division) > 1
order by SeasonYear desc

select TournamentLocation, TournamentDate, Name, BowlerId, TotalScratch, SUM(DivisionCode) as DivisionCode
INTO #normalizedResults
from (
select t.TournamentLocation, cast(t.TournamentDetails as datetime2) as TournamentDate, r.Name, r.BowlerId, 
	case r.Division
		WHEN 'Tournament' THEN 1
		WHEN 'Teaching' THEN 2
		WHEN 'Senior' THEN 4
	end	as DivisionCode, 
	r.TotalScratch
from TournamentResultsScratch r
join TournamentTable t	
	on r.TournamentId = t.Id
	and t.SeasonCode > 2008
) as br
group by TournamentLocation, TournamentDate, Name, BowlerId, TotalScratch

select *, case DivisionCode WHEN 1 THEN 'Tournament' WHEN 2 THEN 'Teaching' WHEN 3 THEN 'Teaching/Tournament' WHEN 4 THEN 'Senior' end as Division,
	(select SUM(TotalScratch) FROM (
		select top 12 TotalScratch
		from #normalizedResults ir
		where ir.BowlerId = r.BowlerId
			and ir.TournamentDate <= r.TournamentDate
		order by ir.TournamentDate desc
		) as x) as Last12,
	(select COUNT(TotalScratch) FROM (
		select top 12 TotalScratch
		from #normalizedResults ir
		where ir.BowlerId = r.BowlerId
			and ir.TournamentDate <= r.TournamentDate
		order by ir.TournamentDate desc
		) as x) as CountLast12,
	(select SUM(TotalScratch) FROM (
		select top 12 TotalScratch
		from #normalizedResults ir
		where ir.BowlerId = r.BowlerId
			and ir.TournamentDate <= r.TournamentDate
			and DivisionCode & 2 = 2
		order by ir.TournamentDate desc
		) as x) as Last12Teaching,
	(select Count(TotalScratch) FROM (
		select top 12 TotalScratch
		from #normalizedResults ir
		where ir.BowlerId = r.BowlerId
			and ir.TournamentDate <= r.TournamentDate
			and DivisionCode & 2 = 2
		order by ir.TournamentDate desc
		) as x) as Count12Teaching,
	(select SUM(TotalScratch) FROM (
		select top 12 TotalScratch
		from #normalizedResults ir
		where ir.BowlerId = r.BowlerId
			and ir.TournamentDate <= r.TournamentDate
			and DivisionCode & 4 = 4
		order by ir.TournamentDate desc
		) as x) as Last12Senior,
	(select Count(TotalScratch) FROM (
		select top 12 TotalScratch
		from #normalizedResults ir
		where ir.BowlerId = r.BowlerId
			and ir.TournamentDate <= r.TournamentDate
			and DivisionCode & 4 = 4
		order by ir.TournamentDate desc
		) as x) as Count12Senior,
	(select SUM(TotalScratch) FROM (
		select top 12 TotalScratch
		from #normalizedResults ir
		where ir.BowlerId = r.BowlerId
			and ir.TournamentDate <= r.TournamentDate
			and (DivisionCode & 4 = 4 or DivisionCode & 2 = 2)
		order by ir.TournamentDate desc
		) as x) as Last12POA,
	(select Count(TotalScratch) FROM (
		select top 12 TotalScratch
		from #normalizedResults ir
		where ir.BowlerId = r.BowlerId
			and ir.TournamentDate <= r.TournamentDate
			and (DivisionCode & 4 = 4 or DivisionCode & 2 = 2)
		order by ir.TournamentDate desc
		) as x) as Count12POA
into #resultsWithTotals
from #normalizedResults r
where exists (select 1 from #bowlersAffected b where r.BowlerId = b.BowlerId)
order by Name, TournamentDate desc

--select * from #bowlersAffected 
--	order by SeasonYear desc, Name

----percentage
--select x.SeasonYear, Affected, TotalBowlers, 100 * (1.0 * Affected / TotalBowlers) as '%'
--from (select SeasonYear, count(Name) as Affected from #bowlersAffected group by SeasonYear) as x
--join (select SeasonYear, count(Name) as TotalBowlers from (select distinct SeasonYear, Name, Division from TournamentResultsScratch) as x group by SeasonYear) as y
--	on x.SeasonYear = y.SeasonYear
--order by SeasonYear desc

select TournamentLocation, TournamentDate, Name, TotalScratch, Division, Last12, Last12Teaching, Last12Senior, Last12POA, 
	case countLast12 when 12 then Last12/96 else null end as Last12Avg, 
	case count12Teaching when 12 then Last12Teaching/96 else null end as Last12TeachingAvg, 
	case count12Senior when 12 then Last12Senior/96 else null end as Last12SeniorAvg,
	case count12Poa when 12 then Last12POA/96 else null end as Last12PoaAvg
	from #resultsWithTotals
	where count12senior = 12 or count12poa = 12
	order by Name, TournamentDate desc