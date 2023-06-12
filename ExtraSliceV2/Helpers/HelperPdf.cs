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
            //string html = "<div><h1>Factura</h1><table><th>Nombre</th><th>Cantidad</th><th>Precio</th><th>Total</th></tr><table>";
            string tablaHtml = "<table>";
            tablaHtml += "<th>Producto</th>";
            tablaHtml += "<th>Descripción</th>";
            tablaHtml += "<th>Precio</th>";
            tablaHtml += "<th>Cantidad</th>";
            tablaHtml += "<th>Total</th>";
            double total = 0;
            foreach (Producto pro in productos)
            {
                total += (double)pro.Precio;
            }
            for (var i = 0; i < productos.Count(); i++)
            {
                Producto prod = productos[i];
                int cant = cantidad[i];

                tablaHtml += "<tr>";
                tablaHtml += "<td>" + prod.Nombre_producto + "</td>";
                tablaHtml += "<td>" + prod.Descripcion + "</td>";
                tablaHtml += "<td>" + prod.Precio + "€" + "</td>";
                tablaHtml += "<td>" + cant + "</td>";
                total += (double)prod.Precio * (cant - 1);

            }
            tablaHtml += "<td>" + total + "€" + "</td>";
            tablaHtml += "</tr>";
            tablaHtml += "</table>";
            //for (int i = 0; i < productos.Count; i++)
            //{
            //    //html += "<tr><td>{productos[i].Nombre_producto}</td>" +
            //    //    "<td>" + cantidad[i] + "</td>" +
            //    //    "<td>" + productos[i].Precio  + " €</td>" +
            //    //    "<td>" + productos[i].Precio * cantidad[i] + "€</td>" +
            //    //    "</tr>";
            //    total += ((double)productos[i].Precio * cantidad[i]);
            //}
            //html += "<td colspan='3'>Total:</td><td>" + total + "€</td></tr></div>";

            Document document = new Document();
            PdfWriter.GetInstance(document, new FileStream(ruta, FileMode.Create));
            document.Open();
            HTMLWorker htmlWorker = new HTMLWorker(document);
            StringReader stringReader = new StringReader(tablaHtml);
            htmlWorker.Parse(stringReader);
            document.Close();
            return ruta;
        }
    }
}
