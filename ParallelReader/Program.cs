
// See https://aka.ms/new-console-template for more information
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.Net;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Start!");
        ChromeDriver cd = new ChromeDriver();
        try
        {
            cd.Url = @"https://new.kos.cvut.cz/login";
            cd.Navigate();
            Thread.Sleep(1000);
            IWebElement e = cd.FindElement(By.Id("username"));
            Console.Clear();
            Console.WriteLine("Zadejte přihlašovají jméno:");
            e.SendKeys(Console.ReadLine());
            e = cd.FindElement(By.Id("password"));
            Console.WriteLine("Heslo:");
            e.SendKeys(Console.ReadLine());
            Console.Clear();
            IList<IWebElement> webs = cd.FindElements(By.TagName("button"));
            foreach (IWebElement web in webs)
            {
                if (web.Text == "PŘIHLÁSIT")
                    e = web;
            }
            e.Click();
        } catch (Exception ex)
        {
            Console.WriteLine("Chyba: " + ex.Message);
        }
        Thread.Sleep(200);
        if (cd.Url != @"https://new.kos.cvut.cz/login")
        {
            Console.WriteLine("Logged in");
        }
        else
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
        }

        CookieContainer cc = new CookieContainer();

        //Get the cookies
        foreach (OpenQA.Selenium.Cookie c in cd.Manage().Cookies.AllCookies)
        {
            string name = c.Name;
            string value = c.Value;
            cc.Add(new System.Net.Cookie(name, value, c.Path, c.Domain));
        }

        Thread.Sleep(1000);
        cd.Url = @"https://new.kos.cvut.cz/course-register";
        cd.Navigate();
        Thread.Sleep(1000);
        IWebElement element = cd.FindElement(By.TagName("html"));
        IList<IWebElement> nums = cd.FindElement(By.ClassName("pagination")).FindElements(By.ClassName("page-item"));
        foreach (IWebElement num in nums)
        {
            if (num.Text.Equals("100"))
                element = num;
        }
        Thread.Sleep(1000);
        element.Click();
        Console.WriteLine("Načítám...");
        Thread.Sleep(7000);

        IList<IWebElement> dummy = cd.FindElements(By.TagName("li"));
        List<IWebElement> pages = new List<IWebElement>();
        foreach (IWebElement page in dummy)
        {
            int temp = 1000;
            int.TryParse(page.Text, out temp);
            if (temp <= 0)
                continue;
            if (temp <= 4)
            {
                pages.Add(page);
            }
        }

        List<string> ids = new List<string>();

        for (int it = 1; it <= 4; it++)
        {
            IList<IWebElement> subjects = cd.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
            foreach (IWebElement subject in subjects)
            {
                var items = subject.Text.Split("\n");
                if (items.Length > 3)
                    ids.Add(items[0]);
            }
            if (it != 4)
                pages[it].Click();
            Thread.Sleep(5000);
        }
        Console.WriteLine("Zapsáno IDs: " + ids.Count);
        Console.WriteLine("Vyber semestr: B221 (B-Rok-pololetí)");
        var sem = Console.ReadLine();

        //AutoReading
        List<Subject> predmety = new List<Subject>();
        for (int i = 0; i < ids.Count; i++)
        {
            cd.Url = @$"https://new.kos.cvut.cz/course-syllabus/{ids[i]}/{sem}";
            cd.Navigate();
            Thread.Sleep(500);
            try
            {
                IList<IWebElement> names = cd.FindElements(By.CssSelector("div[data-testid='name']"));
                string nameE = "";
                foreach (IWebElement namet in names)
                {
                    try
                    {
                        if (namet.Text.Contains("Název:"))
                            nameE = namet.Text.Replace("Název:", "");
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                    }
                    Console.WriteLine(nameE);
                }
                IList<IWebElement> elements = cd.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                bool pred = false; //Bool přednášky (závislost)
                foreach (IWebElement webElement in elements)
                {
                    if (webElement.Text.Length < 3)
                        continue;
                    var items = webElement.Text.Split("\n");
                    if (items.Length > 5)
                    {
                        if (items[1].StartsWith("P"))
                            pred = true;
                        if (items[1].StartsWith("C") || items[1].StartsWith("P")){
                            var itemT = items[3];
                            var teach = items[4];
                            if (items[4].StartsWith("("))
                            {
                                itemT += " " + items[4];
                                teach = items[5];
                            }
                            if (pred)
                                itemT = "*" + itemT;
                            items[0].Replace("\n", "");
                            items[1].Replace("\n", "");
                            itemT.Replace("\n", "");
                            teach.Replace("\n", "");
                            ids[i].Replace("\n", "");
                            predmety.Add(new Subject(nameE, itemT, ids[i], items[1], teach, int.Parse(items[0]))); 
                        }
                    }

                }
            }
            catch (Exception) { }
            Debug.WriteLine(predmety.Count);
            Console.WriteLine($"{i}/{ids.Count}");
        }

        Debug.WriteLine(predmety.Count);
        predmety.Sort((s1,s2) => s1.name.CompareTo(s2.name));
        predmety.Sort((s1,s2) => s1.name.CompareTo(s2.name));



        bool end = false;
        string path = "";
        Console.WriteLine("Konec programu, zadej příkaz: \n sel (po/út/st/čt/pá)\nprint\nend");
        while (!end)
        {
            var uInput = Console.ReadLine();
            if (uInput is null) continue;
            var command = uInput.Split(" ");
            switch (command[0])
            {
                case "sel":
                    string den = "po";
                    if (command[1] == null)
                    {
                        Console.WriteLine("Dny:\t PO\t ÚT\t ST\t ČT\t PÁ");
                        den = Console.ReadLine();
                        if (den == null)
                            den = "po";
                    }
                    else
                    {
                        den = command[1];
                    }
                    foreach(var predmet in predmety)
                    {
                        if (den == null)
                            return;
                        if (predmet.time.StartsWith(den.ToLower()))
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
                    File.WriteAllLines(path + "\\Rozvrh.csv", list);
                    break;/*
                case "parse":
                    if (path.Length < 2)
                        break;
                    var linky = File.ReadAllText(path + "\\Rozvrh.csv");
                    linky = linky.Replace("\n", "");
                    linky = linky.Replace("\t", "");
                    var itemS = linky.Split("|");
                    File.WriteAllLines(path + "\\Rozvrh.csv", itemS);
                    break;*/
                case "end":
                    end = true;
                    break;
            }
        }
        cd.Close();
        Console.Clear();
    }

}

