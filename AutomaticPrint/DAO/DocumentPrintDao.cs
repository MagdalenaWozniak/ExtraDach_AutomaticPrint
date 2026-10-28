using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomaticPrint.DTO;
using System.Data;
using System.Data.SqlClient;

namespace AutomaticPrint.DAO
{
    class DocumentPrintDao
    {
        readonly string connectionString;

        public DocumentPrintDao(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void ExecuteCommandNoTransaction(string command)
        {
            try
            {
                SqlConnection objConnection = new SqlConnection(connectionString);
                objConnection.Open();
                SqlCommand UpdateConfirm = new SqlCommand(command, objConnection);
                SqlDataAdapter AdapterUpd = new SqlDataAdapter(UpdateConfirm);
                UpdateConfirm.CommandTimeout = 0;
                UpdateConfirm.ExecuteNonQuery();
                objConnection.Close();
            }
            catch (Exception ex)
            {
                
                    Console.WriteLine("Zgłoszono wyjątek: {0}", ex);
               
            }
        }
        public DocumentPrintDto GetPrintForDocument(int mainDocumentType, int mainDocumentId, int rp, string mainDocumentShortcut, int mainDocumentIsClip, int mainDocumentCenterId, int mainDocumentWithAdvance, int mainDocumentManual)
        {
            try
            {
                //string query = @"select top 1
                //                DP_Par1,DP_Par2,DP_Par3
                //                from
                //                [CDN].[GR_MW_DocumentsPrint]
                //                where DP_Type=@documentType and DP_Shortcut=@documentShortcut and DP_IsClip=@documentIsClip and (DP_CenterId=@documentCenterId or DP_CenterId=0)
                //                and DP_WithAdvance=@documentWithAdvance and DP_Manual=@documentManual
                //                order by DP_CenterId desc
                //                ";
                string query = "";
                if (rp == 1)
                    query = @"select 
                             top 1 
                            1 as DP_Par1, RpF2_RpWId as DP_Par2,RpF2_Id as DP_Par3
                            from cdn.RptFormaty2
                            join cdn.RptMapa2 on RpM2_RpWId = RpF2_RpWId
                            where
                            (RpM2_ProcID = 8302
                            and
                            RpF2_Nazwa like '%KW##'
                            )
                            and exists 
                            (
                            select 
                            *
                            from
                            CDN.Zapisy
                            join CDN.Operacje on KAO_GIDTyp=KAZ_KAOTyp and KAO_GIDNumer=KAZ_KAONumer
                            where kao_typ=1 and kaz_gidtyp="+mainDocumentType.ToString()+@" and kaz_gidnumer="+mainDocumentId.ToString()+ @"
                            and datediff(second,convert(datetime,'01-01-1990',105),getdate())-KAZ_CzasZapisu<=300
                            )
                            and not exists(select * from [CDN].[One_MW_PrintedDocuments] where documentId=" + mainDocumentId.ToString() + @")
                            ";
                else
                    query = @"select 
                             top 1 
                            1 as DP_Par1, RpF2_RpWId as DP_Par2,RpF2_Id as DP_Par3
                            from cdn.RptFormaty2
                            join cdn.RptMapa2 on RpM2_RpWId = RpF2_RpWId
                            where
                            (RpM2_ProcID = 8302
                            and
                            RpF2_Nazwa like '%KP##'
                            )
                            and exists 
                            (
                            select 
                            *
                            from
                            CDN.Zapisy
                            join CDN.Operacje on KAO_GIDTyp=KAZ_KAOTyp and KAO_GIDNumer=KAZ_KAONumer
                            where kao_typ=1 and kaz_gidtyp=" + mainDocumentType.ToString() + @" and kaz_gidnumer=" + mainDocumentId.ToString() + @"
                            and datediff(second,convert(datetime,'01-01-1990',105),getdate())-KAZ_CzasZapisu<=300
                            )
                            and not exists(select * from [CDN].[One_MW_PrintedDocuments] where documentId=" + mainDocumentId.ToString() + @")
                            ";


                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(query, connectionString);
                //da.SelectCommand.Parameters.Add("@documentType", SqlDbType.Int).Value = mainDocumentType;
                //da.SelectCommand.Parameters.Add("@documentShortcut", SqlDbType.NVarChar).Value = mainDocumentShortcut;
                //da.SelectCommand.Parameters.Add("@documentIsClip", SqlDbType.Int).Value = mainDocumentIsClip;
                //da.SelectCommand.Parameters.Add("@documentCenterId", SqlDbType.Int).Value = mainDocumentCenterId;
                //da.SelectCommand.Parameters.Add("@documentWithAdvance", SqlDbType.Int).Value = mainDocumentWithAdvance;
                //da.SelectCommand.Parameters.Add("@documentManual", SqlDbType.Int).Value = mainDocumentManual;
                da.SelectCommand.CommandTimeout = 0;
                da.Fill(ds, "InternalPrintTest");
                DataTable dt = ds.Tables["InternalPrintTest"];

                DocumentPrintDto documentPrint = null;

                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {

                        int printPar1 = Int32.Parse(row["DP_Par1"].ToString());
                        int printPar2 = Int32.Parse(row["DP_Par2"].ToString());
                        int printPar3 = Int32.Parse(row["DP_Par3"].ToString());

                        documentPrint = new DocumentPrintDto(printPar1, printPar2, printPar3);

                    }
                }

                return documentPrint;
            }
            catch (Exception)
            {
                throw;
               
            }
        }


    }
}
