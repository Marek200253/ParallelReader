# ParallelReader
Aplikace funguje přes Selenium, díky kterému není potřeba API informačního systému KOS, který je na ČVUT využívaný pro správu předmětů.
Před spuštěním je potřeba stáhnout Google chrome a případně chromedriver pokud aplikace vyhazuje chybu kvůli verzi.

Momentálně se mi nepodařilo přijít na to, proč při tisku do dokumentu jsou předměty odděleny entry (\n), které rozbijí import do excelu. Prozatím jsem musel manuálně parsovat do UTF-8 přes MS Word a pak ukládat v novém formátu jako prostý text... díky vyexportování jako .txt je možné přebsat příponu na .csv, což je formát, který je kompatibilní s excelem.

ChromeDriver ke stažení [zde](https://chromedriver.storage.googleapis.com/index.html)