class Subject{
    public string name;
    public string time;
    public string id;
    public string type;
    public string teacher;
    public int parallel;
    public Subject(string name, string time, string id, string type, string teacher, int parallel)
    {
        var pismenka = "a b c d e f g h i j k l m n o p q r s t u v w x y z ě š č ř ž ý á í é 0 1 2 3 4 5 6 7 8 9 ( ) ".Split(" ");
        bool nameB = false;
        bool timeB = false;
        bool idB = false;
        bool typeB = false;
        bool teacherB = false;
        foreach(string pismenko in pismenka)
        {
            if (!nameB)
                nameB = name.EndsWith(pismenko);
            if (!timeB)
                timeB = time.EndsWith(pismenko);
            if (!idB)
                idB = id.ToLower().EndsWith(pismenko);
            if (!typeB)
                typeB = type.EndsWith(pismenko);
            if (!teacherB)
                teacherB = teacher.EndsWith(pismenko);
        }
        if (!nameB)
            name = name.Remove(name.Length - 1);
        if (!timeB)
            time = time.Remove(name.Length - 1);
        if (!idB)
            id = id.Remove(name.Length - 1);
        if (!typeB)
            type = type.Remove(name.Length - 1);
        if (!teacherB)
            teacher = teacher.Remove(name.Length - 1);
        this.name = name;
        this.time = time;
        this.id = id;
        this.type = type;
        this.teacher = teacher;
        this.parallel = parallel;

    }

    public string ToPrint()
    {
        string temp = id + ";" + name.Replace(" ", "|") + ";" + time.Replace(" ", "|") + ";" + type + ";" + parallel + ";" + teacher.Replace(" ", "|") + ";";
        temp = temp.Replace("\n", "").Replace("\t", "").Replace(" ", "");
        return temp.Replace("|", " ");
    }

    override public string ToString()
    {
        return id + "\n" + name + "\n" + time + "\n" + type + "\n" + parallel + "\n" + teacher + "\n";
    }
}