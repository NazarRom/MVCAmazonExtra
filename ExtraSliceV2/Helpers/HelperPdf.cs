using ExtraSliceV2.Models;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace MVCAmazonExtra.Helpers
{
    public class HelperPdf
    {
        public string GeneraPdf(List<Producto> productos, List<int> cantidad, string email, ref string nombrevuelta)
        {
            Random rnd = new Random();
            int generado = rnd.Next(0, 1000000);
            string nombre = generado + email + ".pdf";
            nombrevuelta = nombre;
            string basePath = Directory.GetCurrentDirectory();
            string ruta = Path.Combine(basePath, "wwwroot/", nombre);
            string html = "<h1>Hola</h1>";
            //string html = "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n  </head>\r\n<body style=\"font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 20px;\">\r\n  " +
            //"<h1 style=\"text-align: center;\">Factura</h1>\r\n<table style=\"width: 100%; border-collapse: collapse; margin-top: 20px;\">\r\n        <tr>\r\n           " +
            //" <th style=\"padding: 10px; text-align: left; background-color: #f2f2f2;\">Nombre</th>\r\n            <th style=\"padding: 10px; text-align: left; background-color: #f2f2f2;\">Cantidad</th>\r\n  " +
            //"          <th style=\"padding: 10px; text-align: left; background-color: #f2f2f2;\">Precio</th>\r\n            <th style=\"padding: 10px; text-align: left; background-color: #f2f2f2;\">Total</th>\r\n</tr>" +
            //"<table style=\"width: 100%; border-collapse: collapse; margin-top: 20px;\">\r\n";
            double total = 0;

            //for (int i = 0; i < cantidad.Count; i++)
            //{
            //    //html += $"<tr>\r\n<td style=\"padding: 10px;\">{productos[i].Nombre_producto}</td>\r\n<td style=\"padding: 10px;\">{cantidad[i]}</td>\r\n<td style=\"padding: 10px;\">{productos[i].Precio}€</td>\r\n<td style=\"padding: 10px;\">{productos[i].Precio * cantidad[i]}€</td>\r\n</tr>";
            //    total += ((double)productos[i].Precio * cantidad[i]);
            //}
           // html += $"<td colspan=\"3\" style=\"padding: 10px; text-align: right; font-weight: bold;\">Total:</td>\r\n<td style=\"padding: 10px; font-size: 18px; color: #f44336;\">{total}€</td>\r\n</tr>\r\n</body>\r\n</html>\r\n";
            Document document = new Document();
            PdfWriter.GetInstance(document, new FileStream(ruta, FileMode.Create));
            document.Open();
            HTMLWorker htmlWorker = new HTMLWorker(document);
            StringReader stringReader = new StringReader(html);
            htmlWorker.Parse(stringReader);
            document.Close();
            return ruta;
        }
    }
}
