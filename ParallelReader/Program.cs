
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
        Console.WriteLine("Zadejte argumenty nebo zmáčkněte enter: (? pro info)");
        string[] userData = { "", "" };
        string sem = "";
        int numero = 0;
        bool developer = false;
        bool skip = false;

        StringBuilder stringBuilder = new StringBuilder();
        bool reading = true;
        char readerChar = '\r';

        try //Argumenty
        {
            string argumenty = Console.ReadLine();
            if (argumenty is null)
                argumenty = "";
            if (argumenty.Contains("?"))
            {
                Console.WriteLine("-u [přihlašovací jméno]\n" +
                    "-p [přihlašovací heslo]\n" +
                    "-b (testovací verze programu)\n" +
                    "-s [semestr (typ: B-rok-pololetí; př: B221)]\n" +
                    "-count [počet předmětů k vylistování]\n" +
                    "-skip (program přeskočí přihlašování z konzole a počká na přihlášení v prohlížeči)");
                argumenty = Console.ReadLine();
                if (argumenty is null)
                    argumenty = "";
            }
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
                        developer = true;
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

        ChromeOptions options = new ChromeOptions();
        options.AcceptInsecureCertificates = false;
        ChromeDriver cd = null;

        try
        {
            cd = new ChromeDriver(options);
        }
        catch (Exception ex) { Console.WriteLine("\nChybná verze chromedriveru. Stáhněte správnou(v logu níže je specifikováno jakou) na https://chromedriver.storage.googleapis.com/index.html\n\n" + ex); return; }
        try
        {
            cd.Url = @"https://new.kos.cvut.cz/login";
            cd.Navigate();
            try
            {
                while (cd.Url == @"https://new.kos.cvut.cz/login")
                {
                    if (skip)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Thread.Sleep(1000);
                        IWebElement e = cd.FindElement(By.Id("username"));
                        if (!developer)
                            Console.Clear();
                        if (userData[0].Length < 3)
                        {
                            Console.WriteLine("Zadejte přihlašovají jméno:");
                            string nameS = Console.ReadLine();
                            if (nameS is null)
                                nameS = "";
                            e.SendKeys(nameS);
                            userData[0] = nameS;
                        }
                        else { e.SendKeys(userData[0]); }
                        e = cd.FindElement(By.Id("password"));
                        if (userData[1].Length < 8)
                        {
                            Console.WriteLine("Heslo: ");
                            for (int i = 0; i < 100; i++)
                                e.SendKeys("\b");
                            while (reading)
                            {
                                ConsoleKeyInfo newKey = Console.ReadKey(true);
                                if ((newKey.Key == ConsoleKey.Backspace) || (newKey.Key == ConsoleKey.Delete))  //podmínka pro zobrazování *
                                { Console.Write("\b"); }
                                else { Console.Write("*"); }
                                char passwordKey = newKey.KeyChar;
                                if (passwordKey == readerChar)
                                {
                                    reading = false;
                                }
                                else
                                {
                                    stringBuilder.Append(passwordKey.ToString());
                                }
                            }
                            e.SendKeys(stringBuilder.ToString());
                            reading = true;
                        }
                        else { e.SendKeys(userData[1]); }
                        if (!developer)
                            Console.Clear();
                        e = cd.FindElement(By.CssSelector("button[data-testid='button-login']"));
                        e.Click();
                        Thread.Sleep(1500);
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine("Chyba: " + ex, "Login"); }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Chyba: " + ex.Message);
        }

        if (cd.Url == @"https://new.kos.cvut.cz/login")
        {
            cd.Close();
            Console.WriteLine("Přihlášení neproběhlo úspěšně, spusťe program znovu (zkuste argument -skip pro neomezený počet pokusů přihlášení v prohlížeči)");
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
        try
        {
            IWebElement semesterSel = cd.FindElement(By.CssSelector("div[data-testid='select-semester']"));
            IWebElement clickSem = semesterSel.FindElement(By.CssSelector("svg[class='svg-inline--fa fa-caret-down fa-lg multiselect__caret-icon']"));
            clickSem.Click();
            IList<IWebElement> dropListSem = semesterSel.FindElements(By.CssSelector("li[class='multiselect__element']"));
            if (sem == "")//Zápis semestru pokud není definováno
            {
                string[] dropSemesters = new string[dropListSem.Count];
                Console.WriteLine("Vyber semestr:");
                for (int i = 0; i < dropListSem.Count; i++)
                {
                    Console.WriteLine(i + "." + dropListSem[i].Text);
                    dropSemesters[i] = dropListSem[i].Text.Replace("&nbsp", "").Replace(" ", "");
                }
                int idSem = 0;
                int.TryParse(Console.ReadLine(), out idSem);
                if (idSem < dropSemesters.Length)
                {
                    var fText = dropSemesters[idSem].Split("-");
                    foreach (string s in fText)
                        if (s.StartsWith("B"))
                        {
                            sem = s;
                            break;
                        }
                    //sem = fText[0];
                    Console.WriteLine("Vybráno: " + sem);
                }
                if (sem is null)
                    sem = "";
            }
            clickSem.Click();
            foreach (IWebElement drop in dropListSem)
            {
                if (drop.Text.Contains(sem))
                    drop.Click();
            }
            Thread.Sleep(1500);
        }
        catch (Exception) { }

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
        Console.WriteLine("Konec programu, zadej příkaz: \n sel [po/út/st/čt/pá] - vypíše předměty v určitém dni\nprint - vyexportuje do souboru\nteacher [list/jméno učitele] - vypíše učitele/najde předměty, kde učí\nend");
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
        jmeno = name.Replace("&nbsp", "");
        cas = time.Replace("&nbsp", "");
        ID = id.Replace("&nbsp", "");
        typ = type.Replace("&nbsp", "");
        uci = teacher.Replace("&nbsp", "");
        par = parallel;
        cap = capacita.Replace("&nbsp", "");
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