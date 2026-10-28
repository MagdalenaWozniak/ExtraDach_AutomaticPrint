create table [CDN].[One_MW_PrintedDocuments]
(
documentId int
)
go


grant select on [CDN].[One_MW_PrintedDocuments] to cdnraport
go

grant insert on [CDN].[One_MW_PrintedDocuments] to cdnraport
go


                            declare @minId int;

							select @minId=MIN(documentId) from [CDN].[One_MW_PrintedDocuments]
							
							insert into [CDN].[One_MW_PrintedDocuments] (documentId)
							select 
                            KAZ_GIDNumer
                            from
                            CDN.Zapisy
                            join CDN.Operacje on KAO_GIDTyp=KAZ_KAOTyp and KAO_GIDNumer=KAZ_KAONumer
                            where kao_typ=1 and kaz_gidnumer<@minId