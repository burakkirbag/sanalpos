using _PosnetDotNetModule;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;

namespace SanalPos
{
    public class PosPayment
    {
        #region Public Properties
        public string NameSurname { get; set; }
        public string Email { get; set; }
        public decimal PaymentAmount { get; set; }
        public byte Instalment { get; set; }
        public byte Bank { get; set; }
        public CreditCardModel CreditCard { get; set; }
        #endregion

        #region Public Methods
        public ResultMessageModel Payment()
        {
            try
            {
                switch (Bank)
                {
                    case (byte)SanalPos.Util.Banks.Garanti:
                        return Garanti();
                    case (byte)SanalPos.Util.Banks.Akbank:
                        return Akbank();
                    case (byte)SanalPos.Util.Banks.Garanti3D:
                        return Garanti3D();
                    case (byte)SanalPos.Util.Banks.Vakifbank:
                        return Vakifbank();
                    case (byte)SanalPos.Util.Banks.YapiKredi:
                        return YapiKredi();
                    case (byte)SanalPos.Util.Banks.Isbank:
                        return Isbank();
                    case (byte)SanalPos.Util.Banks.Finansbank:
                        return Finansbank();
                    default:
                        return new ResultMessageModel { Status = false, Message = "Pos bulunamadı." };
                }
            }
            catch (Exception ex)
            {
                return new ResultMessageModel { Status = false, Message = ex.Message };
            }
        }
        #endregion

        #region Private Methods
        ResultMessageModel Garanti()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            #region Pos Configuration
            string strMode = "PROD";
            string strVersion = "v0.01";
            string strTerminalID = ""; //8 Haneli TerminalID yazılmalı.
            string _strTerminalID = "0" + strTerminalID;
            string strProvUserID = "PROVAUT";
            string strProvisionPassword = ""; //TerminalProvUserID şifresi
            string strUserID = "";//
            string strMerchantID = ""; //Üye İşyeri Numarası
            string strHostAddress = "https://sanalposprov.garanti.com.tr/VPServlet";
            #endregion

            string order_code = Util.CreateRandomValue(10, true, true, true, false);
            string amount_send = PaymentAmount.ToString();

            if (amount_send.Contains(","))
            {
                string[] split_amount = PaymentAmount.ToString().Split(',');

                if (split_amount[1].Length > 2)
                    amount_send = split_amount[0] + "" + split_amount[1].Substring(0, 2);
                else
                    amount_send = split_amount[0] + "" + split_amount[1];
            }
            else if (amount_send.Contains("."))
            {
                string[] split_amount = PaymentAmount.ToString().Split('.');

                if (split_amount[1].Length > 2)
                    amount_send = split_amount[0] + "" + split_amount[1].Substring(0, 2);
                else
                    amount_send = split_amount[0] + "" + split_amount[1];
            }


            string strIPAddress = HttpContext.Current.Request.UserHostAddress;
            string strEmailAddress = Email;
            string strOrderID = order_code;
            string strNumber = CreditCard.CardNumber;
            string strExpireDate = CreditCard.Month + "" + CreditCard.Year;
            string strCVV2 = CreditCard.Cvc;
            string strAmount = amount_send;
            string strType = "sales";
            string strCurrencyCode = "949";
            string strCardholderPresentCode = "0";
            string strMotoInd = "N";
            string strInstallmentCount = String.Empty;
            if (Instalment > 1)
                strInstallmentCount = Instalment.ToString();


            string SecurityData = Util.GetSHA1(strProvisionPassword + _strTerminalID).ToUpper();
            string HashData = Util.GetSHA1(strOrderID + strTerminalID + strNumber + strAmount + SecurityData).ToUpper();

