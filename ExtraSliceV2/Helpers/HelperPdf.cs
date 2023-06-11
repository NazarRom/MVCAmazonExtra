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
            //string html = "<h1>Hola</h1>";
            string html = "<body style=\"font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 20px;\">" +
            "<h1 style=\"text-align: center;\">Factura</h1><table style=\"width: 100%; border-collapse: collapse; margin-top: 20px;\"><tr>" +
            "<th style=\"padding: 10px; text-align: left; background-color: #f2f2f2;\">Nombre</th><th style=\"padding: 10px; text-align: left; background-color: #f2f2f2;\">Cantidad</th>" +
            "<th style=\"padding: 10px; text-align: left; background-color: #f2f2f2;\">Precio</th><th style=\"padding: 10px; text-align: left; background-color: #f2f2f2;\">Total</th></tr>" +
            "<table style=\"width: 100%; border-collapse: collapse; margin-top: 20px;\">";
            double total = 0;

            for (int i = 0; i < cantidad.Count; i++)
            {
                html += $"<tr><td style=\"padding: 10px;\">{productos[i].Nombre_producto}</td><td style=\"padding: 10px;\">{cantidad[i]}</td><td style=\"padding: 10px;\">{productos[i].Precio}€</td><td style=\"padding: 10px;\">{productos[i].Precio * cantidad[i]}€</td></tr>";
                total += ((double)productos[i].Precio * cantidad[i]);
            }
            html += $"<td colspan=\"3\" style=\"padding: 10px; text-align: right; font-weight: bold;\">Total:</td><td style=\"padding: 10px; font-size: 18px; color: #f44336;\">{total}€</td></tr></body>";
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
