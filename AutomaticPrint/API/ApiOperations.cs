using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using cdn_api;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using AutomaticPrint.DTO;
using AutomaticPrint.DAO;


namespace AutomaticPrint.API
{
    class ApiOperations
    {
        int session = -1, documentApiId = -1;

        [DllImport("ClaRUN.dll")]
        public static extern void AttachThreadToClarion(bool x);

        public bool LoginToERPXL()
        {
            AttachThreadToClarion(true);
            bool result = true;
            int login_result = -1;
            cdn_api.XLLoginInfo_20202 LoginInfo = new cdn_api.XLLoginInfo_20202();
            LoginInfo.Wersja = 20202;
            LoginInfo.ProgramID = "AutoPrint";
            //LoginInfo.PlikLog = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\api.log";
            LoginInfo.TrybNaprawy = 0;
            LoginInfo.Winieta = -1;
            login_result = cdn_api.cdn_api.XLLogin(LoginInfo, ref session);
            AttachThreadToClarion(true);
            if (login_result != 0)
            {
                result = false;
                MessageBox.Show("Wystąpiły błędy logowania do API. Numer błędu: " + login_result.ToString());
            }
            return result;
        }

        public void LogoutFromERPXL()
        {
            try
            {
                AttachThreadToClarion(true);
                cdn_api.cdn_api.XLLogout(session);
                AttachThreadToClarion(true);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Zgłoszono wyjątek: {0}", ex);
            }
        }

        public void PrintDocument(int documentType, int documentCompany, int documentId, int rp, bool documentPrint, bool documentWarehousePrint, string documentShortcut, int documentIsClip, int documentCenterId, int documentWithAdvance, int documentManual)
        {
            try
            {

                if (LoginToERPXL())
                {
                    string connectionString = "";
                    AttachThreadToClarion(true);
                    cdn_api.XLPolaczenieInfo_20202 connectionInfo = new cdn_api.XLPolaczenieInfo_20202();
                    connectionInfo.Wersja = 20202;
                    cdn_api.cdn_api.XLPolaczenie(connectionInfo);
                    AttachThreadToClarion(true);
                    string connection = connectionInfo.ConnectString;
                    connectionString = connection.Substring(connection.IndexOf("SERVER"));
                    DocumentPrintDao documentsDao = new DocumentPrintDao(connectionString);


                    if (documentPrint)
                    {
                        DocumentPrintDto print = documentsDao.GetPrintForDocument(documentType, documentId, rp, documentShortcut, documentIsClip, documentCenterId, documentWithAdvance, documentManual);
                        if (print != null)
                        {
                            AttachThreadToClarion(true);
                            cdn_api.XLWydrukInfo_20202 printInfo = new XLWydrukInfo_20202();
                            printInfo.Wersja = 20202;
                            printInfo.Zrodlo = print.printPar1;
                            printInfo.Wydruk = print.printPar2;
                            printInfo.Format = print.printPar3;
                            printInfo.Urzadzenie = 2;
                            printInfo.FiltrSQL = "(zapisy.KAZ_GIDTyp=" + documentType + " AND zapisy.KAZ_GIDNumer=" + documentId + ")";
                            int printResult = cdn_api.cdn_api.XLWykonajPodanyWydruk(printInfo);
                            if (printResult == 0)
                                documentsDao.ExecuteCommandNoTransaction("insert into [CDN].[One_MW_PrintedDocuments] (documentId) values (" + documentId.ToString() + ");");
                            AttachThreadToClarion(true);
                        }
                    }
                    LogoutFromERPXL();
                }

            }
            catch( Exception ex)
            {
                Console.WriteLine("Zgłoszono wyjątek: {0}", ex);
            }
        }
    }
}