            string strXML = null;
            strXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + "<GVPSRequest>" + "<Mode>" + strMode + "</Mode>" + "<Version>" + strVersion + "</Version>" + "<Terminal><ProvUserID>" + strProvUserID + "</ProvUserID><HashData>" + HashData + "</HashData><UserID>" + strUserID + "</UserID><ID>" + strTerminalID + "</ID><MerchantID>" + strMerchantID + "</MerchantID></Terminal>" + "<Customer><IPAddress>" + strIPAddress + "</IPAddress><EmailAddress>" + strEmailAddress + "</EmailAddress></Customer>" + "<Card><Number>" + strNumber + "</Number><ExpireDate>" + strExpireDate + "</ExpireDate><CVV2>" + strCVV2 + "</CVV2></Card>" + "<Order><OrderID>" + strOrderID + "</OrderID><GroupID></GroupID><AddressList><Address><Type>S</Type><Name></Name><LastName></LastName><Company></Company><Text></Text><District></District><City></City><PostalCode></PostalCode><Country></Country><PhoneNumber></PhoneNumber></Address></AddressList></Order>" + "<Transaction>" + "<Type>" + strType + "</Type><InstallmentCnt>" + strInstallmentCount + "</InstallmentCnt><Amount>" + strAmount + "</Amount><CurrencyCode>" + strCurrencyCode + "</CurrencyCode><CardholderPresentCode>" + strCardholderPresentCode + "</CardholderPresentCode><MotoInd>" + strMotoInd + "</MotoInd>" + "</Transaction>" + "</GVPSRequest>";

            try
            {
                string data = "data=" + strXML;
                WebRequest _WebRequest = WebRequest.Create(strHostAddress);
                _WebRequest.Method = "POST";
                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                _WebRequest.ContentType = "application/x-www-form-urlencoded";
                _WebRequest.ContentLength = byteArray.Length;
                Stream dataStream = _WebRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse _WebResponse = _WebRequest.GetResponse();
                dataStream = _WebResponse.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                string XML = responseFromServer;
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(XML);
                XmlElement xElement1 = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/ReasonCode") as XmlElement;
                XmlElement xElement3 = xDoc.SelectSingleNode("//GVPSResponse/Transaction/Response/ErrorMsg") as XmlElement;

                if (xElement1.InnerText == "00")
                {
                    return new ResultMessageModel { Status = true };
                }
                else
                {
                    return new ResultMessageModel { Status = false, Message = xElement3.InnerText, Code = xElement1.InnerText };
                }

            }
            catch (Exception ex)
            {
                return new ResultMessageModel { Status = false, Message = ex.Message };
            }
        }
        ResultMessageModel Akbank()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                ePayment.cc5payment mycc5pay = new ePayment.cc5payment();

                #region Pos Configuration
                mycc5pay.name = "";
                mycc5pay.password = "";
                mycc5pay.clientid = "";
                mycc5pay.orderresult = 0; // 0 : gercek islem, 1 : test islem
                mycc5pay.host = "https://www.sanalakpos.com/servlet/cc5ApiServer";
                #endregion

                string order_code = Util.CreateRandomValue(10, true, true, true, false);

                mycc5pay.oid = order_code;
                mycc5pay.cardnumber = CreditCard.CardNumber;
                mycc5pay.expmonth = CreditCard.Month;
                mycc5pay.expyear = CreditCard.Year;
                mycc5pay.cv2 = CreditCard.Cvc;
                mycc5pay.taksit = "";
                mycc5pay.subtotal = PaymentAmount.ToString().Replace(",", ".");
                mycc5pay.currency = "949";
                mycc5pay.chargetype = "Auth";
                mycc5pay.bname = NameSurname;
                mycc5pay.email = Email;

                if (Instalment > 1)
                    mycc5pay.taksit = Instalment.ToString();


