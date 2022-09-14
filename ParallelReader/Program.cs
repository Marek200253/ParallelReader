
// See https://aka.ms/new-console-template for more information
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.Net;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        List<Subject> predmety = new List<Subject>();
        Console.WriteLine("Start!");
        Console.WriteLine("Zadejte argumenty nebo zmáčkněte enter:");
        string[] userData = { "", "" };
        string sem = "";
        int numero = 0;
        bool skip = false;
        try //fast-start
        {
            string argumenty = Console.ReadLine();
            var argument = argumenty.Split("-");
            foreach (string arg in argument)
            {
                var cmm = arg.Split(" ");
                switch (cmm[0])
                {
                    case "u": //uzivatel
                        userData[0] = cmm[1];
                        break;
                    case "p": //heslo
                        userData[1] = cmm[1];
                        break;
                    case "b": //test
                        numero = 30;
                        break;
                    case "s": //semestr
                        sem = cmm[1];
                        break;
                    case "count": //počet předmětů
                        int.TryParse(cmm[1], out numero);
                        break;
                    case "skip":
                        skip = true;
                        break;
                }
            }
        }
        catch (Exception) { }

        ChromeDriver cd = new ChromeDriver();
        try
        {
            cd.Url = @"https://new.kos.cvut.cz/login";
            cd.Navigate();
            if (skip)
            {
                while (cd.Url == @"https://new.kos.cvut.cz/login")
                    Thread.Sleep(1000);
            }
            else
            {
                Thread.Sleep(1000);
                IWebElement e = cd.FindElement(By.Id("username"));
                Console.Clear();
                if (userData[0].Length < 3)
                {
                    Console.WriteLine("Zadejte přihlašovají jméno:");
                    e.SendKeys(Console.ReadLine());
                }
                else { e.SendKeys(userData[0]); }
                e = cd.FindElement(By.Id("password"));
                if (userData[1].Length < 8)
                {
                    Console.WriteLine("Heslo:");
                    e.SendKeys(Console.ReadLine());
                }
                else { e.SendKeys(userData[1]); }
                Console.Clear();
                e = cd.FindElement(By.CssSelector("button[data-testid='button-login']"));
                e.Click();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Chyba: " + ex.Message);
        }
        Thread.Sleep(1500);
        if (cd.Url == @"https://new.kos.cvut.cz/login")
            try //druhý pokus
            {
                cd.Url = @"https://new.kos.cvut.cz/login";
                cd.Navigate();
                Thread.Sleep(1000);
                IWebElement e = cd.FindElement(By.Id("username"));
                Console.Clear();
                Console.WriteLine("Zkuste to znovu\nZadejte přihlašovají jméno:");
                e.SendKeys(Console.ReadLine());
                e = cd.FindElement(By.Id("password"));
                Console.WriteLine("Heslo:");
                e.SendKeys(Console.ReadLine());
                Console.Clear();
                e = cd.FindElement(By.CssSelector("button[data-testid='button-login']"));
                e.Click();
                Thread.Sleep(1500);
            }
            catch (NoSuchElementException) { }

        if (cd.Url == @"https://new.kos.cvut.cz/login")
        {
            cd.Close();
            return;
        }
        Console.WriteLine("Přihlášen");

        //Cookies
        CookieContainer cc = new CookieContainer();
        foreach (OpenQA.Selenium.Cookie c in cd.Manage().Cookies.AllCookies)
        {
            string name = c.Name;
            string value = c.Value;
            cc.Add(new System.Net.Cookie(name, value, c.Path, c.Domain));
        }

        //Zapisování ID předmětů
        Thread.Sleep(1000);
        cd.Url = @"https://new.kos.cvut.cz/course-register";
        cd.Navigate();
        Thread.Sleep(1000);
        IWebElement element = cd.FindElement(By.TagName("html"));
        IList<IWebElement> nums = cd.FindElement(By.ClassName("pagination")).FindElements(By.ClassName("page-item"));
        foreach (IWebElement num in nums)//přepnutí na num 100
        {
            if (num.Text.Equals("100"))
                element = num;
        }
        Console.WriteLine("Načítám předměty...");
        Thread.Sleep(1000);
        element.Click();
        Thread.Sleep(7000);
        IList<IWebElement> dummy = cd.FindElements(By.CssSelector("li[class='page-item align-self-center']"));
        List<IWebElement> pages = new List<IWebElement>();
        foreach (IWebElement page in dummy)//Stránky
            try
            {
                pages.Add(page.FindElement(By.TagName("button")));
            }
            catch (NoSuchElementException) { }
        pages.Add(cd.FindElement(By.TagName("html")));
        List<string> ids = new List<string>();
        foreach (var page in pages)
        {
            IList<IWebElement> subjects = cd.FindElement(By.TagName("tbody")).FindElements(By.CssSelector("a[class='link-room text-nowrap']"));
            foreach (IWebElement subject in subjects)
                ids.Add(subject.Text);
            Console.WriteLine("Zapsáno IDs: {0}", ids.Count);
            if (page.TagName == "html")
                continue;
            if (numero == 30)
                break;
            page.Click();
            Thread.Sleep(5000);
        }

        if (sem == "")
        {
            Console.WriteLine("Vyber semestr: B221 (B-Rok-pololetí)");
            sem = Console.ReadLine().ToUpper();
            if (sem is null)
                sem = "B221";
        }

        if (numero == 0)
            numero = ids.Count;
        //AutoReading
        for (int i = 0; i < numero; i++)
        {
            cd.Url = @$"https://new.kos.cvut.cz/course-syllabus/{ids[i]}/{sem}";
            cd.Navigate();
            Thread.Sleep(500);
            try
            {
                string nameE = cd.FindElement(By.CssSelector("div[data-testid='name']")).FindElement(By.CssSelector("span[class='attribute-value']")).Text;
                Console.WriteLine($"\n\n{i + 1}/{numero} - {nameE}");

                bool pred = cd.FindElement(By.CssSelector("div[data-testid='extent']")).Text.Contains("P"); ;
                IList<IWebElement> elements = cd.FindElement(By.CssSelector("div[data-testid='parallels']")).FindElements(By.CssSelector("tr[class='row-headline collapsed']"));
                if (elements.Count > 0)
                {
                    int pocet = 0;
                    foreach (IWebElement webElement in elements)
                    {
                        IWebElement tempElement = webElement.FindElement(By.CssSelector("td[data-testid='parallel-number']")).FindElement(By.CssSelector("div[class='cell-content']"));
                        int par = 0;
                        string[] items = new string[5];
                        int.TryParse(tempElement.Text, out par);
                        tempElement = webElement.FindElement(By.CssSelector("td[data-testid='parallel-type']")).FindElement(By.CssSelector("div[class='cell-content']"));

                        if (pred && !tempElement.Text.StartsWith("P"))
                            items[0] = "*";
                        items[0] += tempElement.Text;
                        if (items[0].StartsWith("P"))
                            pred = true;
                        tempElement = webElement.FindElement(By.CssSelector("td[data-testid='occupied-capacity']")).FindElement(By.CssSelector("div[class='cell-content']"));
                        items[3] = tempElement.Text;
                        tempElement = webElement.FindElement(By.CssSelector("td[data-testid='timetables']")).FindElement(By.CssSelector("div[data-testid='parallel-time']"));
                        items[1] = tempElement.Text;
                        try
                        {
                            tempElement = webElement.FindElement(By.CssSelector("td[data-testid='timetables']")).FindElement(By.CssSelector("div[data-testid='parallel-week']"));
                            items[1] += " " + tempElement.Text;
                        }
                        catch (NoSuchElementException ex) { Debug.WriteLine(ex.Message, "Week"); }
                        var tempElements = webElement.FindElement(By.CssSelector("td[data-testid='timetables']")).FindElements(By.CssSelector("a[data-testid='parallel-teacher']"));
                        if (tempElements.Count > 0)
                        {
                            foreach (IWebElement we in tempElements)
                                items[2] += we.Text + ",";
                            items[2] = items[2].Remove(items[2].Length - 1);
                        }
                        else { items[2] = "-"; }
                        items[4] = cd.FindElement(By.CssSelector("div[data-testid='code']")).FindElement(By.CssSelector("span[class='attribute-value']")).Text;

                        predmety.Add(new Subject(nameE, items[1], items[4], items[0], items[2], par, items[3]));
                        pocet++;
                        string printP = $"Paralelek: {pocet}/{elements.Count}";
                        for (int j = 0; j < printP.Length; j++)
                            Console.Write("\b");
                        Console.Write(printP);

                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message, "WebElement"); }
        }

        Debug.WriteLine(predmety.Count);
        predmety.Sort((s1, s2) => s1.jmeno.CompareTo(s2.jmeno));
        predmety.Sort((s1, s2) => s1.jmeno.CompareTo(s2.jmeno));



        bool end = false;
        string path = "";
        Console.WriteLine("Konec programu, zadej příkaz: \n sel (po/út/st/čt/pá)\nprint\nteacher (list/jmeno)\nend");
        while (!end)
        {
            var uInput = Console.ReadLine();
            if (uInput is null) continue;
            var command = uInput.Split(" ");
            switch (command[0])
            {
                case "sel":
                    string den = "po";
                    if (command.Length == 1)
                    {
                        Console.WriteLine("Dny:\t PO\t ÚT\t ST\t ČT\t PÁ");
                        den = Console.ReadLine();
                    }
                    else
                    {
                        den = command[1];
                    }
                    foreach (var predmet in predmety)
                    {
                        if (den == null)
                            break;
                        if (den == "all")
                        {
                            Console.WriteLine(predmet.ToString());
                            continue;
                        }
                        if (predmet.cas.Contains(den.ToLower()))
                            Console.WriteLine(predmet.ToString());
                    }

                    break;
                case "print":
                    Console.WriteLine("Zapiš cestu k uložení:");
                    List<string> list = new List<string>();
                    predmety.ForEach(item => list.Add(item.ToPrint()));
                    path = Console.ReadLine();
                    if (path == null)
                        path = "";
                    File.WriteAllLines(path + "\\Rozvrh.csv", list, Encoding.UTF8);
                    break;
                case "teacher":
                    string uci = "";
                    HashSet<string> ucitele = new HashSet<string>();
                    if (command.Length == 1)
                    {
                        Console.WriteLine("list\t jmeno ucitele");
                        uci = Console.ReadLine();
                    }
                    if (uci.Length < 1)
                        uci = command[1];
                    foreach (Subject predmet in predmety)
                    {
                        if (uci == "list")
                        {
                            ucitele.Add(predmet.uci);
                            continue;
                        }
                        if (predmet.uci.Contains(uci))
                            Console.WriteLine(predmet.ToString());
                    }
                    foreach (string u in ucitele)
                        Console.WriteLine(u);
                    break;
                case "count":
                    Console.WriteLine(predmety.Count);
                    break;
                case "end":
                    end = true;
                    break;
            }
        }
        cd.Close();
        Console.Clear();
        Console.WriteLine("Můžete zavřít okno");
    }

}

class Subject
{
    public string jmeno;
    public string cas;
    public string ID;
    public string typ;
    public string uci;
    public int par;
    public string cap;
    public Subject(string name, string time, string id, string type, string teacher, int parallel, string capacita)
    {
        jmeno = name;
        cas = time;
        ID = id;
        typ = type;
        uci = teacher;
        par = parallel;
        cap = capacita;
    }

    public string ToPrint()
    {
        return ID + ";" + jmeno + ";" + cas + ";" + typ + ";" + par.ToString() + ";" + uci + ";" + cap;
    }

    override public string ToString()
    {
        return ID + "\n" + jmeno + "\n" + cas + "\n" + typ + "\n" + par + "\n" + uci + "\n" + cap + "\n";
    }
}