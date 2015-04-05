using System;

namespace SanalPos
{
    public partial class _default : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            PosPayment pos = new PosPayment()
            {
                NameSurname = "",
                Email = "",
                Instalment = 1,
                PaymentAmount = 1,
                CreditCard = new CreditCardModel
                {
                    CardNumber = "",
                    Month = "",
                    Year = "",
                    Cvc = ""
                },
                Bank = (byte)Util.Banks.Garanti
            };

            ResultMessageModel result = pos.Payment();
            Response.Write("Sonuç : " + result.Status + "<br>Mesaj : " + result.Message + "<br> Kod : " + result.Code);
        }
    }
}