using System.Text;
using System.Xml.Linq;

namespace TestDataGrid_01.Gestore_Xliff
{
    public static class Xliff_HelperMethods
    {
        public static string GetInnerTextWithoutNamespace(XElement element)
        {
            if (element == null)
            {
                return null;
            }

            StringBuilder innerText = new StringBuilder();

            foreach (XNode node in element.Nodes())
            {
                if (node is XElement el)
                {
                    // Remove the namespace from the element
                    el.Name = XNamespace.None.GetName(el.Name.LocalName);
                    innerText.Append(el.ToString(SaveOptions.DisableFormatting));
                }
                else
                {
                    innerText.Append(node.ToString(SaveOptions.DisableFormatting));
                }
            }
            return innerText.ToString();
        }
    }
}
