using AdminConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminConsole
{
    using Excel = Microsoft.Office.Interop.Excel;
    class ExcellHandler
    {
        public static void CreateNewExcelFile(string filename)
        {
            var excelAplication = new Excel.Application();
            excelAplication.Visible = true;

            var excelWorkbook = excelAplication.Workbooks.Add();
            excelWorkbook.SaveAs(filename, AccessMode: Excel.XlSaveAsAccessMode.xlNoChange);

            excelWorkbook.Close();
            excelAplication.Quit();
        }

        public static void WriteToExcelFile(List<nameTransacoes> transacoesTodas, string filename)
        {
            Excel.Application excelApplication = new Excel.Application();

            Excel.Workbook excelWorkbook = excelApplication.Workbooks.Add();

            Excel.Worksheet excelWorksheet = excelWorkbook.ActiveSheet;
            for (int j = 0; j< transacoesTodas.Count(); j++) {
                excelWorksheet.Name = transacoesTodas[j].name;
                excelWorksheet.Cells[1, 1].Value = "Id Vcard";
                excelWorksheet.Cells[1, 2].Value = "type";
                excelWorksheet.Cells[1, 3].Value = "Phone to";
                excelWorksheet.Cells[1, 4].Value = "Amount";
                excelWorksheet.Cells[1, 5].Value = "Data";
                excelWorksheet.Cells[1, 6].Value = "Category";
                excelWorksheet.Cells[1, 7].Value = "type of payment";
                excelWorksheet.Cells[1, 8].Value = "Payment reference";
                
                List<Transacao> transacoes = transacoesTodas[j].transacoes;
                for (int i = 2; i < transacoes.Count()+2; i++)
                {
                    int k = i - 2; 
                    excelWorksheet.Cells[i, 1].Value = transacoes[k].id_vcard;
                    excelWorksheet.Cells[i, 2].Value = (transacoes[k].tipoTransacao == 0 ? "C":"D");
                    excelWorksheet.Cells[i, 3].Value = transacoes[k].phone_transaction;
                    excelWorksheet.Cells[i, 4].Value = transacoes[k].montante;
                    excelWorksheet.Cells[i, 5].Value = transacoes[k].data;
                    excelWorksheet.Cells[i, 6].Value = transacoes[k].id_category;
                    excelWorksheet.Cells[i, 7].Value = transacoes[k].tipopayment;
                    excelWorksheet.Cells[i, 8].Value = transacoes[k].payment_reference;
                }
                excelWorksheet = excelWorkbook.Worksheets.Add();
            }

            excelWorkbook.SaveAs(filename, Excel.XlFileFormat.xlWorkbookNormal);
            excelWorkbook.Close();
            excelApplication.Quit();
        }
    }
}