                if (mycc5pay.processorder() == "1" && mycc5pay.appr == "Approved")
                {
                    return new ResultMessageModel { Status = true };
                }
                else
                {
                    return new ResultMessageModel { Status = false, Code = mycc5pay.err, Message = mycc5pay.errmsg };
                }
            }
            catch (Exception ex)
            {
                return new ResultMessageModel { Status = false, Message = ex.Message };
            }
        }
        ResultMessageModel Garanti3D()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

                #region Pos Configuration
                string strTerminalID = "";
                string _strTerminalID = "0" + strTerminalID;
                string strStoreKey = "";
                string strMerchantID = "";
                string strProvisionPassword = "";
                string strSuccessURL = "";
                string strErrorURL = "";
                #endregion

                string strAmount = PaymentAmount.ToString();

                if (PaymentAmount.ToString().Contains(","))
                {
                    string[] split_amount = PaymentAmount.ToString().Split(',');

                    if (split_amount[1].Length > 2)
                        strAmount = split_amount[0] + "" + split_amount[1].Substring(0, 2);
                    else
                        strAmount = split_amount[0] + "" + split_amount[1];
                }
                else if (PaymentAmount.ToString().Contains("."))
                {
                    string[] split_amount = PaymentAmount.ToString().Split('.');

                    if (split_amount[1].Length > 2)
                        strAmount = split_amount[0] + "" + split_amount[1].Substring(0, 2);
                    else
                        strAmount = split_amount[0] + "" + split_amount[1];
                }

                string strType = "sales";
                string strInstallmentCount = "";
                string strOrderID = Util.CreateRandomValue(10, true, true, true, false);

                string SecurityData = Util.GetSHA1(strProvisionPassword + _strTerminalID).ToUpper();
                string HashData = Util.GetSHA1(strTerminalID + strOrderID + strAmount + strSuccessURL + strErrorURL + strType + strInstallmentCount + strStoreKey + SecurityData).ToUpper();

                NameValueCollection order_payment = new NameValueCollection();
                order_payment.Add("cardnumber", CreditCard.CardNumber);
                order_payment.Add("cardexpiredatemonth", CreditCard.Month);
                order_payment.Add("cardexpiredateyear", CreditCard.Year);
                order_payment.Add("cardcvv2", CreditCard.Cvc);
                order_payment.Add("mode", "PROD");
                order_payment.Add("secure3dsecuritylevel", "3D");
                order_payment.Add("apiversion", "v0.01");
                order_payment.Add("terminalprovuserid", "PROVAUT");
                order_payment.Add("terminaluserid", strTerminalID);
                order_payment.Add("terminalmerchantid", strMerchantID);
                order_payment.Add("txntype", strType);
                order_payment.Add("txnamount", strAmount);
                order_payment.Add("txncurrencycode", "949");
                order_payment.Add("txninstallmentcount", strInstallmentCount);
                order_payment.Add("orderid", strOrderID);
                order_payment.Add("terminalid", strTerminalID);
                order_payment.Add("successurl", strSuccessURL);
                order_payment.Add("errorurl", strErrorURL);
                order_payment.Add("customeremailaddress", Email);
                order_payment.Add("customeripaddress", HttpContext.Current.Request.UserHostAddress);
                order_payment.Add("secure3dhash", HashData);

                Util.RedirectAndPOST((Page)HttpContext.Current.Handler, "https://sanalposprov.garanti.com.tr/servlet/gt3dengine", order_payment);

                return new ResultMessageModel { Status = false };
            }
            catch (Exception ex)
            {
                return new ResultMessageModel { Status = false, Message = ex.Message };
            }
        }
        ResultMessageModel Vakifbank()
        {
            try
            {
                #region Pos Configuration
                string strUserName = "";
                string strUserPassword = "";
                string strUserClientId = "";
                string strPosNumber = "";
                string strSecurityCode = "";
                #endregion

                string strInstalment = "00";
                string strMonth = string.Format("{0:00}", CreditCard.Year);
                string strYear = CreditCard.Year;
                string strAmount = PaymentAmount.ToString();
                string strIPAddress = HttpContext.Current.Request.UserHostAddress;
                string strOrderID = Util.CreateRandomValue(10, true, true, true, false);

                byte[] b = new byte[5000];
                string result;
                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("ISO-8859-9");

                if (Instalment == 1)
                    strInstalment = "00";
                else
                    strInstalment = String.Format("{0:00}", Instalment);

                if (CreditCard.Year.Length == 4)
                    strYear = strYear.Substring(2, 2);
                else
                    strYear = CreditCard.Year.ToString();

                strAmount = strAmount.Replace(".", "");
                strAmount = strAmount.Replace(",", "");
                strAmount = String.Format("{0:0000000000.00}", Convert.ToInt32(strAmount)).Replace(",", "");

                string provizyonMesaji = "kullanici=" + strUserName + "&sifre=" + strUserPassword + "&islem=PRO&uyeno=" + strUserClientId + "&posno=" + strPosNumber + "&kkno=" + CreditCard.CardNumber + "&gectar=" + strYear
                    + strMonth + "&cvc=" + string.Format("{0:000}", CreditCard.Cvc) + "&tutar=" + strAmount + "&provno=000000&taksits=" + strInstalment + "&islemyeri=I&uyeref=" + strOrderID + "&vbref=0&khip=" + strIPAddress + "&xcip=" + strSecurityCode;

                b.Initialize();
                b = Encoding.ASCII.GetBytes(provizyonMesaji);

                WebRequest h1 = (WebRequest)HttpWebRequest.Create("https://subesiz.vakifbank.com.tr/vpos724v3/?" + provizyonMesaji);
                h1.Method = "GET";

                WebResponse wr = h1.GetResponse();
                Stream s2 = wr.GetResponseStream();

                byte[] buffer = new byte[10000];
                int len = 0, r = 1;
                while (r > 0)
                {
                    r = s2.Read(buffer, len, 10000 - len);
                    len += r;
                }
                s2.Close();
                result = encoding.GetString(buffer, 0, len).Replace("\r", "").Replace("\n", "");

                String successCode, refCode;
                XmlNode node = null;
                XmlDocument _msgTemplate = new XmlDocument();
                _msgTemplate.LoadXml(result);
                node = _msgTemplate.SelectSingleNode("//Cevap/Msg/Kod");
                successCode = node.InnerText.ToString();

                if (successCode == "00")
                {
                    node = _msgTemplate.SelectSingleNode("//Cevap/Msg/ProvNo");
                    refCode = node.InnerText.ToString();
                    return new ResultMessageModel { Status = true, Code = refCode, Message = "" };
                }
                else
                {
                    return new ResultMessageModel { Status = false, Code = successCode, Message = "" };
                }
            }
            catch (Exception ex)
            {
                return new ResultMessageModel { Status = false, Message = ex.Message };
            }
        }
        ResultMessageModel YapiKredi()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                C_Posnet posnetObj = new C_Posnet();
                bool result = false;

                #region Pos Configuration
                posnetObj.SetMid("");
                posnetObj.SetTid("");
                posnetObj.SetURL("https://www.posnet.ykb.com/PosnetWebService/XML");
                #endregion

                string instalment = "00";

                if (Instalment > 1)
                    instalment = "0" + Instalment;

                string amount_send = PaymentAmount.ToString();

                if (PaymentAmount.ToString().Contains(","))
                {
                    string[] split_amount = PaymentAmount.ToString().Split(',');

                    if (split_amount[1].Length > 2)
                        amount_send = split_amount[0] + "" + split_amount[1].Substring(0, 2);
                    else
                        amount_send = split_amount[0] + "" + split_amount[1];
                }
                else if (PaymentAmount.ToString().Contains("."))
                {
                    string[] split_amount = PaymentAmount.ToString().Split('.');

                    if (split_amount[1].Length > 2)
                        amount_send = split_amount[0] + "" + split_amount[1].Substring(0, 2);
                    else
                        amount_send = split_amount[0] + "" + split_amount[1];
                }

                result = posnetObj.DoSaleTran(CreditCard.CardNumber, CreditCard.Year + "" + CreditCard.Month, CreditCard.Cvc, Util.CreateOrderId(), amount_send, "YT", instalment);

                if (result)
                {
                    if (posnetObj.GetApprovedCode() == "1" || posnetObj.GetApprovedCode() == "2")
                    {
                        return new ResultMessageModel { Status = true };
                    }
                    else
                    {
                        return new ResultMessageModel { Status = false, Message = posnetObj.GetResponseText(), Code = posnetObj.GetResponseCode() };
                    }
                }
                else
                {
                    return new ResultMessageModel { Status = false, Message = posnetObj.GetResponseText(), Code = posnetObj.GetResponseCode() };
                }
            }
            catch (Exception ex)
            {
                return new ResultMessageModel { Status = false, Message = ex.Message };
            }
        }
        ResultMessageModel Isbank()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                ePayment.cc5payment mycc5pay = new ePayment.cc5payment();

                #region Pos Configuration
                mycc5pay.name = "";
                mycc5pay.password = "";
                mycc5pay.clientid = "";
                mycc5pay.orderresult = 0; // 0 : gercek islem, 1 : test islem
                mycc5pay.host = "https://sanalpos.isbank.com.tr/servlet/cc5ApiServer";
                #endregion


                string order_code = Util.CreateRandomValue(10, true, true, true, false);
                
                mycc5pay.oid = order_code;
                mycc5pay.cardnumber = CreditCard.CardNumber;
                mycc5pay.expmonth = CreditCard.Month;
                mycc5pay.expyear = CreditCard.Year;
                mycc5pay.cv2 = CreditCard.Cvc;
                mycc5pay.taksit = "";
                mycc5pay.subtotal = PaymentAmount.ToString().Replace(",", ".");
                mycc5pay.currency = "949";
                mycc5pay.chargetype = "Auth";
                mycc5pay.bname = NameSurname;
                mycc5pay.email = Email;

                if (Instalment > 1)
                    mycc5pay.taksit = Instalment.ToString();


                if (mycc5pay.processorder() == "1" && mycc5pay.appr == "Approved")
                {
                    return new ResultMessageModel { Status = true };
                }
                else
                {
                    return new ResultMessageModel { Status = false, Code = mycc5pay.err, Message = mycc5pay.errmsg };
                }
            }
            catch (Exception ex)
            {
                return new ResultMessageModel { Status = false, Message = ex.Message };
            }
        }
        ResultMessageModel Finansbank()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                ePayment.cc5payment mycc5pay = new ePayment.cc5payment();

                #region Pos Configuration
                mycc5pay.name = "";
                mycc5pay.password = "";
                mycc5pay.clientid = "";
                mycc5pay.orderresult = 0; // 0 : gercek islem, 1 : test islem
                mycc5pay.host = "https://www.fbwebpos.com/fim/api";

                #endregion

                string order_code = Util.CreateRandomValue(10, true, true, true, false);

                mycc5pay.oid = order_code;
                mycc5pay.cardnumber = CreditCard.CardNumber;
                mycc5pay.expmonth = CreditCard.Month;
                mycc5pay.expyear = CreditCard.Year;
                mycc5pay.cv2 = CreditCard.Cvc;
                mycc5pay.taksit = "";
                mycc5pay.subtotal = PaymentAmount.ToString().Replace(",", ".");
                mycc5pay.currency = "949";
                mycc5pay.chargetype = "Auth";
                mycc5pay.bname = NameSurname;
                mycc5pay.email = Email;

                if (Instalment > 1)
                    mycc5pay.taksit = Instalment.ToString();

                if (mycc5pay.processorder() == "1" && mycc5pay.appr == "Approved")
                {
                    return new ResultMessageModel { Status = true };
                }
                else
                {
                    return new ResultMessageModel { Status = false, Code = mycc5pay.err, Message = mycc5pay.errmsg };
                }
            }
            catch (Exception ex)
            {
                return new ResultMessageModel { Status = false, Message = ex.Message };
            }
        }
        #endregion
    }
}